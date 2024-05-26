using FeedbackAnalyze.Data.Entities.Enums;

namespace FeedbackAnalyze.Data.Entities;

public class ProductFeedback : BaseEntity
{
    public string Text { get; set; }
    public string? Language { get; set; }
    public string? Sentiment { get; set; }
    public DateTime Created { get; set; }
    public ProcessingStatus Status { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public List<FeedbackSentence> FeedbackSentences { get; set; }
}