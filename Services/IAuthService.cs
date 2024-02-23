namespace todo.Services;
public interface IAuthService
{
    Task<(int, string)> Register(RegisterModel model, string role);
    Task<(int, string)> Login(LoginModel model);

}
