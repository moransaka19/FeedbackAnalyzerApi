using FeedbackAnalyze.Data;
using FeedbackAnalyze.Data.Entities;
using FeedbackAnalyze.Data.Entities.Enums;
using FeedbackAnalyze.Helpers;
using FeedbackAnalyze.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentence = FeedbackAnalyze.Data.Entities.Sentence;

namespace FeedbackAnalyze.Controllers;

[ApiController]
[Route("[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly AppDataContext _context;

    public FeedbackController(AppDataContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProductFeedback(
        [FromBody] CreateProductFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        // Add new feedback to product
        var newProductFeedback = new Feedback
        {
            ProductId = request.ProductId,
            OriginalText = request.Text,
            Created = DateTime.UtcNow,
            Status = ProcessingStatus.Created,
        };
        await _context.Feedbacks.AddAsync(newProductFeedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpGet("product-feedbacks/{productId}")]
    public async Task<IActionResult> GetProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(x => x.ProductId == productId)
            .ToListAsync();
        
        return Ok(feedbacks);
    }
    
    [HttpGet("product-feedbacks/{productId}/positive")]
    public async Task<IActionResult> GetPositiveProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(x => x.ProductId == productId && x.Sentiment == "POSITIVE")
            .ToListAsync();
        
        return Ok(feedbacks);
    }

    [HttpGet("product-feedbacks/{productId}/negative")]
    public async Task<IActionResult> GetNegativeProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(x => x.ProductId == productId && x.Sentiment == "NEGATIVE")
            .ToListAsync();

        return Ok(feedbacks);
    }

    [HttpGet("products/{productId}/negative/tags")]
    public async Task<IActionResult> GetTags([FromRoute] int productId)
    {
        var negativeFeedbacks = await _context.Tags
            .Include(x => x.Sentences)
            .Where(x => x.Sentiment == "negative" && x.Sentences.Any(u => u.Feedback.Product.Id == productId))
            .ToListAsync();

        var response = negativeFeedbacks.Select(x => new GetProductFeedbackModel
        {
            Id = x.Id,
            Tag = x.Name,
            Sentiment = x.Sentiment,
            Sentences = x.Sentences.Select(x => x.Text).ToList()
        });
        
        return Ok(response);
    }

    [HttpGet("tags/sentiment/{sentiment}")]
    public async Task<IActionResult> GetTagsBySentiment([FromRoute] string sentiment)
    {
        var tags = await _context.Tags
            .Where(x => x.Sentiment == sentiment)
            .GroupBy(x => x.CommonTag, y => y, (x, y) => new {Key = x, Value = y.Count()})
            .OrderByDescending(x => x.Value)
            .ToListAsync();
        
        return Ok(tags);
    }
    
    [HttpGet("sentences/{sentiment}/{tag}")]
    public async Task<IActionResult> GetSentencesByCommonTag([FromRoute] string tag)
    {
        var tagSentences = await _context.Tags
            .Include(x => x.Sentences)
            .Where(x => x.CommonTag == tag)
            .ToListAsync();
        
        return Ok(tagSentences);
    }

    [HttpGet("tags/{tagId}")]
    public async Task<IActionResult> GetTagDetails([FromRoute] int tagId)
    {
        var tag = await _context.Tags
            .Include(x => x.Sentences)
                .ThenInclude(x => x.Feedback)
            .FirstAsync(x => x.Id == tagId);

        var result = new GetFeedbackModel
        {
            Id = tag.Id,
            Sentiment = tag.Sentiment,
            Tag = tag.Name,
            Sentences = tag.Sentences.Select(x => x.Text).ToList(),
            FullText = tag.Sentences.First().Feedback.Text
        };
        
        return Ok(result);
    }
    
    public class GetFeedbackModel
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Sentiment { get; set; }
        public List<string> Sentences { get; set; }
        public string FullText { get; set; }
    }

    public class GetProductFeedbackModel
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Sentiment { get; set; }
        public List<string> Sentences { get; set; }
    }
}