using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
        .AddEnvironmentVariables();

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .Enrich.WithCorrelationId()
    .Enrich.WithCorrelationIdHeader()
    .WriteTo.Console(outputTemplate: "[{CorrelationId} {Level:u4} {Message:lj}{NewLine}{Exception}]")
    .Filter.ByExcluding(c => c.Level == Serilog.Events.LogEventLevel.Information && c.Properties.Any(p => p.Value.ToString().Contains("health")))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseEndpoints(endpoints =>
{
    app.MapHealthChecks("health");
});

app.MapControllers();

app.Run();
