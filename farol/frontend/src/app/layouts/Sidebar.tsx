import { NavLink } from 'react-router';
import { navegacaoPermitida } from '@/app/config/navigation';
import { cn } from '@/lib/utils';

/** Navegação lateral filtrada pelas permissões do usuário. */
export function Sidebar({ permissoes, recolhida, onNavigate }: { permissoes: readonly string[]; recolhida?: boolean; onNavigate?: () => void }) {
  const grupos = navegacaoPermitida(permissoes);
  return (
    <nav aria-label="Navegação principal" className="flex flex-col gap-4 px-2 py-3">
      {grupos.map((g) => (
        <div key={g.titulo}>
          {!recolhida && <p className="px-2 pb-1 text-[11px] font-semibold uppercase tracking-wider text-[#9fb4cf]">{g.titulo}</p>}
          <ul className="space-y-0.5">
            {g.itens.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.to === '/'}
                  onClick={onNavigate}
                  title={recolhida ? item.label : undefined}
                  className={({ isActive }) =>
                    cn(
                      'flex items-center gap-2 rounded-md px-2 py-1.5 text-[13px] text-[#dbe5f3] hover:bg-navy-soft hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/50',
                      isActive && 'bg-primary text-white font-medium',
                      recolhida && 'justify-center',
                    )
                  }
                >
                  <item.icon className="h-4 w-4 shrink-0" aria-hidden />
                  {!recolhida && <span className="truncate">{item.label}</span>}
                  {recolhida && <span className="sr-only">{item.label}</span>}
                </NavLink>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </nav>
  );
}
