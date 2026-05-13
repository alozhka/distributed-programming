namespace Valuator.Services;

public class ValuatorService(TextRepository textRepository, EventPublisher eventPublisher)
{
    public async Task<string> EvaluateText(string text, string author)
    {
        string id = Guid.NewGuid().ToString();

        textRepository.SaveText(id, text);
        textRepository.SaveAuthor(id, author);

        double similarity = CalculateSimilarity(text);
        textRepository.SaveSimilarity(id, similarity);

        await eventPublisher.PublishRank(id);
        await eventPublisher.NotifySimilarityCalculated(id, similarity);

        return id;
    }

    public double? GetRank(string id)
    {
        return textRepository.GetRank(id);
    }

    public double GetSimilarity(string id)
    {
        double? similarity = textRepository.GetSimilarity(id);
        return similarity ?? throw new KeyNotFoundException("No similarity found");
    }

    private static double CalculateRank(string text)
    {
        int nonAlphaCount = text.Count(c => !char.IsLetter(c));
        return (double)nonAlphaCount / text.Length;
    }

    private double CalculateSimilarity(string text)
    {
        int matchCount = 0;
        foreach (string existingText in textRepository.ListTexts())
        {
            if (existingText == text)
            {
                matchCount++;
                if (matchCount > 1)
                {
                    return 1;
                }
            }
        }

        return 0;
    }
}