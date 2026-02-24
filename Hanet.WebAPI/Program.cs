using Hanet.SDK;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Đăng ký HanetClient như Singleton với config từ appsettings.json
builder.Services.AddSingleton<HanetClient>(sp =>
{
    var config = new HanetConfig
    {
        ClientId = builder.Configuration["Hanet:ClientId"] ?? "YOUR_CLIENT_ID",
        ClientSecret = builder.Configuration["Hanet:ClientSecret"] ?? "YOUR_CLIENT_SECRET",
        AccessToken = builder.Configuration["Hanet:AccessToken"] ?? ""
    };
    return new HanetClient(config);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Hanet API",
        Version = "v1",
        Description = "API wrapper để test tất cả Hanet endpoints qua Swagger UI",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Hanet SDK",
            Url = new Uri("https://hanet.ai")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Serve static files from wwwroot
app.UseDefaultFiles(); // Serve index.html as default
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hanet API v1");
    options.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
