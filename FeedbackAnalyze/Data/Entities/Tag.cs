using FeedbackAnalyze.Helpers;

namespace FeedbackAnalyze.Data.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; }
    public string CommonTag { get; set; }
    public string Sentiment { get; set; }
    public int ProductId { get; set; }
    
    public List<Sentence> Sentences { get; set; }
}