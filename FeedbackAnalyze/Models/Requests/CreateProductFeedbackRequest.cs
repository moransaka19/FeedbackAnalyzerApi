namespace FeedbackAnalyze.Models.Requests;

public class CreateProductFeedbackRequest
{
    public int ProductId { get; set; }
    public string Text { get; set; }
}