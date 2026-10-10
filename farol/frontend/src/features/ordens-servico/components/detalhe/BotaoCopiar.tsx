import * as React from 'react';
import { Check, Copy } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { notificarErro, notificarSucesso } from '@/components/feedback/toast';
import { copiarTexto } from '@/lib/utils';

/** Botão "copiar" com confirmação visual e toast. */
export function BotaoCopiar({ texto, rotulo = 'Copiar', mensagem = 'Copiado' }: { texto: string; rotulo?: string; mensagem?: string }) {
  const [ok, setOk] = React.useState(false);
  const copiar = async () => {
    if (await copiarTexto(texto)) {
      setOk(true);
      notificarSucesso(mensagem, texto);
      setTimeout(() => setOk(false), 2000);
    } else {
      notificarErro(new Error('Não foi possível copiar automaticamente. Selecione o texto e copie manualmente.'));
    }
  };
  return (
    <Button size="sm" variant="secondary" onClick={copiar} aria-label={rotulo}>
      {ok ? <Check /> : <Copy />} {ok ? 'Copiado' : rotulo}
    </Button>
  );
}
