namespace ProtoKey.Storage;

public class KeyValueStorage
{
    private readonly Dictionary<string, int> _store = new();

    public void Set(string key, int value)
    {
        _store[key] = value;
    }

    public int Get(string key)
    {
        return _store.GetValueOrDefault(key, 0);
    }

    public List<string> Keys(string prefix)
    {
        return _store.Keys
            .Where(k => k.StartsWith(prefix))
            .ToList();
    }
}