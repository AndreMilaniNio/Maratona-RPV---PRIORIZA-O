import { z } from 'zod';
import { TipoManutencaoValores, type ClasseClienteDto, type ClasseSalvarRequest, type TipoOcorrenciaDto, type TipoOcorrenciaSalvarRequest } from '@/types/api';
import { codigoSchema } from '../lib/formUtils';

const ordemSchema = z.number({ invalid_type_error: 'Informe a ordem.' }).int('Número inteiro.').min(0, 'Ordem ≥ 0.');

// ---------- Classe de cliente ----------
export const classeSchema = z.object({
  codigo: codigoSchema,
  nome: z.string().trim().min(1, 'Informe o nome.').max(120),
  essencial: z.boolean(),
  ordem: ordemSchema,
  ativo: z.boolean(),
});
export type ClasseForm = z.infer<typeof classeSchema>;

export const classeParaForm = (c: ClasseClienteDto | null | undefined, proximaOrdem: number): ClasseForm => ({
  codigo: c?.codigo ?? '',
  nome: c?.nome ?? '',
  essencial: c?.essencial ?? false,
  ordem: c?.ordem ?? proximaOrdem,
  ativo: c?.ativo ?? true,
});
export const classeParaRequest = (f: ClasseForm): ClasseSalvarRequest => ({ ...f, codigo: f.codigo.trim(), nome: f.nome.trim() });

// ---------- Tipo de ocorrência ----------
export const tipoOcorrenciaSchema = z.object({
  codigo: codigoSchema,
  nome: z.string().trim().min(1, 'Informe o nome.').max(160),
  tipoManutencaoSugerido: z.enum(TipoManutencaoValores),
  ordem: ordemSchema,
  ativo: z.boolean(),
  qualificacoesIds: z.array(z.number().int().positive()),
});
export type TipoOcorrenciaForm = z.infer<typeof tipoOcorrenciaSchema>;

export const tipoOcorrenciaParaForm = (t: TipoOcorrenciaDto | null | undefined, proximaOrdem: number): TipoOcorrenciaForm => ({
  codigo: t?.codigo ?? '',
  nome: t?.nome ?? '',
  tipoManutencaoSugerido: t?.tipoManutencaoSugerido ?? 'Corretiva',
  ordem: t?.ordem ?? proximaOrdem,
  ativo: t?.ativo ?? true,
  qualificacoesIds: t?.qualificacoes.map((q) => q.id) ?? [],
});
export const tipoOcorrenciaParaRequest = (f: TipoOcorrenciaForm): TipoOcorrenciaSalvarRequest => ({
  ...f,
  codigo: f.codigo.trim(),
  nome: f.nome.trim(),
});
