using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Attendance.domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Attendence.Helper
{
    public class JWTHelper
    {
        private readonly IConfiguration _configuration;

        public JWTHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");
            if (expiryMinutes <= 0)
            {
                expiryMinutes = 60; // Default safety fallback
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (user.EmployeeId.HasValue)
            {
                claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}