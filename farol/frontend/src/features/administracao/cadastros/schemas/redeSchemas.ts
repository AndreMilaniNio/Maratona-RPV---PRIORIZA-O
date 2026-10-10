import { z } from 'zod';
import type {
  ConjuntoSalvarRequest,
  LocalidadeSalvarRequest,
  MunicipioDto,
  MunicipioSalvarRequest,
  SubestacaoDto,
  SubestacaoSalvarRequest,
  TransformadorSalvarRequest,
} from '@/types/api';
import { numeroOpcional, numeroParaTexto, paraNumeroOuNull, textoOuNull, validarCoordenadas } from '../lib/formUtils';
import { TAMANHO_MINIMO_TRANSFORMADOR } from '../lib/transformador';

const municipioIdSchema = z.number({ invalid_type_error: 'Selecione o município.' }).int().positive('Selecione o município.');

// ---------- Município ----------
export const municipioSchema = z
  .object({
    nome: z.string().trim().min(1, 'Informe o nome.').max(120),
    uf: z
      .string()
      .trim()
      .regex(/^[A-Za-z]{2}$/, 'UF com 2 letras.'),
    codigoIbge: z
      .string()
      .trim()
      .refine((v) => v === '' || /^\d{7}$/.test(v), 'Código IBGE tem 7 dígitos.'),
    prefixo: z
      .string()
      .trim()
      .regex(/^[A-Z0-9]{2,6}$/, 'Prefixo: 2 a 6 letras maiúsculas ou dígitos.'),
    latitude: numeroOpcional(),
    longitude: numeroOpcional(),
    raioKm: numeroOpcional().refine((v) => {
      const n = paraNumeroOuNull(v);
      return n === null || n > 0;
    }, 'O raio deve ser positivo.'),
    cepUnico: z
      .string()
      .trim()
      .refine((v) => v === '' || /^\d{8}$/.test(v.replace(/\D/g, '')), 'CEP único com 8 dígitos.'),
    ativo: z.boolean(),
  })
  .superRefine((v, ctx) => validarCoordenadas(v.latitude, v.longitude, ctx));

export type MunicipioForm = z.infer<typeof municipioSchema>;

export function municipioParaForm(m?: MunicipioDto | null): MunicipioForm {
  return {
    nome: m?.nome ?? '',
    uf: m?.uf ?? '',
    codigoIbge: m?.codigoIbge ?? '',
    prefixo: m?.prefixo ?? '',
    latitude: numeroParaTexto(m?.latitude),
    longitude: numeroParaTexto(m?.longitude),
    raioKm: numeroParaTexto(m?.raioKm),
    cepUnico: m?.cepUnico ?? '',
    ativo: m?.ativo ?? true,
  };
}

export function municipioParaRequest(f: MunicipioForm): MunicipioSalvarRequest {
  const cep = f.cepUnico.replace(/\D/g, '');
  return {
    nome: f.nome.trim(),
    uf: f.uf.trim().toUpperCase(),
    codigoIbge: textoOuNull(f.codigoIbge),
    prefixo: f.prefixo.trim(),
    latitude: paraNumeroOuNull(f.latitude),
    longitude: paraNumeroOuNull(f.longitude),
    raioKm: paraNumeroOuNull(f.raioKm),
    cepUnico: cep === '' ? null : cep,
    ativo: f.ativo,
  };
}

/** Requisição completa a partir do DTO, com outra situação (inativar sem abrir o formulário). */
export function municipioComAtivo(m: MunicipioDto, ativo: boolean): MunicipioSalvarRequest {
  return { ...municipioParaRequest(municipioParaForm(m)), ativo };
}

// ---------- Localidade ----------
export const localidadeSchema = z.object({
  codigo: z
    .string()
    .trim()
    .regex(/^\d{3}$/, 'O código da localidade tem exatamente 3 dígitos (zeros à esquerda contam).'),
  nome: z.string().trim().min(1, 'Informe o nome.').max(120),
  municipioId: municipioIdSchema,
});
export type LocalidadeForm = z.infer<typeof localidadeSchema>;
export const localidadeParaRequest = (f: LocalidadeForm): LocalidadeSalvarRequest => ({
  codigo: f.codigo.trim(),
  nome: f.nome.trim(),
  municipioId: f.municipioId,
});

// ---------- Subestação (circuito) ----------
export const subestacaoSchema = z
  .object({
    codigo: z.string().trim().min(1, 'Informe o código.').max(40),
    nome: z.string().trim().min(1, 'Informe o nome.').max(120),
    local: z.string().trim().max(250),
    municipioId: municipioIdSchema,
    latitude: numeroOpcional(),
    longitude: numeroOpcional(),
  })
  .superRefine((v, ctx) => validarCoordenadas(v.latitude, v.longitude, ctx));
export type SubestacaoForm = z.infer<typeof subestacaoSchema>;

export function subestacaoParaForm(s: SubestacaoDto | null | undefined, municipioId: number | null): SubestacaoForm {
  return {
    codigo: s?.codigo ?? '',
    nome: s?.nome ?? '',
    local: s?.local ?? '',
    municipioId: s?.municipioId ?? municipioId ?? 0,
    latitude: numeroParaTexto(s?.latitude),
    longitude: numeroParaTexto(s?.longitude),
  };
}
export const subestacaoParaRequest = (f: SubestacaoForm): SubestacaoSalvarRequest => ({
  codigo: f.codigo.trim(),
  nome: f.nome.trim(),
  local: textoOuNull(f.local),
  municipioId: f.municipioId,
  latitude: paraNumeroOuNull(f.latitude),
  longitude: paraNumeroOuNull(f.longitude),
});

// ---------- Conjunto elétrico ----------
export const conjuntoSchema = z.object({
  subestacaoId: z.number({ invalid_type_error: 'Selecione a subestação.' }).int().positive('Selecione a subestação.'),
  numero: z.string().trim().min(1, 'Informe o número do conjunto.').max(40),
});
export type ConjuntoForm = z.infer<typeof conjuntoSchema>;
export const conjuntoParaRequest = (f: ConjuntoForm): ConjuntoSalvarRequest => ({ subestacaoId: f.subestacaoId, numero: f.numero.trim() });

// ---------- Transformador ----------
/** Esquema do transformador; o tamanho máximo vem do catálogo (`configuracao.transformadorTamanhoMaximo`). */
export function transformadorSchema(tamanhoMaximo: number) {
  return z
    .object({
      numero: z
        .string()
        .trim()
        .regex(/^\d*$/, 'Somente dígitos.')
        .min(TAMANHO_MINIMO_TRANSFORMADOR, `Mínimo de ${TAMANHO_MINIMO_TRANSFORMADOR} dígitos (3 da localidade + equipamento).`)
        .max(tamanhoMaximo, `Máximo de ${tamanhoMaximo} dígitos.`),
      municipioId: municipioIdSchema,
      subestacaoId: z.number().int().nonnegative(),
      conjuntoId: z.number().int().nonnegative(),
      latitude: numeroOpcional(),
      longitude: numeroOpcional(),
    })
    .superRefine((v, ctx) => {
      validarCoordenadas(v.latitude, v.longitude, ctx);
      if (v.conjuntoId > 0 && v.subestacaoId === 0)
        ctx.addIssue({ code: z.ZodIssueCode.custom, path: ['subestacaoId'], message: 'O conjunto exige a subestação.' });
    });
}
export type TransformadorForm = z.infer<ReturnType<typeof transformadorSchema>>;

export const transformadorParaRequest = (f: TransformadorForm): TransformadorSalvarRequest => ({
  numero: f.numero.trim(),
  municipioId: f.municipioId,
  subestacaoId: f.subestacaoId > 0 ? f.subestacaoId : null,
  conjuntoId: f.conjuntoId > 0 ? f.conjuntoId : null,
  latitude: paraNumeroOuNull(f.latitude),
  longitude: paraNumeroOuNull(f.longitude),
});
