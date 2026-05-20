using System.Threading.Channels;

namespace ProtoKey.Storage;

public class StorageService(KeyValueStorage storage)
{
    private const string DataFile = "ProtoKey.data";
    private const int DefaultCapacity = 20;

    private readonly Channel<StorageCommand> _commands = Channel.CreateBounded<StorageCommand>(DefaultCapacity);
    private readonly Channel<SetCommand> _writeLog = Channel.CreateUnbounded<SetCommand>();

    public async Task Set(string key, int value)
    {
        var tcs = new TaskCompletionSource<StorageResponse>();
        await _commands.Writer.WriteAsync(new SetCommand(key, value, tcs));
        await tcs.Task;
    }

    public async Task<GetResponse> Get(string key)
    {
        var tcs = new TaskCompletionSource<StorageResponse>();
        await _commands.Writer.WriteAsync(new GetCommand(key, tcs));
        return (GetResponse)await tcs.Task;
    }

    public async Task<KeysResponse> Keys(string prefix)
    {
        var tcs = new TaskCompletionSource<StorageResponse>();
        await _commands.Writer.WriteAsync(new KeysCommand(prefix, tcs));
        return (KeysResponse)await tcs.Task;
    }

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

        while (_writeLog.Reader.TryRead(out SetCommand? cmd))
        {
            lines.Add($"{cmd.Key} {cmd.Value}");
        }

        if (lines.Count == 0)
        {
            return;
        }

        await File.AppendAllLinesAsync(DataFile, lines, ct);
    }

    public async Task ProcessCommands(CancellationToken ct)
    {
        await foreach (StorageCommand cmd in _commands.Reader.ReadAllAsync(ct))
        {
            switch (cmd)
            {
                case SetCommand set:
                    await HandleSet(set);
                    break;
                case GetCommand get:
                    HandleGet(get);
                    break;
                case KeysCommand keys:
                    HandleKeys(keys);
                    break;
            }
        }
    }

    private async Task HandleSet(SetCommand cmd)
    {
        storage.Set(cmd.Key, cmd.Value);
        cmd.Tcs.SetResult(new SetResponse());
        await _writeLog.Writer.WriteAsync(cmd);
    }

    private void HandleGet(GetCommand cmd)
    {
        int value = storage.Get(cmd.Key);
        cmd.Tcs.SetResult(new GetResponse(value));
    }

    private void HandleKeys(KeysCommand cmd)
    {
        List<string> keys = storage.Keys(cmd.Prefix);
        cmd.Tcs.SetResult(new KeysResponse(keys));
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