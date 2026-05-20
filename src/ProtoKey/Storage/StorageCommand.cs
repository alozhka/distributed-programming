namespace ProtoKey.Storage;

public abstract record StorageCommand(TaskCompletionSource<StorageResponse> Tcs);

public record SetCommand(string Key, int Value, TaskCompletionSource<StorageResponse> Tcs)
    : StorageCommand(Tcs);

public record GetCommand(string Key, TaskCompletionSource<StorageResponse> Tcs)
    : StorageCommand(Tcs);

public record KeysCommand(string Prefix, TaskCompletionSource<StorageResponse> Tcs)
    : StorageCommand(Tcs);