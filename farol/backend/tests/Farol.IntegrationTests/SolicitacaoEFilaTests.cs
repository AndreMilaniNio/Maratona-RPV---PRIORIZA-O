using System.Net;
using System.Text.Json;
using Farol.Application.DTOs;
using Farol.Domain.Enums;
using Farol.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class SolicitacaoEFilaTests(FarolApiFixture api)
{
    private readonly Cenarios cenarios = new(api);

    [Fact]
    public async Task Criar_solicitacao_gera_os_persistida_classificada_e_visivel_na_fila()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var r = await cliente.PostJsonAsync("/api/solicitacoes", await cenarios.SolicitacaoAsync(pularUc: 1));
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var corpo = await r.JsonAsync();
        var osNumero = corpo.GetProperty("osNumero").GetString()!;
        var osId = corpo.GetProperty("osId").GetGuid();
        Assert.Matches(@"^LUM-\d{4}-\d{6}$", osNumero);
        Assert.StartsWith("SOL-LUM-", corpo.GetProperty("solicitacaoNumero").GetString());
        Assert.True(corpo.GetProperty("versaoDemonstrativa").GetBoolean());

        // Persistido no PostgreSQL com snapshot, itens, histórico e auditoria.
        var (resultados, itens, historico, auditoria) = await api.ComBancoAsync(async db => (
            await db.ResultadosPrioridade.CountAsync(x => x.OrdemServicoId == osId),
            await db.ResultadosPrioridade.Where(x => x.OrdemServicoId == osId).SelectMany(x => x.Itens).CountAsync(),
            await db.HistoricosOs.CountAsync(h => h.OrdemServicoId == osId),
            await db.Auditorias.CountAsync(a => a.EntidadeId == osId.ToString() && a.Acao == "CRIAR")));
        Assert.Equal(1, resultados);
        Assert.Equal(19, itens);
        Assert.True(historico >= 1);
        Assert.Equal(1, auditoria);

        // Aparece na fila da cidade, com posição, sem cadastro adicional.
        var municipio = await cenarios.MunicipioAsync("LUM");
        var fila = await (await cliente.GetAsync($"/api/ordens-servico?municipioId={municipio}&busca={osNumero}")).JsonAsync();
        var item = fila.GetProperty("itens")[0];
        Assert.Equal(osNumero, item.GetProperty("numero").GetString());
        Assert.True(item.GetProperty("posicao").GetInt32() >= 1);

        // A explicação bate: soma dos itens = pontuação total.
        var prioridade = await (await cliente.GetAsync($"/api/ordens-servico/{osId}/prioridade")).JsonAsync();
        var atual = prioridade.GetProperty("atual");
        Assert.Equal(atual.GetProperty("pontuacao").GetInt32(), atual.GetProperty("itens").EnumerateArray().Sum(i => i.GetProperty("pontos").GetInt32()));
    }

    [Fact]
    public async Task Mesmo_envio_repetido_com_a_mesma_chave_nao_cria_segunda_os()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var req = await cenarios.SolicitacaoAsync(pularUc: 2);
        var a = await (await cliente.PostJsonAsync("/api/solicitacoes", req)).JsonAsync();
        var b = await (await cliente.PostJsonAsync("/api/solicitacoes", req)).JsonAsync();

        Assert.Equal(a.GetProperty("osNumero").GetString(), b.GetProperty("osNumero").GetString());
        Assert.Equal(1, await api.ComBancoAsync(db => db.Solicitacoes.CountAsync(s => s.ChaveIdempotencia == req.ChaveIdempotencia)));
    }

    [Fact]
    public async Task Numeros_sao_unicos_em_criacoes_simultaneas()
    {
        var cliente = await api.ClienteAsync("supervisor@farol.demo");
        var reqs = new List<NovaSolicitacaoRequest>();
        for (var i = 0; i < 12; i++) reqs.Add(await cenarios.SolicitacaoAsync("CAN", "INTERRUPCAO_PARCIAL", i % 3, classe: "RURAL"));

        var respostas = await Task.WhenAll(reqs.Select(r => cliente.PostJsonAsync("/api/solicitacoes", r)));
        Assert.All(respostas, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var numeros = await Task.WhenAll(respostas.Select(async r => (await r.JsonAsync()).GetProperty("osNumero").GetString()));
        Assert.Equal(numeros.Length, numeros.Distinct().Count());
    }

    [Fact]
    public async Task Nao_sabe_a_uc_com_so_cep_registra_em_triagem_e_sem_localizacao_e_recusado()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var municipio = await cenarios.MunicipioAsync("LUM");
        var tipo = await cenarios.TipoAsync("FALHA_EQUIPAMENTO");
        var baseReq = new NovaSolicitacaoRequest
        {
            ChaveIdempotencia = Guid.NewGuid(), MunicipioId = municipio, TipoOcorrenciaId = tipo, Descricao = "Solicitante na rua.",
            UcNaoInformada = true, MotivoUcNaoInformada = "Fora da própria residência.",
        };

        var semLocal = await cliente.PostJsonAsync("/api/solicitacoes", baseReq);
        Assert.Equal(HttpStatusCode.BadRequest, semLocal.StatusCode);

        var comCep = await cliente.PostJsonAsync("/api/solicitacoes", baseReq with { Localizacao = new LocalizacaoInput { Cep = "00110398" } });
        Assert.Equal(HttpStatusCode.Created, comCep.StatusCode);
        var osId = (await comCep.JsonAsync()).GetProperty("osId").GetGuid();
        var os = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == osId));
        Assert.True(os.LocalizacaoPendente);
        Assert.Equal(StatusOrdemServico.EmTriagem, os.Status);
        Assert.Equal(C.Faixa.Desconhecida, os.PessoasAfetadas);
        var sol = await api.ComBancoAsync(db => db.Solicitacoes.Include(s => s.Ucs).FirstAsync(s => s.OrdemServicoId == osId));
        Assert.True(sol.UcNaoInformada);
        Assert.Empty(sol.Ucs);
    }

    [Fact]
    public async Task Transformador_com_zeros_a_esquerda_e_localidade_nao_cadastrada_e_aceito_como_texto()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var interpretacao = await (await cliente.GetAsync("/api/transformadores/interpretar?numero=0071234")).JsonAsync();
        Assert.True(interpretacao.GetProperty("valido").GetBoolean());
        Assert.Equal("007", interpretacao.GetProperty("codigoLocalidade").GetString());
        Assert.Equal("1234", interpretacao.GetProperty("numeroLocal").GetString());
        Assert.False(interpretacao.GetProperty("localidadeCadastrada").GetBoolean());

        var invalido = await (await cliente.GetAsync("/api/transformadores/interpretar?numero=12A")).JsonAsync();
        Assert.False(invalido.GetProperty("valido").GetBoolean());

        var req = await cenarios.SolicitacaoAsync(pularUc: 3);
        req = req with { Rede = new RedeInput { TransformadorNumero = "0071234" } };
        var r = await cliente.PostJsonAsync("/api/solicitacoes", req);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var corpo = await r.JsonAsync();
        Assert.Contains(corpo.GetProperty("alertas").EnumerateArray(), a => a.GetString()!.Contains("Localidade 007 não cadastrada"));
        var osId = corpo.GetProperty("osId").GetGuid();
        var os = await api.ComBancoAsync(db => db.OrdensServico.FirstAsync(o => o.Id == osId));
        Assert.Equal("0071234", os.TransformadorNumero);
        Assert.Equal("007", os.TransformadorLocalidade);
        Assert.Equal(OrigemRede.Informado, os.OrigemRede);
    }

    [Fact]
    public async Task Coordenadas_fora_do_brasil_geram_alerta_sem_bloquear_e_cep_unico_e_reconhecido()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var req = await cenarios.SolicitacaoAsync(pularUc: 4);
        req = req with { Localizacao = req.Localizacao with { Latitude = 48.8566, Longitude = 2.3522 } };
        var r = await cliente.PostJsonAsync("/api/solicitacoes", req);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        Assert.Contains((await r.JsonAsync()).GetProperty("alertas").EnumerateArray(), a => a.GetString()!.Contains("fora do território brasileiro"));

        var cep = await (await cliente.GetAsync("/api/geocodificacao/cep/00200-000")).JsonAsync();
        Assert.True(cep.GetProperty("cepUnico").GetBoolean());
        Assert.Equal(JsonValueKind.Null, cep.GetProperty("logradouro").ValueKind);

        var coords = await (await cliente.PostJsonAsync("/api/geocodificacao/coordenadas/interpretar",
            new { texto = "21°31'49.594\"S 42°38'20.207\"W", municipioId = await cenarios.MunicipioAsync("LUM") })).JsonAsync();
        Assert.True(coords.GetProperty("valido").GetBoolean());
        Assert.Equal(-21.530443, coords.GetProperty("latitude").GetDouble(), 5);
    }

    [Fact]
    public async Task Busca_de_uc_mascara_o_nome_e_audita_a_consulta()
    {
        var cliente = await api.ClienteAsync("atendente@farol.demo");
        var uc = await cenarios.UcAsync("LUM", 5);
        var r = await (await cliente.GetAsync($"/api/unidades-consumidoras?uc={uc.Numero}")).JsonAsync();
        Assert.DoesNotContain(uc.ClienteNome, r.GetProperty("clienteMascarado").GetString());
        Assert.Contains('*', r.GetProperty("clienteMascarado").GetString()!);
        Assert.True(await api.ComBancoAsync(db => db.Auditorias.AnyAsync(a => a.Acao == "CONSULTA_UC" && a.EntidadeId == uc.Numero)));

        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/unidades-consumidoras?uc=999999999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/unidades-consumidoras?uc=12")).StatusCode);
    }

    [Fact]
    public async Task Hospital_sem_reserva_e_cabo_caido_sobem_por_precedencia()
    {
        var cliente = await api.ClienteAsync("supervisor@farol.demo");
        var cabo = await cenarios.SolicitacaoAsync(pularUc: 6, tipo: "CABO_ROMPIDO", impacto: new ImpactoInput
        {
            PessoasAfetadas = C.Faixa.Ate10, CondicoesSeguranca = [C.Seguranca.CaboEnergizado], CondicaoFornecimento = C.Fornecimento.Parcial,
        });
        var r = await (await cliente.PostJsonAsync("/api/solicitacoes", cabo)).JsonAsync();
        Assert.Equal("URG", r.GetProperty("prioridade").GetProperty("codigo").GetString());

        var hospital = await cenarios.SolicitacaoAsync(pularUc: 7, tipo: "INTERRUPCAO_PARCIAL", impacto: new ImpactoInput
        {
            PessoasAfetadas = C.Faixa.Ate10, ServicoEssencial = C.Essencial.Hospital, FonteReserva = C.Confirmacao.Nao,
            CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional], CondicaoFornecimento = C.Fornecimento.Parcial,
        });
        var h = await (await cliente.PostJsonAsync("/api/solicitacoes", hospital)).JsonAsync();
        var rank = h.GetProperty("prioridade").GetProperty("rank").GetInt32();
        Assert.True(rank <= 2, "Serviço essencial sem reserva deve ser no mínimo Emergente.");
    }

    [Fact]
    public async Task Previa_e_simulador_devolvem_o_mesmo_resultado_que_a_producao()
    {
        var cliente = await api.ClienteAsync("supervisor@farol.demo");
        var req = await cenarios.SolicitacaoAsync(pularUc: 8, tipo: "GALHO_NA_REDE");
        var previa = await (await cliente.PostJsonAsync("/api/solicitacoes/previa", req)).JsonAsync();
        var simulado = await (await cliente.PostJsonAsync("/api/configuracoes/pontuacao/simular", new { municipioId = req.MunicipioId, usarRascunho = false, solicitacao = req })).JsonAsync();
        var criado = await (await cliente.PostJsonAsync("/api/solicitacoes", req)).JsonAsync();

        Assert.Equal(previa.GetProperty("pontuacao").GetInt32(), simulado.GetProperty("pontuacao").GetInt32());
        Assert.Equal(previa.GetProperty("pontuacao").GetInt32(), criado.GetProperty("pontuacao").GetInt32());
        Assert.Equal(previa.GetProperty("prioridade").GetProperty("codigo").GetString(), criado.GetProperty("prioridade").GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Filtros_combinados_funcionam_no_servidor()
    {
        var cliente = await api.ClienteAsync("supervisor@farol.demo");
        var municipio = await cenarios.MunicipioAsync("LUM");
        var preventivas = await (await cliente.GetAsync($"/api/ordens-servico?municipioId={municipio}&tipoManutencao=Preventiva")).JsonAsync();
        Assert.All(preventivas.GetProperty("itens").EnumerateArray(), i => Assert.Equal("Preventiva", i.GetProperty("tipoManutencao").GetString()));
        Assert.True(preventivas.GetProperty("total").GetInt32() >= 2);

        var comRisco = await (await cliente.GetAsync($"/api/ordens-servico?municipioId={municipio}&risco=QUALQUER&status=AguardandoDespacho&status=EmTriagem")).JsonAsync();
        Assert.All(comRisco.GetProperty("itens").EnumerateArray(), i => Assert.True(i.GetProperty("risco").GetBoolean()));

        var vencidas = await (await cliente.GetAsync($"/api/ordens-servico?municipioId={municipio}&prazo=vencido")).JsonAsync();
        Assert.All(vencidas.GetProperty("itens").EnumerateArray(), i => Assert.True(i.GetProperty("proximoPrazo").GetProperty("vencido").GetBoolean()));
        Assert.True(vencidas.GetProperty("total").GetInt32() >= 1, "O cenário demonstrativo inclui uma OS vencida.");
    }
}
