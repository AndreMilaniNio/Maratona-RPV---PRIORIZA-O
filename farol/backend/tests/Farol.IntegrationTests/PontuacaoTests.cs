using System.Net;
using System.Text.Json;
using Farol.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Farol.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class PontuacaoTests(FarolApiFixture api)
{
    private readonly Cenarios cenarios = new(api);

    private static object Salvar(JsonElement rascunho, Func<int, string, int?, int?>? ajustar = null, IEnumerable<object>? faixas = null) => new
    {
        versaoId = rascunho.GetProperty("id").GetInt32(),
        token = rascunho.GetProperty("token").GetString(),
        criterios = rascunho.GetProperty("criterios").EnumerateArray()
            .Select(c => new { criterioId = c.GetProperty("criterioId").GetInt32(), habilitado = c.GetProperty("habilitado").GetBoolean() }).ToList(),
        pontos = rascunho.GetProperty("criterios").EnumerateArray().SelectMany(c => c.GetProperty("opcoes").EnumerateArray().Select(o =>
        {
            int? atual = o.GetProperty("pontos").ValueKind == JsonValueKind.Null ? null : o.GetProperty("pontos").GetInt32();
            return new
            {
                opcaoId = o.GetProperty("opcaoId").GetInt32(),
                pontos = ajustar is null ? atual : ajustar(o.GetProperty("opcaoId").GetInt32(), c.GetProperty("codigo").GetString() + ":" + o.GetProperty("codigo").GetString(), atual),
                requerConfirmacao = o.GetProperty("requerConfirmacao").GetBoolean(),
            };
        })).ToList(),
        faixas = faixas ?? rascunho.GetProperty("faixas").EnumerateArray()
            .Select(f => (object)new { prioridadeId = f.GetProperty("prioridadeId").GetInt32(), minimo = f.GetProperty("minimo").GetInt32(), maximo = f.GetProperty("maximo").ValueKind == JsonValueKind.Null ? (int?)null : f.GetProperty("maximo").GetInt32() }).ToList(),
    };

    [Fact]
    public async Task Rascunho_global_concorrente_pendencias_e_faixas_invalidas_bloqueiam_publicacao()
    {
        var chave = await api.ClienteAsync("chave@farol.demo");
        var substituto = await api.ClienteAsync("chave2@farol.demo");

        var criado = await chave.PostAsync("/api/configuracoes/pontuacao/rascunho", null);
        Assert.Equal(HttpStatusCode.OK, criado.StatusCode);
        var rascunho = await criado.JsonAsync();

        // Primeira gravação vence; a segunda, com o token antigo, recebe 409.
        Assert.Equal(HttpStatusCode.OK, (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho", Salvar(rascunho))).StatusCode);
        var conflito = await substituto.PutJsonAsync("/api/configuracoes/pontuacao/rascunho", Salvar(rascunho));
        Assert.Equal(HttpStatusCode.Conflict, conflito.StatusCode);

        // Opção sem pontos ("sem pontos definidos" ≠ zero) impede publicar.
        rascunho = await (await chave.GetAsync($"/api/configuracoes/pontuacao/versoes/{rascunho.GetProperty("id").GetInt32()}")).JsonAsync();
        var semPontos = await (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho",
            Salvar(rascunho, (_, codigo, p) => codigo == "PESSOAS_AFETADAS:DESCONHECIDA" ? null : p))).JsonAsync();
        Assert.Contains(semPontos.GetProperty("pendencias").EnumerateArray(), p => p.GetString()!.Contains("sem pontos definidos"));
        var publicar = await chave.PostJsonAsync("/api/configuracoes/pontuacao/publicar",
            new { versaoId = rascunho.GetProperty("id").GetInt32(), justificativa = "Teste", aplicacao = "SomenteNovas" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, publicar.StatusCode);
        Assert.Contains((await publicar.JsonAsync()).GetProperty("pendencias").EnumerateArray(), p => p.GetString()!.Contains("Desconhecida"));

        // Faixas com lacuna são rejeitadas.
        var faixas = semPontos.GetProperty("faixas").EnumerateArray().Select(f => new { prioridadeId = f.GetProperty("prioridadeId").GetInt32(), minimo = f.GetProperty("minimo").GetInt32(), maximo = f.GetProperty("maximo").ValueKind == JsonValueKind.Null ? (int?)null : f.GetProperty("maximo").GetInt32() }).ToList();
        faixas[^1] = faixas[^1] with { minimo = faixas[^1].minimo + 5 };
        var comLacuna = await (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho", Salvar(semPontos, faixas: faixas))).JsonAsync();
        Assert.Contains(comLacuna.GetProperty("pendencias").EnumerateArray(), p => p.GetString()!.StartsWith("Lacuna"));

        // Pontos negativos são recusados na gravação.
        Assert.Equal(HttpStatusCode.BadRequest, (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho", Salvar(comLacuna, (_, _, _) => -1))).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await chave.DeleteAsync($"/api/configuracoes/pontuacao/rascunho/{rascunho.GetProperty("id").GetInt32()}")).StatusCode);
    }

    [Fact]
    public async Task Versao_da_cidade_prevalece_e_nova_versao_nao_altera_o_historico()
    {
        var chave = await api.ClienteAsync("chave@farol.demo");
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var vtq = await cenarios.MunicipioAsync("VTQ");

        // OS aberta antes da nova versão.
        var antes = await (await supervisor.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync("VTQ", pularUc: 5, classe: "RURAL"))).JsonAsync();
        var osAntes = antes.GetProperty("osId").GetGuid();
        var pontosAntes = antes.GetProperty("pontuacao").GetInt32();
        var versaoAntes = await api.ComBancoAsync(db => db.OrdensServico.Where(o => o.Id == osAntes).Select(o => o.VersaoPontuacaoId).FirstAsync());

        // Rascunho da cidade: corretiva passa a valer +200.
        var rascunho = await (await chave.PostAsync($"/api/configuracoes/pontuacao/rascunho?municipioId={vtq}", null)).JsonAsync();
        var salvo = await (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho",
            Salvar(rascunho, (_, codigo, p) => codigo == "TIPO_MANUTENCAO:CORRETIVA" ? p + 200 : p))).JsonAsync();
        Assert.Empty(salvo.GetProperty("pendencias").EnumerateArray());

        // Impacto lista OS abertas da cidade que mudariam.
        var impacto = await (await chave.GetAsync($"/api/configuracoes/pontuacao/impacto?versaoId={salvo.GetProperty("id").GetInt32()}")).JsonAsync();
        Assert.True(impacto.GetProperty("avaliadas").GetInt32() >= 1);
        Assert.All(impacto.GetProperty("porMunicipio").EnumerateArray(), m => Assert.Equal(vtq, m.GetProperty("municipioId").GetInt32()));

        // Simulador com rascunho já reflete a mudança; produção ainda não.
        var req = await cenarios.SolicitacaoAsync("VTQ", pularUc: 6, classe: "RURAL");
        var simulado = await (await chave.PostJsonAsync("/api/configuracoes/pontuacao/simular", new { municipioId = vtq, usarRascunho = true, solicitacao = req })).JsonAsync();
        var vigente = await (await chave.PostJsonAsync("/api/configuracoes/pontuacao/simular", new { municipioId = vtq, usarRascunho = false, solicitacao = req })).JsonAsync();
        Assert.Equal(vigente.GetProperty("pontuacao").GetInt32() + 200, simulado.GetProperty("pontuacao").GetInt32());

        var publicada = await chave.PostJsonAsync("/api/configuracoes/pontuacao/publicar",
            new { versaoId = salvo.GetProperty("id").GetInt32(), justificativa = "Ajuste da cidade (teste).", aplicacao = "SomenteNovas" });
        Assert.Equal(HttpStatusCode.OK, publicada.StatusCode);
        var versaoNova = (await publicada.JsonAsync()).GetProperty("id").GetInt32();

        // Nova OS da cidade usa a versão municipal; a OS antiga mantém versão e resultado.
        var depois = await (await supervisor.PostJsonAsync("/api/solicitacoes", req)).JsonAsync();
        Assert.Equal(simulado.GetProperty("pontuacao").GetInt32(), depois.GetProperty("pontuacao").GetInt32());
        var detalhe = await (await supervisor.GetAsync($"/api/ordens-servico/{depois.GetProperty("osId").GetGuid()}")).JsonAsync();
        Assert.Equal(vtq, detalhe.GetProperty("classificacao").GetProperty("versao").GetProperty("municipioId").GetInt32());

        var osAntiga = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == osAntes));
        Assert.Equal(versaoAntes, osAntiga.VersaoPontuacaoId);
        Assert.Equal(pontosAntes, osAntiga.Pontuacao);

        // Outra cidade continua na versão global.
        var lum = await (await supervisor.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync("LUM", pularUc: 30))).JsonAsync();
        var detalheLum = await (await supervisor.GetAsync($"/api/ordens-servico/{lum.GetProperty("osId").GetGuid()}")).JsonAsync();
        Assert.Equal(JsonValueKind.Null, detalheLum.GetProperty("classificacao").GetProperty("versao").GetProperty("municipioId").ValueKind);

        // Restaurar a versão demonstrativa cria um novo rascunho; publicar reclassificando abertas grava novo resultado, sem apagar o antigo.
        var restaurado = await chave.PostAsync($"/api/configuracoes/pontuacao/versoes/{versaoNova}/restaurar?municipioId={vtq}", null);
        Assert.Equal(HttpStatusCode.Created, restaurado.StatusCode);
        var rascunho2 = await restaurado.JsonAsync();
        var salvo2 = await (await chave.PutJsonAsync("/api/configuracoes/pontuacao/rascunho",
            Salvar(rascunho2, (_, codigo, p) => codigo == "TIPO_MANUTENCAO:CORRETIVA" ? p - 150 : p))).JsonAsync();
        var resultadosAntes = await api.ComBancoAsync(db => db.ResultadosPrioridade.CountAsync(r => r.OrdemServicoId == osAntes));
        Assert.Equal(HttpStatusCode.OK, (await chave.PostJsonAsync("/api/configuracoes/pontuacao/publicar",
            new { versaoId = salvo2.GetProperty("id").GetInt32(), justificativa = "Reclassificar abertas (teste).", aplicacao = "ReclassificarAbertas" })).StatusCode);
        var (resultadosDepois, versaoAtual) = await api.ComBancoAsync(async db => (
            await db.ResultadosPrioridade.CountAsync(r => r.OrdemServicoId == osAntes),
            await db.OrdensServico.Where(o => o.Id == osAntes).Select(o => o.VersaoPontuacaoId).FirstAsync()));
        Assert.Equal(resultadosAntes + 1, resultadosDepois);
        Assert.Equal(salvo2.GetProperty("id").GetInt32(), versaoAtual);

        var comparacao = await (await chave.GetAsync($"/api/configuracoes/pontuacao/versoes/comparar?a={versaoNova}&b={salvo2.GetProperty("id").GetInt32()}")).JsonAsync();
        Assert.Contains(comparacao.GetProperty("diferencas").EnumerateArray(), d => d.GetProperty("item").GetString()!.Contains("Corretiva"));
        Assert.True(await api.ComBancoAsync(db => db.Auditorias.CountAsync(a => a.Acao == "PUBLICAR_PONTUACAO")) >= 2);
    }
}
