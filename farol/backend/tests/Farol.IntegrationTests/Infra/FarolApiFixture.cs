using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Farol.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Farol.IntegrationTests.Infra;

/// <summary>
/// Sobe a API real contra um PostgreSQL de teste recriado do zero, com o conjunto demonstrativo.
/// Conexão: variável FAROL_TEST_CONNECTION (padrão: o serviço "postgres" do docker-compose em localhost:5432).
/// </summary>
public class FarolApiFixture : IAsyncLifetime
{
    public const string Senha = "Farol@Demo2026";
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private WebApplicationFactory<Program>? _factory;
    private readonly Dictionary<string, string> _tokens = [];

    public WebApplicationFactory<Program> Factory => _factory!;

    public static string Conexao =>
        Environment.GetEnvironmentVariable("FAROL_TEST_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=farol_test;Username=farol;Password=farol_dev";

    public async Task InitializeAsync()
    {
        var builder = new NpgsqlConnectionStringBuilder(Conexao);
        var banco = builder.Database!;
        builder.Database = "postgres";
        await using (var conexao = new NpgsqlConnection(builder.ConnectionString))
        {
            await conexao.OpenAsync();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{banco}\" WITH (FORCE)", conexao);
            await drop.ExecuteNonQueryAsync();
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__Farol", Conexao);
        Environment.SetEnvironmentVariable("Jwt__Chave", "chave-de-teste-de-integracao-com-mais-de-32-caracteres");
        Environment.SetEnvironmentVariable("Farol__Demo__Habilitado", "true");
        Environment.SetEnvironmentVariable("Farol__Demo__SenhaUsuarios", Senha);
        Environment.SetEnvironmentVariable("Farol__Reclassificacao__Habilitada", "false");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // EnsureCreated não é usado: a API aplica as migrações versionadas e semeia ao subir.
        _factory = new WebApplicationFactory<Program>();
        _ = _factory.CreateClient();
    }

    public async Task<HttpClient> ClienteAsync(string email)
    {
        var cliente = Factory.CreateClient();
        if (!_tokens.TryGetValue(email, out var token))
        {
            var r = await cliente.PostAsJsonAsync("/api/auth/login", new { email, senha = Senha });
            r.EnsureSuccessStatusCode();
            token = (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
            _tokens[email] = token;
        }
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    public async Task<T> ComBancoAsync<T>(Func<FarolDbContext, Task<T>> acao)
    {
        using var escopo = Factory.Services.CreateScope();
        return await acao(escopo.ServiceProvider.GetRequiredService<FarolDbContext>());
    }

    public Task DisposeAsync()
    {
        _factory?.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition(Nome)]
public class ColecaoApi : ICollectionFixture<FarolApiFixture>
{
    public const string Nome = "api";
}

public static class Extensoes
{
    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage r) =>
        await r.Content.ReadFromJsonAsync<JsonElement>(FarolApiFixture.Json);

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient c, string url, object corpo) =>
        c.PostAsJsonAsync(url, corpo, FarolApiFixture.Json);

    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient c, string url, object corpo) =>
        c.PutAsJsonAsync(url, corpo, FarolApiFixture.Json);

    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient c, string url, object corpo) =>
        c.PatchAsJsonAsync(url, corpo, FarolApiFixture.Json);
}
