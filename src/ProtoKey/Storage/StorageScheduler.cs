using System.Threading.Channels;

namespace ProtoKey.Storage;

public class StorageScheduler
{
    private const int DefaultCapacity = 20;

    private readonly Channel<StorageCommand> _commands = Channel.CreateBounded<StorageCommand>(DefaultCapacity);
    private readonly Channel<SetCommand> _writeLog = Channel.CreateUnbounded<SetCommand>();

    public async Task Set(string key, int value)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        ValueTask commandWriter = _commands.Writer.WriteAsync(new SetCommand(key, value, tcs));
        ValueTask logWriter = _writeLog.Writer.WriteAsync(new SetCommand(key, value, tcs));

        await Task.WhenAll(commandWriter.AsTask(), logWriter.AsTask());
    }

    public async Task<GetResponse> Get(string key)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        await _commands.Writer.WriteAsync(new GetCommand(key, tcs));

        return (GetResponse)await tcs.Task;
    }

    public async Task<KeysResponse> Keys(string prefix)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        await _commands.Writer.WriteAsync(new KeysCommand(prefix, tcs));

        return (KeysResponse)await tcs.Task;
    }

    public IAsyncEnumerable<StorageCommand> ReadUnprocessedCommands(CancellationToken ct = default)
    {
        return _commands.Reader.ReadAllAsync(ct);
    }

    public IEnumerable<SetCommand> ReadUnflushedCommands()
    {
        while (_writeLog.Reader.TryRead(out SetCommand? cmd))
        {
            yield return cmd;
        }
    }
}