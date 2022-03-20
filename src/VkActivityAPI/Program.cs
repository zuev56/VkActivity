using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Zs.Common.Exceptions;
using Zs.Common.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost
    .ConfigureAppConfiguration((context, builder) => builder.AddConfiguration(CreateConfiguration(args)))
    .ConfigureServices(ConfigureServices)
    .UseKestrel()
    .UseDefaultServiceProvider((context, options) =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


IConfiguration CreateConfiguration(string[] args)
{
    if (!File.Exists(ProgramUtilites.MainConfigurationPath))
        throw new AppsettingsNotFoundException();

    var configuration = new ConfigurationManager();
    configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);

    foreach (var arg in args)
    {
        if (!File.Exists(arg))
            throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

        configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
    }

    //AssertConfigurationIsCorrect(configuration);

    return configuration;
}

void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
{
    //throw new NotImplementedException();
}


