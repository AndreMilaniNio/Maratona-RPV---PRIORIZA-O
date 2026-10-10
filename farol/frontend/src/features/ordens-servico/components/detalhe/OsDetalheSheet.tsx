// STUB — será substituído pela implementação completa (detalhe da OS em painel lateral).
import { Dialog, SheetContent, DialogTitle } from '@/components/ui/dialog';

export interface OsDetalheSheetProps {
  /** OS aberta; null = fechado. */
  osId: string | null;
  onOpenChange: (open: boolean) => void;
}

export function OsDetalheSheet({ osId, onOpenChange }: OsDetalheSheetProps) {
  return (
    <Dialog open={!!osId} onOpenChange={onOpenChange}>
      <SheetContent aria-describedby={undefined}>
        <DialogTitle className="p-4">OS {osId}</DialogTitle>
      </SheetContent>
    </Dialog>
  );
}
