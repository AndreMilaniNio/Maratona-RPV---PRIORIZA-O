import * as React from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogBody,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { ErrorState } from '@/components/feedback/states';

/** Confirmação simples (ex.: excluir). `onConfirm` devolve Promise; erros ficam no diálogo. */
export function ConfirmDialog({
  title,
  description,
  confirmLabel = 'Confirmar',
  danger,
  onClose,
  onConfirm,
  children,
}: {
  title: string;
  description?: React.ReactNode;
  confirmLabel?: string;
  danger?: boolean;
  onClose: () => void;
  onConfirm: () => Promise<unknown>;
  /** Conteúdo extra (ex.: ação alternativa após um conflito). */
  children?: (erro: unknown) => React.ReactNode;
}) {
  const [enviando, setEnviando] = React.useState(false);
  const [erro, setErro] = React.useState<unknown>(null);

  const confirmar = async () => {
    setEnviando(true);
    setErro(null);
    try {
      await onConfirm();
      onClose();
    } catch (e) {
      setErro(e);
    } finally {
      setEnviando(false);
    }
  };

  const extra = children?.(erro);

  return (
    <Dialog open onOpenChange={(v) => !v && !enviando && onClose()}>
      <DialogContent size="sm" {...(description ? {} : { 'aria-describedby': undefined })}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        {(erro != null || extra) && (
          <DialogBody className="space-y-3">
            {/* Quando o conteúdo extra trata o erro (ex.: 409 → inativar), ele substitui a caixa de erro. */}
            {extra || <ErrorState error={erro} compact />}
          </DialogBody>
        )}
        <DialogFooter>
          <Button variant="secondary" onClick={onClose} disabled={enviando}>
            Cancelar
          </Button>
          <Button variant={danger ? 'danger' : 'primary'} onClick={confirmar} loading={enviando}>
            {confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
