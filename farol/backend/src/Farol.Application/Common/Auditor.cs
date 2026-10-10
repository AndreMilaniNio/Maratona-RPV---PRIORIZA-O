using System.Text.Json;
using Farol.Application.Abstractions;
using Farol.Domain.Entities;

namespace Farol.Application.Common;

/// <summary>Grava a trilha de auditoria (seção 18) na mesma unidade de trabalho da operação auditada.</summary>
public class Auditor(IAppDbContext db, IUsuarioAtual usuario, IRelogio relogio)
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void Registrar(
        string acao, string entidade, object entidadeId, int? municipioId = null,
        object? anteriores = null, object? novos = null, string? justificativa = null)
    {
        db.Auditorias.Add(new Auditoria
        {
            OcorridoEm = relogio.Agora,
            UsuarioId = usuario.Autenticado ? usuario.Id : null,
            UsuarioNome = usuario.Autenticado ? usuario.Nome : null,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId.ToString() ?? string.Empty,
            MunicipioId = municipioId,
            ValoresAnteriores = anteriores is null ? null : JsonSerializer.Serialize(anteriores, Json),
            ValoresNovos = novos is null ? null : JsonSerializer.Serialize(novos, Json),
            Justificativa = justificativa,
            EnderecoIp = usuario.EnderecoIp,
        });
    }

    /// <summary>Registra e grava imediatamente — usado quando a operação principal foi recusada.</summary>
    public async Task RegistrarAgoraAsync(
        string acao, string entidade, object entidadeId, int? municipioId = null, string? justificativa = null,
        CancellationToken ct = default)
    {
        Registrar(acao, entidade, entidadeId, municipioId, justificativa: justificativa);
        await db.SaveChangesAsync(ct);
    }
}

public static class AcoesAuditoria
{
    public const string Criar = "CRIAR";
    public const string Alterar = "ALTERAR";
    public const string Excluir = "EXCLUIR";
    public const string Reclassificar = "RECLASSIFICAR";
    public const string RevisaoManual = "REVISAO_MANUAL_PRIORIDADE";
    public const string Despachar = "DESPACHAR";
    public const string EncerrarDespacho = "ENCERRAR_DESPACHO";
    public const string AlterarStatus = "ALTERAR_STATUS";
    public const string Cancelar = "CANCELAR";
    public const string Concluir = "CONCLUIR";
    public const string TrocarMunicipio = "TROCAR_MUNICIPIO";
    public const string Unificar = "UNIFICAR";
    public const string Vincular = "VINCULAR_SOLICITACAO";
    public const string PublicarPontuacao = "PUBLICAR_PONTUACAO";
    public const string AprovarPontuacao = "APROVAR_PONTUACAO";
    public const string EditarPontuacao = "EDITAR_PONTUACAO";
    public const string AcessoNegado = "ACESSO_NEGADO";
    public const string ConsultaUc = "CONSULTA_UC";
    public const string Login = "LOGIN";
    public const string LoginRecusado = "LOGIN_RECUSADO";
}
