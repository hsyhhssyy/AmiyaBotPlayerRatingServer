﻿using Aliyun.OSS;
using AmiyaBotPlayerRatingServer.Data;
using System.Text;
using System.Text.Json.Serialization;
using AmiyaBotPlayerRatingServer.Controllers.Policy;
using AmiyaBotPlayerRatingServer.Hangfire;
using AmiyaBotPlayerRatingServer.Utility;
using Hangfire;
using DateTimeConverter = AmiyaBotPlayerRatingServer.Utility.DateTimeConverter;
using Microsoft.EntityFrameworkCore;
using AmiyaBotPlayerRatingServer.Model;
using AmiyaBotPlayerRatingServer.Services.MAAServices;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Hangfire.Storage;
using AmiyaBotPlayerRatingServer.Localization;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;

builder.Services.AddControllers(config =>
{
    // 全局授权策略，所有未标记的接口都默认要求Authorize
    // 匿名接口需要显式使用[AllowAnonymous]标出
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PlayerRatingDatabaseContext>(options =>
{
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PlayerRatingDatabaseContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<LocalizationIdentityErrorDescriber>(); ;

builder.Services.Configure<IdentityOptions>(options =>
{
    // 设置密码长度
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddScoped(_ => new OssClient(configuration["Aliyun:Oss:EndPoint"],
    configuration["Aliyun:Oss:Key"],
    configuration["Aliyun:Oss:Secret"]));
builder.Services.AddControllers()
    .AddMvcOptions(o => o.ReturnHttpNotAcceptable = false)
    .AddJsonOptions(o =>
    {
            o.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

#region Hangfire

builder.Services.AddHangfire(hfConf => hfConf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new PostgreSqlStorage(PlayerRatingDatabaseContext.GetConnectionString(configuration))));

builder.Services.AddHangfireServer();

builder.Services.AddSingleton(provider =>
    JobStorage.Current.GetMonitoringApi());

#endregion

#region Auth and OAuth

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (authHeader?.StartsWith("Bearer ") == true)
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                else if (context.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddOpenIddict()
    // 注册Entity Framework Core存储。
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<PlayerRatingDatabaseContext>();
    })
    // 注册AspNetCore组件。
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetTokenEndpointUris("/connect/token");
        options.SetUserinfoEndpointUris("/connect/userinfo");

        options.SetConfigurationEndpointUris("/connect/.well-known/openid-configuration");

        options.AllowClientCredentialsFlow();
        options.AllowAuthorizationCodeFlow();

        var keyManager = new OpenIddictKeyManager(configuration);
        var securityKey = keyManager.GetKeys();
        options.AddSigningKey(securityKey);
        options.AddEncryptionKey(securityKey);

        if (env.IsDevelopment())
        {
            options.DisableAccessTokenEncryption();
        }

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .DisableTransportSecurityRequirement();
    }).AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddCredentialOwnerPolicy();
builder.Services.AddOpenIddictScopePolicy();

#endregion

builder.Services.AddHttpClient();

//注入自定义服务
builder.Services.AddScoped<CreateMAATaskService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

//初始化一些Service
using (var scope = app.Services.CreateScope())
{
    //执行数据迁移
    var dbContext = scope.ServiceProvider.GetRequiredService<PlayerRatingDatabaseContext>();
    dbContext.Database.Migrate();
    
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    // 初始化添加全局任务。
    recurringJobManager.AddOrUpdate<MAATakeSnapshotOnAllConnectionsService>("MAATakeSnapshotOnAllConnectionsService", service => service.Collect(), Cron.Hourly);
}



app.AddSystemRoleAsync();

app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireCustomFilter() }
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
