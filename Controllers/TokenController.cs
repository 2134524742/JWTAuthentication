using JWTAuthentication.Models;
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
        [HttpPost]
        public string CreateToken(TokenModel tokenModel)
        {
            //DateTime utc = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti,tokenModel.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,tokenModel.Email),
                // 令牌颁发时间
                new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                 // 过期时间 100秒
                new Claim(JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Iss,"API"), // 签发者
                new Claim(JwtRegisteredClaimNames.Aud,"User") // 接收者
            };


            // 密钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenConst.SECRETKEY));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var tokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwt = new JwtSecurityToken(

                claims: claims,// 声明的集合
                               //expires: .AddSeconds(36), // token的有效时间
                signingCredentials: creds
                );
            var handler = new JwtSecurityTokenHandler();
            // 生成 jwt字符串
            var strJWT = handler.WriteToken(jwt);
            return strJWT;
        }

        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        [HttpGet("SerializeJwt")]
        public TokenModel SerializeJwt(string jwtStr)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(jwtStr);
            object role;
            try
            {
                jwt.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception)
            {
                throw;
            }
            var tm = new TokenModel
            {
                ID = int.Parse(jwt.Id)
                //Role = role != null ? role.ObjToString() : "",
            };
            return tm;
        }
    }
}
