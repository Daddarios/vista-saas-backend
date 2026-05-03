using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;
using Vista.Core.Data;
using Vista.Core.Middleware;
using Vista.Core.Models;
using Vista.Core.Services;
using Vista.Core.Services.ChatBot;
using Vista.Core.Plugins;
using Vista.Core.Validators.Auth;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File("Logs/log-error-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<Benutzer, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequiredLength = 8;
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            else if (context.Request.Cookies.TryGetValue("accessToken", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<JwtService>();

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration["Redis:Configuration"];
    opt.InstanceName = "Vista:";
});
builder.Services.AddScoped<ZweiFaktorService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<FileStorageService>();

// VIKA ChatBot — Semantic Kernel + Ollama (Phi-4 Mini)
var ollamaEndpoint = builder.Configuration["Vika:OllamaEndpoint"] ?? "http://localhost:11434";
var ollamaModel = builder.Configuration["Vika:Model"] ?? "phi4-mini";

var kernelBuilder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
// CPU-Inferenz auf Ollama kann lange dauern; Timeout deutlich erhöhen
var ollamaHttpClient = new HttpClient
{
    BaseAddress = new Uri(ollamaEndpoint),
    Timeout = TimeSpan.FromMinutes(10)
};
kernelBuilder.AddOllamaChatCompletion(ollamaModel, ollamaHttpClient);
#pragma warning restore SKEXP0070
var kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>());

// Kernel Memory (RAG) — Qdrant (Production) veya SimpleVectorDb (Development)
var qdrantEndpoint = builder.Configuration["Vika:QdrantEndpoint"];
var useQdrant = !string.IsNullOrEmpty(qdrantEndpoint) && builder.Environment.IsProduction();

builder.Services.AddSingleton<IKernelMemory>(sp => 
{
    try
    {
        var memoryBuilder = new KernelMemoryBuilder()
            .WithOllamaTextEmbeddingGeneration(new OllamaConfig
            {
                Endpoint = ollamaEndpoint,
                TextModel = new OllamaModelConfig(builder.Configuration["Vika:EmbeddingModel"] ?? "nomic-embed-text")
            })
            .WithOllamaTextGeneration(new OllamaConfig
            {
                Endpoint = ollamaEndpoint,
                TextModel = new OllamaModelConfig(ollamaModel)
            });

        if (useQdrant)
        {
            Log.Information("KernelMemory: Qdrant @ {Endpoint}", qdrantEndpoint);
            memoryBuilder.WithQdrantMemoryDb(qdrantEndpoint!);
        }
        else
        {
            Log.Information("KernelMemory: SimpleVectorDb (Volatile)");
            memoryBuilder.WithSimpleVectorDb(new Microsoft.KernelMemory.MemoryStorage.DevTools.SimpleVectorDbConfig 
            { 
                StorageType = Microsoft.KernelMemory.FileSystem.DevTools.FileSystemTypes.Volatile 
            });
        }

        return memoryBuilder.Build<MemoryServerless>();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "KernelMemory initialisierung fehlgeschlagen.");
        throw;
    }
});

builder.Services.AddScoped<DataIngestionService>(); // RAG aktif

builder.Services.AddSingleton<ChatInputFilter>();
builder.Services.AddSingleton<ChatOutputFilter>();
builder.Services.AddScoped<ChatRateLimiter>();
builder.Services.AddScoped<VikaChatBotService>();
builder.Services.AddScoped<KundePlugin>();
builder.Services.AddScoped<TicketPlugin>();
builder.Services.AddScoped<ProjektPlugin>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddSwaggerGen();

// CORS: Frontend IP'lerine izin ver (güvenli whitelist)
builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontend", policy =>
{
    policy.WithOrigins(
        "http://localhost:5173",      // Local development
        "http://192.168.0.45:5173"    // Network üzerinden erişim (telefon/başka PC)
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
}));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(dbContext);
    await DataSeeder.SeedRolesAndUsersAsync(services);
}

app.UseMiddleware<ExceptionMiddleware>();

// Static files: Logos ve Avatarlar için
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Storage")),
    RequestPath = "/storage",
    OnPrepareResponse = ctx =>
    {
        // Cache ayarları (1 gün)
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400");
    }
});

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Vista.Core.Hubs.BenachrichtigungHub>("/hubs/benachrichtigung");
app.MapHub<Vista.Core.Hubs.ChatHub>("/hubs/chat");
app.MapHub<Vista.Core.Hubs.VikaChatBotHub>("/hubs/vika");

app.Run();
