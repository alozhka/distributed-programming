namespace Valuator.Data;

public interface ITextRepository
{
    void SaveText(string id, string text);
    string? GetText(string id);
    IEnumerable<string> GetAllTexts();

    void SaveRank(string id, double rank);
    double? GetRank(string id);

    void SaveSimilarity(string id, double similarity);
    double? GetSimilarity(string id);
}
