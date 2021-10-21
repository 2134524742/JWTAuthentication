using JWTAuthentication.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthentication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication("Bearer").AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    // �Ƿ���ǩ����֤
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConst.SECRETKEY)),
                    // ��������֤������Ҫ��token����Claim���͵ķ����˱���һ��
                    ValidateIssuer = true,
                    ValidIssuer = "API",//������
                    // ��������֤
                    ValidateAudience = true,
                    ValidAudience = "User",//������
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWTAuthentication", Version = "v1" });
                //swagger ��֤����
                c.AddSecurityDefinition("CoreAPI", new OpenApiSecurityScheme
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ���·�����Bearer {token} ���ɣ�ע������֮���пո�",
                    //jwtĬ�ϵĲ�������
                    Name = "authorization",
                    //jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    In =  ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                //�������
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "CoreAPI"
                                    }
                                },
                                new string[] { }
                            }
                        });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();               
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTAuthentication v1"));
            }

            app.Use(async (context, next) =>
            {
                foreach (var item in context.Request.Headers)
                {
                    Console.WriteLine(item);
                }
                await next.Invoke();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            //�û���֤
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
