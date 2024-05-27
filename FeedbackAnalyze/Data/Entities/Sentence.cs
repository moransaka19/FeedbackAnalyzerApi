namespace FeedbackAnalyze.Data.Entities;

public class Sentence : BaseEntity
{
    public string Text { get; set; }
    public int BeginOffset { get; set; }
    public int EndOffset { get; set; }

    public int FeedbackId { get; set; }
    public Feedback Feedback { get; set; }

    public List<Tag> Tags { get; set; }
}