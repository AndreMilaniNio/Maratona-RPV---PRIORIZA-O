import { z } from 'zod';
import { StatusEquipeValores, type EquipeDto, type EquipeSalvarRequest, type StatusEquipe } from '@/types/api';

const textoOpcional = z.string().trim().max(120, 'Máximo de 120 caracteres.');

export const integranteSchema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome do integrante.').max(150, 'Máximo de 150 caracteres.'),
  matricula: textoOpcional,
  funcao: textoOpcional,
});

/** Cadastro/edição de equipe (EquipeSalvarRequest). Regras espelham as validações do servidor. */
export const equipeFormSchema = z.object({
  codigo: z.string().trim().min(1, 'Informe o código operacional.').max(40, 'Máximo de 40 caracteres.'),
  nome: z.string().trim().min(1, 'Informe o nome da equipe.').max(150, 'Máximo de 150 caracteres.'),
  municipioBaseId: z.number({ invalid_type_error: 'Selecione o município-base.' }).int().positive('Selecione o município-base.'),
  municipiosAdicionaisIds: z.array(z.number().int()),
  status: z.enum(StatusEquipeValores),
  capacidade: z
    .number({ invalid_type_error: 'Informe a capacidade.' })
    .int('Use um número inteiro.')
    .min(1, 'A capacidade mínima é 1.')
    .max(10, 'A capacidade máxima é 10.'),
  integrantes: z.array(integranteSchema).min(1, 'A equipe precisa de ao menos um integrante.'),
  qualificacoesIds: z.array(z.number().int()),
  recursosIds: z.array(z.number().int()),
  ativa: z.boolean(),
});

export type EquipeFormValues = z.infer<typeof equipeFormSchema>;

export const CAMPOS_EQUIPE_FORM = [
  'codigo',
  'nome',
  'municipioBaseId',
  'municipiosAdicionaisIds',
  'status',
  'capacidade',
  'integrantes',
  'qualificacoesIds',
  'recursosIds',
  'ativa',
] as const;

export function equipeFormInicial(e?: EquipeDto | null, municipioPadrao?: number | null): EquipeFormValues {
  if (!e) {
    return {
      codigo: '',
      nome: '',
      municipioBaseId: municipioPadrao ?? 0,
      municipiosAdicionaisIds: [],
      status: 'Disponivel',
      capacidade: 1,
      integrantes: [{ nome: '', matricula: '', funcao: '' }],
      qualificacoesIds: [],
      recursosIds: [],
      ativa: true,
    };
  }
  return {
    codigo: e.codigo ?? '',
    nome: e.nome ?? '',
    municipioBaseId: e.municipioBaseId,
    municipiosAdicionaisIds: e.municipiosAdicionais.map((m) => m.id),
    status: e.status,
    capacidade: e.capacidade,
    integrantes: e.integrantes.map((i) => ({ nome: i.nome ?? '', matricula: i.matricula ?? '', funcao: i.funcao ?? '' })),
    qualificacoesIds: e.qualificacoes.map((q) => q.id),
    recursosIds: e.recursos.map((r) => r.id),
    ativa: e.ativa,
  };
}

const vazioParaNull = (v: string) => (v.trim() === '' ? null : v.trim());

export function paraEquipeRequest(v: EquipeFormValues): EquipeSalvarRequest {
  return {
    codigo: v.codigo.trim(),
    nome: v.nome.trim(),
    municipioBaseId: v.municipioBaseId,
    municipiosAdicionaisIds: v.municipiosAdicionaisIds.filter((m) => m !== v.municipioBaseId),
    status: v.status,
    capacidade: v.capacidade,
    integrantes: v.integrantes.map((i) => ({ nome: i.nome.trim(), matricula: vazioParaNull(i.matricula), funcao: vazioParaNull(i.funcao) })),
    qualificacoesIds: v.qualificacoesIds,
    recursosIds: v.recursosIds,
    ativa: v.ativa,
  };
}

/** Status que o servidor exige justificativa ao mudar a equipe para eles. */
export const STATUS_EQUIPE_EXIGE_JUSTIFICATIVA: StatusEquipe[] = ['Indisponivel', 'EmPausa'];

export const statusEquipeFormSchema = z
  .object({
    status: z.enum(StatusEquipeValores),
    justificativa: z.string().trim().max(500, 'Máximo de 500 caracteres.'),
  })
  .superRefine((v, ctx) => {
    if (STATUS_EQUIPE_EXIGE_JUSTIFICATIVA.includes(v.status) && v.justificativa.length < 5) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ['justificativa'], message: 'Informe a justificativa (mínimo de 5 caracteres).' });
    }
  });

export type StatusEquipeFormValues = z.infer<typeof statusEquipeFormSchema>;

/** Converte texto digitado (aceita vírgula decimal) em número; vazio/inválido → NaN. */
export function paraNumero(v: string): number {
  const t = v.trim().replace(',', '.');
  return t === '' ? Number.NaN : Number(t);
}

export const localizacaoFormSchema = z.object({
  latitude: z
    .string()
    .trim()
    .min(1, 'Informe a latitude.')
    .refine((v) => Number.isFinite(paraNumero(v)), 'Latitude inválida.')
    .refine((v) => Math.abs(paraNumero(v)) <= 90, 'Latitude entre -90 e 90.'),
  longitude: z
    .string()
    .trim()
    .min(1, 'Informe a longitude.')
    .refine((v) => Number.isFinite(paraNumero(v)), 'Longitude inválida.')
    .refine((v) => Math.abs(paraNumero(v)) <= 180, 'Longitude entre -180 e 180.'),
  origem: z.enum(['Cadastrada', 'Gps']),
});

export type LocalizacaoFormValues = z.infer<typeof localizacaoFormSchema>;

/** Código de qualificação/recurso: mesmo padrão do servidor (^[A-Z0-9_]{2,40}$). */
export const itemEquipeSchema = z.object({
  codigo: z
    .string()
    .trim()
    .regex(/^[A-Z0-9_]{2,40}$/, 'Use 2 a 40 caracteres: MAIÚSCULAS, dígitos e _ (ex.: TRABALHO_ALTURA).'),
  nome: z.string().trim().min(1, 'Informe o nome.').max(150, 'Máximo de 150 caracteres.'),
});

export type ItemEquipeFormValues = z.infer<typeof itemEquipeSchema>;
