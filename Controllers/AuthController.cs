using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using todo.Models;
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
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)  return BadRequest("Invalid payload");

            var (status, message, user) = await _authService.Login(model);

            if (status == 0)
            {
                return BadRequest(message);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(14),
            };

            Response.Cookies.Append("bearer", message, cookieOptions);

            var refreshToken = GeterateRefreshToken();


            SetRefreshToken(refreshToken);

            if(user is not null)
            {
                user.RefreshToken = refreshToken.Token;
                user.TokenExpires = refreshToken.Expires;
                user.TokenCreated = refreshToken.Created;

                //TODO return part of user only
                return Ok(user);
            }

            return BadRequest("User not found");
           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private void SetRefreshToken(RefreshToken newRefreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.Expires,
        };

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
    }

    private RefreshToken GeterateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddDays(7)
        };

        return refreshToken;
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