using DotNetEnv;
using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.MyProfile;
using EbayCloneBuyerService_CoreAPI.Repositories.Impl;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Impl;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using EbayCloneBuyerService_CoreAPI.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
// Add services to the container.

// Load file .env
Env.Load();

var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== JWT Authentication & Authorization =====



//==== AutoMapper =====
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<UserProfile>();
});

// ===== DB Context =====
builder.Services.AddDbContext<CloneEbayDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// ===== DI: Repository & Service =====
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<JwtService>();

//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IUserService, UserService>();


// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowAll");

app.MapControllers();
//====== Exception Middleware =====
app.UseMiddleware<ExceptionMiddleware>();
app.Run();
