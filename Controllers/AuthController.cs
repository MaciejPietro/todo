using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using todo.Services;

namespace todo.Controllers;


[Route("api/[controller]")] 
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid payload");
            var (status, message) = await _authService.Login(model);

            if (status == 0)
                return BadRequest(message);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Invalid payload");

            var role = model.Role;

            // Ensure the role is valid before proceeding
            if (!IsValidRole(role)) return BadRequest("Invalid role");


            var (status, message) = await _authService.Register(model, role);

            if (status == 0) return BadRequest(message);

            return CreatedAtAction(nameof(Register), model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private bool IsValidRole(string role)
    {
         return role == UserRoles.Admin || role == UserRoles.User;
    }
}