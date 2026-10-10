import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Dialog, DialogBody, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/input';
import { FormField } from '@/components/forms/FormField';
import { ErrorState } from '@/components/feedback/states';

/**
 * Diálogo de confirmação com justificativa (obrigatória ou opcional).
 * `onConfirm` deve devolver uma Promise; erros da API são exibidos no próprio diálogo.
 */
export function JustificativaDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel = 'Confirmar',
  required = true,
  minLength = 5,
  label = 'Justificativa',
  onConfirm,
  danger,
  children,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  title: string;
  description?: React.ReactNode;
  confirmLabel?: string;
  required?: boolean;
  minLength?: number;
  label?: string;
  onConfirm: (justificativa: string) => Promise<unknown>;
  danger?: boolean;
  /** Campos extras renderizados acima da justificativa. */
  children?: React.ReactNode;
}) {
  const [texto, setTexto] = React.useState('');
  const [erroLocal, setErroLocal] = React.useState<string | null>(null);
  const [erroApi, setErroApi] = React.useState<unknown>(null);
  const [enviando, setEnviando] = React.useState(false);

  React.useEffect(() => {
    if (open) {
      setTexto('');
      setErroLocal(null);
      setErroApi(null);
    }
  }, [open]);

  const confirmar = async () => {
    const t = texto.trim();
    if (required && t.length < minLength) {
      setErroLocal(`Informe a justificativa (mínimo de ${minLength} caracteres).`);
      return;
    }
    setErroLocal(null);
    setErroApi(null);
    setEnviando(true);
    try {
      await onConfirm(t);
      onOpenChange(false);
    } catch (e) {
      setErroApi(e);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent size="md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <DialogBody className="space-y-3">
          {children}
          <FormField id="justificativa-dialog" label={label} required={required} error={erroLocal}>
            <Textarea value={texto} onChange={(e) => setTexto(e.target.value)} rows={3} />
          </FormField>
          {erroApi != null && <ErrorState error={erroApi} compact />}
        </DialogBody>
        <DialogFooter>
          <Button variant="secondary" onClick={() => onOpenChange(false)} disabled={enviando}>
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
