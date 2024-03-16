using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Security.Principal;
using todo.Helpers;


namespace todo.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        public AuthService(UserManager<UserModel> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<(int, string)> Register(RegisterModel model, string role)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return (0, "User already exists");

            UserModel user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
            };
            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded) return (0, "User creation failed! Please check user details and try again.");

            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

            if (await roleManager.RoleExistsAsync(UserRoles.User))
                await userManager.AddToRoleAsync(user, role);

            return (1, "User created successfully!");
        }

        public async Task<(int, string, UserModel?)> Login(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                return (0, "Invalid credentials", null);


            var userRoles = await userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, user.Email),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            string token = GenerateToken(authClaims);


            //SetRefreshToken(refreshToken);

            return (1, token, user);
        }


        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var tokenOptions = ConfigurationHelper.config.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(tokenOptions["Key"]!);

            var expiryHoursString = tokenOptions["ExpiryHours"];

            _ = double.TryParse(expiryHoursString, out double expiryHours);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = tokenOptions["Issuer"],
                Audience = tokenOptions["Audience"],
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static bool ValidateToken(string authToken)
        {

            var tokenOptions = ConfigurationHelper.config.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(tokenOptions["Key"]!);

            var validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = true, // Because there is no expiration in the generated token
                ValidateAudience = true, // Because there is no audiance in the generated token
                ValidateIssuer = true,   // Because there is no issuer in the generated token
                ValidateIssuerSigningKey = true,
                ValidIssuer = tokenOptions["Issuer"],
                ValidAudience = tokenOptions["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key) // The same key as the one that generate the token
            };


            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(authToken, validationParameters, out SecurityToken validatedToken);
                var jwt = (JwtSecurityToken) validatedToken;

                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                // Log the reason why the token is not valid
                return false;
            }

        }


    }
}