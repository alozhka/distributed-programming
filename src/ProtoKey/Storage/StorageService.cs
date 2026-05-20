using System.Threading.Channels;

namespace ProtoKey.Storage;

public class StorageService
{
    private const int DefaultCapacity = 20;
    private readonly Channel<StorageCommand> _commands = Channel.CreateBounded<StorageCommand>(DefaultCapacity);

    private readonly Dictionary<string, int> _store = new();

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

    internal async Task Process(CancellationToken ct)
    {
        await foreach (StorageCommand cmd in _commands.Reader.ReadAllAsync(ct))
        {
            switch (cmd)
            {
                case SetCommand set:
                    HandleSet(set);
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

    private void HandleSet(SetCommand cmd)
    {
        _store[cmd.Key] = cmd.Value;
        cmd.Tcs.SetResult(new SetResponse());
    }

    private void HandleGet(GetCommand cmd)
    {
        int value = _store.GetValueOrDefault(cmd.Key, 0);
        cmd.Tcs.SetResult(new GetResponse(value));
    }

    private void HandleKeys(KeysCommand cmd)
    {
        List<string> keys = _store.Keys
            .Where(k => k.StartsWith(cmd.Prefix))
            .ToList();

        cmd.Tcs.SetResult(new KeysResponse(keys));
    }
}