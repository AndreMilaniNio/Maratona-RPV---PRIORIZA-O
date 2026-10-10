using System.Net;
using Farol.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Farol.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class CidadesEPermissoesTests(FarolApiFixture api)
{
    private readonly Cenarios cenarios = new(api);

    [Fact]
    public async Task Sem_token_recebe_401_e_senha_errada_e_recusada()
    {
        var anonimo = api.Factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.GetAsync("/api/ordens-servico")).StatusCode);
        var login = await anonimo.PostJsonAsync("/api/auth/login", new { email = "atendente@farol.demo", senha = "errada-123456" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Atendente_de_outra_cidade_nao_acessa_os_e_a_tentativa_e_auditada()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var criada = await (await supervisor.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync(pularUc: 10))).JsonAsync();
        var osId = criada.GetProperty("osId").GetGuid();

        var vtq = await api.ClienteAsync("atendente.vtq@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await vtq.GetAsync($"/api/ordens-servico/{osId}")).StatusCode);
        Assert.True(await api.ComBancoAsync(db => db.Auditorias.AnyAsync(a => a.Acao == "ACESSO_NEGADO" && a.EntidadeId == osId.ToString())));

        // Também não registra ocorrência em cidade não vinculada.
        var reqLumiara = await cenarios.SolicitacaoAsync(pularUc: 11);
        Assert.Equal(HttpStatusCode.Forbidden, (await vtq.PostJsonAsync("/api/solicitacoes", reqLumiara)).StatusCode);
    }

    [Fact]
    public async Task Fila_de_cidade_nao_autorizada_e_visao_consolidada_restrita()
    {
        var lumiara = await cenarios.MunicipioAsync("LUM");
        var vtq = await api.ClienteAsync("atendente.vtq@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await vtq.GetAsync($"/api/ordens-servico?municipioId={lumiara}")).StatusCode);

        // Atendente com duas cidades e sem permissão de "todas": precisa escolher uma.
        var atendente = await api.ClienteAsync("atendente@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await atendente.GetAsync("/api/ordens-servico")).StatusCode);

        // Supervisor vê todas; a posição continua calculada dentro de cada cidade.
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var fila = await (await supervisor.GetAsync("/api/ordens-servico?tamanhoPagina=200")).JsonAsync();
        var itens = fila.GetProperty("itens").EnumerateArray().ToList();
        Assert.True(itens.Select(i => i.GetProperty("municipioId").GetInt32()).Distinct().Count() >= 3);
        foreach (var grupo in itens.GroupBy(i => i.GetProperty("municipioId").GetInt32()))
            Assert.Contains(grupo, i => i.GetProperty("posicao").GetInt32() == 1);

        var indicadores = await (await supervisor.GetAsync("/api/ordens-servico/indicadores")).JsonAsync();
        Assert.True(indicadores.GetProperty("porMunicipio").GetArrayLength() >= 3);
    }

    [Fact]
    public async Task Atendente_nao_edita_nem_publica_pontuacao_e_nao_despacha()
    {
        var atendente = await api.ClienteAsync("atendente@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await atendente.PostAsync("/api/configuracoes/pontuacao/rascunho", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await atendente.PostJsonAsync("/api/configuracoes/pontuacao/publicar",
            new { versaoId = 1, justificativa = "x", aplicacao = "SomenteNovas" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await atendente.PostJsonAsync($"/api/ordens-servico/{Guid.NewGuid()}/despachos",
            new { equipeId = 1, excecao = false, apoioIntermunicipal = false })).StatusCode);

        // Mas consulta a pontuação publicada em modo leitura.
        Assert.Equal(HttpStatusCode.OK, (await atendente.GetAsync("/api/configuracoes/pontuacao")).StatusCode);
    }

    [Fact]
    public async Task Troca_de_municipio_exige_justificativa_e_recalcula()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var criada = await (await supervisor.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync(pularUc: 12))).JsonAsync();
        var osId = criada.GetProperty("osId").GetGuid();
        var can = await cenarios.MunicipioAsync("CAN");

        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/municipio", new { municipioId = can, justificativa = "" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/municipio", new { municipioId = can, justificativa = "Endereço pertence a Cerro Anil." })).StatusCode);

        var os = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == osId));
        Assert.Equal(can, os.MunicipioId);
        Assert.True(await api.ComBancoAsync(db => db.ResultadosPrioridade.CountAsync(r => r.OrdemServicoId == osId)) == 2);
        Assert.True(await api.ComBancoAsync(db => db.Auditorias.AnyAsync(a => a.Acao == "TROCAR_MUNICIPIO" && a.EntidadeId == osId.ToString())));
    }
}
