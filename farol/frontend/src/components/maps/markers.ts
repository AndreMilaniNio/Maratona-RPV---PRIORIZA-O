import L from 'leaflet';
import type { StatusEquipe } from '@/types/api';
import { corTextoSobre } from '@/lib/utils';

function esc(s: string): string {
  return s.replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]!);
}

/**
 * Marcador de OS: cor da prioridade (vinda da API) + código em texto.
 * OS críticas usam formato distinto (losango com borda grossa e "!").
 */
export function iconeOs(opts: { cor?: string | null; codigo?: string | null; critica?: boolean; selecionada?: boolean; atenuada?: boolean }): L.DivIcon {
  const cor = opts.cor ?? '#5b6b80';
  const txt = corTextoSobre(cor);
  const codigo = esc(opts.codigo ?? '?');
  const borda = opts.selecionada ? '3px solid #17365D' : '2px solid #ffffff';
  const opacidade = opts.atenuada ? 0.45 : 1;
  const sombra = opts.selecionada ? '0 0 0 3px rgba(36,87,166,.35)' : '0 1px 3px rgba(0,0,0,.35)';
  if (opts.critica) {
    const html = `<div style="opacity:${opacidade};width:30px;height:30px;display:flex;align-items:center;justify-content:center">
      <div style="width:24px;height:24px;transform:rotate(45deg);background:${cor};border:${borda};box-shadow:${sombra};display:flex;align-items:center;justify-content:center">
        <span style="transform:rotate(-45deg);color:${txt};font:700 10px/1 Segoe UI,Arial">${codigo}!</span>
      </div></div>`;
    return L.divIcon({ html, className: 'farol-marker', iconSize: [30, 30], iconAnchor: [15, 15], popupAnchor: [0, -14] });
  }
  const html = `<div style="opacity:${opacidade};background:${cor};color:${txt};border:${borda};box-shadow:${sombra};border-radius:999px;padding:2px 5px;font:700 10px/1.2 Segoe UI,Arial;white-space:nowrap;transform:translate(-50%,-50%);position:absolute;left:50%;top:50%">${codigo}</div>`;
  return L.divIcon({ html, className: 'farol-marker', iconSize: [34, 20], iconAnchor: [17, 10], popupAnchor: [0, -10] });
}

export const COR_STATUS_EQUIPE: Record<StatusEquipe, string> = {
  Disponivel: '#2f6b2f',
  ACaminho: '#2457a6',
  EmAtendimento: '#b54708',
  Indisponivel: '#6b7280',
  EmPausa: '#8a6d1d',
  DeslocamentoOutraAtividade: '#5b4b8a',
};

const SIGLA_STATUS_EQUIPE: Record<StatusEquipe, string> = {
  Disponivel: 'D',
  ACaminho: 'C',
  EmAtendimento: 'E',
  Indisponivel: 'I',
  EmPausa: 'P',
  DeslocamentoOutraAtividade: 'O',
};

/** Marcador de equipe: quadrado com a cor e a sigla do status + código da equipe. */
export function iconeEquipe(opts: { status: StatusEquipe; codigo?: string | null; destacada?: boolean; recomendada?: boolean; demonstrativa?: boolean }): L.DivIcon {
  const cor = COR_STATUS_EQUIPE[opts.status] ?? '#6b7280';
  const borda = opts.recomendada ? '3px solid #667A45' : opts.destacada ? '3px solid #17365D' : '2px solid #fff';
  const tracejado = opts.demonstrativa ? 'outline:1px dashed #7a5a00;outline-offset:1px;' : '';
  const html = `<div style="display:flex;align-items:center;gap:2px;transform:translate(-12px,-12px)">
    <div style="width:22px;height:22px;border-radius:4px;background:${cor};border:${borda};${tracejado}box-shadow:0 1px 3px rgba(0,0,0,.35);color:#fff;font:700 11px/22px Segoe UI,Arial;text-align:center">${SIGLA_STATUS_EQUIPE[opts.status] ?? '?'}</div>
    <span style="background:#fff;border:1px solid #DCE3EC;border-radius:3px;padding:0 3px;font:600 10px/14px Segoe UI,Arial;color:#243247;white-space:nowrap">${esc(opts.codigo ?? '')}</span>
  </div>`;
  return L.divIcon({ html, className: 'farol-marker', iconSize: [24, 24], iconAnchor: [0, 0], popupAnchor: [0, -12] });
}

/** Marcador de subestação (circuito): triângulo azul-escuro. */
export function iconeSubestacao(codigo?: string | null): L.DivIcon {
  const html = `<div style="transform:translate(-9px,-9px)" title="${esc(codigo ?? '')}">
    <svg width="18" height="18" viewBox="0 0 18 18"><polygon points="9,1 17,16 1,16" fill="#17365D" stroke="#fff" stroke-width="1.5"/><text x="9" y="14" font-size="8" text-anchor="middle" fill="#fff" font-family="Arial" font-weight="700">S</text></svg>
  </div>`;
  return L.divIcon({ html, className: 'farol-marker', iconSize: [18, 18], iconAnchor: [0, 0] });
}

/** Marcador simples (pino) para seleção de ponto em formulários. */
export function iconePino(cor = '#2457a6'): L.DivIcon {
  const html = `<div style="transform:translate(-12px,-24px)"><svg width="24" height="24" viewBox="0 0 24 24"><path d="M12 23s7-7.6 7-13A7 7 0 0 0 5 10c0 5.4 7 13 7 13z" fill="${cor}" stroke="#fff" stroke-width="1.5"/><circle cx="12" cy="10" r="2.6" fill="#fff"/></svg></div>`;
  return L.divIcon({ html, className: 'farol-marker', iconSize: [24, 24], iconAnchor: [0, 0], popupAnchor: [0, -24] });
}
