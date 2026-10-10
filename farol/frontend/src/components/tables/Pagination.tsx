import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { NativeSelect } from '@/components/ui/input';
import { formatNumber } from '@/lib/format';

export function Pagination({
  pagina,
  tamanhoPagina,
  total,
  onPagina,
  onTamanho,
  tamanhos = [10, 25, 50, 100],
}: {
  pagina: number;
  tamanhoPagina: number;
  total: number;
  onPagina: (p: number) => void;
  onTamanho?: (t: number) => void;
  tamanhos?: number[];
}) {
  const paginas = Math.max(1, Math.ceil(total / Math.max(1, tamanhoPagina)));
  const inicio = total === 0 ? 0 : (pagina - 1) * tamanhoPagina + 1;
  const fim = Math.min(total, pagina * tamanhoPagina);
  return (
    <nav className="flex flex-wrap items-center justify-between gap-2 px-1 py-2 text-[13px] text-muted" aria-label="Paginação">
      <span>
        {formatNumber(inicio)}–{formatNumber(fim)} de {formatNumber(total)}
      </span>
      <div className="flex items-center gap-2">
        {onTamanho && (
          <label className="flex items-center gap-1">
            Por página
            <NativeSelect className="h-7 w-20" value={tamanhoPagina} onChange={(e) => onTamanho(Number(e.target.value))}>
              {tamanhos.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </NativeSelect>
          </label>
        )}
        <Button size="icon-sm" variant="secondary" disabled={pagina <= 1} onClick={() => onPagina(pagina - 1)} aria-label="Página anterior">
          <ChevronLeft />
        </Button>
        <span className="tabular-nums">
          Página {pagina} de {paginas}
        </span>
        <Button size="icon-sm" variant="secondary" disabled={pagina >= paginas} onClick={() => onPagina(pagina + 1)} aria-label="Próxima página">
          <ChevronRight />
        </Button>
      </div>
    </nav>
  );
}
