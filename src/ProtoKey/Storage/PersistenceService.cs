namespace ProtoKey.Storage;

public class PersistenceService(StorageScheduler scheduler, KeyValueStorage storage)
{
    private const string DataFile = "ProtoKey.data";

    public async Task Load(CancellationToken ct)
    {
        if (!File.Exists(DataFile))
        {
            return;
        }

        foreach (string line in await File.ReadAllLinesAsync(DataFile, ct))
        {
            (string key, int value)? parsed = ParseLine(line);
            if (parsed is null)
            {
                continue;
            }

            storage.Set(parsed.Value.key, parsed.Value.value);
        }
    }


    public async Task Flush(CancellationToken ct)
    {
        var lines = new List<string>();

        while (scheduler.WriteLog.Reader.TryRead(out SetCommand? cmd))
        {
            lines.Add($"{cmd.Key} {cmd.Value}");
        }

        if (lines.Count == 0)
        {
            return;
        }

        await File.AppendAllLinesAsync(DataFile, lines, ct);
    }

    private static (string Key, int Value)? ParseLine(string line)
    {
        int sep = line.IndexOf(' ');
        if (sep < 0)
        {
            return null;
        }

        string key = line[..sep];
        if (!int.TryParse(line[(sep + 1)..], out int value))
        {
            return null;
        }

        return (key, value);
    }
}