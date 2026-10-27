using FaceId.Api.Data;
using FaceId.Api.Servises;
using FaceId.Api.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CreateUserRequestValidator>();
builder.Services.AddScoped<UpdateUserRequestValidator>();

builder.Services.AddControllers();

builder.Services.AddHttpClient<FaceRecognitionClient>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8001");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddDbContext<FaceIdDbContext>(option => 
    option.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
