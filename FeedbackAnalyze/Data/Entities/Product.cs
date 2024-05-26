namespace FeedbackAnalyze.Data.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }

    public List<ProductFeedback> ProductFeedbacks { get; set; }
}