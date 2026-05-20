using System.CommandLine;

namespace ProtoKey.CLI;

class Program
{
    private const string ProtoKeyHost = "http://127.0.0.1:7777";

    static async Task<int> Main(string[] args)
    {
        using HttpClient http = new();
        http.BaseAddress = new Uri(ProtoKeyHost);
        ApiClient api = new(http);
        ProtoKeyService service = new(api);

        RootCommand rootCommand = new("ProtoKey CLI — command-line interface to the key-value store")
        {
            BuildSetCommand(service),
            BuildGetCommand(service),
            BuildKeysCommand(service),
        };

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static Command BuildSetCommand(ProtoKeyService service)
    {
        Argument<string> keyArg = new("key") { Description = "The key to set" };
        Argument<int> valueArg = new("value") { Description = "The 32-bit integer value to store" };
        Command command = new("set", "Set a value by key") { keyArg, valueArg };

        command.SetAction(async parseResult =>
        {
            string key = parseResult.GetValue(keyArg)!;
            int value = parseResult.GetValue(valueArg);
            await service.Set(key, value);
        });

        return command;
    }

    private static Command BuildGetCommand(ProtoKeyService service)
    {
        Argument<string> keyArg = new("key") { Description = "The key to look up" };
        Command command = new("get", "Get a value by key") { keyArg };

        command.SetAction(async parseResult =>
        {
            string key = parseResult.GetValue(keyArg)!;
            await service.Get(key);
        });

        return command;
    }

    private static Command BuildKeysCommand(ProtoKeyService service)
    {
        Argument<string> prefixArg = new("prefix") { Description = "The prefix to filter keys by" };
        Command command = new("keys", "List all keys matching a prefix") { prefixArg };

        command.SetAction(async parseResult =>
        {
            string prefix = parseResult.GetValue(prefixArg)!;
            await service.Keys(prefix);
        });

        return command;
    }
}