using FeedbackAnalyze.Data.Entities.Enums;

namespace FeedbackAnalyze.Data.Entities;

public class Feedback : BaseEntity
{
    public string Text { get; set; }
    public string OriginalText { get; set; }
    public string? Language { get; set; }
    public string? Sentiment { get; set; }
    public DateTime Created { get; set; }
    public ProcessingStatus Status { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public List<Sentence> Sentences { get; set; }
}