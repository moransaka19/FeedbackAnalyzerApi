namespace FeedbackAnalyze.Helpers;

public static class StringHelper
{
    public static List<Sentence> SplitTextIntoSentencesSimple(string text)
    {
        var sentences = new List<Sentence>();
        var sentenceEnders = new[] { '.', '!', '?' };

        var startIndex = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (!sentenceEnders.Contains(text[i])) continue;

            sentences.Add(new Sentence
            {
                Text = text.Substring(startIndex, i - startIndex + 1),
                BeginOffset = startIndex,
                EndOffset = i + 1
            });
            startIndex = i + 1;
        }

        // Add the last sentence if it doesn't end with a sentence terminator
        if (startIndex < text.Length)
        {
            sentences.Add(new Sentence
            {
                Text = text.Substring(startIndex),
                BeginOffset = startIndex,
                EndOffset = text.Length
            });
        }

        return sentences;
    }
}

public class Sentence
{
    public string Text { get; set; }
    public int BeginOffset { get; set; }
    public int EndOffset { get; set; }
}