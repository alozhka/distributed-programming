namespace ProtoKey.Storage;

public class StorageService(StorageScheduler scheduler, KeyValueStorage storage)
{
    public async Task ProcessCommands(CancellationToken ct)
    {
        await foreach (StorageCommand cmd in scheduler.ReadUnprocessedCommands(ct))
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
        storage.Set(cmd.Key, cmd.Value);
        cmd.Tcs.SetResult(new SetResponse());
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
}