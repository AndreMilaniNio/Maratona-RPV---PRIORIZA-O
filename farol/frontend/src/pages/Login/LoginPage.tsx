import * as React from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { LogIn } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { FormField } from '@/components/forms/FormField';
import { ErrorState, InlineAlert } from '@/components/feedback/states';
import { useAuth } from '@/features/autenticacao/hooks/AuthProvider';
import { loginSchema, type LoginForm } from '@/features/autenticacao/schemas/loginSchema';
import { ApiError } from '@/services/api/client';

export function LoginPage() {
  const { estado, login, motivoSaida } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const destino = (location.state as { de?: string } | null)?.de ?? '/';
  const [erro, setErro] = React.useState<unknown>(null);
  const form = useForm<LoginForm>({ resolver: zodResolver(loginSchema), defaultValues: { email: '', senha: '' } });

  if (estado === 'autenticado') return <Navigate to={destino} replace />;

  const onSubmit = form.handleSubmit(async (v) => {
    setErro(null);
    try {
      await login(v.email, v.senha);
      navigate(destino, { replace: true });
    } catch (e) {
      setErro(e);
    }
  });

  const credenciaisInvalidas = erro instanceof ApiError && erro.status === 401;

  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas px-4">
      <div className="w-full max-w-sm">
        <div className="mb-4 flex items-center gap-3">
          <img src="/farol.svg" alt="" className="h-10 w-10" />
          <div>
            <h1 className="text-xl font-semibold text-navy">Farol</h1>
            <p className="text-[13px] text-muted">Gestão, priorização e despacho de ordens de serviço</p>
          </div>
        </div>
        <form onSubmit={onSubmit} className="space-y-3 rounded-md border border-line bg-white p-5" noValidate>
          <h2 className="text-base font-semibold">Entrar</h2>
          {motivoSaida && <InlineAlert tone="info">{motivoSaida}</InlineAlert>}
          <FormField id="email" label="E-mail" required error={form.formState.errors.email?.message}>
            <Input type="email" autoComplete="username" autoFocus {...form.register('email')} />
          </FormField>
          <FormField id="senha" label="Senha" required error={form.formState.errors.senha?.message}>
            <Input type="password" autoComplete="current-password" {...form.register('senha')} />
          </FormField>
          {credenciaisInvalidas ? (
            <InlineAlert tone="danger">Credenciais inválidas.</InlineAlert>
          ) : erro ? (
            <ErrorState error={erro} compact />
          ) : null}
          <Button type="submit" className="w-full" size="lg" loading={form.formState.isSubmitting}>
            <LogIn /> Entrar
          </Button>
        </form>
        <p className="mt-3 text-center text-xs text-muted">Ambiente com dados demonstrativos (fictícios).</p>
      </div>
    </div>
  );
}
