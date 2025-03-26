using backendRetake.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backendRetake.Services
{
    static public class TokenService
    {
        static public string GetClaims(UserModel user)
        {
            string role = user.Email == "user@example.com" ? "Admin" : "User";
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.Authentication, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        static public async Task<bool> CheckToken(LogoutToken logout, ApplicationDbContext _context)
        {
            LogoutToken? tokenCheck = await _context.TokenBlackListed.FirstOrDefaultAsync(p => p.Token == logout.Token);

            if (tokenCheck != null)
            {
                return true;
            }

            return false;
        }
    }
}
