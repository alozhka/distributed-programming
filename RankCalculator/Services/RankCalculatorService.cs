namespace RankCalculator.Services;

public class RankCalculatorService(TextRepository textRepository)
{
    public void CalculateRank(string id)
    {
        string? text = textRepository.GetText(id);

        if (text == null)
        {
            throw new KeyNotFoundException($"Text with id {id} was not found.");
        }

        double rank = CalculateRankImpl(text);
        textRepository.SaveRank(id, rank);
    }


    private static double CalculateRankImpl(string text)
    {
        int nonAlphaCount = text.Count(c => !char.IsLetter(c));
        return (double)nonAlphaCount / text.Length;
    }
}