using Farol.Application.Abstractions;
using Farol.Domain.Exceptions;

namespace Farol.Application.Common;

/// <summary>Aplica o recorte por cidade no servidor (Key decision 1). Nenhuma consulta confia no frontend.</summary>
public class EscopoMunicipio(IUsuarioAtual usuario, Auditor auditor)
{
    /// <summary>Garante acesso ao município; recusa e audita quando fora do escopo.</summary>
    public async Task ExigirAsync(int municipioId, string entidade, object entidadeId, CancellationToken ct = default)
    {
        var permitidos = await usuario.MunicipiosPermitidosAsync(ct);
        if (permitidos is null || permitidos.Contains(municipioId)) return;

        await auditor.RegistrarAgoraAsync(AcoesAuditoria.AcessoNegado, entidade, entidadeId, municipioId,
            "Tentativa de acesso a município não autorizado.", ct);
        throw new AcessoNegadoException("Você não tem acesso a este município.");
    }

    /// <summary>
    /// Resolve os municípios de uma consulta: o pedido (se permitido) ou todos os permitidos.
    /// Nulo = sem restrição (perfil com acesso a todas as cidades e nenhuma cidade pedida).
    /// </summary>
    public async Task<IReadOnlySet<int>?> ResolverAsync(int? municipioPedido, CancellationToken ct = default)
    {
        var permitidos = await usuario.MunicipiosPermitidosAsync(ct);
        if (municipioPedido is { } m)
        {
            if (permitidos is not null && !permitidos.Contains(m))
            {
                await auditor.RegistrarAgoraAsync(AcoesAuditoria.AcessoNegado, "Municipio", m, m,
                    "Consulta a município não autorizado.", ct);
                throw new AcessoNegadoException("Você não tem acesso a este município.");
            }
            return new HashSet<int> { m };
        }

        if (permitidos is null) return null;
        // "Todas as cidades" sem a permissão: o usuário vê apenas as suas, e só se for uma.
        if (!usuario.Tem(Permissoes.CidadesTodas) && permitidos.Count > 1)
            throw new AcessoNegadoException("A visão de todas as cidades é restrita. Selecione uma cidade.");
        return permitidos;
    }
}
