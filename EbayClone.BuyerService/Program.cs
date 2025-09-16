using Contracts;
using NLog;
using LoggerService;
using System.Xml.Schema;
using EbayClone.BuyerService.Extensions;

var builder = WebApplication.CreateBuilder(args);

LogManager.Setup().LoadConfigurationFromFile(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureCors();
builder.Services.ConfigureLoggerManager();  
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();


var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();

if (app.Environment.IsProduction())
    app.UseHsts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("CorsPolicy");

app.Run();
