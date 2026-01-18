using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using AuthLibrary.Permissions;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Application;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AuthLibrary.Constants.Authentication;
using System.Security.Claims;
using System;
using System.Net;
using ResponseResult.Wrappers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;


namespace WebApi
{
    public static class DependencyInjection
    {
        internal static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var seeder = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>();
                await seeder.SeedIdentityDatabaseAsync();
            }
            return app;
        }

        internal static IServiceCollection AddIdentitySettings(this IServiceCollection services)
        {
            services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }

        internal static TokenSettings GetTokenSettings(this IServiceCollection services, IConfiguration config)
        {
            var tokenSettingsConfig = config.GetSection(nameof(TokenSettings));
            services.Configure<TokenSettings>(tokenSettingsConfig);
            return tokenSettingsConfig.Get<TokenSettings>();
        }


        internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services, 
            TokenSettings tokenSettings)
        {
            var secret = Encoding.ASCII.GetBytes(tokenSettings.Secret);

            services
                .AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = AppClaim.Issuer,
                        ValidAudience = AppClaim.Audience,
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(secret)
                    };
                    bearer.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if(context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("The token has expired."));
                                return context.Response.WriteAsync(result);
                            }
                            else
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("An error occurred processing your authentication."));
                                return context.Response.WriteAsync(result);
                            }
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if(!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorized to access this resource."));
                                return context.Response.WriteAsync(result);
                            }
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You do not have permission to access this resource."));
                            return context.Response.WriteAsync(result);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                foreach(var prop in typeof(AppPermissions).GetNestedTypes()
                    .SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var propertyValue = prop.GetValue(null);
                    if(propertyValue is not null)
                    {
                        options.AddPolicy(propertyValue.ToString(),
                            policy => policy.RequireClaim(AppClaim.Permission, propertyValue.ToString()));
                    }
                }
            });
            return services;
        }

        internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Identity Service",
                    Version = "v1",
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer token Bearer {your token here} to access this API",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    //Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, Array.Empty<string>()}
                });
            });
            return services;
        }

        //internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        //{
        //    services.AddSwaggerGen(c =>
        //    {
        //        c.SwaggerDoc("v1", new OpenApiInfo
        //        {
        //            Title = "Identity Service",
        //            Version = "v1",
        //            License = new OpenApiLicense
        //            {
        //                Name = "MIT License",
        //                Url = new Uri("https://opensource.org/licenses/MIT")
        //            }
        //        });
        //        var securityScheme = new OpenApiSecurityScheme
        //        {
        //            Name = "Authorization",
        //            Description = "Enter JWT Bearer token Bearer {your token here} to access this API",
        //            In = ParameterLocation.Header,
        //            Type = SecuritySchemeType.Http,
        //            Scheme = "bearer",
        //            BearerFormat = "JWT",
        //            //Reference = new OpenApiReference
        //            //{
        //            //    Id = JwtBearerDefaults.AuthenticationScheme,
        //            //    Type = ReferenceType.SecurityScheme
        //            //}
        //        };
        //        //c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        //        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        //        {
        //            {
        //                new OpenApiSecurityScheme
        //                {
        //                    Reference = new OpenApiReference
        //                    {
        //                        Type = ReferenceType.SecurityScheme,
        //                        Id = "Bearer",
        //                    },
        //                    Scheme = "Bearer",
        //                    Name = "Bearer",
        //                    In = ParameterLocation.Header,
        //                }, new List<string>()
        //            }, 
        //        });
        //    });
        //    return services;
        //}
    }
}
