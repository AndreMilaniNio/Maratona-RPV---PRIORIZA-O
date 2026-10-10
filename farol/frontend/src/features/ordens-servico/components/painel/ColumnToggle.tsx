import { Columns3 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { COLUNAS_SECUNDARIAS, type ColunaId } from './colunas';

/** Exibir/ocultar colunas secundárias; as essenciais ficam sempre visíveis. */
export function ColumnToggle({
  ocultas,
  onAlternar,
  onRestaurar,
  todasAsCidades,
}: {
  ocultas: ColunaId[];
  onAlternar: (id: ColunaId, visivel: boolean) => void;
  onRestaurar: () => void;
  todasAsCidades: boolean;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="secondary" size="sm">
          <Columns3 /> Colunas
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent className="w-60">
        <DropdownMenuLabel>Colunas secundárias</DropdownMenuLabel>
        {COLUNAS_SECUNDARIAS.map((c) => {
          const indisponivel = c.id === 'municipio' && !todasAsCidades;
          return (
            <DropdownMenuCheckboxItem
              key={c.id}
              checked={!ocultas.includes(c.id)}
              disabled={indisponivel}
              onCheckedChange={(v) => onAlternar(c.id, v === true)}
              onSelect={(e) => e.preventDefault()}
            >
              {c.rotulo}
              {indisponivel && <span className="ml-1 text-xs text-muted">(só em “Todas as cidades”)</span>}
            </DropdownMenuCheckboxItem>
          );
        })}
        <DropdownMenuSeparator />
        <DropdownMenuItem onSelect={onRestaurar}>Restaurar padrão</DropdownMenuItem>
        <p className="px-2 py-1 text-xs text-muted">Posição, número, prioridade, ocorrência, endereço, prazo, status, equipe e ações são sempre exibidos.</p>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
