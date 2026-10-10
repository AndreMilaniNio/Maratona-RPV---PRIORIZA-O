import { z } from 'zod';
import type {
  CanalEntrada,
  CatalogoDto,
  CriterioFormularioDto,
  NovaSolicitacaoRequest,
  OrigemCoordenada,
  TipoManutencao,
} from '@/types/api';
import { CanalEntradaValores, OrigemCoordenadaValores, TipoManutencaoValores } from '@/types/api';
import { fromDateTimeLocalInput } from '@/lib/format';
import { apenasDigitos } from '@/lib/utils';

/** Códigos dos critérios fixos usados no formulário (opções vêm sempre do catálogo). */
export const CRITERIO = {
  pessoas: 'PESSOAS_AFETADAS',
  ucs: 'UCS_AFETADAS',
  classe: 'CLASSE_CLIENTE',
  servicoEssencial: 'SERVICO_ESSENCIAL',
  situacao: 'SITUACAO_CLIENTE',
  seguranca: 'RISCO_SEGURANCA',
  fornecimento: 'CONDICAO_FORNECIMENTO',
  redundancia: 'REDUNDANCIA',
  fonteReserva: 'FONTE_RESERVA',
  equipeEspecializada: 'EQUIPE_ESPECIALIZADA',
  equipamento: 'EQUIPAMENTO_AFETADO',
  abrangencia: 'ABRANGENCIA',
  nivelRede: 'NIVEL_REDE',
} as const;

/** Opções de segurança que não podem ser combinadas com outras. */
export const SEGURANCA_EXCLUSIVAS = ['SEM_RISCO_ADICIONAL', 'DESCONHECIDA'];
const SEM_INTERRUPCAO = 'SEM_INTERRUPCAO';

export const MSG_MEIO_LOCALIZAR = 'Informe ao menos um meio de localizar a ocorrência: rua, CEP ou coordenadas.';

const texto = z.string().max(4000);
const numeroOpcional = z.number().finite().nullable();

const localizacaoSchema = z.object({
  logradouro: z.string().max(200),
  numero: z.string().max(30),
  bairro: z.string().max(120),
  /** CEP com máscara (00000-000) ou vazio. */
  cep: z.string().refine((v) => v === '' || apenasDigitos(v).length === 8, 'CEP deve ter 8 dígitos.'),
  enderecoCompleto: z.string().max(400),
  pontoReferencia: z.string().max(300),
  observacoes: z.string().max(1000),
  latitude: numeroOpcional.refine((v) => v === null || (v >= -90 && v <= 90), 'Latitude deve estar entre -90 e 90.'),
  longitude: numeroOpcional.refine((v) => v === null || (v >= -180 && v <= 180), 'Longitude deve estar entre -180 e 180.'),
  origemCoordenada: z.enum(OrigemCoordenadaValores).nullable(),
  precisaoMetros: numeroOpcional,
});

const redeSchema = z.object({
  subestacaoId: z.number().int().nullable(),
  conjuntoId: z.number().int().nullable(),
  transformadorNumero: z.string().regex(/^\d*$/, 'Use somente dígitos.'),
  /** UI: circuito/conjunto aplicados a partir do cadastro (rótulo "identificado pelo cadastro, a confirmar"). */
  identificadoPeloCadastro: z.boolean(),
  trecho: z.string().max(200),
  equipamentoDescricao: z.string().max(200),
  identificadorEquipamento: z.string().max(100),
  chaveEletrica: z.string().max(100),
});

const impactoSchema = z.object({
  pessoasAfetadas: z.string().min(1),
  quantidadePessoas: numeroOpcional.refine((v) => v === null || v >= 0, 'Informe um número maior ou igual a zero.'),
  ucsAfetadas: z.string().min(1),
  quantidadeUcs: numeroOpcional.refine((v) => v === null || v >= 0, 'Informe um número maior ou igual a zero.'),
  servicoEssencial: z.string().min(1),
  situacaoCliente: z.string().min(1),
  condicoesSeguranca: z
    .array(z.string())
    .min(1, 'Marque ao menos uma condição (ou "Situação desconhecida").')
    .refine(
      (c) => c.length <= 1 || !c.some((x) => SEGURANCA_EXCLUSIVAS.includes(x)),
      '"Sem risco adicional" e "Situação desconhecida" não podem ser combinadas com outras condições.',
    ),
  condicaoFornecimento: z.string().min(1),
  redundancia: z.string().min(1),
  fonteReserva: z.string().min(1),
  equipeEspecializada: z.string().min(1),
  equipamentoAfetado: z.string().min(1),
  abrangencia: z.string().min(1),
  nivelRede: z.string().min(1),
  quantidadeEquipamentos: numeroOpcional.refine((v) => v === null || v >= 0, 'Informe um número maior ou igual a zero.'),
  duracaoEstimadaMin: numeroOpcional.refine((v) => v === null || v >= 0, 'Informe um número maior ou igual a zero.'),
  /** Valor de <input type="datetime-local"> (horário local) ou vazio. */
  dataLimite: z.string(),
  recursosIds: z.array(z.number().int()),
});

const baseSchema = z.object({
  municipioId: z.number({ invalid_type_error: 'Selecione o município.' }).int().nullable(),
  canal: z.enum(CanalEntradaValores),
  origem: z.string().max(200),
  protocoloExterno: z.string().max(100),
  ucNaoInformada: z.boolean(),
  motivoUcNaoInformada: z.string().max(300),
  ucs: z.array(z.object({ numero: z.string().max(30) })),
  classesIds: z.array(z.number().int()),
  /** UI: a classe foi pré-preenchida a partir da UC e precisa de confirmação explícita. */
  classeDaUc: z.boolean(),
  classeConfirmada: z.boolean(),
  /** UI: atendente identificou serviço essencial (mostra o critério mesmo sem classe essencial). */
  servicoEssencialIdentificado: z.boolean(),
  /** UI: registro feito no local da ocorrência (habilita a geolocalização do dispositivo). */
  registroNoLocal: z.boolean(),
  localizacao: localizacaoSchema,
  rede: redeSchema,
  tipoManutencao: z.enum(TipoManutencaoValores).nullable(),
  /** UI: o atendente escolheu o tipo de manutenção (não sobrescrever com a sugestão do tipo de ocorrência). */
  tipoManutencaoEscolhido: z.boolean(),
  tipoOcorrenciaId: z.number().int().nullable(),
  impacto: impactoSchema,
  respostasPersonalizadas: z.array(z.object({ criterioId: z.number().int(), opcaoId: z.number().int().nullable() })),
  descricao: texto,
  osExistenteId: z.string().nullable(),
  /** UI: número da OS vinculada (texto do botão). */
  osExistenteNumero: z.string().nullable(),
});

export type SolicitacaoFormValues = z.infer<typeof baseSchema>;

export interface OpcoesSchema {
  /** 'registro' (padrão) aplica todas as regras; 'simulador' só exige o que a classificação precisa. */
  modo?: 'registro' | 'simulador';
  /** Regex do formato da UC (catalogo.configuracao.formatoUc). */
  formatoUc?: string | null;
}

function temMeioDeLocalizar(l: SolicitacaoFormValues['localizacao']): boolean {
  return l.logradouro.trim() !== '' || apenasDigitos(l.cep) !== '' || (l.latitude !== null && l.longitude !== null);
}

function regexSegura(fonte: string | null | undefined): RegExp | null {
  if (!fonte) return null;
  try {
    return new RegExp(fonte);
  } catch {
    return null;
  }
}

/** Monta o schema conforme o modo e o formato de UC configurado. */
export function criarSolicitacaoSchema({ modo = 'registro', formatoUc }: OpcoesSchema = {}) {
  const regexUc = regexSegura(formatoUc);
  return baseSchema.superRefine((v, ctx) => {
    if (v.municipioId === null) ctx.addIssue({ code: 'custom', path: ['municipioId'], message: 'Selecione o município.' });
    if (v.tipoOcorrenciaId === null)
      ctx.addIssue({ code: 'custom', path: ['tipoOcorrenciaId'], message: 'Selecione o tipo de ocorrência.' });
    if (v.tipoManutencao === null)
      ctx.addIssue({ code: 'custom', path: ['tipoManutencao'], message: 'Selecione o tipo de manutenção.' });
    if (v.localizacao.latitude === null !== (v.localizacao.longitude === null))
      ctx.addIssue({ code: 'custom', path: ['localizacao', 'longitude'], message: 'Informe latitude e longitude juntas.' });
    if (v.rede.conjuntoId !== null && v.rede.subestacaoId === null)
      ctx.addIssue({ code: 'custom', path: ['rede', 'conjuntoId'], message: 'Selecione o circuito antes do conjunto.' });
    if (v.impacto.condicaoFornecimento === SEM_INTERRUPCAO && (v.impacto.duracaoEstimadaMin ?? 0) > 0)
      ctx.addIssue({
        code: 'custom',
        path: ['impacto', 'duracaoEstimadaMin'],
        message: 'Duração de interrupção informada para ocorrência sem interrupção.',
      });
    if (v.rede.transformadorNumero !== '' && v.rede.transformadorNumero.length < 4)
      ctx.addIssue({
        code: 'custom',
        path: ['rede', 'transformadorNumero'],
        message: 'O número do transformador precisa de ao menos 4 dígitos.',
      });

    if (modo === 'simulador') return;

    const ucsPreenchidas = v.ucs.filter((u) => u.numero.trim() !== '');
    if (regexUc) {
      v.ucs.forEach((u, i) => {
        if (u.numero.trim() !== '' && !regexUc.test(u.numero.trim()))
          ctx.addIssue({ code: 'custom', path: ['ucs', i, 'numero'], message: 'Formato de UC inválido.' });
      });
    }
    if (v.ucNaoInformada) {
      if (v.motivoUcNaoInformada.trim() === '')
        ctx.addIssue({ code: 'custom', path: ['motivoUcNaoInformada'], message: 'Informe por que a UC não foi informada.' });
      if (!temMeioDeLocalizar(v.localizacao))
        ctx.addIssue({ code: 'custom', path: ['localizacao'], message: MSG_MEIO_LOCALIZAR });
    } else if (ucsPreenchidas.length === 0) {
      ctx.addIssue({
        code: 'custom',
        path: ['ucs'],
        message: 'Informe ao menos uma UC ou marque "Solicitante fora da própria residência / não sabe a UC".',
      });
    }
    if (v.classeDaUc && !v.classeConfirmada)
      ctx.addIssue({
        code: 'custom',
        path: ['classeConfirmada'],
        message: 'Confirme a classe do cliente identificada pelo cadastro da UC.',
      });
    if (v.descricao.trim() === '') ctx.addIssue({ code: 'custom', path: ['descricao'], message: 'Descreva a ocorrência.' });
  });
}

/** Schema do registro (regras completas). */
export const solicitacaoSchema = criarSolicitacaoSchema();
/** Schema do simulador (UC, localização e descrição opcionais). */
export const solicitacaoSimuladorSchema = criarSolicitacaoSchema({ modo: 'simulador' });

function desconhecida(catalogo: CatalogoDto | undefined, codigo: string): string {
  const c: CriterioFormularioDto | undefined = catalogo?.criteriosFixos.find((x) => x.codigo === codigo);
  const op = c?.opcoes.find((o) => o.representaDesconhecido) ?? c?.opcoes[c.opcoes.length - 1];
  return op?.codigo ?? '';
}

/**
 * Valores iniciais: todo critério de impacto começa na sua opção explícita de "desconhecido"
 * (`representaDesconhecido`), nunca vazio ou zero.
 */
export function valoresIniciais(catalogo: CatalogoDto | undefined, municipioId: number | null): SolicitacaoFormValues {
  const d = (codigo: string) => desconhecida(catalogo, codigo);
  const canal: CanalEntrada = 'Telefone';
  return {
    municipioId,
    canal,
    origem: '',
    protocoloExterno: '',
    ucNaoInformada: false,
    motivoUcNaoInformada: '',
    ucs: [],
    classesIds: [],
    classeDaUc: false,
    classeConfirmada: false,
    servicoEssencialIdentificado: false,
    registroNoLocal: false,
    localizacao: {
      logradouro: '',
      numero: '',
      bairro: '',
      cep: '',
      enderecoCompleto: '',
      pontoReferencia: '',
      observacoes: '',
      latitude: null,
      longitude: null,
      origemCoordenada: null,
      precisaoMetros: null,
    },
    rede: {
      subestacaoId: null,
      conjuntoId: null,
      transformadorNumero: '',
      identificadoPeloCadastro: false,
      trecho: '',
      equipamentoDescricao: '',
      identificadorEquipamento: '',
      chaveEletrica: '',
    },
    tipoManutencao: null,
    tipoManutencaoEscolhido: false,
    tipoOcorrenciaId: null,
    impacto: {
      pessoasAfetadas: d(CRITERIO.pessoas),
      quantidadePessoas: null,
      ucsAfetadas: d(CRITERIO.ucs),
      quantidadeUcs: null,
      servicoEssencial: d(CRITERIO.servicoEssencial),
      situacaoCliente: d(CRITERIO.situacao),
      condicoesSeguranca: [d(CRITERIO.seguranca) || 'DESCONHECIDA'],
      condicaoFornecimento: d(CRITERIO.fornecimento),
      redundancia: d(CRITERIO.redundancia),
      fonteReserva: d(CRITERIO.fonteReserva),
      equipeEspecializada: d(CRITERIO.equipeEspecializada),
      equipamentoAfetado: d(CRITERIO.equipamento),
      abrangencia: d(CRITERIO.abrangencia),
      nivelRede: d(CRITERIO.nivelRede),
      quantidadeEquipamentos: null,
      duracaoEstimadaMin: null,
      dataLimite: '',
      recursosIds: [],
    },
    respostasPersonalizadas: (catalogo?.criteriosPersonalizados ?? []).map((c) => ({
      criterioId: c.id,
      opcaoId: c.opcoes.find((o) => o.representaDesconhecido)?.id ?? null,
    })),
    descricao: '',
    osExistenteId: null,
    osExistenteNumero: null,
  };
}

const vazioParaNull = (v: string | null | undefined): string | null => {
  const t = (v ?? '').trim();
  return t === '' ? null : t;
};

/** Mapeador ÚNICO do formulário para o payload da API (prévia, registro e simulador). */
export function paraNovaSolicitacaoRequest(values: SolicitacaoFormValues, chaveIdempotencia: string): NovaSolicitacaoRequest {
  const l = values.localizacao;
  const temCoord = l.latitude !== null && l.longitude !== null;
  const origem: OrigemCoordenada | undefined = temCoord ? (l.origemCoordenada ?? 'Informada') : undefined;
  const r = values.rede;
  const i = values.impacto;
  return {
    chaveIdempotencia,
    municipioId: values.municipioId ?? 0,
    canal: values.canal,
    origem: vazioParaNull(values.origem),
    protocoloExterno: vazioParaNull(values.protocoloExterno),
    ucNaoInformada: values.ucNaoInformada,
    motivoUcNaoInformada: values.ucNaoInformada ? vazioParaNull(values.motivoUcNaoInformada) : null,
    ucs: values.ucs.map((u) => u.numero.trim()).filter((u) => u !== ''),
    classesIds: [...values.classesIds],
    localizacao: {
      logradouro: vazioParaNull(l.logradouro),
      numero: vazioParaNull(l.numero),
      bairro: vazioParaNull(l.bairro),
      cep: vazioParaNull(apenasDigitos(l.cep)),
      enderecoCompleto: vazioParaNull(l.enderecoCompleto),
      pontoReferencia: vazioParaNull(l.pontoReferencia),
      observacoes: vazioParaNull(l.observacoes),
      latitude: temCoord ? l.latitude : null,
      longitude: temCoord ? l.longitude : null,
      ...(origem ? { origemCoordenada: origem } : {}),
      precisaoMetros: temCoord ? l.precisaoMetros : null,
    },
    rede: {
      subestacaoId: r.subestacaoId,
      conjuntoId: r.subestacaoId === null ? null : r.conjuntoId,
      transformadorNumero: vazioParaNull(r.transformadorNumero),
      trecho: vazioParaNull(r.trecho),
      equipamentoDescricao: vazioParaNull(r.equipamentoDescricao),
      identificadorEquipamento: vazioParaNull(r.identificadorEquipamento),
      chaveEletrica: vazioParaNull(r.chaveEletrica),
    },
    tipoManutencao: (values.tipoManutencao ?? undefined) as TipoManutencao | undefined,
    tipoOcorrenciaId: values.tipoOcorrenciaId ?? 0,
    impacto: {
      pessoasAfetadas: vazioParaNull(i.pessoasAfetadas),
      quantidadePessoas: i.quantidadePessoas,
      ucsAfetadas: vazioParaNull(i.ucsAfetadas),
      quantidadeUcs: i.quantidadeUcs,
      servicoEssencial: vazioParaNull(i.servicoEssencial),
      situacaoCliente: vazioParaNull(i.situacaoCliente),
      condicoesSeguranca: [...i.condicoesSeguranca],
      condicaoFornecimento: vazioParaNull(i.condicaoFornecimento),
      redundancia: vazioParaNull(i.redundancia),
      fonteReserva: vazioParaNull(i.fonteReserva),
      equipeEspecializada: vazioParaNull(i.equipeEspecializada),
      equipamentoAfetado: vazioParaNull(i.equipamentoAfetado),
      abrangencia: vazioParaNull(i.abrangencia),
      nivelRede: vazioParaNull(i.nivelRede),
      quantidadeEquipamentos: i.quantidadeEquipamentos,
      duracaoEstimadaMin: i.duracaoEstimadaMin,
      dataLimite: fromDateTimeLocalInput(i.dataLimite),
      recursosIds: [...i.recursosIds],
    },
    descricao: vazioParaNull(values.descricao),
    osExistenteId: values.osExistenteId,
    respostasPersonalizadas: values.respostasPersonalizadas
      .filter((x) => x.opcaoId !== null)
      .map((x) => ({ criterioId: x.criterioId, opcaoId: x.opcaoId as number })),
  };
}

/**
 * Payload da prévia: o mesmo mapeador; a descrição e o motivo (texto livre, que não pontuam)
 * recebem um marcador quando vazios, porque a API valida esses campos também na prévia.
 */
export function paraPreviaRequest(values: SolicitacaoFormValues, chave: string): NovaSolicitacaoRequest {
  const req = paraNovaSolicitacaoRequest(values, chave);
  return {
    ...req,
    descricao: req.descricao ?? 'Prévia de classificação (descrição ainda não informada).',
    motivoUcNaoInformada: req.ucNaoInformada ? (req.motivoUcNaoInformada ?? 'Motivo ainda não informado (prévia).') : null,
  };
}

/** Faixa de quantidade (mesmos limites das opções: até 10, 11–100, 101–500, 501–999, 1.000+). */
export function faixaDeQuantidade(n: number): string {
  if (n <= 10) return 'ATE_10';
  if (n <= 100) return 'DE_11_A_100';
  if (n <= 500) return 'DE_101_A_500';
  if (n <= 999) return 'DE_501_A_999';
  return 'MIL_OU_MAIS';
}
