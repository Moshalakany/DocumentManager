using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Document_Manager.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration.GetValue<string>("AppSettings:Token")!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration.GetValue<string>("AppSettings:Issuer"),
                    ValidAudience = configuration.GetValue<string>("AppSettings:Audience"),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}
