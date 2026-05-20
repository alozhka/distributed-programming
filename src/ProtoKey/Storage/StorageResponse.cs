namespace ProtoKey.Storage;

public abstract record StorageResponse;

public record SetResponse() : StorageResponse;

public record GetResponse(int Value) : StorageResponse;

public record KeysResponse(IReadOnlyList<string> Keys) : StorageResponse;