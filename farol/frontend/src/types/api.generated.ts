/* eslint-disable */
// Tipos gerados a partir de farol/docs/openapi.json (OpenAPI 3.0.4).
// Enums são serializados como strings (nomes PascalCase do C#).
// Propriedades de DTOs de resposta são obrigatórias (o servidor sempre as envia);
// referências a objetos são anuláveis. DTOs de requisição têm propriedades opcionais.

export type AgregacaoCriterio = "Maximo" | "Soma";
export const AgregacaoCriterioValores = ["Maximo", "Soma"] as const satisfies readonly AgregacaoCriterio[];

export interface AlterarStatusRequest {
  status?: StatusOrdemServico;
  justificativa?: string | null;
}

export interface AnexoDto {
  id: string;
  nomeArquivo: string | null;
  tipoConteudo: string | null;
  tamanho: number;
  enviadoEm: string;
}

export type AplicacaoVersao = "SomenteNovas" | "ReclassificarAbertas";
export const AplicacaoVersaoValores = ["SomenteNovas", "ReclassificarAbertas"] as const satisfies readonly AplicacaoVersao[];

export interface AtualizarFatosRequest {
  impacto?: ImpactoInput;
  classesIds?: number[] | null;
  latitude?: number | null;
  longitude?: number | null;
  origemCoordenada?: OrigemCoordenada;
  trechoConfirmado?: boolean | null;
  redeConfirmada?: boolean | null;
  justificativa?: string | null;
}

export interface AuditoriaDto {
  id: number;
  ocorridoEm: string;
  usuario: string | null;
  acao: string | null;
  entidade: string | null;
  entidadeId: string | null;
  municipioId: number | null;
  valoresAnteriores: string | null;
  valoresNovos: string | null;
  justificativa: string | null;
}

export interface AuditoriaDtoPaginaDto {
  itens: AuditoriaDto[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export type CalendarioPrazo = "Corrido" | "Util";
export const CalendarioPrazoValores = ["Corrido", "Util"] as const satisfies readonly CalendarioPrazo[];

export type CanalEntrada = "Telefone" | "SistemaInterno" | "Presencial" | "Integracao";
export const CanalEntradaValores = ["Telefone", "SistemaInterno", "Presencial", "Integracao"] as const satisfies readonly CanalEntrada[];

export interface CandidatasDto {
  osId: string;
  osNumero: string | null;
  osLatitude: number | null;
  osLongitude: number | null;
  qualificacoesExigidas: string[];
  recursosExigidos: string[];
  roteamentoDisponivel: boolean;
  equipes: EquipeCandidataDto[];
}

export interface CatalogoDto {
  municipios: MunicipioDto[];
  tiposOcorrencia: TipoOcorrenciaDto[];
  classes: ClasseClienteDto[];
  recursos: ItemCodigoDto[];
  qualificacoes: ItemCodigoDto[];
  prioridades: PrioridadeDto[];
  criteriosFixos: CriterioFormularioDto[];
  criteriosPersonalizados: CriterioFormularioDto[];
  rotulosSituacao: Record<string, string> | null;
  configuracao: ConfiguracaoFormularioDto | null;
}

export interface CepDto {
  encontrado: boolean;
  cep: string | null;
  logradouro: string | null;
  bairro: string | null;
  municipioId: number | null;
  municipio: string | null;
  cepUnico: boolean;
  fonte: string | null;
  aviso: string | null;
}

export interface ClasseClienteDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  essencial: boolean;
  ordem: number;
  ativo: boolean;
}

export interface ClasseSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  essencial?: boolean;
  ordem?: number;
  ativo?: boolean;
}

export interface ClassificacaoDto {
  pontuacao: number;
  prioridade: PrioridadeResumoDto | null;
  prioridadePelaFaixa: PrioridadeResumoDto | null;
  nivelPrecedencia: number;
  motivoPrincipal: string | null;
  itens: ItemPontuacaoDto[];
  regrasAplicadas: RegraAplicadaDto[];
  versao: VersaoResumoDto | null;
  calculadoEm: string;
  alertas: string[];
}

export interface ComentarioDto {
  id: number;
  usuario: string | null;
  texto: string | null;
  criadoEm: string;
}

export interface ComentarioRequest {
  texto?: string | null;
}

export interface ComparacaoDto {
  a: VersaoListaDto | null;
  b: VersaoListaDto | null;
  diferencas: DiferencaDto[];
}

export interface CondicaoDto {
  criterio: string | null;
  criterioNome: string | null;
  opcoes: string[];
  opcoesRotulos: string[];
}

export interface CondicaoRegra {
  criterio?: string | null;
  opcoes?: string[] | null;
}

export interface ConfiguracaoFormularioDto {
  formatoUc: string | null;
  transformadorTamanhoMaximo: number;
  fusoHorario: string | null;
  demoHabilitado: boolean;
  roteamentoDisponivel: boolean;
}

export interface ConfiguracaoPontuacaoDto {
  municipioId: number | null;
  escopo: string | null;
  vigente: VersaoDetalheDto | null;
  vigenteHerdadaDaGlobal: boolean;
  rascunho: VersaoDetalheDto | null;
  aguardandoAprovacao: VersaoDetalheDto | null;
  regrasPrecedencia: RegraPrecedenciaDto[];
  exigeAprovacao: boolean;
  pontosMaximosPorOpcao: number;
  podeEditar: boolean;
  podePublicar: boolean;
  podeAprovar: boolean;
}

export interface ConjuntoDto {
  id: number;
  subestacaoId: number;
  numero: string | null;
  demonstrativo: boolean;
}

export interface ConjuntoSalvarRequest {
  subestacaoId?: number;
  numero?: string | null;
}

export interface CoordenadaDto {
  latitude: number;
  longitude: number;
  origem: OrigemCoordenada;
  precisaoMetros: number | null;
  registradaEm: string | null;
}

export interface CoordenadasInterpretadasDto {
  valido: boolean;
  latitude: number | null;
  longitude: number | null;
  alertas: string[];
  erro: string | null;
}

export interface CriterioAdminDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  descricao: string | null;
  regraAplicacao: string | null;
  tipo: TipoCriterio;
  agregacao: AgregacaoCriterio;
  multiplaEscolha: boolean;
  ordem: number;
  ativo: boolean;
  opcoes: OpcaoFormularioDto[];
  municipiosIds: number[];
  criadoEm: string;
  alteradoEm: string;
}

export interface CriterioFormularioDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  descricao: string | null;
  multiplaEscolha: boolean;
  opcoes: OpcaoFormularioDto[];
}

export interface CriterioPontosDto {
  criterioId: number;
  codigo: string | null;
  nome: string | null;
  descricao: string | null;
  regraAplicacao: string | null;
  tipo: TipoCriterio;
  agregacao: AgregacaoCriterio;
  multiplaEscolha: boolean;
  habilitado: boolean;
  minimoCriterio: number | null;
  maximoCriterio: number | null;
  opcoes: OpcaoPontosDto[];
}

export interface CriterioSalvarDto {
  criterioId?: number;
  habilitado?: boolean;
}

export interface CriterioSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  descricao?: string | null;
  regraAplicacao?: string | null;
  agregacao?: AgregacaoCriterio;
  multiplaEscolha?: boolean;
  opcoes?: OpcaoSalvarDto[] | null;
  municipiosIds?: number[] | null;
  ativo?: boolean;
}

export interface DesignacaoDto {
  despachoId: string;
  osId: string;
  osNumero: string | null;
  equipe: EquipeResumoDto | null;
  entrega: EntregaEquipeDto | null;
}

export interface DesignarRequest {
  equipeId?: number;
  justificativa?: string | null;
  excecao?: boolean;
  apoioIntermunicipal?: boolean;
}

export interface DespachoDto {
  id: string;
  equipe: EquipeResumoDto | null;
  ativo: boolean;
  designadoPor: string | null;
  designadoEm: string;
  aceitoEm: string | null;
  iniciadoEm: string | null;
  encerradoEm: string | null;
  motivoEncerramento: string | null;
  distanciaKm: number | null;
  tempoEstimadoMin: number | null;
  origemEstimativa: string | null;
  apoioIntermunicipal: boolean;
  excecao: boolean;
  justificativa: string | null;
}

export interface DiferencaDto {
  tipo: string | null;
  item: string | null;
  valorA: string | null;
  valorB: string | null;
}

export interface DuplicidadeDto {
  osId: string;
  numero: string | null;
  status: string | null;
  tipoOcorrencia: string | null;
  endereco: string | null;
  distanciaM: number | null;
  motivos: string[];
  abertaEm: string;
}

export interface EntregaEquipeDto {
  latitude: number | null;
  longitude: number | null;
  coordenadas: string | null;
  localizacaoAproximada: boolean;
  avisoLocalizacao: string | null;
  endereco: string | null;
  cep: string | null;
  ucs: string[];
  subestacao: string | null;
  conjunto: string | null;
  transformador: string | null;
  linkMapa: string | null;
  geoUri: string | null;
}

export interface EquipeCandidataDto {
  equipe: EquipeDto | null;
  compativel: boolean;
  faltando: string[];
  disponivel: boolean;
  motivoIndisponivel: string | null;
  apoioIntermunicipal: boolean;
  distanciaKm: number | null;
  tempoEstimadoMin: number | null;
  origemEstimativa: string | null;
  recomendada: boolean;
  justificativa: string | null;
}

export interface EquipeDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  municipioBaseId: number;
  municipioBase: string | null;
  municipiosAdicionais: ItemCodigoDto[];
  status: StatusEquipe;
  capacidade: number;
  despachosAtivos: number;
  disponivel: boolean;
  latitude: number | null;
  longitude: number | null;
  localizacaoEm: string | null;
  origemLocalizacao: OrigemLocalizacaoEquipe;
  integrantes: IntegranteDto[];
  qualificacoes: ItemCodigoDto[];
  recursos: ItemCodigoDto[];
  osAtribuidas: OsAtribuidaDto[];
  ativa: boolean;
  demonstrativa: boolean;
}

export interface EquipeResumoDto {
  id: number;
  codigo: string | null;
  nome: string | null;
}

export interface EquipeSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  municipioBaseId?: number;
  municipiosAdicionaisIds?: number[] | null;
  status?: StatusEquipe;
  capacidade?: number;
  integrantes?: IntegranteSalvarDto[] | null;
  qualificacoesIds?: number[] | null;
  recursosIds?: number[] | null;
  ativa?: boolean;
}

export interface EventoDespachoRequest {
  observacao?: string | null;
  motivo?: string | null;
}

export interface FaixaDto {
  prioridadeId: number;
  codigo: string | null;
  nome: string | null;
  cor: string | null;
  rank: number;
  minimo: number | null;
  maximo: number | null;
}

export interface FaixaSalvarDto {
  prioridadeId?: number;
  minimo?: number;
  maximo?: number | null;
}

export interface FeriadoDto {
  id: number;
  data: string;
  nome: string | null;
  municipioId: number | null;
}

export interface FeriadoSalvarRequest {
  data?: string;
  nome?: string | null;
  municipioId?: number | null;
}

export interface FilaDto {
  itens: FilaItemDto[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
  versaoDemonstrativa: boolean;
  atualizadoEm: string;
}

export interface FilaItemDto {
  posicao: number | null;
  id: string;
  numero: string | null;
  prioridade: PrioridadeResumoDto | null;
  prioridadeManual: boolean;
  pontuacao: number;
  nivelPrecedencia: number;
  tipoManutencao: TipoManutencao;
  tipoOcorrencia: string | null;
  endereco: string | null;
  bairro: string | null;
  trecho: string | null;
  municipioId: number;
  municipio: string | null;
  ucs: string[];
  ucNaoInformada: boolean;
  classes: string[];
  subestacao: string | null;
  conjunto: string | null;
  transformador: string | null;
  faixaPessoas: string | null;
  faixaUcs: string | null;
  risco: boolean;
  condicoesSeguranca: string[];
  servicoEssencial: string | null;
  situacaoCliente: string | null;
  abertaEm: string;
  proximoPrazo: ProximoPrazoDto | null;
  status: StatusOrdemServico;
  equipe: EquipeResumoDto | null;
  localizacaoPendente: boolean;
  temCoordenadas: boolean;
  versaoDemonstrativa: boolean;
}

export interface HistoricoDto {
  ocorridoEm: string;
  tipo: string | null;
  descricao: string | null;
  usuario: string | null;
  statusAnterior: string | null;
  statusNovo: string | null;
  justificativa: string | null;
}

export interface ImpactoCidadeDto {
  municipioId: number;
  municipio: string | null;
  avaliadas: number;
  mudariam: number;
}

export interface ImpactoDto {
  pessoasAfetadas: string | null;
  quantidadePessoas: number | null;
  ucsAfetadas: string | null;
  quantidadeUcs: number | null;
  servicoEssencial: string | null;
  situacaoCliente: string | null;
  condicoesSeguranca: string[];
  condicaoFornecimento: string | null;
  redundancia: string | null;
  fonteReserva: string | null;
  equipeEspecializada: string | null;
  equipamentoAfetado: string | null;
  abrangencia: string | null;
  nivelRede: string | null;
  quantidadeEquipamentos: number | null;
  duracaoEstimadaMin: number | null;
  dataLimite: string | null;
  recursosNecessarios: string[];
  classes: string[];
}

export interface ImpactoInput {
  pessoasAfetadas?: string | null;
  quantidadePessoas?: number | null;
  ucsAfetadas?: string | null;
  quantidadeUcs?: number | null;
  servicoEssencial?: string | null;
  situacaoCliente?: string | null;
  condicoesSeguranca?: string[] | null;
  condicaoFornecimento?: string | null;
  redundancia?: string | null;
  fonteReserva?: string | null;
  equipeEspecializada?: string | null;
  equipamentoAfetado?: string | null;
  abrangencia?: string | null;
  nivelRede?: string | null;
  quantidadeEquipamentos?: number | null;
  duracaoEstimadaMin?: number | null;
  dataLimite?: string | null;
  recursosIds?: number[] | null;
}

export interface ImpactoOsDto {
  osId: string;
  numero: string | null;
  municipio: string | null;
  de: string | null;
  para: string | null;
  pontuacaoDe: number;
  pontuacaoPara: number;
}

export interface ImpactoVersaoDto {
  versaoId: number;
  avaliadas: number;
  mudariam: number;
  porMunicipio: ImpactoCidadeDto[];
  ordens: ImpactoOsDto[];
}

export interface IndicadoresCidadeDto {
  municipioId: number;
  municipio: string | null;
  abertas: number;
  criticas: number;
  aguardandoDespacho: number;
  emAtendimento: number;
  vencidas: number;
  equipesDisponiveis: number;
  equipesDeslocamento: number;
  equipesExecutando: number;
}

export interface IndicadoresDto {
  abertas: number;
  criticas: number;
  aguardandoDespacho: number;
  emAtendimento: number;
  vencidas: number;
  equipesDisponiveis: number;
  equipesDeslocamento: number;
  equipesExecutando: number;
  porMunicipio: IndicadoresCidadeDto[];
  atualizadoEm: string;
}

export interface IntegranteDto {
  id: number;
  nome: string | null;
  matricula: string | null;
  funcao: string | null;
}

export interface IntegranteSalvarDto {
  nome?: string | null;
  matricula?: string | null;
  funcao?: string | null;
}

export interface InterpretacaoTransformadorDto {
  valido: boolean;
  erro: string | null;
  numero: string | null;
  codigoLocalidade: string | null;
  numeroLocal: string | null;
  localidade: LocalidadeDto | null;
  localidadeCadastrada: boolean;
  transformador: TransformadorDto | null;
  alertas: string[];
}

export interface InterpretarCoordenadasRequest {
  texto?: string | null;
  municipioId?: number | null;
}

export interface ItemCodigoDto {
  id: number;
  codigo: string | null;
  nome: string | null;
}

export interface ItemPontuacaoDto {
  criterioCodigo: string | null;
  criterio: string | null;
  opcoes: string[];
  opcoesCodigos: string[];
  pontos: number;
  semPontosDefinidos: boolean;
  requerConfirmacao: boolean;
}

export interface ItemSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
}

export interface LocalidadeDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  municipioId: number;
  municipio: string | null;
  demonstrativo: boolean;
}

export interface LocalidadeSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  municipioId?: number;
}

export interface LocalizacaoEquipeRequest {
  latitude?: number;
  longitude?: number;
  origem?: OrigemLocalizacaoEquipe;
}

export interface LocalizacaoInput {
  logradouro?: string | null;
  numero?: string | null;
  bairro?: string | null;
  cep?: string | null;
  enderecoCompleto?: string | null;
  pontoReferencia?: string | null;
  observacoes?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  origemCoordenada?: OrigemCoordenada;
  precisaoMetros?: number | null;
}

export interface LoginRequest {
  email?: string | null;
  senha?: string | null;
}

export interface LoginResponse {
  token: string | null;
  expiraEm: string;
  usuario: UsuarioSessaoDto | null;
}

export interface MapaDto {
  ordens: MapaOsDto[];
  equipes: MapaEquipeDto[];
  subestacoes: MapaSubestacaoDto[];
  semCoordenadas: number;
  centroLatitude: number | null;
  centroLongitude: number | null;
}

export interface MapaEquipeDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  status: StatusEquipe;
  disponivel: boolean;
  latitude: number;
  longitude: number;
  localizacaoEm: string | null;
  origem: OrigemLocalizacaoEquipe;
  municipioId: number;
}

export interface MapaOsDto {
  id: string;
  numero: string | null;
  latitude: number;
  longitude: number;
  prioridade: PrioridadeResumoDto | null;
  status: StatusOrdemServico;
  tipoOcorrencia: string | null;
  endereco: string | null;
  critica: boolean;
  origem: OrigemCoordenada;
  municipioId: number;
}

export interface MapaSubestacaoDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  latitude: number;
  longitude: number;
  demonstrativo: boolean;
}

export interface MunicipioDto {
  id: number;
  nome: string | null;
  uf: string | null;
  codigoIbge: string | null;
  prefixo: string | null;
  latitude: number | null;
  longitude: number | null;
  raioKm: number | null;
  cepUnico: string | null;
  ativo: boolean;
  demonstrativo: boolean;
}

export interface MunicipioResumoDto {
  id: number;
  nome: string | null;
  prefixo: string | null;
}

export interface MunicipioSalvarRequest {
  nome?: string | null;
  uf?: string | null;
  codigoIbge?: string | null;
  prefixo?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  raioKm?: number | null;
  cepUnico?: string | null;
  ativo?: boolean;
}

export interface NovaSolicitacaoRequest {
  chaveIdempotencia?: string;
  municipioId?: number;
  canal?: CanalEntrada;
  origem?: string | null;
  protocoloExterno?: string | null;
  ucNaoInformada?: boolean;
  motivoUcNaoInformada?: string | null;
  ucs?: string[] | null;
  classesIds?: number[] | null;
  localizacao?: LocalizacaoInput;
  rede?: RedeInput;
  tipoManutencao?: TipoManutencao;
  tipoOcorrenciaId?: number;
  impacto?: ImpactoInput;
  descricao?: string | null;
  osExistenteId?: string | null;
  respostasPersonalizadas?: RespostaPersonalizadaInput[] | null;
}

export interface NovaSolicitacaoResponse {
  solicitacaoId: string;
  solicitacaoNumero: string | null;
  osId: string;
  osNumero: string | null;
  vinculadaAOsExistente: boolean;
  prioridade: PrioridadeResumoDto | null;
  pontuacao: number;
  versaoDemonstrativa: boolean;
  alertas: string[];
  possiveisDuplicidades: DuplicidadeDto[];
}

export interface OpcaoFormularioDto {
  id: number;
  codigo: string | null;
  rotulo: string | null;
  representaDesconhecido: boolean;
}

export interface OpcaoPontosDto {
  opcaoId: number;
  codigo: string | null;
  rotulo: string | null;
  representaDesconhecido: boolean;
  pontos: number | null;
  requerConfirmacao: boolean;
  origem: OrigemPontos;
  alteradoPor: string | null;
  alteradoEm: string | null;
}

export interface OpcaoSalvarDto {
  codigo?: string | null;
  rotulo?: string | null;
  representaDesconhecido?: boolean;
}

export interface OrdemServicoDetalheDto {
  id: string;
  numero: string | null;
  posicao: number | null;
  status: StatusOrdemServico;
  tipoManutencao: TipoManutencao;
  tipoOcorrenciaId: number;
  tipoOcorrencia: string | null;
  municipioId: number;
  municipio: string | null;
  abertaEm: string;
  atualizadaEm: string;
  encerradaEm: string | null;
  motivoCancelamento: string | null;
  solicitacao: SolicitacaoDto | null;
  solicitacoesVinculadas: SolicitacaoDto[];
  ucs: UcDetalheDto[];
  logradouro: string | null;
  numeroEndereco: string | null;
  bairro: string | null;
  cep: string | null;
  enderecoCompleto: string | null;
  pontoReferencia: string | null;
  observacoesLocalizacao: string | null;
  localizacaoPendente: boolean;
  coordenadas: CoordenadaDto | null;
  rede: RedeDto | null;
  impacto: ImpactoDto | null;
  classificacao: ClassificacaoDto | null;
  prioridadeManual: boolean;
  justificativaManual: string | null;
  prazos: PrazosDto | null;
  despachoAtivo: DespachoDto | null;
  despachos: DespachoDto[];
  historico: HistoricoDto[];
  comentarios: ComentarioDto[];
  anexos: AnexoDto[];
  possiveisDuplicidades: DuplicidadeDto[];
  acoesDisponiveis: string[];
}

export type OrigemCoordenada = "Informada" | "Mapa" | "Geocodificada" | "Dispositivo";
export const OrigemCoordenadaValores = ["Informada", "Mapa", "Geocodificada", "Dispositivo"] as const satisfies readonly OrigemCoordenada[];

export type OrigemLocalizacaoEquipe = "Demonstrativa" | "Cadastrada" | "Gps";
export const OrigemLocalizacaoEquipeValores = ["Demonstrativa", "Cadastrada", "Gps"] as const satisfies readonly OrigemLocalizacaoEquipe[];

export type OrigemPontos = "Demonstrativo" | "UsuarioChave";
export const OrigemPontosValores = ["Demonstrativo", "UsuarioChave"] as const satisfies readonly OrigemPontos[];

export type OrigemRede = "NaoInformado" | "Informado" | "CadastroAConfirmar" | "Confirmado";
export const OrigemRedeValores = ["NaoInformado", "Informado", "CadastroAConfirmar", "Confirmado"] as const satisfies readonly OrigemRede[];

export interface OsAtribuidaDto {
  osId: string;
  numero: string | null;
  status: StatusOrdemServico;
  designadoEm: string;
}

export interface PontoSalvarDto {
  opcaoId?: number;
  pontos?: number | null;
  requerConfirmacao?: boolean;
}

export interface PrazosDto {
  triagem: string | null;
  despacho: string | null;
  inicio: string | null;
  restabelecimento: string | null;
  conclusao: string | null;
  proximo: ProximoPrazoDto | null;
  calendario: string | null;
  demonstrativos: boolean;
}

export interface PreferenciasRequest {
  municipioPreferidoId?: number | null;
}

export interface PrioridadeDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  descricao: string | null;
  rank: number;
  cor: string | null;
  critica: boolean;
  prazoTriagemMin: number;
  prazoDespachoMin: number;
  prazoInicioMin: number;
  prazoRestabelecimentoMin: number | null;
  prazoConclusaoMin: number;
  unidadePrazo: string | null;
  calendario: CalendarioPrazo;
  consideraFeriados: boolean;
  tratamentoCritico: string | null;
  escalonamento: string | null;
  vigenciaInicio: string;
  vigenciaFim: string | null;
  ativo: boolean;
  demonstrativa: boolean;
}

export interface PrioridadeResumoDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  cor: string | null;
  rank: number;
  critica: boolean;
}

export interface PrioridadeSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  descricao?: string | null;
  rank?: number;
  cor?: string | null;
  critica?: boolean;
  prazoTriagemMin?: number;
  prazoDespachoMin?: number;
  prazoInicioMin?: number;
  prazoRestabelecimentoMin?: number | null;
  prazoConclusaoMin?: number;
  unidadePrazo?: string | null;
  calendario?: CalendarioPrazo;
  consideraFeriados?: boolean;
  tratamentoCritico?: string | null;
  escalonamento?: string | null;
  vigenciaInicio?: string | null;
  vigenciaFim?: string | null;
  ativo?: boolean;
}

export interface ProximoPrazoDto {
  tipo: TipoPrazo;
  limite: string;
  vencido: boolean;
  minutosRestantes: number;
}

export interface PublicarRequest {
  versaoId?: number;
  justificativa?: string | null;
  vigenciaInicio?: string | null;
  aplicacao?: AplicacaoVersao;
}

export interface ReclassificarRequest {
  prioridadeManualId?: number | null;
  justificativa?: string | null;
}

export interface RedeDto {
  subestacaoId: number | null;
  subestacaoCodigo: string | null;
  subestacaoNome: string | null;
  subestacaoLocal: string | null;
  conjuntoId: number | null;
  conjunto: string | null;
  transformadorNumero: string | null;
  transformadorLocalidade: string | null;
  localidadeNome: string | null;
  transformadorLocal: string | null;
  origem: OrigemRede;
  trecho: string | null;
  trechoConfirmado: boolean;
  equipamento: string | null;
  identificadorEquipamento: string | null;
  chaveEletrica: string | null;
}

export interface RedeInput {
  subestacaoId?: number | null;
  conjuntoId?: number | null;
  transformadorNumero?: string | null;
  trecho?: string | null;
  equipamentoDescricao?: string | null;
  identificadorEquipamento?: string | null;
  chaveEletrica?: string | null;
}

export interface RegraAplicadaDto {
  codigo: string | null;
  nome: string | null;
  nivelPrecedencia: number;
  prioridadeMinima: string | null;
}

export interface RegraPrecedenciaDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  descricao: string | null;
  condicoes: CondicaoDto[];
  nivelPrecedencia: number;
  prioridadeMinimaId: number;
  prioridadeMinima: string | null;
  ativa: boolean;
  demonstrativa: boolean;
  versao: number;
  aprovadaEm: string | null;
}

export interface RegraSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  descricao?: string | null;
  condicoes?: CondicaoRegra[] | null;
  nivelPrecedencia?: number;
  prioridadeMinimaId?: number;
  ativa?: boolean;
  justificativa?: string | null;
}

export interface RespostaPersonalizadaInput {
  criterioId?: number;
  opcaoId?: number;
}

export interface RotaDto {
  disponivel: boolean;
  distanciaKm: number;
  tempoMin: number | null;
  origem: string | null;
  geometria: number[][] | null;
  aviso: string | null;
}

export interface SalvarRascunhoRequest {
  versaoId?: number;
  token?: string | null;
  criterios?: CriterioSalvarDto[] | null;
  pontos?: PontoSalvarDto[] | null;
  faixas?: FaixaSalvarDto[] | null;
}

export interface SimularRequest {
  municipioId?: number;
  usarRascunho?: boolean;
  versaoId?: number | null;
  solicitacao?: NovaSolicitacaoRequest;
}

export interface SolicitacaoDto {
  id: string;
  numero: string | null;
  municipioId: number;
  municipio: string | null;
  canal: CanalEntrada;
  origem: string | null;
  protocoloExterno: string | null;
  registradaEm: string;
  registradaPor: string | null;
  descricao: string | null;
  ucNaoInformada: boolean;
  motivoUcNaoInformada: string | null;
  ucs: SolicitacaoUcDto[];
  ordemServicoId: string;
  ordemServicoNumero: string | null;
  dadosInformados: string | null;
}

export interface SolicitacaoUcDto {
  numero: string | null;
  validadaNoCadastro: boolean;
}

export type StatusEquipe = "Disponivel" | "ACaminho" | "EmAtendimento" | "Indisponivel" | "EmPausa" | "DeslocamentoOutraAtividade";
export const StatusEquipeValores = ["Disponivel", "ACaminho", "EmAtendimento", "Indisponivel", "EmPausa", "DeslocamentoOutraAtividade"] as const satisfies readonly StatusEquipe[];

export interface StatusEquipeRequest {
  status?: StatusEquipe;
  justificativa?: string | null;
}

export type StatusOrdemServico = "Aberta" | "EmTriagem" | "AguardandoDespacho" | "EquipeDesignada" | "EquipeACaminho" | "EmExecucao" | "AguardandoRecurso" | "Suspensa" | "Concluida" | "Cancelada";
export const StatusOrdemServicoValores = ["Aberta", "EmTriagem", "AguardandoDespacho", "EquipeDesignada", "EquipeACaminho", "EmExecucao", "AguardandoRecurso", "Suspensa", "Concluida", "Cancelada"] as const satisfies readonly StatusOrdemServico[];

export type StatusVersaoPontuacao = "Rascunho" | "AguardandoAprovacao" | "Publicada" | "Substituida";
export const StatusVersaoPontuacaoValores = ["Rascunho", "AguardandoAprovacao", "Publicada", "Substituida"] as const satisfies readonly StatusVersaoPontuacao[];

export interface SubestacaoDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  local: string | null;
  municipioId: number;
  latitude: number | null;
  longitude: number | null;
  conjuntos: number;
  demonstrativo: boolean;
}

export interface SubestacaoSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  local?: string | null;
  municipioId?: number;
  latitude?: number | null;
  longitude?: number | null;
}

export type TipoCriterio = "Fixo" | "Personalizado";
export const TipoCriterioValores = ["Fixo", "Personalizado"] as const satisfies readonly TipoCriterio[];

export type TipoManutencao = "Corretiva" | "Preventiva";
export const TipoManutencaoValores = ["Corretiva", "Preventiva"] as const satisfies readonly TipoManutencao[];

export interface TipoOcorrenciaDto {
  id: number;
  codigo: string | null;
  nome: string | null;
  tipoManutencaoSugerido: TipoManutencao;
  ordem: number;
  ativo: boolean;
  qualificacoes: ItemCodigoDto[];
}

export interface TipoOcorrenciaSalvarRequest {
  codigo?: string | null;
  nome?: string | null;
  tipoManutencaoSugerido?: TipoManutencao;
  ordem?: number;
  ativo?: boolean;
  qualificacoesIds?: number[] | null;
}

export type TipoPrazo = "Triagem" | "Despacho" | "Inicio" | "Restabelecimento" | "Conclusao";
export const TipoPrazoValores = ["Triagem", "Despacho", "Inicio", "Restabelecimento", "Conclusao"] as const satisfies readonly TipoPrazo[];

export interface TransformadorDto {
  id: number;
  numeroCompleto: string | null;
  codigoLocalidade: string | null;
  numeroLocal: string | null;
  localidade: string | null;
  subestacaoId: number | null;
  subestacao: string | null;
  conjuntoId: number | null;
  conjunto: string | null;
  municipioId: number;
  ucsLigadas: number;
  demonstrativo: boolean;
}

export interface TransformadorDtoPaginaDto {
  itens: TransformadorDto[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface TransformadorSalvarRequest {
  numero?: string | null;
  municipioId?: number;
  subestacaoId?: number | null;
  conjuntoId?: number | null;
  latitude?: number | null;
  longitude?: number | null;
}

export interface TrocarMunicipioRequest {
  municipioId?: number;
  justificativa?: string | null;
}

export interface UcDetalheDto {
  numero: string | null;
  validadaNoCadastro: boolean;
  clienteMascarado: string | null;
  classe: string | null;
  situacao: string | null;
  endereco: string | null;
  demonstrativa: boolean;
}

export interface UnidadeConsumidoraDto {
  numero: string | null;
  clienteMascarado: string | null;
  classeId: number;
  classe: string | null;
  situacao: string | null;
  logradouro: string | null;
  numeroImovel: string | null;
  bairro: string | null;
  cep: string | null;
  municipioId: number;
  municipio: string | null;
  transformador: string | null;
  subestacaoId: number | null;
  subestacao: string | null;
  conjuntoId: number | null;
  conjunto: string | null;
  latitude: number | null;
  longitude: number | null;
  demonstrativa: boolean;
}

export interface UnificarRequest {
  osDuplicadaId?: string;
  justificativa?: string | null;
}

export interface UsuarioDto {
  id: string;
  nome: string | null;
  email: string | null;
  ativo: boolean;
  perfis: string[];
  municipiosIds: number[];
  equipeId: number | null;
  demonstrativo: boolean;
}

export interface UsuarioSalvarRequest {
  nome?: string | null;
  email?: string | null;
  senha?: string | null;
  ativo?: boolean;
  perfis?: string[] | null;
  municipiosIds?: number[] | null;
  equipeId?: number | null;
}

export interface UsuarioSessaoDto {
  id: string;
  nome: string | null;
  email: string | null;
  perfis: string[];
  permissoes: string[];
  municipios: MunicipioResumoDto[];
  todasCidades: boolean;
  municipioPreferidoId: number | null;
  equipeId: number | null;
}

export interface VersaoDetalheDto {
  id: number;
  numero: number;
  status: StatusVersaoPontuacao;
  demonstrativa: boolean;
  municipioId: number | null;
  municipioNome: string | null;
  vigenciaInicio: string | null;
  aplicacao: AplicacaoVersao;
  justificativa: string | null;
  autor: string | null;
  criadaEm: string;
  alteradaEm: string;
  publicadaPor: string | null;
  publicadaEm: string | null;
  aprovadaPor: string | null;
  aprovadaEm: string | null;
  token: string | null;
  criterios: CriterioPontosDto[];
  faixas: FaixaDto[];
  minimoPossivel: number;
  maximoPossivel: number;
  pendencias: string[];
  alertas: string[];
}

export interface VersaoListaDto {
  id: number;
  numero: number;
  status: StatusVersaoPontuacao;
  demonstrativa: boolean;
  municipioId: number | null;
  municipioNome: string | null;
  vigenciaInicio: string | null;
  aplicacao: AplicacaoVersao;
  justificativa: string | null;
  autor: string | null;
  criadaEm: string;
  publicadaPor: string | null;
  publicadaEm: string | null;
  aprovadaPor: string | null;
  restauradaDeId: number | null;
  emVigor: boolean;
}

export interface VersaoResumoDto {
  id: number;
  numero: number;
  municipioId: number | null;
  municipioNome: string | null;
  demonstrativa: boolean;
  vigenciaInicio: string | null;
}

