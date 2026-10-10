import * as React from 'react';
import type { CampoOrdenacao } from '@/features/ordens-servico/schemas/filtroFila';

export type ColunaId =
  | 'posicao'
  | 'numero'
  | 'prioridade'
  | 'pontuacao'
  | 'manutencao'
  | 'ocorrencia'
  | 'endereco'
  | 'municipio'
  | 'uc'
  | 'classe'
  | 'rede'
  | 'afetados'
  | 'risco'
  | 'abertura'
  | 'prazo'
  | 'restante'
  | 'status'
  | 'equipe'
  | 'acoes';

export interface ColunaDef {
  id: ColunaId;
  rotulo: string;
  /** Essenciais ficam sempre visíveis (seção 5.2). */
  essencial?: boolean;
  /** Campo de OrdenarPor no servidor. */
  ordenacao?: CampoOrdenacao;
  /** Oculta por padrão (pode ser exibida pelo usuário). */
  ocultaPorPadrao?: boolean;
  className?: string;
}

export const COLUNAS: ColunaDef[] = [
  { id: 'posicao', rotulo: 'Pos.', essencial: true, ordenacao: 'fila', className: 'w-12 text-right' },
  { id: 'numero', rotulo: 'Número', essencial: true, ordenacao: 'numero' },
  { id: 'prioridade', rotulo: 'Prioridade', essencial: true },
  { id: 'pontuacao', rotulo: 'Pontos', ordenacao: 'pontuacao', className: 'text-right' },
  { id: 'manutencao', rotulo: 'Manutenção' },
  { id: 'ocorrencia', rotulo: 'Ocorrência', essencial: true },
  { id: 'endereco', rotulo: 'Endereço / trecho', essencial: true },
  { id: 'municipio', rotulo: 'Município', ordenacao: 'municipio' },
  { id: 'uc', rotulo: 'UC' },
  { id: 'classe', rotulo: 'Classe', ocultaPorPadrao: true },
  { id: 'rede', rotulo: 'Circuito / conjunto / trafo', ocultaPorPadrao: true },
  { id: 'afetados', rotulo: 'Pessoas / UCs' },
  { id: 'risco', rotulo: 'Risco' },
  { id: 'abertura', rotulo: 'Abertura', ordenacao: 'abertura' },
  { id: 'prazo', rotulo: 'Prazo', ordenacao: 'prazo' },
  { id: 'restante', rotulo: 'Tempo restante', essencial: true },
  { id: 'status', rotulo: 'Status', essencial: true },
  { id: 'equipe', rotulo: 'Equipe', essencial: true },
  { id: 'acoes', rotulo: 'Ações', essencial: true },
];

export const COLUNAS_SECUNDARIAS = COLUNAS.filter((c) => !c.essencial);

const CHAVE_STORAGE = 'farol.painel.colunasOcultas';
const OCULTAS_PADRAO: ColunaId[] = COLUNAS.filter((c) => c.ocultaPorPadrao).map((c) => c.id);

function lerOcultas(): ColunaId[] {
  try {
    const bruto = localStorage.getItem(CHAVE_STORAGE);
    if (!bruto) return OCULTAS_PADRAO;
    const lista: unknown = JSON.parse(bruto);
    if (!Array.isArray(lista)) return OCULTAS_PADRAO;
    const validas = new Set<string>(COLUNAS_SECUNDARIAS.map((c) => c.id));
    return lista.filter((x): x is ColunaId => typeof x === 'string' && validas.has(x));
  } catch {
    return OCULTAS_PADRAO;
  }
}

/**
 * Colunas secundárias ocultas pelo usuário (preferência de interface, em localStorage).
 * "Município" só aparece na visão "Todas as cidades".
 */
export function useColunasVisiveis(todasAsCidades: boolean) {
  const [ocultas, setOcultas] = React.useState<ColunaId[]>(lerOcultas);

  const alternar = React.useCallback((id: ColunaId, visivel: boolean) => {
    setOcultas((atual) => {
      const novo = visivel ? atual.filter((x) => x !== id) : [...new Set([...atual, id])];
      try {
        localStorage.setItem(CHAVE_STORAGE, JSON.stringify(novo));
      } catch {
        /* preferência só em memória */
      }
      return novo;
    });
  }, []);

  const restaurar = React.useCallback(() => {
    setOcultas(OCULTAS_PADRAO);
    try {
      localStorage.removeItem(CHAVE_STORAGE);
    } catch {
      /* ignore */
    }
  }, []);

  const visiveis = React.useMemo(
    () => COLUNAS.filter((c) => (c.id === 'municipio' ? todasAsCidades && !ocultas.includes(c.id) : c.essencial || !ocultas.includes(c.id))),
    [ocultas, todasAsCidades],
  );

  return { visiveis, ocultas, alternar, restaurar };
}
