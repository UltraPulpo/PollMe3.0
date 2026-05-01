using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;
using PollMe.Api.Exceptions;
using PollMe.Api.Hubs;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using PollMe.Api.Repositories;
using PollMe.Api.Services;

SqlMapper.AddTypeHandler(new EnumTypeHandler<PollMode>());
SqlMapper.AddTypeHandler(new EnumTypeHandler<ResultsVisibility>());

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.Get<AppConfig>() ?? new AppConfig();
if (string.IsNullOrEmpty(config.Jwt.Secret))
    throw new InvalidOperationException("Jwt__Secret is required");

builder.Services.AddSingleton(config);
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new DbConnectionFactory($"Data Source={config.Database.Path}"));

builder.Services.AddScoped<ICreatorRepository, CreatorRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IVoteService, VoteService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = config.Jwt.Issuer,
            ValidAudience = config.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config.Jwt.Secret)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .AddSource("PollMe")
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(r => r
        .AddSQLite()
        .WithGlobalConnectionString($"Data Source={config.Database.Path}")
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

builder.Services.AddHostedService<MigrationHostedService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (ConflictException ex) when (!ctx.Response.HasStarted)
    {
        ctx.Response.StatusCode = 409;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (NotFoundException ex) when (!ctx.Response.HasStarted)
    {
        ctx.Response.StatusCode = 404;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (PollMe.Api.Exceptions.ValidationException ex) when (!ctx.Response.HasStarted)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception ex) when (!ctx.Response.HasStarted)
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TallyHub>("/hubs/tally");

app.Run();
