import { z } from 'zod';
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form';
import { ApiError } from '@/services/api/client';

/** Códigos administráveis seguem o padrão do backend: MAIÚSCULAS, dígitos e _ (2 a 40). */
export const CODIGO_REGEX = /^[A-Z0-9_]{2,40}$/;
export const codigoSchema = z
  .string()
  .trim()
  .regex(CODIGO_REGEX, 'Use 2 a 40 caracteres: letras maiúsculas, dígitos e _ (ex.: RISCO_VIDA).');

/** Converte texto digitado (aceita vírgula decimal) em número; vazio → null; inválido → NaN. */
export function paraNumeroOuNull(v: string | null | undefined): number | null {
  const t = (v ?? '').trim().replace(',', '.');
  if (t === '') return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : Number.NaN;
}

/** Número opcional digitado como texto (campos de coordenada, raio etc.). */
export function numeroOpcional(mensagem = 'Informe um número válido.') {
  return z
    .string()
    .trim()
    .refine((v) => !Number.isNaN(paraNumeroOuNull(v)), mensagem);
}

/** Texto para inputs a partir de um número opcional da API. */
export function numeroParaTexto(n: number | null | undefined): string {
  return n === null || n === undefined ? '' : String(n);
}

/** Valida o par latitude/longitude (ambos ou nenhum, dentro dos limites WGS84). */
export function validarCoordenadas(
  lat: string,
  lng: string,
  ctx: z.RefinementCtx,
  campos: { lat: string; lng: string } = { lat: 'latitude', lng: 'longitude' },
) {
  const a = paraNumeroOuNull(lat);
  const b = paraNumeroOuNull(lng);
  if ((a === null) !== (b === null)) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, path: [a === null ? campos.lat : campos.lng], message: 'Informe latitude e longitude juntas.' });
    return;
  }
  if (a !== null && !Number.isNaN(a) && (a < -90 || a > 90))
    ctx.addIssue({ code: z.ZodIssueCode.custom, path: [campos.lat], message: 'Latitude entre -90 e 90.' });
  if (b !== null && !Number.isNaN(b) && (b < -180 || b > 180))
    ctx.addIssue({ code: z.ZodIssueCode.custom, path: [campos.lng], message: 'Longitude entre -180 e 180.' });
}

/** Texto opcional: vazio → null. */
export function textoOuNull(v: string | null | undefined): string | null {
  const t = (v ?? '').trim();
  return t === '' ? null : t;
}

/**
 * Leva os erros da API para os campos do formulário.
 * - 400 com `errors` (chaves camelCase): cada chave conhecida vira erro do campo.
 * - 409 (conflito, ex.: "Código já cadastrado"): vai para o campo `conflito`, se informado.
 * Devolve `true` quando tudo foi exibido nos campos (não precisa de caixa de erro geral).
 */
export function aplicarErrosApi<T extends FieldValues>(
  err: unknown,
  setError: UseFormSetError<T>,
  campos: readonly string[],
  conflito?: Path<T>,
): boolean {
  if (!(err instanceof ApiError)) return false;
  if (err.status === 409 && conflito) {
    setError(conflito, { type: 'server', message: err.message });
    return true;
  }
  if (err.status !== 400) return false;
  const entradas = Object.entries(err.fieldErrors);
  if (entradas.length === 0) return false;
  let todos = true;
  for (const [chave, msgs] of entradas) {
    const raiz = chave.split('.')[0].split('[')[0];
    const alvo = campos.includes(chave) ? chave : campos.includes(raiz) ? raiz : null;
    if (alvo) setError(alvo as Path<T>, { type: 'server', message: msgs.join(' ') });
    else todos = false;
  }
  return todos;
}
