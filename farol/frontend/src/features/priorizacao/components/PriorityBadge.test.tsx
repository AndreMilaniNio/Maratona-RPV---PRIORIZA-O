import { render, screen } from '@testing-library/react';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import type { PrioridadeResumoDto } from '@/types/api';

const urgente: PrioridadeResumoDto = { id: 1, codigo: 'URG', nome: 'Urgente', cor: '#B42318', rank: 1, critica: true };
const baixa: PrioridadeResumoDto = { id: 5, codigo: 'BAI', nome: 'Baixa', cor: '#667A45', rank: 5, critica: false };

describe('PriorityBadge', () => {
  it('usa a cor vinda da API e mostra texto (código + nome) e ícone', () => {
    render(<PriorityBadge prioridade={urgente} />);
    const badge = screen.getByTestId('priority-badge');
    expect(badge).toHaveStyle({ backgroundColor: '#B42318' });
    expect(badge).toHaveTextContent('URG');
    expect(badge).toHaveTextContent('Urgente');
    expect(screen.getByTestId('priority-icon')).toBeInTheDocument();
    expect(badge).toHaveAccessibleName(/Prioridade Urgente \(URG\), crítica/);
  });

  it('não depende só da cor: prioridades diferentes têm ícones diferentes', () => {
    const { rerender } = render(<PriorityBadge prioridade={urgente} />);
    const iconeUrgente = screen.getByTestId('priority-icon').getAttribute('class');
    rerender(<PriorityBadge prioridade={baixa} />);
    const iconeBaixa = screen.getByTestId('priority-icon').getAttribute('class');
    expect(iconeUrgente).not.toEqual(iconeBaixa);
    expect(screen.getByTestId('priority-badge')).toHaveTextContent('BAI');
  });

  it('no modo compacto mantém o nome acessível', () => {
    render(<PriorityBadge prioridade={baixa} compact />);
    const badge = screen.getByTestId('priority-badge');
    expect(badge).not.toHaveTextContent('Baixa');
    expect(badge).toHaveAccessibleName(/Prioridade Baixa \(BAI\)/);
  });

  it('indica prioridade definida manualmente', () => {
    render(<PriorityBadge prioridade={baixa} manual />);
    expect(screen.getByTestId('priority-badge')).toHaveAccessibleName(/definida manualmente/);
  });

  it('sem prioridade mostra texto explícito', () => {
    render(<PriorityBadge prioridade={null} />);
    expect(screen.getByText('Sem prioridade')).toBeInTheDocument();
  });
});
