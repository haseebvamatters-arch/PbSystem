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


var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection(); // keep for local dev; if Railway terminates TLS you can keep or remove
app.UseAuthorization();
app.MapControllers();

app.Run();
