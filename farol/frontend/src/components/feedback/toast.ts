import { toast } from 'sonner';
import { ApiError, mensagemErro } from '@/services/api/client';

export function notificarSucesso(mensagem: string, descricao?: string) {
  toast.success(mensagem, { description: descricao });
}

export function notificarErro(err: unknown, titulo?: string) {
  let descricao: string | undefined;
  if (err instanceof ApiError) {
    const campos = Object.values(err.fieldErrors).flat();
    const pend = err.pendencias;
    descricao = [...campos, ...pend].slice(0, 5).join(' • ') || undefined;
  }
  toast.error(titulo ?? mensagemErro(err), { description: titulo ? mensagemErro(err) + (descricao ? ` — ${descricao}` : '') : descricao });
}

export function notificarInfo(mensagem: string, descricao?: string) {
  toast(mensagem, { description: descricao });
}
