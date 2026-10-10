import * as React from 'react';
import { createBrowserRouter, Link } from 'react-router';
import { AppLayout } from '@/app/layouts/AppLayout';
import { RequireAuth, RequirePermission } from '@/app/router/guards';
import { ROTAS, NAVEGACAO } from '@/app/config/navigation';
import { LoadingState, EmptyState } from '@/components/feedback/states';
import { PERMISSOES, type RequisitoPermissao } from '@/lib/permissions';
import { LoginPage } from '@/pages/Login/LoginPage';

const DashboardPage = React.lazy(() => import('@/pages/Dashboard/DashboardPage'));
const NovaSolicitacaoPage = React.lazy(() => import('@/pages/NovaSolicitacao/NovaSolicitacaoPage'));
const PainelOperacionalPage = React.lazy(() => import('@/pages/PainelOperacional/PainelOperacionalPage'));
const DetalhesOSPage = React.lazy(() => import('@/pages/DetalhesOS/DetalhesOSPage'));
const MapaOperacionalPage = React.lazy(() => import('@/pages/MapaOperacional/MapaOperacionalPage'));
const EquipesPage = React.lazy(() => import('@/pages/Equipes/EquipesPage'));
const EquipeDetalhePage = React.lazy(() => import('@/pages/Equipes/EquipeDetalhePage'));
const HistoricoPage = React.lazy(() => import('@/pages/Historico/HistoricoPage'));
const ConfiguracaoPontuacaoPage = React.lazy(() => import('@/pages/ConfiguracaoPontuacao/ConfiguracaoPontuacaoPage'));
const RegrasPrioridadePage = React.lazy(() => import('@/pages/Configuracoes/RegrasPrioridadePage'));
const CodigosPrazosPage = React.lazy(() => import('@/pages/Configuracoes/CodigosPrazosPage'));
const CadastrosPage = React.lazy(() => import('@/pages/Configuracoes/CadastrosPage'));
const UsuariosPage = React.lazy(() => import('@/pages/Configuracoes/UsuariosPage'));
const SuportePage = React.lazy(() => import('@/pages/Suporte/SuportePage'));

function requisitoDe(to: string): RequisitoPermissao | undefined {
  for (const g of NAVEGACAO) for (const i of g.itens) if (i.to === to) return i.requisito;
  return undefined;
}

function pagina(el: React.ReactNode, requisito?: RequisitoPermissao) {
  return (
    <RequirePermission requisito={requisito}>
      <React.Suspense fallback={<LoadingState label="Carregando tela…" />}>{el}</React.Suspense>
    </RequirePermission>
  );
}

function NaoEncontrada() {
  return (
    <EmptyState
      title="Página não encontrada"
      description="O endereço acessado não existe."
      action={
        <Link to="/" className="text-primary underline">
          Voltar ao início
        </Link>
      }
    />
  );
}

export const router = createBrowserRouter([
  { path: ROTAS.login, element: <LoginPage /> },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: pagina(<DashboardPage />, requisitoDe(ROTAS.dashboard)) },
          { path: ROTAS.novaSolicitacao, element: pagina(<NovaSolicitacaoPage />, requisitoDe(ROTAS.novaSolicitacao)) },
          { path: ROTAS.painel, element: pagina(<PainelOperacionalPage />, requisitoDe(ROTAS.painel)) },
          { path: '/os/:id', element: pagina(<DetalhesOSPage />, { todas: [PERMISSOES.osConsultar] }) },
          { path: ROTAS.mapa, element: pagina(<MapaOperacionalPage />, requisitoDe(ROTAS.mapa)) },
          { path: ROTAS.equipes, element: pagina(<EquipesPage />, requisitoDe(ROTAS.equipes)) },
          { path: '/equipes/:id', element: pagina(<EquipeDetalhePage />, requisitoDe(ROTAS.equipes)) },
          { path: ROTAS.auditoria, element: pagina(<HistoricoPage />, requisitoDe(ROTAS.auditoria)) },
          { path: ROTAS.pontuacao, element: pagina(<ConfiguracaoPontuacaoPage />, requisitoDe(ROTAS.pontuacao)) },
          { path: ROTAS.regras, element: pagina(<RegrasPrioridadePage />, requisitoDe(ROTAS.regras)) },
          { path: ROTAS.prioridades, element: pagina(<CodigosPrazosPage />, requisitoDe(ROTAS.prioridades)) },
          { path: ROTAS.cadastros, element: pagina(<CadastrosPage />, requisitoDe(ROTAS.cadastros)) },
          { path: ROTAS.usuarios, element: pagina(<UsuariosPage />, requisitoDe(ROTAS.usuarios)) },
          { path: ROTAS.suporte, element: pagina(<SuportePage />, requisitoDe(ROTAS.suporte)) },
          { path: '*', element: <NaoEncontrada /> },
        ],
      },
    ],
  },
]);
