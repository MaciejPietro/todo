using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// ...

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromForm] UserModel model)
    {

        if (model.Email == "admin@wp.pl" && model.Password == "password")
        {
            var tokenOptions = _configuration.GetSection("JwtSettings");

            var expiryHoursString = tokenOptions["ExpiryHours"];
            //var expiryHoursString = _configuration.GetValue<string>("JwtSettings:ExpiryDays");

            var key = Encoding.UTF8.GetBytes(tokenOptions["Key"]!);

            if (!double.TryParse(expiryHoursString, out double expiryHours))
            {
                return BadRequest("Invalid expiry days configuration");
            }

            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, loginViewModel.UserName),
                //new Claim("fullName", loginViewModel.FirstName + " " + loginViewModel.LastName),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, tokenOptions["Audience"]!),
                new Claim(JwtRegisteredClaimNames.Iss, tokenOptions["Issuer"]!),
                new Claim(ClaimTypes.Name, model.Email)
            };


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

        return Unauthorized();
    }
}