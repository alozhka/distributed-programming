using System.Threading.Channels;

namespace ProtoKey.Storage;

public class StorageScheduler
{
    private const int DefaultCapacity = 20;

    public readonly Channel<StorageCommand> Commands = Channel.CreateBounded<StorageCommand>(DefaultCapacity);
    public readonly Channel<SetCommand> WriteLog = Channel.CreateUnbounded<SetCommand>();

    public async Task Set(string key, int value)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        ValueTask commandWriter = Commands.Writer.WriteAsync(new SetCommand(key, value, tcs));
        ValueTask logWriter = WriteLog.Writer.WriteAsync(new SetCommand(key, value, tcs));

        await Task.WhenAll(commandWriter.AsTask(), logWriter.AsTask());
    }

    public async Task<GetResponse> Get(string key)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        await Commands.Writer.WriteAsync(new GetCommand(key, tcs));

        return (GetResponse)await tcs.Task;
    }

    public async Task<KeysResponse> Keys(string prefix)
    {
        TaskCompletionSource<StorageResponse> tcs = new();

        await Commands.Writer.WriteAsync(new KeysCommand(prefix, tcs));

        return (KeysResponse)await tcs.Task;
    }
}