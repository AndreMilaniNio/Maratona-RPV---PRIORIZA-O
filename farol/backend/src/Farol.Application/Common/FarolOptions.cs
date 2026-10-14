namespace Farol.Application.Common;

/// <summary>Configurações operacionais (seção "Farol" do appsettings / variáveis Farol__*).</summary>
public class FarolOptions
{
    public const string Secao = "Farol";

    /// <summary>Fuso para exibição e calendário útil. Instantes ficam em UTC no banco.</summary>
    public string FusoHorario { get; set; } = "America/Sao_Paulo";
    /// <summary>Modo local de operador único: dispensa login, preservando auditoria com uma identidade interna.</summary>
    public bool ModoOperadorUnico { get; set; } = true;

    public UcOptions Uc { get; set; } = new();
    public TransformadorOptions Transformador { get; set; } = new();
    public NumeracaoOptions Numeracao { get; set; } = new();
    public DuplicidadeOptions Duplicidade { get; set; } = new();
    public PontuacaoOptions Pontuacao { get; set; } = new();
    public DemoOptions Demo { get; set; } = new();
    public ReclassificacaoOptions Reclassificacao { get; set; } = new();
    public RotulosOptions Rotulos { get; set; } = new();

    public class UcOptions
    {
        /// <summary>Formato do número da UC. Não há formato oficial presumido.</summary>
        public string Formato { get; set; } = @"^\d{6,10}$";
    }

    public class TransformadorOptions
    {
        public int TamanhoMaximo { get; set; } = 12;
    }

    public class NumeracaoOptions
    {
        /// <summary>Marcadores: {PREFIXO}, {ANO}, {SEQ} e {TIPO}. O formato é uma sugestão, não regra oficial.</summary>
        public string FormatoOs { get; set; } = "{PREFIXO}-{ANO}-{SEQ}";
        public string FormatoSolicitacao { get; set; } = "SOL-{PREFIXO}-{ANO}-{SEQ}";
        public int DigitosSequencia { get; set; } = 6;
    }

    public class DuplicidadeOptions
    {
        public double RaioMetros { get; set; } = 300;
        public int JanelaHoras { get; set; } = 24;
    }

    public class PontuacaoOptions
    {
        public int PontosMaximosPorOpcao { get; set; } = 1000;
        /// <summary>Quando verdadeiro, a publicação aguarda a aprovação de um segundo usuário.</summary>
        public bool ExigirAprovacao { get; set; }
    }

    public class DemoOptions
    {
        /// <summary>Carrega o conjunto demonstrativo (base fictícia) na primeira inicialização.</summary>
        public bool Habilitado { get; set; }
        /// <summary>Senha dos usuários demonstrativos. Só usada com Demo habilitado; nunca em produção.</summary>
        public string? SenhaUsuarios { get; set; }
    }

    public class ReclassificacaoOptions
    {
        public int IntervaloSegundos { get; set; } = 60;
        public bool Habilitada { get; set; } = true;
    }

    /// <summary>Rótulos configuráveis da situação do cliente (premissa 0.2).</summary>
    public class RotulosOptions
    {
        public string Ligado { get; set; } = "Ligado";
        public string Desligado { get; set; } = "Desligado";
        public string NaoInformada { get; set; } = "Não informada";
    }
}
