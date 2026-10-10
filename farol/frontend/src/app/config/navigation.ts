import {
  ClipboardList,
  BookOpen,
  FilePlus2,
  Gauge,
  History,
  LayoutDashboard,
  ListChecks,
  Map,
  Network,
  Scale,
  Timer,
  Users,
  UsersRound,
  type LucideIcon,
} from 'lucide-react';
import { atende, PERMISSOES, type RequisitoPermissao } from '@/lib/permissions';

export interface ItemNavegacao {
  to: string;
  label: string;
  icon: LucideIcon;
  requisito?: RequisitoPermissao;
  /** Mostra o seletor de cidade no cabeçalho. */
  operacional?: boolean;
}

export interface GrupoNavegacao {
  titulo: string;
  itens: ItemNavegacao[];
}

export const ROTAS = {
  login: '/login',
  dashboard: '/',
  novaSolicitacao: '/solicitacoes/nova',
  painel: '/painel',
  os: (id: string) => `/os/${id}`,
  mapa: '/mapa',
  equipes: '/equipes',
  equipe: (id: number) => `/equipes/${id}`,
  auditoria: '/historico',
  pontuacao: '/configuracao-pontuacao',
  regras: '/admin/regras-prioridade',
  prioridades: '/admin/codigos-prazos',
  cadastros: '/admin/cadastros',
  usuarios: '/admin/usuarios',
  suporte: '/suporte',
} as const;

export const NAVEGACAO: GrupoNavegacao[] = [
  {
    titulo: 'Operação',
    itens: [
      { to: ROTAS.dashboard, label: 'Dashboard', icon: LayoutDashboard, requisito: { todas: [PERMISSOES.osConsultar] }, operacional: true },
      {
        to: ROTAS.novaSolicitacao,
        label: 'Nova solicitação',
        icon: FilePlus2,
        requisito: { todas: [PERMISSOES.solicitacaoRegistrar] },
        operacional: true,
      },
      { to: ROTAS.painel, label: 'Painel operacional', icon: ClipboardList, requisito: { todas: [PERMISSOES.osConsultar] }, operacional: true },
      { to: ROTAS.mapa, label: 'Mapa operacional', icon: Map, requisito: { todas: [PERMISSOES.osConsultar] }, operacional: true },
      {
        to: ROTAS.equipes,
        label: 'Equipes',
        icon: UsersRound,
        requisito: { algumaDe: [PERMISSOES.osConsultar, PERMISSOES.equipesAdministrar] },
        operacional: true,
      },
    ],
  },
  {
    titulo: 'Priorização',
    itens: [
      {
        to: ROTAS.pontuacao,
        label: 'Formulário e pontuação',
        icon: Gauge,
        requisito: { algumaDe: [PERMISSOES.osConsultar, PERMISSOES.pontuacaoEditar] },
      },
      { to: ROTAS.regras, label: 'Regras de prioridade', icon: Scale, requisito: { todas: [PERMISSOES.criteriosAdministrar] } },
      { to: ROTAS.prioridades, label: 'Códigos e prazos', icon: Timer, requisito: { todas: [PERMISSOES.prazosAdministrar] } },
    ],
  },
  {
    titulo: 'Administração',
    itens: [
      { to: ROTAS.cadastros, label: 'Cadastros da rede', icon: Network, requisito: { todas: [PERMISSOES.cadastrosAdministrar] } },
      { to: ROTAS.usuarios, label: 'Usuários e permissões', icon: Users, requisito: { todas: [PERMISSOES.usuariosAdministrar] } },
      { to: ROTAS.auditoria, label: 'Histórico / auditoria', icon: History, requisito: { todas: [PERMISSOES.auditoriaConsultar] } },
    ],
  },
  {
    titulo: 'Ajuda',
    itens: [{ to: ROTAS.suporte, label: 'Suporte e guia', icon: BookOpen, requisito: { todas: [PERMISSOES.osConsultar] } }],
  },
];

/** Filtra a navegação pelas permissões do usuário (grupos vazios somem). */
export function navegacaoPermitida(permissoes: readonly string[] | null | undefined): GrupoNavegacao[] {
  return NAVEGACAO.map((g) => ({ ...g, itens: g.itens.filter((i) => atende(permissoes, i.requisito)) })).filter((g) => g.itens.length > 0);
}

export const ICONE_PADRAO = ListChecks;
