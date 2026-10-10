// STUB — implementado na integração final (reutiliza as seções do formulário de Nova Solicitação).
export interface SimuladorTabProps {
  /** Escopo selecionado na tela (null = global). */
  municipioId: number | null;
  /** Há rascunho no escopo (permite simular com o rascunho). */
  temRascunho: boolean;
}

export function SimuladorTab({ municipioId, temRascunho }: SimuladorTabProps) {
  return (
    <p className="text-[13px] text-muted">
      Simulador ({municipioId ?? 'global'}, rascunho: {temRascunho ? 'sim' : 'não'}) — em construção.
    </p>
  );
}
