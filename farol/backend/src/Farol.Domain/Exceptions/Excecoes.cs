namespace Farol.Domain.Exceptions;

/// <summary>Regra de negócio violada pelo pedido (400).</summary>
public class RegraNegocioException(string mensagem, IDictionary<string, string[]>? erros = null) : Exception(mensagem)
{
    public IDictionary<string, string[]> Erros { get; } = erros ?? new Dictionary<string, string[]>();
}

/// <summary>Estado atual impede a operação: corrida perdida, transição inválida, versão alterada (409).</summary>
public class ConflitoException(string mensagem) : Exception(mensagem);

/// <summary>A operação é válida, mas o conteúdo não pode ser processado — p.ex. publicar com pendências (422).</summary>
public class NaoProcessavelException(string mensagem, IReadOnlyList<string> pendencias) : Exception(mensagem)
{
    public IReadOnlyList<string> Pendencias { get; } = pendencias;
}

public class NaoEncontradoException(string mensagem) : Exception(mensagem);

/// <summary>Usuário autenticado sem permissão para o recurso ou o município (403).</summary>
public class AcessoNegadoException(string mensagem) : Exception(mensagem);
