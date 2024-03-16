using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace todo.Models;


public class UserModel : IdentityUser
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; }

    public DateTime TokenExpires { get; set; }
};
