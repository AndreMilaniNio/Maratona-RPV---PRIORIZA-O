using Farol.Application.Services;
using Farol.Domain.Enums;
using Farol.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Farol.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class ReclassificacaoTests(FarolApiFixture api)
{
    private readonly Cenarios cenarios = new(api);

    [Fact]
    public async Task Passagem_do_tempo_reclassifica_e_registra_historico_somente_quando_muda()
    {
        var cliente = await api.ClienteAsync("supervisor@farol.demo");
        var criada = await (await cliente.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync(pularUc: 15, classe: "COMERCIAL"))).JsonAsync();
        var osId = criada.GetProperty("osId").GetGuid();
        var pontosIniciais = criada.GetProperty("pontuacao").GetInt32();

        // Nada mudou: a rotina não grava novo resultado para esta OS.
        await ExecutarAsync();
        Assert.Equal(1, await api.ComBancoAsync(db => db.ResultadosPrioridade.CountAsync(r => r.OrdemServicoId == osId)));

        // 30 horas depois, o critério "tempo de espera" muda de faixa.
        await api.ComBancoAsync(db => db.OrdensServico.Where(o => o.Id == osId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.AbertaEm, o => o.AbertaEm.AddHours(-30))));
        await ExecutarAsync();

        var resultados = await api.ComBancoAsync(db => db.ResultadosPrioridade.Where(r => r.OrdemServicoId == osId).OrderBy(r => r.Id).ToListAsync());
        Assert.Equal(2, resultados.Count);
        Assert.Equal(MotivoClassificacao.Tempo, resultados[1].Motivo);
        Assert.True(resultados[1].Pontuacao > pontosIniciais);
        Assert.Equal(pontosIniciais, resultados[0].Pontuacao);
    }

    private Task<int> ExecutarAsync() => api.ComBancoAsync(async _ =>
    {
        using var escopo = api.Factory.Services.CreateScope();
        var (_, total) = await escopo.ServiceProvider.GetRequiredService<ReclassificacaoPeriodicaService>().ExecutarAsync();
        return total;
    });
}
