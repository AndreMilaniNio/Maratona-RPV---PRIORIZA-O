import { FUSO_OPERACIONAL } from '@/app/config/env';

type DataEntrada = string | number | Date | null | undefined;

function toDate(v: DataEntrada): Date | null {
  if (v === null || v === undefined || v === '') return null;
  const d = v instanceof Date ? v : new Date(v);
  return Number.isNaN(d.getTime()) ? null : d;
}

const fmtDataHora = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO_OPERACIONAL,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});
const fmtDataHoraSeg = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO_OPERACIONAL,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
});
const fmtData = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO_OPERACIONAL,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
});
const fmtHora = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO_OPERACIONAL,
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
});
const fmtDataCurta = new Intl.DateTimeFormat('pt-BR', {
  timeZone: FUSO_OPERACIONAL,
  day: '2-digit',
  month: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
});

/** "10/10/2026 11:01" no fuso operacional. */
export function formatDateTime(v: DataEntrada, comSegundos = false): string {
  const d = toDate(v);
  if (!d) return '—';
  return (comSegundos ? fmtDataHoraSeg : fmtDataHora).format(d).replace(',', '');
}

/** "10/10 11:01" — usado em tabelas densas. */
export function formatDateTimeShort(v: DataEntrada): string {
  const d = toDate(v);
  return d ? fmtDataCurta.format(d).replace(',', '') : '—';
}

/** "10/10/2026" no fuso operacional. Datas puras (yyyy-mm-dd) são exibidas sem conversão. */
export function formatDate(v: DataEntrada): string {
  if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v)) {
    const [y, m, d] = v.split('-');
    return `${d}/${m}/${y}`;
  }
  const d = toDate(v);
  return d ? fmtData.format(d) : '—';
}

/** "hh:mm:ss" no fuso operacional. */
export function formatTime(v: DataEntrada): string {
  const d = toDate(v);
  return d ? fmtHora.format(d) : '—';
}

/** Duração em minutos → "45 min", "1 h 20 min", "2 d 3 h". */
export function formatDuration(minutos: number): string {
  const total = Math.max(0, Math.round(Math.abs(minutos)));
  if (total < 1) return 'menos de 1 min';
  const dias = Math.floor(total / 1440);
  const horas = Math.floor((total % 1440) / 60);
  const min = total % 60;
  if (dias > 0) return horas > 0 ? `${dias} d ${horas} h` : `${dias} d`;
  if (horas > 0) return min > 0 ? `${horas} h ${min} min` : `${horas} h`;
  return `${min} min`;
}

export type EstadoPrazo = 'ok' | 'proximo' | 'vencido' | 'sem-prazo';

export interface PrazoFormatado {
  texto: string;
  estado: EstadoPrazo;
  minutos: number | null;
}

/** Limite (em minutos) abaixo do qual o prazo é considerado "próximo do vencimento". */
export const LIMITE_PROXIMO_MIN = 30;

/**
 * Tempo restante ou atraso de um prazo.
 * Usa o instante-limite (UTC) e o relógio atual, para continuar correto entre atualizações.
 */
export function formatPrazoRestante(
  prazo: { limite?: string | null; vencido?: boolean; minutosRestantes?: number | null } | null | undefined,
  agora: Date = new Date(),
): PrazoFormatado {
  if (!prazo || (!prazo.limite && prazo.minutosRestantes == null)) {
    return { texto: 'Sem prazo', estado: 'sem-prazo', minutos: null };
  }
  const limite = toDate(prazo.limite ?? null);
  const minutos = limite ? (limite.getTime() - agora.getTime()) / 60000 : (prazo.minutosRestantes ?? 0);
  if (minutos <= 0) {
    return { texto: `Vencido há ${formatDuration(-minutos)}`, estado: 'vencido', minutos };
  }
  return {
    texto: `Restam ${formatDuration(minutos)}`,
    estado: minutos <= LIMITE_PROXIMO_MIN ? 'proximo' : 'ok',
    minutos,
  };
}

const fmtNum = new Intl.NumberFormat('pt-BR');
export function formatNumber(n: number | null | undefined, casas?: number): string {
  if (n === null || n === undefined || Number.isNaN(n)) return '—';
  if (casas !== undefined) {
    return new Intl.NumberFormat('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas }).format(n);
  }
  return fmtNum.format(n);
}

export function formatDistanciaKm(km: number | null | undefined): string {
  if (km === null || km === undefined) return '—';
  if (km < 1) return `${formatNumber(Math.round(km * 1000))} m`;
  return `${formatNumber(km, 1)} km`;
}

export function formatCoordenada(lat: number | null | undefined, lng: number | null | undefined): string {
  if (lat === null || lat === undefined || lng === null || lng === undefined) return '—';
  return `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
}

/** CEP "00110404" → "00110-404". */
export function formatCep(cep: string | null | undefined): string {
  if (!cep) return '—';
  const d = cep.replace(/\D/g, '');
  return d.length === 8 ? `${d.slice(0, 5)}-${d.slice(5)}` : cep;
}

/** Máscara de digitação de CEP. */
export function maskCep(v: string): string {
  const d = v.replace(/\D/g, '').slice(0, 8);
  return d.length > 5 ? `${d.slice(0, 5)}-${d.slice(5)}` : d;
}

export function formatBytes(n: number | null | undefined): string {
  if (!n && n !== 0) return '—';
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${formatNumber(n / 1024, 1)} KB`;
  return `${formatNumber(n / 1024 / 1024, 1)} MB`;
}

/** Converte um instante UTC para o valor de <input type="datetime-local"> no horário local do navegador. */
export function toDateTimeLocalInput(v: DataEntrada): string {
  const d = toDate(v);
  if (!d) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** Converte o valor de <input type="datetime-local"> para ISO UTC. */
export function fromDateTimeLocalInput(v: string | null | undefined): string | null {
  if (!v) return null;
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? null : d.toISOString();
}
