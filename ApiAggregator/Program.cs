using ApiAggregator.Services;
using ApiAggregator.Services.ExternalApis;
using ApiAggregator.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<AggregatedService>();

builder.Services.AddScoped<IExternalApiService, GitHubService>();
builder.Services.AddScoped<IExternalApiService, DevToApiService>();
builder.Services.AddScoped<IExternalApiService>(sp => 
    new NewsService(
        sp.GetRequiredService<HttpClient>(),
        "dd214b667cd74ae89b93659d318088fc"
    ));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
