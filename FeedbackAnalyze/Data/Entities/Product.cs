namespace FeedbackAnalyze.Data.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }

    public List<Feedback> Feedbacks { get; set; }
}