using System.Net;
using System.Text.Json;
using Farol.Domain.Enums;
using Farol.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Farol.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class DespachoTests(FarolApiFixture api)
{
    private readonly Cenarios cenarios = new(api);

    private Task<int> EquipeAsync(string codigo) => api.ComBancoAsync(db => db.Equipes.Where(e => e.Codigo == codigo).Select(e => e.Id).FirstAsync());

    private async Task<Guid> NovaOsAsync(HttpClient c, string prefixo, int pular, string classe = "RESIDENCIAL")
    {
        var r = await c.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync(prefixo, pularUc: pular, classe: classe));
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await r.JsonAsync()).GetProperty("osId").GetGuid();
    }

    [Fact]
    public async Task Ciclo_completo_recomenda_designa_entrega_coordenadas_e_libera_a_equipe()
    {
        var c = await api.ClienteAsync("despachante@farol.demo");
        var osId = await NovaOsAsync(c, "LUM", 20);

        var candidatas = await (await c.GetAsync($"/api/ordens-servico/{osId}/equipes-candidatas")).JsonAsync();
        var lista = candidatas.GetProperty("equipes").EnumerateArray().ToList();
        var recomendada = lista.Single(e => e.GetProperty("recomendada").GetBoolean());
        Assert.True(recomendada.GetProperty("compativel").GetBoolean());
        Assert.True(recomendada.GetProperty("disponivel").GetBoolean());
        Assert.False(recomendada.GetProperty("apoioIntermunicipal").GetBoolean());
        Assert.Equal("LINHA_RETA", recomendada.GetProperty("origemEstimativa").GetString());
        Assert.Equal(JsonValueKind.Null, recomendada.GetProperty("tempoEstimadoMin").ValueKind);
        // Equipe incompatível nunca é recomendada, mesmo mais próxima.
        Assert.All(lista.Where(e => !e.GetProperty("compativel").GetBoolean()), e => Assert.False(e.GetProperty("recomendada").GetBoolean()));

        var equipeId = recomendada.GetProperty("equipe").GetProperty("id").GetInt32();
        var designacao = await c.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId, excecao = false, apoioIntermunicipal = false });
        Assert.Equal(HttpStatusCode.Created, designacao.StatusCode);
        var corpo = await designacao.JsonAsync();
        var entrega = corpo.GetProperty("entrega");
        Assert.NotEqual(JsonValueKind.Null, entrega.GetProperty("latitude").ValueKind);
        Assert.Contains("openstreetmap", entrega.GetProperty("linkMapa").GetString());
        Assert.NotEmpty(entrega.GetProperty("ucs").EnumerateArray());
        var despachoId = corpo.GetProperty("despachoId").GetGuid();

        // Segunda designação para a mesma OS é recusada.
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId, excecao = false, apoioIntermunicipal = false })).StatusCode);
        // Transição fora de ordem é recusada.
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostJsonAsync($"/api/despachos/{despachoId}/conclusao", new { })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await c.PostJsonAsync($"/api/despachos/{despachoId}/aceite", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.PostJsonAsync($"/api/despachos/{despachoId}/inicio", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.PostJsonAsync($"/api/despachos/{despachoId}/conclusao", new { observacao = "Fusível substituído." })).StatusCode);

        var (os, equipe, auditado) = await api.ComBancoAsync(async db => (
            await db.OrdensServico.FirstAsync(o => o.Id == osId),
            await db.Equipes.FirstAsync(e => e.Id == equipeId),
            await db.Auditorias.AnyAsync(a => a.Acao == "DESPACHAR" && a.EntidadeId == osId.ToString())));
        Assert.Equal(StatusOrdemServico.Concluida, os.Status);
        Assert.Equal(StatusEquipe.Disponivel, equipe.Status);
        Assert.True(auditado);
    }

    [Fact]
    public async Task Designacoes_simultaneas_da_mesma_equipe_so_uma_vence()
    {
        var c = await api.ClienteAsync("supervisor@farol.demo");
        var os1 = await NovaOsAsync(c, "VTQ", 1, "RURAL");
        var os2 = await NovaOsAsync(c, "VTQ", 2, "RURAL");
        var equipeId = await EquipeAsync("VTQ-02");

        var respostas = await Task.WhenAll(
            c.PostJsonAsync($"/api/ordens-servico/{os1}/despachos", new { equipeId, excecao = false, apoioIntermunicipal = false }),
            c.PostJsonAsync($"/api/ordens-servico/{os2}/despachos", new { equipeId, excecao = false, apoioIntermunicipal = false }));

        Assert.Equal(1, respostas.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, respostas.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(1, await api.ComBancoAsync(db => db.Despachos.CountAsync(d => d.EquipeId == equipeId && d.Ativo)));

        // Remove a equipe (preserva histórico) para liberar a equipe aos demais testes.
        var vencedor = await api.ComBancoAsync(db => db.Despachos.FirstAsync(d => d.EquipeId == equipeId && d.Ativo));
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostJsonAsync($"/api/despachos/{vencedor.Id}/encerrar", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.PostJsonAsync($"/api/despachos/{vencedor.Id}/encerrar", new { motivo = "Troca de equipe (teste)." })).StatusCode);
        var os = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == vencedor.OrdemServicoId));
        Assert.Equal(StatusOrdemServico.AguardandoDespacho, os.Status);
        Assert.True(await api.ComBancoAsync(db => db.Despachos.AnyAsync(d => d.Id == vencedor.Id && !d.Ativo && d.MotivoEncerramento != null)));
    }

    [Fact]
    public async Task Incompativel_ou_indisponivel_so_por_excecao_autorizada()
    {
        var despachante = await api.ClienteAsync("despachante@farol.demo");
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var osId = await NovaOsAsync(supervisor, "CAN", 3, "RURAL");
        var emPausa = await EquipeAsync("CAN-02");

        Assert.Equal(HttpStatusCode.Conflict, (await despachante.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = emPausa, excecao = false, apoioIntermunicipal = false })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await despachante.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = emPausa, excecao = true, justificativa = "x", apoioIntermunicipal = false })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = emPausa, excecao = true, apoioIntermunicipal = false })).StatusCode);

        var ok = await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = emPausa, excecao = true, justificativa = "Única equipe com acesso à área (teste).", apoioIntermunicipal = false });
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        var despacho = await api.ComBancoAsync(db => db.Despachos.FirstAsync(d => d.OrdemServicoId == osId && d.Ativo));
        Assert.True(despacho.Excecao);
        await supervisor.PostJsonAsync($"/api/despachos/{despacho.Id}/encerrar", new { motivo = "Fim do teste." });
    }

    [Fact]
    public async Task Equipe_de_outra_cidade_so_como_apoio_intermunicipal_justificado()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var osId = await NovaOsAsync(supervisor, "CAN", 4, "RURAL");
        var vtq01 = await EquipeAsync("VTQ-01");

        var padrao = await (await supervisor.GetAsync($"/api/ordens-servico/{osId}/equipes-candidatas")).JsonAsync();
        Assert.DoesNotContain(padrao.GetProperty("equipes").EnumerateArray(), e => e.GetProperty("equipe").GetProperty("id").GetInt32() == vtq01);
        var comApoio = await (await supervisor.GetAsync($"/api/ordens-servico/{osId}/equipes-candidatas?incluirApoio=true")).JsonAsync();
        Assert.Contains(comApoio.GetProperty("equipes").EnumerateArray(), e => e.GetProperty("equipe").GetProperty("id").GetInt32() == vtq01 && e.GetProperty("apoioIntermunicipal").GetBoolean());

        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = vtq01, excecao = false, apoioIntermunicipal = false })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = vtq01, excecao = false, apoioIntermunicipal = true })).StatusCode);
        var ok = await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/despachos", new { equipeId = vtq01, excecao = false, apoioIntermunicipal = true, justificativa = "Equipe local em pausa (teste)." });
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        var despacho = await api.ComBancoAsync(db => db.Despachos.FirstAsync(d => d.OrdemServicoId == osId && d.Ativo));
        Assert.True(despacho.ApoioIntermunicipal);
        await supervisor.PostJsonAsync($"/api/despachos/{despacho.Id}/encerrar", new { motivo = "Fim do teste." });
    }

    [Fact]
    public async Task Transicoes_de_status_sao_validadas_no_servidor()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var osId = await NovaOsAsync(supervisor, "LUM", 25);

        Assert.Equal(HttpStatusCode.Conflict, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/status", new { status = "Concluida" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/status", new { status = "Cancelada" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/status", new { status = "Cancelada", justificativa = "Duplicada por engano (teste)." })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await supervisor.PatchJsonAsync($"/api/ordens-servico/{osId}/status", new { status = "AguardandoDespacho", justificativa = "x" })).StatusCode);

        var atendente = await api.ClienteAsync("atendente@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await atendente.PatchJsonAsync($"/api/ordens-servico/{osId}/status", new { status = "Suspensa", justificativa = "x" })).StatusCode);
    }

    [Fact]
    public async Task Revisao_manual_exige_justificativa_e_preserva_o_calculo()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var osId = await NovaOsAsync(supervisor, "LUM", 26);
        var urgente = await api.ComBancoAsync(db => db.Prioridades.Where(p => p.Codigo == "URG").Select(p => p.Id).FirstAsync());

        Assert.Equal(HttpStatusCode.BadRequest, (await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/reclassificar", new { prioridadeManualId = urgente, justificativa = "" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await supervisor.PostJsonAsync($"/api/ordens-servico/{osId}/reclassificar", new { prioridadeManualId = urgente, justificativa = "Cliente eletrodependente (teste)." })).StatusCode);

        var os = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == osId));
        Assert.Equal(urgente, os.PrioridadeId);
        Assert.NotEqual(urgente, os.PrioridadeCalculadaId);
        Assert.Equal(2, await api.ComBancoAsync(db => db.ResultadosPrioridade.CountAsync(r => r.OrdemServicoId == osId)));

        var despachante = await api.ClienteAsync("despachante@farol.demo");
        Assert.Equal(HttpStatusCode.Forbidden, (await despachante.PostJsonAsync($"/api/ordens-servico/{osId}/reclassificar", new { prioridadeManualId = (int?)null, justificativa = "x" })).StatusCode);
    }

    [Fact]
    public async Task Unificacao_cancela_a_duplicada_e_transfere_as_solicitacoes()
    {
        var supervisor = await api.ClienteAsync("supervisor@farol.demo");
        var principal = await NovaOsAsync(supervisor, "LUM", 27);
        var duplicada = await NovaOsAsync(supervisor, "LUM", 27);

        var detalhe = await (await supervisor.GetAsync($"/api/ordens-servico/{duplicada}")).JsonAsync();
        Assert.Contains(detalhe.GetProperty("possiveisDuplicidades").EnumerateArray(), d => d.GetProperty("osId").GetGuid() == principal);

        Assert.Equal(HttpStatusCode.NoContent, (await supervisor.PostJsonAsync($"/api/ordens-servico/{principal}/unificar", new { osDuplicadaId = duplicada, justificativa = "Mesmo evento (teste)." })).StatusCode);
        var (dup, solicitacoes, vinculos) = await api.ComBancoAsync(async db => (
            await db.OrdensServico.FirstAsync(o => o.Id == duplicada),
            await db.Solicitacoes.CountAsync(s => s.OrdemServicoId == principal),
            await db.VinculosSolicitacao.CountAsync(v => v.OrdemServicoOrigemId == duplicada)));
        Assert.Equal(StatusOrdemServico.Cancelada, dup.Status);
        Assert.Contains("Unificada", dup.MotivoCancelamento);
        Assert.Equal(2, solicitacoes);
        Assert.Equal(1, vinculos);
    }
}
