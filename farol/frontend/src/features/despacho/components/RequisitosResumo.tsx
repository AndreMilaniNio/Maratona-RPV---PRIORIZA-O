import { Badge } from '@/components/ui/badge';
import type { CandidatasDto } from '@/types/api';

/** Qualificações e recursos exigidos pela OS + situação do serviço de rotas. */
export function RequisitosResumo({ dados }: { dados: CandidatasDto }) {
  return (
    <div className="space-y-1 text-[13px]">
      <div className="flex flex-wrap items-center gap-1">
        <span className="text-muted">Qualificações exigidas:</span>
        {dados.qualificacoesExigidas.length ? (
          dados.qualificacoesExigidas.map((q) => (
            <Badge key={q} variant="primary">
              {q}
            </Badge>
          ))
        ) : (
          <span>nenhuma específica</span>
        )}
      </div>
      <div className="flex flex-wrap items-center gap-1">
        <span className="text-muted">Recursos exigidos:</span>
        {dados.recursosExigidos.length ? (
          dados.recursosExigidos.map((r) => (
            <Badge key={r} variant="neutral">
              {r}
            </Badge>
          ))
        ) : (
          <span>nenhum específico</span>
        )}
      </div>
      {!dados.roteamentoDisponivel && (
        <p className="text-xs text-muted">Serviço de rotas não configurado: distâncias em linha reta, sem estimativa de tempo de deslocamento.</p>
      )}
    </div>
  );
}
