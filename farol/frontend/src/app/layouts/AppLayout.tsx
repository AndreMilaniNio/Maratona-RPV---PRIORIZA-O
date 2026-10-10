import * as React from 'react';
import { Outlet } from 'react-router';
import { LogOut, Menu, PanelLeftClose, PanelLeftOpen, Radio, UserRound } from 'lucide-react';
import { Sidebar } from '@/app/layouts/Sidebar';
import { CitySelector } from '@/app/layouts/CitySelector';
import { Button } from '@/components/ui/button';
import { Tooltip } from '@/components/ui/tooltip';
import { useAuth } from '@/features/autenticacao/hooks/AuthProvider';
import { CidadeProvider, useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { useOperacaoRealtime } from '@/services/realtime/useOperacaoRealtime';
import type { EstadoTempoReal } from '@/services/realtime/operacaoHub';
import { cn } from '@/lib/utils';

const PERFIL_LABEL: Record<string, string> = {
  Administrador: 'Administrador',
  Atendente: 'Atendente',
  Despachante: 'Despachante',
  Supervisor: 'Supervisor',
  EquipeCampo: 'Equipe de campo',
  UsuarioChave: 'Usuário Chave',
};

export function AppLayout() {
  return (
    <CidadeProvider>
      <Shell />
    </CidadeProvider>
  );
}

function Shell() {
  const { usuario, logout } = useAuth();
  const { municipioId } = useCidade();
  const tempoReal = useOperacaoRealtime(!!usuario, municipioId);
  const [recolhida, setRecolhida] = React.useState(false);
  const [menuMovel, setMenuMovel] = React.useState(false);

  return (
    <div className="flex h-screen min-h-0 flex-col">
      <a href="#conteudo" className="sr-only focus:not-sr-only focus:absolute focus:left-2 focus:top-2 focus:z-[2000] focus:rounded focus:bg-white focus:px-3 focus:py-1">
        Pular para o conteúdo
      </a>
      <header className="flex h-12 shrink-0 items-center gap-3 border-b border-[#0f2744] bg-navy px-3 text-white">
        <Button
          variant="ghost"
          size="icon-sm"
          className="text-white hover:bg-navy-soft lg:hidden"
          onClick={() => setMenuMovel((v) => !v)}
          aria-label="Abrir menu"
          aria-expanded={menuMovel}
        >
          <Menu />
        </Button>
        <Button
          variant="ghost"
          size="icon-sm"
          className="hidden text-white hover:bg-navy-soft lg:inline-flex"
          onClick={() => setRecolhida((v) => !v)}
          aria-label={recolhida ? 'Expandir menu lateral' : 'Recolher menu lateral'}
        >
          {recolhida ? <PanelLeftOpen /> : <PanelLeftClose />}
        </Button>
        <div className="flex items-center gap-2">
          <img src="/farol.svg" alt="" className="h-6 w-6" />
          <span className="text-[15px] font-semibold tracking-wide">Farol</span>
          <span className="hidden text-xs text-[#9fb4cf] xl:inline">Gestão, priorização e despacho de OS</span>
        </div>
        <div className="ml-auto flex items-center gap-3">
          <CitySelector />
          <IndicadorTempoReal estado={tempoReal} />
          <div className="hidden items-center gap-1.5 text-[13px] md:flex">
            <UserRound className="h-4 w-4 text-[#9fb4cf]" aria-hidden />
            <span className="max-w-48 truncate" title={usuario?.email ?? undefined}>
              {usuario?.nome}
            </span>
            <span className="text-xs text-[#9fb4cf]">({(usuario?.perfis ?? []).map((p) => PERFIL_LABEL[p] ?? p).join(', ')})</span>
          </div>
          <Button variant="ghost" size="sm" className="text-white hover:bg-navy-soft" onClick={() => logout()}>
            <LogOut /> Sair
          </Button>
        </div>
      </header>
      <div className="flex min-h-0 flex-1">
        <aside
          className={cn(
            'shrink-0 overflow-y-auto bg-navy transition-[width] duration-150',
            recolhida ? 'lg:w-14' : 'lg:w-60',
            menuMovel ? 'fixed inset-y-12 left-0 z-[900] w-60 shadow-xl' : 'hidden lg:block',
          )}
        >
          <Sidebar permissoes={usuario?.permissoes ?? []} recolhida={recolhida && !menuMovel} onNavigate={() => setMenuMovel(false)} />
        </aside>
        <main id="conteudo" className="min-w-0 flex-1 overflow-y-auto px-4 py-3" tabIndex={-1}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function IndicadorTempoReal({ estado }: { estado: EstadoTempoReal }) {
  const mapa: Record<EstadoTempoReal, { cor: string; texto: string }> = {
    conectado: { cor: 'text-[#9fd49f]', texto: 'Tempo real conectado (a fila também é reconciliada a cada 60 s)' },
    conectando: { cor: 'text-[#f4d28a]', texto: 'Conectando ao tempo real…' },
    reconectando: { cor: 'text-[#f4d28a]', texto: 'Reconectando ao tempo real… a fila segue atualizando a cada 60 s' },
    desconectado: { cor: 'text-[#9fb4cf]', texto: 'Tempo real indisponível — a fila atualiza a cada 60 s' },
  };
  const m = mapa[estado];
  return (
    <Tooltip content={m.texto}>
      <span className={cn('inline-flex items-center gap-1 text-xs', m.cor)} tabIndex={0} aria-label={m.texto}>
        <Radio className="h-4 w-4" aria-hidden />
        <span className="hidden lg:inline">{estado === 'conectado' ? 'Ao vivo' : estado === 'desconectado' ? 'Offline' : '…'}</span>
      </span>
    </Tooltip>
  );
}
