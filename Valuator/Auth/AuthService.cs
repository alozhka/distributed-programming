namespace Valuator.Auth;

public class AuthService(UserRepository userRepository)
{
    public void Register(string username, string password)
    {
        if (userRepository.UsernameExists(username))
        {
            throw new UsernameAlreadyTakenException(username);
        }

        string hash = PasswordHasher.Hash(password);
        User user = new() { Username = username, PasswordHash = hash };
        userRepository.Save(user);
    }

    public bool Validate(string login, string password)
    {
        User? user = userRepository.Get(login);
        return user is not null && PasswordHasher.Verify(password, user.PasswordHash);
    }
}

public class UsernameAlreadyTakenException(string login)
    : Exception($"Имя пользователя '{login}' уже занято");