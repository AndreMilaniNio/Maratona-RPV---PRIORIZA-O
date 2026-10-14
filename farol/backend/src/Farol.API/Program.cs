using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Farol.API.Extensions;
using Farol.API.Filters;
using Farol.API.Hubs;
using Farol.API.Middleware;
using Farol.Application;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Infrastructure;
using Farol.Infrastructure.Authentication;
using Farol.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var modoOperadorUnico = config.GetValue<bool>("Farol:ModoOperadorUnico", true);

builder.Services.Configure<FarolOptions>(config.GetSection(FarolOptions.Secao));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);
builder.Services.AddScoped<INotificadorFila, NotificadorFilaSignalR>();
builder.Services.AddHostedService<ReclassificacaoHostedService>();

builder.Services.AddControllers(o => o.Filters.Add<ValidacaoFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSignalR().AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Autenticação JWT
var jwt = config.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
if (modoOperadorUnico && jwt.Chave.Length < 32)
    jwt.Chave = "modo-operador-unico-local-sem-login-2026";
if (!modoOperadorUnico && jwt.Chave.Length < 32)
    throw new InvalidOperationException("Defina Jwt__Chave com ao menos 32 caracteres (veja .env.example).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emissor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "nome",
        };
        // SignalR envia o token pela query string.
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs")) ctx.Token = token;
                return Task.CompletedTask;
            },
        };
    });
if (modoOperadorUnico)
    builder.Services.PostConfigure<JwtOptions>(o =>
    {
        if (o.Chave.Length < 32) o.Chave = jwt.Chave;
    });

// Uma policy por permissão; as permissões derivam dos perfis do token.
builder.Services.AddAuthorization(o =>
{
    if (modoOperadorUnico)
        o.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true).Build();
    foreach (var permissao in Permissoes.Todas)
        o.AddPolicy(permissao, p => p.RequireAssertion(ctx => modoOperadorUnico ||
            (ctx.User.Identity?.IsAuthenticated == true && Perfis.PermissoesDe(ctx.User.FindAll(ClaimTypes.Role).Select(c => c.Value)).Contains(permissao))));
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(config.GetSection("Farol:Cors:Origens").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Farol API",
        Version = "v1",
        Description = "Gestão, priorização e despacho de ordens de serviço elétricas. Dados e pontuações demonstrativos não são oficiais.",
    });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido em POST /api/auth/login.",
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", doc)] = [] });
    foreach (var xml in new[] { Assembly.GetExecutingAssembly(), typeof(FarolOptions).Assembly }.Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.GetName().Name}.xml")))
        if (File.Exists(xml)) o.IncludeXmlComments(xml);
});

var app = builder.Build();

if (config.GetValue("Farol:Banco:InicializarAoSubir", true))
{
    using var escopo = app.Services.CreateScope();
    await escopo.ServiceProvider.GetRequiredService<Semeador>().ExecutarAsync();
}

app.UseMiddleware<TratamentoErros>();
app.UseSwagger();
app.UseSwaggerUI(o => o.DocumentTitle = "Farol API");
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OperacaoHub>("/hubs/operacao");
app.MapGet("/api/saude", () => Results.Ok(new { status = "ok", em = DateTimeOffset.UtcNow })).AllowAnonymous();

app.Run();

public partial class Program;
