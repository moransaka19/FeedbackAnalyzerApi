namespace FeedbackAnalyze.Services.Stem;

public struct StemmedWord
{
    public readonly string Value;

    public readonly string Unstemmed;

    public StemmedWord(string value, string unstemmed)
    {
        Value = value;
        Unstemmed = unstemmed;
    }
}