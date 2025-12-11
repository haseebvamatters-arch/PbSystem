using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
// register the signature service so controllers can receive ISignatureService
builder.Services.AddSingleton<PbSystem.Interfaces.ISignatureService, PbSystem.Interfaces.SignatureService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KSeF Signer Demo API",
        Version = "v1",
        Description = "Demo: sign unsigned KSeF auth XML using a temporary self-signed certificate (demo-only)."
    });
});

// --- Set the listen URL BEFORE building the app ---
// Railway provides PORT env var; fallback to 8080 if missing.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Show Swagger only in Development (move outside if you want it enabled in prod)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger"; // or "" for root
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KSeF Signer Demo API v1");
    });
}

app.UseHttpsRedirection(); // keep for local dev; if Railway terminates TLS you can keep or remove
app.UseAuthorization();
app.MapControllers();

app.Run();
