import type { EquipeDto, StatusEquipe } from '@/types/api';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';

/**
 * Explica por que a equipe não está disponível. A disponibilidade em si vem da API
 * (`disponivel`); aqui só se descreve o motivo para o operador, na mesma ordem de
 * verificação do servidor (inativa → capacidade → status).
 */
export function motivoIndisponibilidade(e: Pick<EquipeDto, 'disponivel' | 'ativa' | 'status' | 'despachosAtivos' | 'capacidade'>): string | null {
  if (e.disponivel) return null;
  if (!e.ativa) return 'Equipe inativa';
  if (e.despachosAtivos >= e.capacidade) return `Capacidade esgotada (${e.despachosAtivos}/${e.capacidade})`;
  if (e.status !== 'Disponivel') return STATUS_EQUIPE_LABEL[e.status] ?? e.status;
  return 'Indisponível';
}

export type FiltroDisponibilidade = 'todas' | 'disponiveis' | 'indisponiveis';

export interface FiltroEquipes {
  busca: string;
  status: StatusEquipe | '';
  disponibilidade: FiltroDisponibilidade;
}

export const FILTRO_EQUIPES_INICIAL: FiltroEquipes = { busca: '', status: '', disponibilidade: 'todas' };

function normalizar(s: string): string {
  return s
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim();
}

/** Busca textual (código, nome, município, integrantes, qualificações, recursos) + filtros. */
export function filtrarEquipes(lista: EquipeDto[], f: FiltroEquipes): EquipeDto[] {
  const termo = normalizar(f.busca);
  return lista.filter((e) => {
    if (f.status && e.status !== f.status) return false;
    if (f.disponibilidade === 'disponiveis' && !e.disponivel) return false;
    if (f.disponibilidade === 'indisponiveis' && e.disponivel) return false;
    if (!termo) return true;
    const campos = [
      e.codigo,
      e.nome,
      e.municipioBase,
      ...e.municipiosAdicionais.map((m) => m.nome),
      ...e.integrantes.flatMap((i) => [i.nome, i.matricula]),
      ...e.qualificacoes.flatMap((q) => [q.codigo, q.nome]),
      ...e.recursos.flatMap((r) => [r.codigo, r.nome]),
      ...e.osAtribuidas.map((o) => o.numero),
    ];
    return campos.some((c) => c && normalizar(c).includes(termo));
  });
}
