using System.Text.RegularExpressions;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Services;

public sealed record OsMontada(OrdemServico Os, List<string> Alertas, List<SolicitacaoUc> Ucs);

/// <summary>
/// Constrói uma OS (ainda não gravada) a partir do formulário, validando tudo que depende do cadastro.
/// Usado pela criação, pela prévia e pelo simulador — os três veem exatamente os mesmos fatos.
/// </summary>
public class MontadorOrdemServico(IAppDbContext db, IOptions<FarolOptions> opcoes)
{
    private readonly FarolOptions _opcoes = opcoes.Value;

    public async Task<OsMontada> MontarAsync(NovaSolicitacaoRequest r, DateTimeOffset agora, CancellationToken ct = default)
    {
        var erros = new Dictionary<string, string[]>();
        var alertas = new List<string>();
        void Erro(string campo, string msg) => erros[campo] = [.. erros.GetValueOrDefault(campo, []), msg];

        var municipio = await db.Municipios.AsNoTracking().FirstOrDefaultAsync(m => m.Id == r.MunicipioId, ct);
        if (municipio is null || !municipio.Ativo) Erro("municipioId", "Município inexistente ou inativo.");

        var tipo = await db.TiposOcorrencia.AsNoTracking().FirstOrDefaultAsync(t => t.Id == r.TipoOcorrenciaId, ct);
        if (tipo is null || !tipo.Ativo) Erro("tipoOcorrenciaId", "Tipo de ocorrência inexistente ou inativo.");

        await ValidarOpcoesAsync(r.Impacto, Erro, ct);

        var os = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Numero = string.Empty,
            MunicipioId = r.MunicipioId,
            TipoManutencao = r.TipoManutencao,
            TipoOcorrenciaId = r.TipoOcorrenciaId,
            AbertaEm = agora,
            AtualizadaEm = agora,
            PessoasAfetadas = r.Impacto.PessoasAfetadas,
            QuantidadePessoas = r.Impacto.QuantidadePessoas,
            UcsAfetadas = r.Impacto.UcsAfetadas,
            QuantidadeUcs = r.Impacto.QuantidadeUcs,
            ServicoEssencial = r.Impacto.ServicoEssencial,
            SituacaoCliente = r.Impacto.SituacaoCliente,
            CondicaoFornecimento = r.Impacto.CondicaoFornecimento,
            Redundancia = r.Impacto.Redundancia,
            FonteReserva = r.Impacto.FonteReserva,
            EquipeEspecializada = r.Impacto.EquipeEspecializada,
            EquipamentoAfetado = r.Impacto.EquipamentoAfetado,
            Abrangencia = r.Impacto.Abrangencia,
            NivelRede = r.Impacto.NivelRede,
            QuantidadeEquipamentos = r.Impacto.QuantidadeEquipamentos,
            DuracaoEstimadaMin = r.Impacto.DuracaoEstimadaMin,
            DataLimite = r.Impacto.DataLimite,
            RecursosEspeciais = r.Impacto.RecursosIds.Count > 0,
        };
        os.CondicoesSeguranca = r.Impacto.CondicoesSeguranca.Distinct().Select(c => new OsCondicaoSeguranca { OrdemServicoId = os.Id, Codigo = c }).ToList();

        // Localização
        var l = r.Localizacao;
        os.Logradouro = Limpo(l.Logradouro);
        os.NumeroEndereco = Limpo(l.Numero);
        os.Bairro = Limpo(l.Bairro);
        os.Cep = string.IsNullOrWhiteSpace(l.Cep) ? null : new string(l.Cep.Where(char.IsAsciiDigit).ToArray());
        os.PontoReferencia = Limpo(l.PontoReferencia);
        os.ObservacoesLocalizacao = Limpo(l.Observacoes);
        os.EnderecoCompleto = Limpo(l.EnderecoCompleto) ?? MontarEndereco(os, municipio);
        if (l.Latitude is { } lat && l.Longitude is { } lng)
        {
            var c = new Coordenadas(lat, lng);
            os.Latitude = lat;
            os.Longitude = lng;
            os.OrigemCoordenada = l.OrigemCoordenada ?? OrigemCoordenada.Informada;
            os.PrecisaoMetros = l.PrecisaoMetros;
            os.CoordenadaRegistradaEm = agora;
            alertas.AddRange(AlertasCoordenada(c, municipio));
        }
        os.LocalizacaoPendente = os.Latitude is null && os.Logradouro is null;

        if (os.Cep is not null && municipio is not null)
        {
            var cepMunicipio = await db.Ceps.AsNoTracking().Where(c => c.Cep == os.Cep).Select(c => (int?)c.MunicipioId).FirstOrDefaultAsync(ct);
            if (cepMunicipio is { } cm && cm != municipio.Id)
                alertas.Add("O município do CEP é diferente do município selecionado.");
        }

        // Rede elétrica
        await MontarRedeAsync(r.Rede, os, municipio, Erro, alertas, ct);

        // UCs
        var ucs = new List<SolicitacaoUc>();
        var formatoUc = new Regex(_opcoes.Uc.Formato);
        foreach (var numero in r.Ucs.Select(u => u.Trim()).Where(u => u.Length > 0).Distinct())
        {
            if (!formatoUc.IsMatch(numero))
            {
                Erro("ucs", $"Formato de UC inválido: {numero}.");
                continue;
            }
            var uc = await db.UnidadesConsumidoras.AsNoTracking().FirstOrDefaultAsync(u => u.Numero == numero, ct);
            ucs.Add(new SolicitacaoUc { Numero = numero, UnidadeConsumidoraId = uc?.Id, ValidadaNoCadastro = uc is not null });
            if (uc is null)
            {
                alertas.Add($"UC {numero} não validada no cadastro.");
                continue;
            }
            if (uc.MunicipioId != r.MunicipioId)
                alertas.Add($"A UC {numero} pertence a outro município no cadastro.");
            if (os.Logradouro is not null && uc.Logradouro is not null &&
                !string.Equals(os.Logradouro, uc.Logradouro, StringComparison.OrdinalIgnoreCase))
                alertas.Add($"O endereço da UC {numero} ({uc.Logradouro}) difere do local informado; prevalece o local da ocorrência.");
        }

        // Classes
        var classesIds = r.ClassesIds.Distinct().ToList();
        var classesValidas = await db.ClassesCliente.Where(c => classesIds.Contains(c.Id) && c.Ativo).Select(c => c.Id).ToListAsync(ct);
        if (classesValidas.Count != classesIds.Count) Erro("classesIds", "Classe de cliente inexistente ou inativa.");
        os.Classes = classesValidas.Select(id => new OsClasseCliente { OrdemServicoId = os.Id, ClasseClienteId = id }).ToList();

        // Recursos necessários
        var recursosIds = r.Impacto.RecursosIds.Distinct().ToList();
        var recursosValidos = await db.Recursos.Where(x => recursosIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (recursosValidos.Count != recursosIds.Count) Erro("impacto.recursosIds", "Recurso inexistente.");
        os.RecursosNecessarios = recursosValidos.Select(id => new OsRecurso { OrdemServicoId = os.Id, RecursoId = id }).ToList();

        // Critérios personalizados
        foreach (var resposta in r.RespostasPersonalizadas)
        {
            var valida = await db.OpcoesCriterio.AnyAsync(o => o.Id == resposta.OpcaoId && o.CriterioId == resposta.CriterioId
                && o.Criterio!.Tipo == TipoCriterio.Personalizado && o.Criterio.Ativo, ct);
            if (!valida) Erro("respostasPersonalizadas", "Resposta inválida para critério personalizado.");
        }
        os.RespostasPersonalizadas = r.RespostasPersonalizadas
            .Select(x => new OsRespostaCriterio { OrdemServicoId = os.Id, CriterioId = x.CriterioId, OpcaoId = x.OpcaoId }).ToList();

        if (erros.Count > 0) throw new RegraNegocioException("Há campos inválidos na solicitação.", erros);
        return new OsMontada(os, alertas, ucs);
    }

    private async Task MontarRedeAsync(
        RedeInput rede, OrdemServico os, Municipio? municipio, Action<string, string> erro, List<string> alertas, CancellationToken ct)
    {
        os.Trecho = Limpo(rede.Trecho);
        os.EquipamentoDescricao = Limpo(rede.EquipamentoDescricao);
        os.IdentificadorEquipamento = Limpo(rede.IdentificadorEquipamento);
        os.ChaveEletrica = Limpo(rede.ChaveEletrica);

        if (rede.SubestacaoId is { } sid)
        {
            var sub = await db.Subestacoes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sid, ct);
            if (sub is null) erro("rede.subestacaoId", "Circuito (subestação) inexistente.");
            else if (municipio is not null && sub.MunicipioId != municipio.Id)
                alertas.Add("O circuito selecionado pertence a outro município.");
            os.SubestacaoId = sid;
        }
        if (rede.ConjuntoId is { } cid)
        {
            var pertence = await db.Conjuntos.AnyAsync(c => c.Id == cid && c.SubestacaoId == rede.SubestacaoId, ct);
            if (!pertence) erro("rede.conjuntoId", "O conjunto elétrico não pertence ao circuito selecionado.");
            os.ConjuntoId = cid;
        }

        Transformador? cadastrado = null;
        if (!string.IsNullOrWhiteSpace(rede.TransformadorNumero))
        {
            if (!NumeroTransformador.TentarCriar(rede.TransformadorNumero, _opcoes.Transformador.TamanhoMaximo, out var numero, out var msg))
            {
                erro("rede.transformadorNumero", msg!);
                return;
            }
            os.TransformadorNumero = numero!.Completo;
            os.TransformadorLocalidade = numero.CodigoLocalidade;
            os.TransformadorLocal = numero.NumeroLocal;

            cadastrado = await db.Transformadores.AsNoTracking()
                .FirstOrDefaultAsync(t => t.CodigoLocalidade == numero.CodigoLocalidade && t.NumeroLocal == numero.NumeroLocal, ct);
            os.TransformadorId = cadastrado?.Id;

            var localidadeCadastrada = await db.Localidades.AnyAsync(x => x.Codigo == numero.CodigoLocalidade, ct);
            if (!localidadeCadastrada) alertas.Add($"Localidade {numero.CodigoLocalidade} não cadastrada, confirmar.");
            if (cadastrado is not null && municipio is not null && cadastrado.MunicipioId != municipio.Id)
                alertas.Add("O transformador pertence a outro município no cadastro.");
        }

        // A origem é derivada no servidor: coincide com o cadastro → "identificado pelo cadastro, a confirmar".
        var algoInformado = os.SubestacaoId is not null || os.ConjuntoId is not null || os.TransformadorNumero is not null;
        if (!algoInformado) os.OrigemRede = OrigemRede.NaoInformado;
        else if (cadastrado is not null &&
                 (os.SubestacaoId is null || os.SubestacaoId == cadastrado.SubestacaoId) &&
                 (os.ConjuntoId is null || os.ConjuntoId == cadastrado.ConjuntoId))
        {
            os.SubestacaoId ??= cadastrado.SubestacaoId;
            os.ConjuntoId ??= cadastrado.ConjuntoId;
            os.OrigemRede = OrigemRede.CadastroAConfirmar;
        }
        else os.OrigemRede = OrigemRede.Informado;
    }

    /// <summary>Confere se cada código de opção existe no critério correspondente.</summary>
    private async Task ValidarOpcoesAsync(ImpactoInput i, Action<string, string> erro, CancellationToken ct)
    {
        var codigos = new (string Campo, string Criterio, IEnumerable<string> Valores)[]
        {
            ("impacto.pessoasAfetadas", Criterios.PessoasAfetadas, [i.PessoasAfetadas]),
            ("impacto.ucsAfetadas", Criterios.UcsAfetadas, [i.UcsAfetadas]),
            ("impacto.servicoEssencial", Criterios.ServicoEssencial, [i.ServicoEssencial]),
            ("impacto.situacaoCliente", Criterios.SituacaoCliente, [i.SituacaoCliente]),
            ("impacto.condicoesSeguranca", Criterios.RiscoSeguranca, i.CondicoesSeguranca),
            ("impacto.condicaoFornecimento", Criterios.CondicaoFornecimento, [i.CondicaoFornecimento]),
            ("impacto.redundancia", Criterios.Redundancia, [i.Redundancia]),
            ("impacto.fonteReserva", Criterios.FonteReserva, [i.FonteReserva]),
            ("impacto.equipeEspecializada", Criterios.EquipeEspecializada, [i.EquipeEspecializada]),
            ("impacto.equipamentoAfetado", Criterios.EquipamentoAfetado, [i.EquipamentoAfetado]),
            ("impacto.abrangencia", Criterios.Abrangencia, [i.Abrangencia]),
            ("impacto.nivelRede", Criterios.NivelRede, [i.NivelRede]),
        };
        var criterios = codigos.Select(c => c.Criterio).ToList();
        var validas = (await db.OpcoesCriterio.AsNoTracking()
                .Where(o => criterios.Contains(o.Criterio!.Codigo))
                .Select(o => new { Criterio = o.Criterio!.Codigo, o.Codigo })
                .ToListAsync(ct))
            .Select(x => (x.Criterio, x.Codigo))
            .ToHashSet();

        foreach (var (campo, criterio, valores) in codigos)
            foreach (var valor in valores)
                if (!validas.Contains((criterio, valor)))
                    erro(campo, $"Opção \"{valor}\" inválida.");
    }

    public static List<string> AlertasCoordenada(Coordenadas c, Municipio? municipio)
    {
        var alertas = new List<string>();
        if (!c.DentroDoBrasil) alertas.Add("As coordenadas estão fora do território brasileiro.");
        if (municipio is { Latitude: { } mlat, Longitude: { } mlng, RaioKm: { } raio })
        {
            var distancia = c.DistanciaKm(new Coordenadas(mlat, mlng));
            if (distancia > raio)
                alertas.Add($"As coordenadas estão a {distancia:0.0} km do centro de {municipio.Nome}, fora do raio cadastrado ({raio:0.#} km).");
        }
        return alertas;
    }

    private static string? MontarEndereco(OrdemServico os, Municipio? m)
    {
        var partes = new List<string>();
        if (os.Logradouro is not null) partes.Add(os.NumeroEndereco is null ? os.Logradouro : $"{os.Logradouro}, {os.NumeroEndereco}");
        if (os.Bairro is not null) partes.Add(os.Bairro);
        if (m is not null) partes.Add($"{m.Nome}/{m.Uf}");
        if (os.Cep is { Length: 8 } cep) partes.Add($"CEP {cep[..5]}-{cep[5..]}");
        return os.Logradouro is null && os.Bairro is null ? null : string.Join(" — ", partes);
    }

    private static string? Limpo(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
