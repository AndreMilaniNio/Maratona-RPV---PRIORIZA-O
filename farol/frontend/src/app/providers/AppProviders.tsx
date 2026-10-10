import * as React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { TooltipProvider } from '@/components/ui/tooltip';
import { AuthProvider } from '@/features/autenticacao/hooks/AuthProvider';
import { deveRetentar } from '@/services/api/client';

export function criarQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: deveRetentar,
        retryDelay: (n) => Math.min(1000 * 2 ** n, 8000),
        refetchOnWindowFocus: false,
        staleTime: 15_000,
      },
      mutations: { retry: false },
    },
  });
}

export function AppProviders({ children, client }: { children: React.ReactNode; client?: QueryClient }) {
  const [queryClient] = React.useState(() => client ?? criarQueryClient());
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider delayDuration={300}>
        <AuthProvider>{children}</AuthProvider>
        <Toaster position="top-right" richColors closeButton duration={5000} />
      </TooltipProvider>
    </QueryClientProvider>
  );
}
