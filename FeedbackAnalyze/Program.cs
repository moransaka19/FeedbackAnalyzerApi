using System.Text.Json.Serialization;
using Amazon.Comprehend;
using Amazon.Translate;
using FeedbackAnalyze.Data;
using FeedbackAnalyze.Services;
using FeedbackAnalyze.Services.Stem;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<AmazonComprehendClient>();
builder.Services.AddScoped<AmazonTranslateClient>();
builder.Services.AddScoped<IPorter2Stemmer, EnglishPorter2Stemmer>();
builder.Services.AddHostedService<FeedbackProcessingHostedService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddRouting(x => x.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("name",
        policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("name");
app.MapControllers();
app.UseHttpsRedirection();
app.Run();