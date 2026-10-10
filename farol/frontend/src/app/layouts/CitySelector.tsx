import { Building2, Loader2 } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';

const TODAS = 'todas';

/** Seletor de cidade do cabeçalho; "Todas as cidades" só para quem tem cidades.todas. */
export function CitySelector() {
  const { municipioId, opcoes, podeTodas, selecionar, salvando } = useCidade();
  if (opcoes.length === 0) return null;
  const valor = municipioId === null ? TODAS : String(municipioId);
  return (
    <div className="flex items-center gap-1.5">
      <Building2 className="h-4 w-4 text-[#9fb4cf]" aria-hidden />
      <Select value={valor} onValueChange={(v) => selecionar(v === TODAS ? null : Number(v))}>
        <SelectTrigger className="h-8 w-48 border-[#3a5c86] bg-navy-soft text-white data-[placeholder]:text-white" aria-label="Cidade">
          <SelectValue placeholder="Cidade" />
        </SelectTrigger>
        <SelectContent>
          {podeTodas && <SelectItem value={TODAS}>Todas as cidades</SelectItem>}
          {opcoes.map((m) => (
            <SelectItem key={m.id} value={String(m.id)}>
              {m.nome} ({m.prefixo})
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {salvando && <Loader2 className="h-4 w-4 animate-spin text-[#9fb4cf]" aria-label="Salvando preferência" />}
    </div>
  );
}
