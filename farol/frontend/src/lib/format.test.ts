import { formatDateTime, formatDuration, formatPrazoRestante, formatCep, maskCep, LIMITE_PROXIMO_MIN } from '@/lib/format';

describe('formatDuration', () => {
  it('formata minutos, horas e dias', () => {
    expect(formatDuration(0.4)).toBe('menos de 1 min');
    expect(formatDuration(45)).toBe('45 min');
    expect(formatDuration(60)).toBe('1 h');
    expect(formatDuration(80)).toBe('1 h 20 min');
    expect(formatDuration(1440 + 180)).toBe('1 d 3 h');
    expect(formatDuration(2880)).toBe('2 d');
  });
});

describe('formatPrazoRestante', () => {
  const agora = new Date('2026-10-10T14:00:00Z');

  it('prazo futuro: "Restam …"', () => {
    const r = formatPrazoRestante({ limite: '2026-10-10T16:30:00Z' }, agora);
    expect(r.texto).toBe('Restam 2 h 30 min');
    expect(r.estado).toBe('ok');
  });

  it('prazo próximo do vencimento', () => {
    const r = formatPrazoRestante({ limite: '2026-10-10T14:18:00Z' }, agora);
    expect(r.texto).toBe('Restam 18 min');
    expect(r.estado).toBe('proximo');
    expect(LIMITE_PROXIMO_MIN).toBeGreaterThanOrEqual(18);
  });

  it('prazo vencido: "Vencido há …"', () => {
    const r = formatPrazoRestante({ limite: '2026-10-10T12:40:00Z', vencido: true }, agora);
    expect(r.texto).toBe('Vencido há 1 h 20 min');
    expect(r.estado).toBe('vencido');
  });

  it('usa minutosRestantes quando não há limite', () => {
    expect(formatPrazoRestante({ minutosRestantes: -5 }, agora).texto).toBe('Vencido há 5 min');
  });

  it('sem prazo', () => {
    expect(formatPrazoRestante(null, agora)).toEqual({ texto: 'Sem prazo', estado: 'sem-prazo', minutos: null });
  });
});

describe('datas no fuso operacional', () => {
  it('converte UTC para America/Sao_Paulo', () => {
    // 14:01 UTC = 11:01 em São Paulo (UTC-3)
    expect(formatDateTime('2026-10-10T14:01:54Z')).toBe('10/10/2026 11:01');
  });
  it('trata valor ausente', () => {
    expect(formatDateTime(null)).toBe('—');
  });
});

describe('CEP', () => {
  it('formata e mascara', () => {
    expect(formatCep('00110404')).toBe('00110-404');
    expect(maskCep('00110abc404999')).toBe('00110-404');
    expect(maskCep('0011')).toBe('0011');
  });
});
