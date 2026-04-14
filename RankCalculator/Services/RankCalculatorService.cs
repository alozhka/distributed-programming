namespace RankCalculator.Services;

public class RankCalculatorService(TextRepository textRepository, EventPublisher eventPublisher)
{
    public async Task CalculateRank(string id, CancellationToken ct = default)
    {
        string? text = textRepository.GetText(id);

        if (text == null)
        {
            throw new KeyNotFoundException($"Text with id {id} was not found.");
        }

        await Wait(ct);
        double rank = CalculateRankImpl(text);
        textRepository.SaveRank(id, rank);

        await eventPublisher.NotifyRankCalculated(id, rank);
    }

    private static double CalculateRankImpl(string text)
    {
        int nonAlphaCount = text.Count(c => !char.IsLetter(c));
        return (double)nonAlphaCount / text.Length;
    }

    private static Task Wait(CancellationToken ct = default)
    {
        Random random = new();
        TimeSpan interval = TimeSpan.FromSeconds(random.Next(3, 15));
        return Task.Delay(interval, ct);
    }
}