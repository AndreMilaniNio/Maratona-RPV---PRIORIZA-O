namespace Farol.Application.Common;

/// <summary>Permissões (seção 18). Cada uma vira uma policy de autorização.</summary>
public static class Permissoes
{
    public const string SolicitacaoRegistrar = "solicitacao.registrar";
    public const string OsConsultar = "os.consultar";
    public const string OsStatus = "os.status";
    public const string OsReclassificar = "os.reclassificar";
    public const string OsAtualizarFatos = "os.atualizar-fatos";
    public const string OsTrocarMunicipio = "os.trocar-municipio";
    public const string OsUnificar = "os.unificar";
    public const string OsReabrir = "os.reabrir";
    public const string OsEncerrar = "os.encerrar";
    public const string DespachoDesignar = "despacho.designar";
    public const string DespachoExcecao = "despacho.excecao";
    public const string LocalizacaoConsultar = "localizacao.consultar";
    public const string CidadesTodas = "cidades.todas";
    public const string PontuacaoEditar = "pontuacao.editar";
    public const string PontuacaoPublicar = "pontuacao.publicar";
    public const string PontuacaoAprovar = "pontuacao.aprovar";
    public const string CriteriosAdministrar = "criterios.administrar";
    public const string PrazosAdministrar = "prazos.administrar";
    public const string CadastrosAdministrar = "cadastros.administrar";
    public const string EquipesAdministrar = "equipes.administrar";
    public const string UsuariosAdministrar = "usuarios.administrar";
    public const string AuditoriaConsultar = "auditoria.consultar";

    public static readonly string[] Todas =
    [
        SolicitacaoRegistrar, OsConsultar, OsStatus, OsReclassificar, OsAtualizarFatos, OsTrocarMunicipio, OsUnificar,
        OsReabrir, OsEncerrar, DespachoDesignar, DespachoExcecao, LocalizacaoConsultar, CidadesTodas, PontuacaoEditar,
        PontuacaoPublicar, PontuacaoAprovar, CriteriosAdministrar, PrazosAdministrar, CadastrosAdministrar,
        EquipesAdministrar, UsuariosAdministrar, AuditoriaConsultar,
    ];
}

/// <summary>Perfis iniciais e suas permissões.</summary>
public static class Perfis
{
    public const string Administrador = "Administrador";
    public const string Atendente = "Atendente";
    public const string Despachante = "Despachante";
    public const string Supervisor = "Supervisor";
    public const string EquipeCampo = "EquipeCampo";
    public const string UsuarioChave = "UsuarioChave";

    public static readonly IReadOnlyDictionary<string, string[]> PermissoesPorPerfil = new Dictionary<string, string[]>
    {
        [Administrador] = Permissoes.Todas,
        [Atendente] =
        [
            Permissoes.SolicitacaoRegistrar, Permissoes.OsConsultar, Permissoes.LocalizacaoConsultar,
        ],
        [Despachante] =
        [
            Permissoes.OsConsultar, Permissoes.OsStatus, Permissoes.OsAtualizarFatos, Permissoes.OsEncerrar,
            Permissoes.DespachoDesignar, Permissoes.LocalizacaoConsultar, Permissoes.SolicitacaoRegistrar,
        ],
        [Supervisor] =
        [
            Permissoes.OsConsultar, Permissoes.OsStatus, Permissoes.OsAtualizarFatos, Permissoes.OsEncerrar,
            Permissoes.DespachoDesignar, Permissoes.LocalizacaoConsultar, Permissoes.SolicitacaoRegistrar,
            Permissoes.OsReclassificar, Permissoes.OsTrocarMunicipio, Permissoes.OsUnificar, Permissoes.OsReabrir,
            Permissoes.DespachoExcecao, Permissoes.CidadesTodas, Permissoes.AuditoriaConsultar, Permissoes.PontuacaoAprovar,
        ],
        [EquipeCampo] =
        [
            Permissoes.OsConsultar, Permissoes.OsStatus, Permissoes.LocalizacaoConsultar,
        ],
        [UsuarioChave] =
        [
            Permissoes.OsConsultar, Permissoes.PontuacaoEditar, Permissoes.PontuacaoPublicar, Permissoes.PontuacaoAprovar,
        ],
    };

    public static readonly string[] Todos = [Administrador, Atendente, Despachante, Supervisor, EquipeCampo, UsuarioChave];

    public static IReadOnlySet<string> PermissoesDe(IEnumerable<string> perfis) =>
        perfis.SelectMany(p => PermissoesPorPerfil.TryGetValue(p, out var lista) ? lista : []).ToHashSet();
}
