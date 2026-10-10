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

/**
 * Diálogo de formulário de cadastro: título, corpo rolável, erro geral da API e rodapé Cancelar/Salvar.
 * O pai monta o diálogo só quando aberto (estado do formulário recomeça a cada abertura).
 */
export function FormDialog({
  title,
  description,
  size = 'md',
  onClose,
  onSubmit,
  salvando,
  erro,
  submitLabel = 'Salvar',
  children,
}: {
  title: string;
  description?: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  onClose: () => void;
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => void;
  salvando?: boolean;
  erro?: unknown;
  submitLabel?: string;
  children: React.ReactNode;
}) {
  return (
    <Dialog open onOpenChange={(v) => !v && !salvando && onClose()}>
      <DialogContent size={size} {...(description ? {} : { 'aria-describedby': undefined })}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <form onSubmit={onSubmit} noValidate className="flex min-h-0 flex-1 flex-col">
          <DialogBody className="space-y-3">
            {children}
            {erro != null && <ErrorState error={erro} compact title="Não foi possível salvar" />}
          </DialogBody>
          <DialogFooter>
            <Button type="button" variant="secondary" onClick={onClose} disabled={salvando}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvando}>
              {submitLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
