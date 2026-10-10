using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.IntegrationTests.Infra;

/// <summary>Monta solicitações a partir do cadastro demonstrativo carregado no banco de teste.</summary>
public class Cenarios(FarolApiFixture api)
{
    public Task<int> MunicipioAsync(string prefixo) =>
        api.ComBancoAsync(db => db.Municipios.Where(m => m.Prefixo == prefixo).Select(m => m.Id).FirstAsync());

    public Task<int> TipoAsync(string codigo) =>
        api.ComBancoAsync(db => db.TiposOcorrencia.Where(t => t.Codigo == codigo).Select(t => t.Id).FirstAsync());

    public Task<UnidadeConsumidora> UcAsync(string prefixo, int pular = 0, string classe = "RESIDENCIAL") =>
        api.ComBancoAsync(db => db.UnidadesConsumidoras.AsNoTracking().Include(u => u.Transformador)
            .Where(u => u.Municipio!.Prefixo == prefixo && u.ClasseCliente!.Codigo == classe)
            .OrderByDescending(u => u.Numero).Skip(pular).FirstAsync());

    public async Task<NovaSolicitacaoRequest> SolicitacaoAsync(
        string prefixo = "LUM", string tipo = "INTERRUPCAO_TOTAL", int pularUc = 0, ImpactoInput? impacto = null,
        TipoManutencao manutencao = TipoManutencao.Corretiva, string classe = "RESIDENCIAL")
    {
        var uc = await UcAsync(prefixo, pularUc, classe);
        return new NovaSolicitacaoRequest
        {
            ChaveIdempotencia = Guid.NewGuid(),
            MunicipioId = uc.MunicipioId,
            Canal = CanalEntrada.Telefone,
            Ucs = [uc.Numero],
            ClassesIds = [uc.ClasseClienteId],
            TipoManutencao = manutencao,
            TipoOcorrenciaId = await TipoAsync(tipo),
            Descricao = "Teste de integração — dado fictício.",
            Localizacao = new LocalizacaoInput
            {
                Logradouro = uc.Logradouro, Numero = uc.NumeroImovel, Bairro = uc.Bairro, Cep = uc.Cep,
                Latitude = uc.Latitude, Longitude = uc.Longitude, OrigemCoordenada = OrigemCoordenada.Informada,
            },
            Rede = new RedeInput { SubestacaoId = uc.Transformador!.SubestacaoId, ConjuntoId = uc.Transformador.ConjuntoId, TransformadorNumero = uc.Transformador.NumeroCompleto },
            Impacto = impacto ?? new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.Ate10, UcsAfetadas = C.Faixa.Ate10, CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional],
                CondicaoFornecimento = C.Fornecimento.Total, SituacaoCliente = C.Situacao.Ligado,
            },
        };
    }
}
