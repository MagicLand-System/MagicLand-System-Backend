using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.User;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicLand_System.Utils
{
    public class JwtUtil
    {
        private JwtUtil()
        {

        }
        public static string GenerateJwtToken(User account, Tuple<string, Guid> guidClaim)
        {
            IConfiguration config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true, true)
           .Build();
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            SymmetricSecurityKey secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
            string issuer = config["Jwt:Issuer"];
            List<Claim> claims = new List<Claim>()
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub,account.Phone),
                new Claim(ClaimTypes.Role,account.Role.Name)
            };
            if (guidClaim != null) claims.Add(new Claim(guidClaim.Item1, guidClaim.Item2.ToString()));
            var expires = DateTime.Now.AddHours(1);
            var token = new JwtSecurityToken(issuer, null, claims, notBefore: DateTime.Now, expires, credentials);
            return jwtHandler.WriteToken(token);

            #region
            // if (account == null)
            // {
            //     IConfiguration config = new ConfigurationBuilder()
            //   .SetBasePath(Directory.GetCurrentDirectory())
            //   .AddJsonFile("appsettings.json", true, true)
            //.Build();
            //     JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            //     SymmetricSecurityKey secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            //     var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
            //     string issuer = config["Jwt:Issuer"];
            //     List<Claim> claims = new List<Claim>()
            // {
            //     new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            //     new Claim(JwtRegisteredClaimNames.Sub,student.User.Phone),
            //     new Claim(ClaimTypes.Role,"Student"),
            // };
            //     if (guidClaim != null) claims.Add(new Claim(guidClaim.Item1, guidClaim.Item2.ToString()));
            //     var expires = DateTime.Now.AddHours(1);
            //     var token = new JwtSecurityToken(issuer, null, claims, notBefore: DateTime.Now, expires, credentials);
            //     return jwtHandler.WriteToken(token);
            // }
            // return string.Empty;
            #endregion
        }
        public static string ReadToken(string token)
        {
            IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", true, true)
             .Build();
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(token))
            {
                IEnumerable<Claim> claims = jwtHandler.ReadJwtToken(token).Claims;
                string userId = claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                return userId;
            }
            return string.Empty;
        }
    }
}
