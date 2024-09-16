using Microsoft.AspNetCore.ResponseCompression;
using Backend;
using Backend.Data;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new GridElementExtConverter());
    options.PayloadSerializerOptions.Converters.Add(new FlipResultConverter());
});;
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient();
// Configure DbContext based on the environment
if (builder.Environment.IsDevelopment())
{
    // builder.Services.AddDbContext<GridDbContext>(options =>
    //     options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection")));
    builder.Services.AddDbContext<GridDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb")); 
}
else
{
    builder.Services.AddDbContext<GridDbContext>(options =>
        options.UseCosmos(
            builder.Configuration["ConnectionStrings:CosmosConnection"],
            databaseName: "YourDatabaseName"));
}

var app = builder.Build();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GridDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        DataGenerator.SeedData(dbContext, 10000);
    }
}

if (builder.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GridHub>("/gridHub");
app.Run();

public class ApiSettings
{
    public string BaseUrl { get; set; }
}