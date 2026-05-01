using Dapper;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using System.Text.Json.Serialization;

SqlMapper.AddTypeHandler(new EnumTypeHandler<PollMode>());
SqlMapper.AddTypeHandler(new EnumTypeHandler<ResultsVisibility>());

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
app.Run();
