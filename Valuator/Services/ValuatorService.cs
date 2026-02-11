namespace Valuator.Services;

public class ValuatorService(TextRepository textRepository)
{
    public string EvaluateText(string text)
    {
        string id = Guid.NewGuid().ToString();

        textRepository.SaveText(id, text);

        double rank = CalculateRank(text);
        textRepository.SaveRank(id, rank);

        double similarity = CalculateSimilarity(text);
        textRepository.SaveSimilarity(id, similarity);

        return id;
    }

    public double GetRank(string id)
    {
        double? rank = textRepository.GetRank(id);
        return rank ?? throw new KeyNotFoundException("No rank found");
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