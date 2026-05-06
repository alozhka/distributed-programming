using Valuator.Shards;

namespace Valuator.Services;

public static class CountryRegionResolver
{
    public static Region Resolve(string country)
    {
        return country switch
        {
            "Russia" => Region.Ru,
            "France" or "Germany" => Region.Eu,
            "UAE" or "India" => Region.Asia,
            _ => throw new ArgumentException("Unsupported country " + country),
        };
    }
}