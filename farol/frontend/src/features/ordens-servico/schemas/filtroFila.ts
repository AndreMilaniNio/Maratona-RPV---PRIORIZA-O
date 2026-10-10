import { z } from 'zod';
import { StatusOrdemServicoValores, TipoManutencaoValores, type FiltroFila, type StatusOrdemServico } from '@/types/api';

/**
 * Estado dos filtros do Painel Operacional (seção 5.3).
 * É serializado na URL (`?prioridade=1&status=Aberta…`) para sobreviver a
 * atualizações, recarga da página e navegação "voltar".
 */
export const TAMANHOS_PAGINA = [10, 25, 50, 100] as const;
export const TAMANHO_PAGINA_PADRAO = 25;

/** Campos aceitos pelo backend em `OrdenarPor` (prefixo "-" inverte). Vazio = política da fila. */
export const CAMPOS_ORDENACAO = ['fila', 'numero', 'pontuacao', 'abertura', 'prazo', 'municipio'] as const;
export type CampoOrdenacao = (typeof CAMPOS_ORDENACAO)[number];

const texto = z.string().trim().max(120).catch('');
const idOpcional = z.coerce.number().int().positive().nullable().catch(null);
const dataIso = z
  .string()
  .regex(/^\d{4}-\d{2}-\d{2}$/)
  .or(z.literal(''))
  .catch('');

export const filtroPainelSchema = z.object({
  busca: texto,
  prioridadeIds: z.array(z.coerce.number().int().positive()).catch([]),
  status: z.array(z.enum(StatusOrdemServicoValores)).catch([]),
  tipoManutencao: z.enum(TipoManutencaoValores).or(z.literal('')).catch(''),
  tipoOcorrenciaId: idOpcional,
  bairro: texto,
  logradouro: texto,
  trecho: texto,
  faixaPessoas: texto,
  faixaUcs: texto,
  /** '' | 'QUALQUER' (qualquer risco real) | código de condição de segurança. */
  risco: texto,
  /** '' | 'QUALQUER' (qualquer serviço essencial identificado) | código. */
  servicoEssencial: texto,
  equipamento: texto,
  equipeId: idOpcional,
  /** yyyy-mm-dd (dia inteiro no fuso operacional). */
  abertaDe: dataIso,
  abertaAte: dataIso,
  prazo: z.enum(['vencido', 'proximo']).or(z.literal('')).catch(''),
  subestacaoId: idOpcional,
  conjuntoId: idOpcional,
  transformador: texto,
  classeId: idOpcional,
  situacaoCliente: texto,
  uc: texto,
  incluirEncerradas: z.boolean().catch(false),
  /** '' = política da fila; senão um de CAMPOS_ORDENACAO, com "-" para decrescente. */
  ordenarPor: z
    .string()
    .regex(new RegExp(`^-?(${CAMPOS_ORDENACAO.join('|')})$`))
    .or(z.literal(''))
    .catch(''),
  pagina: z.coerce.number().int().min(1).catch(1),
  tamanhoPagina: z.coerce
    .number()
    .int()
    .refine((n) => (TAMANHOS_PAGINA as readonly number[]).includes(n))
    .catch(TAMANHO_PAGINA_PADRAO),
});

export type FiltroPainel = z.infer<typeof filtroPainelSchema>;

export const FILTRO_PADRAO: FiltroPainel = filtroPainelSchema.parse({});

/** Nome do parâmetro na URL de cada campo do filtro. */
const PARAM: Record<keyof FiltroPainel, string> = {
  busca: 'busca',
  prioridadeIds: 'prioridade',
  status: 'status',
  tipoManutencao: 'manutencao',
  tipoOcorrenciaId: 'ocorrencia',
  bairro: 'bairro',
  logradouro: 'logradouro',
  trecho: 'trecho',
  faixaPessoas: 'pessoas',
  faixaUcs: 'ucs',
  risco: 'risco',
  servicoEssencial: 'essencial',
  equipamento: 'equipamento',
  equipeId: 'equipe',
  abertaDe: 'de',
  abertaAte: 'ate',
  prazo: 'prazo',
  subestacaoId: 'circuito',
  conjuntoId: 'conjunto',
  transformador: 'trafo',
  classeId: 'classe',
  situacaoCliente: 'situacao',
  uc: 'uc',
  incluirEncerradas: 'encerradas',
  ordenarPor: 'ordem',
  pagina: 'pagina',
  tamanhoPagina: 'tamanho',
};

const LISTAS: (keyof FiltroPainel)[] = ['prioridadeIds', 'status'];

/** Lê o filtro dos parâmetros da URL (valores inválidos voltam ao padrão). */
export function filtroDeSearchParams(sp: URLSearchParams): FiltroPainel {
  const bruto: Record<string, unknown> = {};
  for (const [campo, nome] of Object.entries(PARAM) as [keyof FiltroPainel, string][]) {
    if (LISTAS.includes(campo)) {
      const v = sp.getAll(nome).filter(Boolean);
      if (v.length) bruto[campo] = v;
    } else if (campo === 'incluirEncerradas') {
      bruto[campo] = sp.get(nome) === '1';
    } else {
      const v = sp.get(nome);
      if (v !== null && v !== '') bruto[campo] = v;
    }
  }
  return filtroPainelSchema.parse(bruto);
}

/**
 * Escreve o filtro nos parâmetros da URL, só com o que difere do padrão.
 * Parâmetros alheios ao filtro (ex.: `os` do detalhe aberto) são preservados.
 */
export function filtroParaSearchParams(f: FiltroPainel, base?: URLSearchParams): URLSearchParams {
  const sp = new URLSearchParams(base);
  for (const nome of Object.values(PARAM)) sp.delete(nome);
  for (const [campo, nome] of Object.entries(PARAM) as [keyof FiltroPainel, string][]) {
    const v = f[campo];
    const padrao = FILTRO_PADRAO[campo];
    if (Array.isArray(v)) {
      for (const item of v) sp.append(nome, String(item));
    } else if (typeof v === 'boolean') {
      if (v) sp.set(nome, '1');
    } else if (v !== null && v !== '' && v !== padrao) {
      sp.set(nome, String(v));
    }
  }
  return sp;
}

/** Converte yyyy-mm-dd no início/fim do dia no fuso operacional (America/Sao_Paulo, UTC−3). */
function limiteDoDia(data: string, fim: boolean): string | undefined {
  if (!data) return undefined;
  return `${data}T${fim ? '23:59:59' : '00:00:00'}-03:00`;
}

/**
 * Monta os parâmetros de GET /api/ordens-servico (FiltroFila) a partir do filtro da tela.
 * Circuito/conjunto só valem com uma única cidade selecionada.
 */
export function filtroParaQuery(f: FiltroPainel, municipioId: number | null): FiltroFila {
  const umaCidade = municipioId != null;
  const q: FiltroFila = {
    MunicipioId: municipioId ?? undefined,
    PrioridadeIds: f.prioridadeIds.length ? [...f.prioridadeIds] : undefined,
    Status: f.status.length ? [...f.status] : undefined,
    TipoManutencao: f.tipoManutencao || undefined,
    TipoOcorrenciaId: f.tipoOcorrenciaId ?? undefined,
    Bairro: f.bairro || undefined,
    Logradouro: f.logradouro || undefined,
    Trecho: f.trecho || undefined,
    FaixaPessoas: f.faixaPessoas || undefined,
    FaixaUcs: f.faixaUcs || undefined,
    Risco: f.risco || undefined,
    ServicoEssencial: f.servicoEssencial || undefined,
    Equipamento: f.equipamento || undefined,
    EquipeId: f.equipeId ?? undefined,
    AbertaDe: limiteDoDia(f.abertaDe, false),
    AbertaAte: limiteDoDia(f.abertaAte, true),
    Prazo: f.prazo || undefined,
    SubestacaoId: umaCidade ? (f.subestacaoId ?? undefined) : undefined,
    ConjuntoId: umaCidade ? (f.conjuntoId ?? undefined) : undefined,
    Transformador: f.transformador || undefined,
    ClasseId: f.classeId ?? undefined,
    SituacaoCliente: f.situacaoCliente || undefined,
    Uc: f.uc || undefined,
    Busca: f.busca || undefined,
    OrdenarPor: f.ordenarPor || undefined,
    IncluirEncerradas: f.incluirEncerradas || undefined,
    Pagina: f.pagina,
    TamanhoPagina: f.tamanhoPagina,
  };
  // Remove chaves vazias: a chave de cache fica estável e a URL da API limpa.
  for (const k of Object.keys(q) as (keyof FiltroFila)[]) if (q[k] === undefined) delete q[k];
  return q;
}

/** Campos que não contam como "filtro ativo" (paginação, ordenação, busca exibida à parte). */
const NAO_FILTROS: (keyof FiltroPainel)[] = ['pagina', 'tamanhoPagina', 'ordenarPor', 'busca'];

/** Quantos filtros (além da busca) estão diferentes do padrão. */
export function contarFiltrosAtivos(f: FiltroPainel): number {
  let n = 0;
  for (const campo of Object.keys(FILTRO_PADRAO) as (keyof FiltroPainel)[]) {
    if (NAO_FILTROS.includes(campo)) continue;
    const v = f[campo];
    if (Array.isArray(v) ? v.length > 0 : v !== FILTRO_PADRAO[campo]) n++;
  }
  return n;
}

/** Status "aguardando despacho" do indicador (backend: Despacháveis). */
export const STATUS_DESPACHAVEIS: StatusOrdemServico[] = ['Aberta', 'EmTriagem', 'AguardandoDespacho'];
/** Status "em atendimento" do indicador (backend: com despacho ativo). */
export const STATUS_EM_ATENDIMENTO: StatusOrdemServico[] = ['EquipeDesignada', 'EquipeACaminho', 'EmExecucao', 'AguardandoRecurso'];

/** Ordenação: alterna entre crescente/decrescente no mesmo campo. */
export function proximaOrdenacao(atual: string, campo: CampoOrdenacao): string {
  if (campo === 'fila') return '';
  const asc = campo;
  const desc = `-${campo}`;
  // Pontuação começa pela maior; demais começam crescentes.
  const primeiro = campo === 'pontuacao' ? desc : asc;
  const segundo = primeiro === asc ? desc : asc;
  if (atual === primeiro) return segundo;
  if (atual === segundo) return '';
  return primeiro;
}

/** Direção atual da ordenação em um campo (para aria-sort). */
export function direcaoOrdenacao(atual: string, campo: CampoOrdenacao): 'ascending' | 'descending' | 'none' {
  if (campo === 'fila') return atual === '' || atual === 'fila' ? 'ascending' : 'none';
  if (atual === campo) return 'ascending';
  if (atual === `-${campo}`) return 'descending';
  return 'none';
}
