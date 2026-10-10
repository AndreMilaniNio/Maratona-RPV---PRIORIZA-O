/** Quantidade de dígitos que identificam a localidade no número do transformador (seção 3). */
export const DIGITOS_LOCALIDADE = 3;
export const TAMANHO_MINIMO_TRANSFORMADOR = DIGITOS_LOCALIDADE + 1;

export interface NumeroTransformadorDividido {
  /** 3 primeiros dígitos: código da localidade. */
  localidade: string;
  /** Dígitos restantes: número do equipamento na localidade. */
  local: string;
}

/**
 * Divide o número completo (texto, zeros à esquerda preservados) em localidade + número local.
 * Devolve null enquanto o número não tiver ao menos 4 dígitos ou contiver algo além de dígitos.
 * Apenas apresentação — o backend é quem valida e grava a divisão.
 */
export function dividirNumeroTransformador(numero: string | null | undefined): NumeroTransformadorDividido | null {
  const v = (numero ?? '').trim();
  if (v.length < TAMANHO_MINIMO_TRANSFORMADOR || !/^\d+$/.test(v)) return null;
  return { localidade: v.slice(0, DIGITOS_LOCALIDADE), local: v.slice(DIGITOS_LOCALIDADE) };
}
