import { FlaskConical, TriangleAlert } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

/** Faixa âmbar persistente exibida quando a versão de pontuação em vigor é demonstrativa. */
export function DemoScoringBanner({ className, show = true }: { className?: string; show?: boolean }) {
  if (!show) return null;
  return (
    <div
      role="note"
      className={cn(
        'flex items-center gap-2 rounded-md border border-amber-border bg-amber-banner px-3 py-1.5 text-[13px] font-medium text-[#6b4e00]',
        className,
      )}
    >
      <TriangleAlert className="h-4 w-4 shrink-0" aria-hidden />
      <span>Pontuação demonstrativa, não oficial</span>
      <span className="font-normal text-[#7a5a00]">— os pontos e faixas ainda não foram publicados pelo Usuário Chave.</span>
    </div>
  );
}

/** Selo para registros demonstrativos (dados fictícios). */
export function DemoBadge({ show = true, className, label = 'Demonstrativo' }: { show?: boolean | null; className?: string; label?: string }) {
  if (!show) return null;
  return (
    <Badge variant="demo" className={className} title="Dado demonstrativo (fictício), não oficial">
      <FlaskConical aria-hidden />
      {label}
    </Badge>
  );
}
