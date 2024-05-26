using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using FeedbackAnalyze.Data;
using FeedbackAnalyze.Data.Entities;
using FeedbackAnalyze.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAnalyze.Services;

public class FeedbackProcessingHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private AppDataContext context;
    private AmazonComprehendClient comprehendClient;

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
        context = scope.ServiceProvider.GetRequiredService<AppDataContext>();
        comprehendClient = scope.ServiceProvider.GetRequiredService<AmazonComprehendClient>();

        var toCheckLanguageFeedbacks = await context.ProductFeedbacks
            .Where(x => x.Status == ProcessingStatus.Created)
            .ToListAsync(cancellationToken);

        if (toCheckLanguageFeedbacks.Any())
        {
            await GetLanguageAsync(toCheckLanguageFeedbacks);
        }

        var toCheckAllTextSentimentFeedbacks = await context.ProductFeedbacks
            .Where(x => x.Status == ProcessingStatus.Language)
            .ToListAsync(cancellationToken);

        if (toCheckAllTextSentimentFeedbacks.Any())
        {
            // get full sentiment
            await GetSentimentAsync(toCheckAllTextSentimentFeedbacks);
        }

        var toAnalyzeSentences = await context.ProductFeedbacks
            .Include(x => x.FeedbackSentences)
            .Where(x => x.Status == ProcessingStatus.Sentiment)
            .ToListAsync(cancellationToken);

        if (toAnalyzeSentences.Any())
        {
            // analyze sentences
            await GetSentenceSentimentAsync(toAnalyzeSentences);
        }
    }

    private async Task GetLanguageAsync(List<ProductFeedback> feedbacks)
    {
        foreach (var productFeedback in feedbacks)
        {
            var detectDominantLanguageRequest = new DetectDominantLanguageRequest()
            {
                Text = productFeedback.Text,
            };

            var response = await comprehendClient.DetectDominantLanguageAsync(detectDominantLanguageRequest);
            productFeedback.Language = response.Languages.OrderByDescending(x => x.Score).First().LanguageCode;
            productFeedback.Status = ProcessingStatus.Language;
        }

        context.UpdateRange(feedbacks);
        await context.SaveChangesAsync();
    }

    private async Task GetSentimentAsync(List<ProductFeedback> feedbacks)
    {
        foreach (var productFeedback in feedbacks)
        {
            var detectSentimentRequest = new DetectSentimentRequest
            {
                Text = productFeedback.Text,
                LanguageCode = productFeedback.Language
            };

            var response = await comprehendClient.DetectSentimentAsync(detectSentimentRequest);
            productFeedback.Sentiment = response.Sentiment;
            productFeedback.Status = ProcessingStatus.Sentiment;
        }

        context.UpdateRange(feedbacks);
        await context.SaveChangesAsync();
    }

    private async Task GetSentenceSentimentAsync(List<ProductFeedback> feedbacks)
    {
        foreach (var productFeedback in feedbacks)
        {
            var detectDominantLanguageRequest = new DetectTargetedSentimentRequest
            {
                Text = productFeedback.Text,
                LanguageCode = productFeedback.Language
            };

            var response = await comprehendClient.DetectTargetedSentimentAsync(detectDominantLanguageRequest);
            var feedbackModels = new List<FeedbackModel>();

            foreach (var targetedSentimentEntity in response.Entities)
            {
                if (targetedSentimentEntity.Mentions.All(x => x.MentionSentiment.Sentiment != SentimentType.NEGATIVE))
                {
                    continue;
                }

                var feedbackSentences = new List<FeedbackSentence>();
                
                foreach (var mention in targetedSentimentEntity.Mentions)
                {
                    var sentence = productFeedback.FeedbackSentences.First(x =>
                        x.BeginOffset <= mention.BeginOffset && x.EndOffset >= mention.EndOffset);
                    feedbackSentences.Add(sentence);
                }
                
                var feedbackModel = new FeedbackModel
                {
                    MainName = targetedSentimentEntity.Mentions[targetedSentimentEntity.DescriptiveMentionIndex.First()].Text,
                    FeedbackSentences = feedbackSentences
                };
                
                feedbackModels.Add(feedbackModel);
            }

            var toInsertFeedbacks = new List<Feedback>();

            foreach (var tag in feedbackModels)
            {
                var feedback = new Feedback
                {
                    Sentiment = "negative",
                    Tag = tag.MainName,
                    FeedbackSentences = tag.FeedbackSentences
                };
                toInsertFeedbacks.Add(feedback);
            }

            await context.Feedbacks.AddRangeAsync(toInsertFeedbacks);

            productFeedback.Status = ProcessingStatus.Finished;
        }

        context.UpdateRange(feedbacks);
        await context.SaveChangesAsync();
    }

    public class FeedbackModel
    {
        public string MainName { get; set; }
        public List<FeedbackSentence> FeedbackSentences { get; set; }
    }
}