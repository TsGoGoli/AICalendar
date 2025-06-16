using AICalendar.Application;
using AICalendar.Infrastructure;
using AICalendar.WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Calendar API", Version = "v1" });
});

// Add Application services
builder.Services.AddApplicationServices();

// Add Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Register API endpoints
app.MapUserEndpoints();
app.MapEventEndpoints();
app.MapParticipantEndpoints();
app.MapSchedulingEndpoints();

app.Run();
