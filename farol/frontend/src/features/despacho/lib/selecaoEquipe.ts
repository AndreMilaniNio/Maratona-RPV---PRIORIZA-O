import type { EquipeCandidataDto } from '@/types/api';
import { formatDuration } from '@/lib/format';

export interface AvaliacaoSelecao {
  /** Pode ser escolhida pelo usuário atual. */
  selecionavel: boolean;
  /** A escolha exige justificativa (apoio intermunicipal e/ou exceção). */
  exigeJustificativa: boolean;
  /** A designação precisa ir com `excecao: true` (incompatível ou indisponível). */
  excecao: boolean;
  /** Motivo de não poder ser escolhida (exibido na linha). */
  motivoBloqueio: string | null;
}

/** Capacidade esgotada é invariante no servidor: nem a exceção autorizada a ultrapassa. */
export function capacidadeEsgotada(c: EquipeCandidataDto): boolean {
  const e = c.equipe;
  return !!e && e.capacidade > 0 && e.despachosAtivos >= e.capacidade;
}

/**
 * Regras de seleção espelhando o servidor (que é quem decide):
 * compatível + disponível → livre; incompatível/indisponível → só como exceção com `despacho.excecao`
 * e justificativa; apoio intermunicipal → justificativa; capacidade esgotada → nunca.
 */
export function avaliarSelecao(c: EquipeCandidataDto, podeExcecao: boolean): AvaliacaoSelecao {
  const regular = c.compativel && c.disponivel;
  if (capacidadeEsgotada(c)) {
    return {
      selecionavel: false,
      exigeJustificativa: false,
      excecao: false,
      motivoBloqueio: 'Capacidade de atendimento esgotada — não pode receber outra OS, nem por exceção.',
    };
  }
  if (!regular && !podeExcecao) {
    return {
      selecionavel: false,
      exigeJustificativa: false,
      excecao: false,
      motivoBloqueio: !c.compativel
        ? 'Equipe incompatível com os requisitos da OS. Só pode ser designada como exceção por quem tem a permissão "despacho.excecao".'
        : 'Equipe indisponível. Só pode ser designada como exceção por quem tem a permissão "despacho.excecao".',
    };
  }
  return {
    selecionavel: true,
    excecao: !regular,
    exigeJustificativa: !regular || c.apoioIntermunicipal,
    motivoBloqueio: null,
  };
}

/** Texto do tempo estimado: sem roteamento, nunca estima tempo pela linha reta. */
export function textoTempoEstimado(c: Pick<EquipeCandidataDto, 'origemEstimativa' | 'tempoEstimadoMin'>): string {
  if (c.origemEstimativa === 'LINHA_RETA' || c.tempoEstimadoMin == null) return 'linha reta — sem estimativa de tempo';
  return `${formatDuration(c.tempoEstimadoMin)} (rota estimada)`;
}

/** Recomendadas primeiro; depois a ordem da API (que já considera compatibilidade antes da distância). */
export function ordenarCandidatas(lista: EquipeCandidataDto[]): EquipeCandidataDto[] {
  return [...lista.filter((c) => c.recomendada), ...lista.filter((c) => !c.recomendada)];
}
