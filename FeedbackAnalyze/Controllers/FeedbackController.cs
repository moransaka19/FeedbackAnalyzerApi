using FeedbackAnalyze.Data;
using FeedbackAnalyze.Data.Entities;
using FeedbackAnalyze.Data.Entities.Enums;
using FeedbackAnalyze.Helpers;
using FeedbackAnalyze.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var sentences = StringHelper.SplitTextIntoSentencesSimple(request.Text);
        
        // Add new feedback to product
        var newProductFeedback = new ProductFeedback
        {
            ProductId = request.ProductId,
            Text = request.Text,
            Created = DateTime.UtcNow,
            Status = ProcessingStatus.Created,
            FeedbackSentences = sentences.Select(x => new FeedbackSentence
            {
                Text = x.Text,
                BeginOffset = x.BeginOffset,
                EndOffset = x.EndOffset
            }).ToList()
        };
        await _context.ProductFeedbacks.AddAsync(newProductFeedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpGet("product-feedbacks/{productId}")]
    public async Task<IActionResult> GetProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.ProductFeedbacks
            .Where(x => x.ProductId == productId)
            .ToListAsync();
        
        return Ok(feedbacks);
    }
    
    [HttpGet("product-feedbacks/{productId}/positive")]
    public async Task<IActionResult> GetPositiveProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.ProductFeedbacks
            .Where(x => x.ProductId == productId && x.Sentiment == "POSITIVE")
            .ToListAsync();
        
        return Ok(feedbacks);
    }

    [HttpGet("product-feedbacks/{productId}/negative")]
    public async Task<IActionResult> GetNegativeProductFeedbacks([FromRoute] int productId)
    {
        var feedbacks = await _context.ProductFeedbacks
            .Where(x => x.ProductId == productId && x.Sentiment == "NEGATIVE")
            .ToListAsync();

        return Ok(feedbacks);
    }

    [HttpGet("products/{productId}/negative/tags")]
    public async Task<IActionResult> GetTags([FromRoute] int productId)
    {
        var negativeFeedbacks = await _context.Feedbacks
            .Include(x => x.FeedbackSentences)
            .Where(x => x.Sentiment == "negative" && x.FeedbackSentences.Any(u => u.ProductFeedback.Product.Id == productId))
            .ToListAsync();

        var response = negativeFeedbacks.Select(x => new GetProductFeedbackModel
        {
            Id = x.Id,
            Tag = x.Tag,
            Sentiment = x.Sentiment,
            Sentences = x.FeedbackSentences.Select(x => x.Text).ToList()
        });
        
        return Ok(response);
    }

    [HttpGet("tags/{tagId}")]
    public async Task<IActionResult> GetTagDetails([FromRoute] int tagId)
    {
        var tag = await _context.Feedbacks
            .Include(x => x.FeedbackSentences)
                .ThenInclude(x => x.ProductFeedback)
            .FirstAsync(x => x.Id == tagId);

        var result = new GetFeedbackModel
        {
            Id = tag.Id,
            Sentiment = tag.Sentiment,
            Tag = tag.Tag,
            Sentences = tag.FeedbackSentences.Select(x => x.Text).ToList(),
            FullText = tag.FeedbackSentences.First().ProductFeedback.Text
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