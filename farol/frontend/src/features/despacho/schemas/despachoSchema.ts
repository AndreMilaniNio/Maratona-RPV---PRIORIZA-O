import { z } from 'zod';

/** Justificativa da designação: obrigatória para exceção e apoio intermunicipal. */
export const despachoSchema = z
  .object({
    justificativa: z.string().max(2000, 'Máximo de 2000 caracteres.'),
    exigeJustificativa: z.boolean(),
  })
  .superRefine((v, ctx) => {
    if (v.exigeJustificativa && v.justificativa.trim().length < 5) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['justificativa'],
        message: 'Informe a justificativa (mínimo de 5 caracteres) — exigida para exceção ou apoio intermunicipal.',
      });
    }
  });
