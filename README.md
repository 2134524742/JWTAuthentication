# JWTAuthentication
在.net core 项目中简单实现token 颁发 与验证
# **.net core 中实现 JWT 身份验证**

主要参考：https://www.codemag.com/Article/2105051/Implementing-JWT-Authentication-in-ASP.NET-Core-5

## **什么是JWT(Json Web Token)**

```
JSON Web Token 是一个开放标准 (RFC 7519)，它定义了一种安全、紧凑和自包含的安全方式，用于通过 URL、POST 参数或 HTTP 标头内部在发送方和接收方之间传输信息。需要注意的是，两方之间要安全传输的信息以JSON格式表示，并经过加密签名以验证其真实性。JWT 通常用于在 Web 应用程序中实现身份验证和授权。因为 JWT 是一个标准，所以所有 JWT 都是令牌，但反之则不然。您可以在 .NET、Python、Node.js、Java、PHP、Ruby、Go、JavaScript 等中使用 JSON Web 令牌。
```



## JWT 身份验证的工作原理

![image-20211022110343776](C:\Users\21345\AppData\Roaming\Typora\typora-user-images\image-20211022110343776.png)

## JWT 由三个部分组成：

- 标题
- 载荷
- 签名



### 标题

JWT 标头包含三个部分 - 这些部分包括：令牌的元数据、签名类型和加密算法。它包括两个属性，即“alg”和“typ”。虽然前者涉及使用的密码算法，即本例中的 HS256，但后者用于指定令牌的类型，即本例中的 JWT。

```json
{
  "typ": "JWT",
  "alg": "HS256"
}
```



### 载荷

表示要通过网络传输的 JSON 格式的实际信息。下面给出的代码片段说明了一个简单的有效负载。

```json
{
  "sub": "1234567890",
  "name": "Joydip Kanjilal",
  "admin": true,
  "jti": "cdafc246-109d-4ac9-9aa1-eb689fad9357",
  "iat": 1611497332,
  "exp": 1611500932
}
```

负载通常包含声明、用户的身份信息、允许的权限等。您可以使用声明来传输附加信息。这些也称为 JWT 声明，有两种类型：保留和自定义。以下是保留声明的列表：

- iss：这代表令牌的发行者。
- sub：这是令牌的主题。
- aud：这代表令牌的受众。
- exp：这用于定义令牌到期。
- nbf：这用于指定不得处理令牌的时间。
- iat：这代表令牌的发行时间。
- jti：这代表令牌的唯一标识符。

您还可以使用自定义声明，可以使用规则将其添加到令牌中。



### 签名

签名遵循 JSON Web 签名 (JWS) 规范，用于验证通过网络传输的数据的完整性。它包含标头、有效载荷和秘密的散列，用于确保消息在传输时没有更改。最终签名令牌是通过遵守 JSON Web 签名 (JWS) 规范创建的。编码的 JWT 标头和编码的 JWT 有效载荷组合在一起，然后使用强加密算法（如 HMAC SHA 256）对其进行签名。





## 入门

环境：

Visual Studio 2019

.net 5.0



### 1、创建项目

新建一个web api 项目

![image-20211022111716517](C:\Users\21345\AppData\Roaming\Typora\typora-user-images\image-20211022111716517.png)

选择.net 5 ，是否启用opentapi（Swagger）自由选择

![image-20211022111826710](C:\Users\21345\AppData\Roaming\Typora\typora-user-images\image-20211022111826710.png)



### 2、创建token 工具类

#### TokenModel 用户信息

```C#
 public class TokenModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 身份
        /// </summary>
        public string Sub { get; set; }
    }
```



#### 生成token 

```C#
/// <summary>
/// token 颁布密钥
/// </summary>
public const string SECRETKEY = "xiutao@2134524742@qq.com";

/// <summary>
/// 颁发token
/// </summary>
/// <returns></returns>
public static string CreateToken(TokenModel tokenModel)
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
// 签发者
new Claim(JwtRegisteredClaimNames.Iss,"API"), 
// 接收者
new Claim(JwtRegisteredClaimNames.Aud,"User") 
	};


	// 密钥
	var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenTool.SECRETKEY));
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
```

#### 解析token 信息

```C#
/// <summary>
/// 解析token 信息
/// </summary>
/// <param name="jwtStr"></param>
/// <returns></returns>
public static TokenModel SerializeJwt(string jwtStr)
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
```



### 3、设置接口是否需要保护

添加权限验证

```
    [ApiController]
    [Route("[controller]")]
    //添加权限 也可具体到某一个接口
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [AllowAnonymous]//取消权限
        public string DeleteById(string id)
        {
            return $"删除：{id}";
        }
    }
```

//添加权限 也可具体到某一个接口
[Authorize]

//取消权限 

[AllowAnonymous]



### 4、验证接口

环境：

```
dotnet 添加包 Microsoft.AspNetCore.Authentication
dotnet 添加包 Microsoft.AspNetCore.Authentication.JwtBearer
```

#### Startup 类

##### ConfigureServices

```
public void ConfigureServices(IServiceCollection services)
{

services.AddAuthentication("Bearer").AddJwtBearer(option =>
{
	option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
	{
        // 是否开启签名认证
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenTool.SECRETKEY)),
        // 发行人验证，这里要和token类中Claim类型的发行人保持一致
        ValidateIssuer = true,
        ValidIssuer = "API",//发行人
        // 接收人验证
        ValidateAudience = true,
        ValidAudience = "User",//订阅人
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        };
	});
}
```

注意：这里发行人与订阅人，颁布token密钥 需和颁布密钥配置相同

##### Configure 

```
//用户认证
app.UseAuthentication();
```





dome地址：https://github.com/2134524742/JWTAuthentication.git

