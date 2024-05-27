using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.Translate;
using Amazon.Translate.Model;
using FeedbackAnalyze.Data;
using FeedbackAnalyze.Data.Entities;
using FeedbackAnalyze.Data.Entities.Enums;
using FeedbackAnalyze.Helpers;
using FeedbackAnalyze.Services.Stem;
using Microsoft.EntityFrameworkCore;
using Sentence = FeedbackAnalyze.Data.Entities.Sentence;
using Tag = FeedbackAnalyze.Data.Entities.Tag;

namespace FeedbackAnalyze.Services;

public class FeedbackProcessingHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private AppDataContext _context;
    private AmazonComprehendClient _comprehendClient;
    private AmazonTranslateClient _translateClient;
    private IPorter2Stemmer _stemmer;

    public FeedbackProcessingHostedService(IServiceScopeFactory scopeFactory)
    {
        _serviceScopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await ProcessFeedbacksAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task ProcessFeedbacksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDataContext>();
        _comprehendClient = scope.ServiceProvider.GetRequiredService<AmazonComprehendClient>();
        _stemmer = scope.ServiceProvider.GetRequiredService<IPorter2Stemmer>();
        _translateClient = scope.ServiceProvider.GetRequiredService<AmazonTranslateClient>();

        var toCheckLanguageFeedbacks = await _context.Feedbacks
            .Where(x => x.Status == ProcessingStatus.Created)
            .ToListAsync(cancellationToken);

        if (toCheckLanguageFeedbacks.Any())
        {
            await GetLanguageAsync(toCheckLanguageFeedbacks);
        }

        var toCheckAllTextSentimentFeedbacks = await _context.Feedbacks
            .Where(x => x.Status == ProcessingStatus.Language)
            .ToListAsync(cancellationToken);

        if (toCheckAllTextSentimentFeedbacks.Any())
        {
            // get full sentiment
            await GetSentimentAsync(toCheckAllTextSentimentFeedbacks);
        }

        var toAnalyzeSentences = await _context.Feedbacks
            .Include(x => x.Sentences)
            .Where(x => x.Status == ProcessingStatus.Sentiment)
            .ToListAsync(cancellationToken);

        if (toAnalyzeSentences.Any())
        {
            // analyze sentences
            await GetSentenceSentimentAsync(toAnalyzeSentences);
        }
    }

    private async Task GetLanguageAsync(List<Feedback> feedbacks)
    {
        var sentenceList = new List<Sentence>();
        
        foreach (var productFeedback in feedbacks)
        {
            var detectDominantLanguageRequest = new DetectDominantLanguageRequest()
            {
                Text = productFeedback.OriginalText,
            };

            var response = await _comprehendClient.DetectDominantLanguageAsync(detectDominantLanguageRequest);
            productFeedback.Language = response.Languages.OrderByDescending(x => x.Score).First().LanguageCode;
            productFeedback.Status = ProcessingStatus.Language;

            if (productFeedback.Language != "en")
            {
                var request = new TranslateTextRequest
                {
                    Text = productFeedback.OriginalText,
                    SourceLanguageCode = productFeedback.Language,
                    TargetLanguageCode = "en"
                };

                var result = await _translateClient.TranslateTextAsync(request);
                productFeedback.Text = result.TranslatedText;
            }
            else
            {
                productFeedback.Text = productFeedback.OriginalText;
            }
            
            var splitSentences = StringHelper.SplitTextIntoSentencesSimple(productFeedback.Text);
            var sentences = splitSentences.Select(x => new Sentence
            {
                FeedbackId = productFeedback.Id,
                Text = x.Text,
                BeginOffset = x.BeginOffset,
                EndOffset = x.EndOffset
            }).ToList();
            sentenceList.AddRange(sentences);
        }
        
        _context.UpdateRange(feedbacks);
        await _context.Sentences.AddRangeAsync(sentenceList);
        await _context.SaveChangesAsync();
    }

    private async Task GetSentimentAsync(List<Feedback> feedbacks)
    {
        foreach (var productFeedback in feedbacks)
        {
            var detectSentimentRequest = new DetectSentimentRequest
            {
                Text = productFeedback.Text,
                LanguageCode = LanguageCode.En
            };

            var response = await _comprehendClient.DetectSentimentAsync(detectSentimentRequest);
            productFeedback.Sentiment = response.Sentiment;
            productFeedback.Status = ProcessingStatus.Sentiment;
        }

        _context.UpdateRange(feedbacks);
        await _context.SaveChangesAsync();
    }

    private async Task GetSentenceSentimentAsync(List<Feedback> feedbacks)
    {
        foreach (var productFeedback in feedbacks)
        {
            var detectDominantLanguageRequest = new DetectTargetedSentimentRequest
            {
                Text = productFeedback.Text,
                LanguageCode = LanguageCode.En
            };

            var response = await _comprehendClient.DetectTargetedSentimentAsync(detectDominantLanguageRequest);
            var tagModels = new List<FeedbackModel>();

            foreach (var targetedSentimentEntity in response.Entities)
            {
                var feedbackSentences = new List<Sentence>();

                if (!targetedSentimentEntity.Mentions.Any(x => x.MentionSentiment.Sentiment == SentimentType.NEGATIVE ||
                                                               x.MentionSentiment.Sentiment == SentimentType.POSITIVE))
                {
                    continue;
                }

                foreach (var mention in targetedSentimentEntity.Mentions)
                {
                    var sentence = productFeedback.Sentences.First(x =>
                        x.BeginOffset <= mention.BeginOffset && x.EndOffset >= mention.EndOffset);
                    feedbackSentences.Add(sentence);
                }

                var sentiment = targetedSentimentEntity.Mentions
                    .GroupBy(x => x.MentionSentiment.Sentiment.ToString(), x => x,
                        (x, z) => new { sentimnetName = x, Count = z.Count() })
                    .OrderByDescending(x => x.Count)
                    .First().sentimnetName;

                var tagModel = new FeedbackModel
                {
                    ProductId = productFeedback.ProductId,
                    MainName = targetedSentimentEntity.Mentions[targetedSentimentEntity.DescriptiveMentionIndex.First()]
                        .Text,
                    FeedbackSentences = feedbackSentences,
                    Sentiment = sentiment
                };

                tagModels.Add(tagModel);
            }

            if (!tagModels.Any())
            {
                return;
            }

            var toInsertFeedbacks = new List<Tag>();

            foreach (var tag in tagModels)
            {
                var feedback = new Tag
                {
                    Sentiment = tag.Sentiment,
                    Name = tag.MainName,
                    CommonTag = _stemmer.Stem(tag.MainName).Value,
                    Sentences = tag.FeedbackSentences,
                    ProductId = tag.ProductId
                };
                toInsertFeedbacks.Add(feedback);
            }

            await _context.Tags.AddRangeAsync(toInsertFeedbacks);

            productFeedback.Status = ProcessingStatus.Finished;
        }

        _context.UpdateRange(feedbacks);
        await _context.SaveChangesAsync();
    }

    public class FeedbackModel
    {
        public int ProductId { get; set; }
        public string Sentiment { get; set; }
        public string MainName { get; set; }
        public List<Sentence> FeedbackSentences { get; set; }
    }
}