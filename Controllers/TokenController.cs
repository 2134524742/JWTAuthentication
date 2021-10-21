using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthentication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        /// <summary>
        /// 颁发token
        /// </summary>
        /// <returns></returns>
        [HttpGet("Create")]
        public string Create()
        {
            // 定义用户信息
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, "痴者工良"),
                new Claim(JwtRegisteredClaimNames.Email, "66666666666@qq.com"),
            };

            // 和 Startup 中的配置一致
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("abcdABCD1234abcdABCD1234"));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: "server",
                audience: "client007",
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }
    }
}
