namespace FeedbackAnalyze.Data.Entities;

public class FeedbackSentence : BaseEntity
{
    public string Text { get; set; }
    public int BeginOffset { get; set; }
    public int EndOffset { get; set; }

    public int ProductFeedbackId { get; set; }
    public ProductFeedback ProductFeedback { get; set; }

    public List<Feedback> Feedbacks { get; set; }
}