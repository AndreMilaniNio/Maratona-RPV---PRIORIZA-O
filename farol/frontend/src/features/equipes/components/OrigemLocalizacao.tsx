import { FlaskConical, MapPin, Satellite } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { ORIGEM_LOCALIZACAO_EQUIPE_LABEL } from '@/lib/labels';
import { formatDateTimeShort } from '@/lib/format';
import type { OrigemLocalizacaoEquipe } from '@/types/api';

/** Origem da posição da equipe. "Demonstrativa" nunca deve parecer posição real. */
export function OrigemLocalizacaoBadge({ origem }: { origem: OrigemLocalizacaoEquipe | string | null | undefined }) {
  if (!origem) return <span className="text-muted">—</span>;
  if (origem === 'Demonstrativa')
    return (
      <Badge variant="demo" title="Posição fictícia carregada com os dados demonstrativos">
        <FlaskConical aria-hidden /> Posição demonstrativa
      </Badge>
    );
  if (origem === 'Gps')
    return (
      <Badge variant="primary" title="Posição enviada por dispositivo com GPS">
        <Satellite aria-hidden /> GPS
      </Badge>
    );
  return (
    <Badge variant="neutral" title="Posição informada manualmente (não é rastreamento em tempo real)">
      <MapPin aria-hidden /> {ORIGEM_LOCALIZACAO_EQUIPE_LABEL[origem as OrigemLocalizacaoEquipe] ?? origem}
    </Badge>
  );
}

/** Última atualização de posição (horário + origem) em formato de célula de tabela. */
export function UltimaLocalizacao({ em, origem }: { em: string | null; origem: OrigemLocalizacaoEquipe }) {
  if (!em) return <span className="text-muted">Sem posição registrada</span>;
  return (
    <span className="inline-flex flex-col items-start gap-0.5">
      <span className="tabular-nums">{formatDateTimeShort(em)}</span>
      <OrigemLocalizacaoBadge origem={origem} />
    </span>
  );
}
