using EbayCloneBuyerService_CoreAPI.Hubs;
using EbayCloneBuyerService_CoreAPI.Models;
using DotNetEnv;
using EbayCloneBuyerService_CoreAPI.Exceptions;
using EbayCloneBuyerService_CoreAPI.MyProfile;
using EbayCloneBuyerService_CoreAPI.Repositories.Impl;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Impl;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using EbayCloneBuyerService_CoreAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Load file .env
Env.Load();


builder.Services.AddControllers()
 .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .AddOData(options =>
        options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
            .AddRouteComponents("api", GetEdmModel()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== JWT Authentication & Authorization =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

builder.Services.AddAuthorization();



//==== AutoMapper =====
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<UserProfile>();
});

// ===== DB Context =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CloneEbayDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    );
});

// ===== DI: Repository & Service =====
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});



builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRememberTokenRepository, RememberTokenRepository>();
builder.Services.AddScoped<IRememberTokenService, RememberTokenService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<JwtService>();






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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://127.0.0.1:5500") 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
           .SetIsOriginAllowed(_ => true);
});
});

    var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
//app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();
// ===== SignalR Hub =====
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Category>("Category");
    builder.EntitySet<Product>("Product");
    return builder.GetEdmModel();
}
