namespace todo.Services;
public interface IAuthService
{
    Task<(int, string)> Register(RegisterModel model, string role);
    Task<(int, string, UserModel?)> Login(LoginModel model);

}
