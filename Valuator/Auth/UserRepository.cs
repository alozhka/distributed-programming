using StackExchange.Redis;

namespace Valuator.Auth;

public class UserRepository(IConnectionMultiplexer redis)
{
    private const string UserKeyPrefix = "USER-";

    private readonly IDatabase _db = redis.GetDatabase();

    public void Save(User user)
    {
        _db.StringSet(UserKeyPrefix + user.Username, user.PasswordHash, when: When.NotExists);
    }

    public User? Get(string username)
    {
        string? hash = _db.StringGet(UserKeyPrefix + username);
        return hash is null ? null : new User { Username = username, PasswordHash = hash };
    }

    public bool UsernameExists(string username)
    {
        return _db.KeyExists(UserKeyPrefix + username);
    }
}
