/**
 * Chaves de consulta do TanStack Query, centralizadas para que a invalidação
 * (SignalR, mutações) seja consistente entre telas.
 */
export const queryKeys = {
  me: ['me'] as const,
  catalogo: ['catalogo'] as const,
  municipios: (todos?: boolean) => ['municipios', { todos: !!todos }] as const,

  /** Prefixo de tudo que depende da fila operacional (fila, indicadores, mapa, OS). */
  operacao: ['operacao'] as const,
  fila: (filtro: unknown) => ['operacao', 'fila', filtro] as const,
  indicadores: (municipioId: number | null) => ['operacao', 'indicadores', municipioId] as const,
  mapa: (municipioId: number | null) => ['operacao', 'mapa', municipioId] as const,
  os: (id: string) => ['operacao', 'os', id] as const,
  osPrioridade: (id: string) => ['operacao', 'os', id, 'prioridade'] as const,
  osHistorico: (id: string) => ['operacao', 'os', id, 'historico'] as const,
  candidatas: (id: string, incluirApoio: boolean) => ['operacao', 'os', id, 'candidatas', incluirApoio] as const,
  duplicidades: (params: unknown) => ['duplicidades', params] as const,

  equipes: (municipioId: number | null) => ['operacao', 'equipes', municipioId] as const,
  equipe: (id: number) => ['operacao', 'equipe', id] as const,
  equipeLocalizacoes: (id: number) => ['operacao', 'equipe', id, 'localizacao'] as const,

  subestacoes: (municipioId: number) => ['rede', 'subestacoes', municipioId] as const,
  conjuntos: (subestacaoId: number) => ['rede', 'conjuntos', subestacaoId] as const,
  localidades: (municipioId: number | null) => ['rede', 'localidades', municipioId] as const,
  transformadores: (params: unknown) => ['rede', 'transformadores', params] as const,
  interpretarTransformador: (numero: string, municipioId: number | null) => ['rede', 'transformador-interpretar', numero, municipioId] as const,

  uc: (numero: string) => ['uc', numero] as const,
  cep: (cep: string) => ['cep', cep] as const,

  pontuacao: (municipioId: number | null) => ['pontuacao', municipioId] as const,
  pontuacaoVersoes: (municipioId: number | null) => ['pontuacao', 'versoes', municipioId] as const,
  pontuacaoVersao: (id: number) => ['pontuacao', 'versao', id] as const,
  pontuacaoImpacto: (versaoId: number) => ['pontuacao', 'impacto', versaoId] as const,
  pontuacaoComparar: (a: number, b: number) => ['pontuacao', 'comparar', a, b] as const,

  prioridades: ['config', 'prioridades'] as const,
  regras: ['config', 'regras'] as const,
  criterios: ['config', 'criterios'] as const,
  feriados: ['config', 'feriados'] as const,
  tiposOcorrencia: (todos: boolean) => ['config', 'tipos-ocorrencia', todos] as const,
  usuarios: ['admin', 'usuarios'] as const,
  auditoria: (filtro: unknown) => ['admin', 'auditoria', filtro] as const,
};
