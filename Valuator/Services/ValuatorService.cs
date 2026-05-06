using Valuator.Shards;

namespace Valuator.Services;

public class ValuatorService(TextRepository textRepository, EventPublisher eventPublisher)
{
    public async Task<string> EvaluateText(string text, string country)
    {
        string id = Guid.NewGuid().ToString();
        Region region = CountryRegionResolver.Resolve(country);

        textRepository.SaveText(id, text, region);

        double similarity = CalculateSimilarity(region, text);
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

    private double CalculateSimilarity(Region region, string text)
    {
        int matchCount = 0;
        foreach (string existingText in textRepository.ListTexts(region))
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