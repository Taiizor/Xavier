using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Add Xavier
builder.AddXavier();

WebApplication app = builder.Build();

// Use Xavier middleware
app.UseXavier();

// Map controllers
app.MapControllers();

// Map Xavier infrastructure endpoints
app.MapXavierInfrastructureEndpoints();

app.Run();
