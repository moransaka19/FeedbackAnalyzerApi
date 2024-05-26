using FeedbackAnalyze.Helpers;

namespace FeedbackAnalyze.Data.Entities;

public class Feedback : BaseEntity
{
    public string Tag { get; set; }
    public string CommonTag { get; set; }
    public string Sentiment { get; set; }

    public List<FeedbackSentence> FeedbackSentences { get; set; }
}