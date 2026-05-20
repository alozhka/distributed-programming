namespace ProtoKey.CLI;

public class ProtoKeyService(ApiClient api)
{
    public async Task Set(string key, int value)
    {
        try
        {
            await api.Set(key, value);
            Console.WriteLine("OK");
        }
        catch (HttpRequestException e)
        {
            await Console.Error.WriteLineAsync($"Error: {e.Message}");
        }
    }

    public async Task Get(string key)
    {
        try
        {
            int value = await api.Get(key);
            Console.WriteLine(value);
        }
        catch (HttpRequestException e)
        {
            await Console.Error.WriteLineAsync($"Error: {e.Message}");
        }
    }

    public async Task Keys(string prefix)
    {
        try
        {
            string[] keys = await api.Keys(prefix);
            if (keys.Length > 0)
            {
                foreach (string k in keys)
                {
                    Console.WriteLine(k);
                }
            }
            else
            {
                Console.WriteLine("(no keys found)");
            }
        }
        catch (HttpRequestException e)
        {
            await Console.Error.WriteLineAsync($"Error: {e.Message}");
        }
    }
}