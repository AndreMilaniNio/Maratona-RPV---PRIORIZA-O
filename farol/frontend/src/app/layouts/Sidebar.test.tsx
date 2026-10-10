import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { Sidebar } from '@/app/layouts/Sidebar';
import { navegacaoPermitida } from '@/app/config/navigation';
import { atende } from '@/lib/permissions';

const atendente = ['localizacao.consultar', 'os.consultar', 'solicitacao.registrar'];
const usuarioChave = ['os.consultar', 'pontuacao.aprovar', 'pontuacao.editar', 'pontuacao.publicar'];
const admin = [
  'auditoria.consultar',
  'cadastros.administrar',
  'cidades.todas',
  'criterios.administrar',
  'despacho.designar',
  'equipes.administrar',
  'os.consultar',
  'prazos.administrar',
  'solicitacao.registrar',
  'usuarios.administrar',
];

function renderSidebar(perms: string[]) {
  return render(
    <MemoryRouter>
      <Sidebar permissoes={perms} />
    </MemoryRouter>,
  );
}

describe('menu lateral filtrado por permissão', () => {
  it('atendente vê operação mas não administração', () => {
    renderSidebar(atendente);
    expect(screen.getByRole('link', { name: /Nova solicitação/ })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /Painel operacional/ })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Usuários e permissões/ })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Cadastros da rede/ })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Histórico \/ auditoria/ })).not.toBeInTheDocument();
    expect(screen.queryByText('Administração')).not.toBeInTheDocument();
  });

  it('Usuário Chave vê a tela de pontuação mas não registra solicitações', () => {
    renderSidebar(usuarioChave);
    expect(screen.getByRole('link', { name: /Pontuação \(Usuário Chave\)/ })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Nova solicitação/ })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /Regras de prioridade/ })).not.toBeInTheDocument();
  });

  it('administrador vê todas as telas administrativas', () => {
    renderSidebar(admin);
    for (const nome of [/Regras de prioridade/, /Códigos e prazos/, /Cadastros da rede/, /Usuários e permissões/, /Histórico \/ auditoria/]) {
      expect(screen.getByRole('link', { name: nome })).toBeInTheDocument();
    }
  });

  it('sem permissões não há itens', () => {
    expect(navegacaoPermitida([])).toEqual([]);
  });
});

describe('atende()', () => {
  it('combina "todas" e "algumaDe"', () => {
    expect(atende(['a', 'b'], { todas: ['a'], algumaDe: ['x', 'b'] })).toBe(true);
    expect(atende(['a'], { todas: ['a', 'b'] })).toBe(false);
    expect(atende(['a'], { algumaDe: ['x'] })).toBe(false);
    expect(atende([], undefined)).toBe(true);
  });
});
