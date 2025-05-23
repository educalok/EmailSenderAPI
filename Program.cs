using EmailSenderAPI.Services.Email;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------ Service Configuration ------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "EmailSenderAPI",
		Version = "v1",
		Description = "Email sending API"
	});
});

// Register custom services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();