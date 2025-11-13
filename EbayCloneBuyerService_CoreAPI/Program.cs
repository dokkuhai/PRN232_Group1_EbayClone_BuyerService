using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Impl;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Impl;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// odata
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .AddOData(options =>
        options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
            .AddRouteComponents("api", GetEdmModel()));

// ===== JWT Authentication & Authorization =====




// ===== DB Context =====
builder.Services.AddDbContext<CloneEbayDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// ===== DI: Repository & Service =====
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IProductServices, ProductServices>();





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

app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Category>("Category");
    builder.EntitySet<Product>("Product");
    return builder.GetEdmModel();
}
