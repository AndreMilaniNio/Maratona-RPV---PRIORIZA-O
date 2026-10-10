import { AlertTriangle, CircleAlert, CircleCheck, MapPinned } from 'lucide-react';
import type { InterpretacaoTransformadorDto } from '@/types/api';
import { Spinner } from '@/components/ui/misc';
import { Badge } from '@/components/ui/badge';
import { DemoBadge } from '@/components/feedback/demo';
import { mensagemErro } from '@/services/api/client';
import { cn } from '@/lib/utils';

/**
 * Interpretação do número do transformador: 3 primeiros dígitos = localidade (com nome, se
 * cadastrada) e o restante = número do equipamento na localidade. Puramente apresentacional.
 */
export function TransformadorInterpretacao({
  interpretacao,
  loading,
  error,
  className,
}: {
  interpretacao: InterpretacaoTransformadorDto | undefined;
  loading?: boolean;
  error?: unknown;
  className?: string;
}) {
  if (loading) {
    return (
      <div className={cn('flex items-center gap-2 text-xs text-muted', className)} role="status">
        <Spinner className="h-3.5 w-3.5" /> Interpretando número…
      </div>
    );
  }
  if (error) {
    return (
      <p className={cn('flex items-center gap-1.5 text-xs font-medium text-danger', className)} role="alert">
        <CircleAlert className="h-3.5 w-3.5" aria-hidden /> {mensagemErro(error)}
      </p>
    );
  }
  if (!interpretacao) return null;

  const i = interpretacao;
  if (!i.valido) {
    return (
      <p className={cn('flex items-center gap-1.5 text-xs font-medium text-danger', className)} role="alert" data-testid="transformador-erro">
        <CircleAlert className="h-3.5 w-3.5" aria-hidden /> {i.erro ?? 'Número de transformador inválido.'}
      </p>
    );
  }

  const nomeLocalidade = i.localidade?.nome;
  // O selo já sinaliza a localidade não cadastrada; evita repetir o mesmo alerta da API.
  const alertas = i.alertas.filter((a) => i.localidadeCadastrada || !/localidade não cadastrada/i.test(a));
  return (
    <div className={cn('space-y-1 text-xs', className)} aria-live="polite" data-testid="transformador-interpretacao">
      <p className="flex flex-wrap items-center gap-1.5 text-[13px] text-ink">
        <MapPinned className="h-3.5 w-3.5 text-primary" aria-hidden />
        <span>
          Localidade <strong className="font-mono">{i.codigoLocalidade}</strong>
          {nomeLocalidade ? <> ({nomeLocalidade})</> : null}
          {' · '}equipamento <strong className="font-mono">{i.numeroLocal}</strong>
        </span>
        {i.localidadeCadastrada ? (
          <Badge variant="success">
            <CircleCheck aria-hidden /> localidade cadastrada
          </Badge>
        ) : (
          <Badge variant="warning">
            <AlertTriangle aria-hidden /> localidade não cadastrada, confirmar
          </Badge>
        )}
        {i.transformador && <DemoBadge show={i.transformador.demonstrativo} />}
      </p>
      {i.transformador && (
        <p className="text-muted">
          No cadastro da rede: circuito {i.transformador.subestacao ?? '—'}, conjunto {i.transformador.conjunto ?? '—'}
          {' · '}
          {i.transformador.ucsLigadas} UC(s) ligada(s)
        </p>
      )}
      {alertas.length > 0 && (
        <ul className="space-y-0.5" aria-label="Alertas do transformador">
          {alertas.map((a) => (
            <li key={a} className="flex items-start gap-1.5 text-[#7a3306]">
              <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-warning" aria-hidden />
              {a}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
