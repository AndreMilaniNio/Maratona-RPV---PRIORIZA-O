import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { InterpretacaoTransformadorDto } from '@/types/api';
import { TransformadorInterpretacao } from './TransformadorInterpretacao';

function interpretacao(over: Partial<InterpretacaoTransformadorDto> = {}): InterpretacaoTransformadorDto {
  return {
    valido: true,
    erro: null,
    numero: '1013381',
    codigoLocalidade: '101',
    numeroLocal: '3381',
    localidade: { id: 5, codigo: '101', nome: 'Centro (Lumiara)', municipioId: 1, municipio: 'Lumiara', demonstrativo: true },
    localidadeCadastrada: true,
    transformador: null,
    alertas: [],
    ...over,
  };
}

describe('TransformadorInterpretacao', () => {
  it('mostra a localidade (3 dígitos), o nome da localidade e o número local do equipamento', () => {
    render(<TransformadorInterpretacao interpretacao={interpretacao()} />);
    const bloco = screen.getByTestId('transformador-interpretacao');
    expect(bloco).toHaveTextContent('Localidade 101 (Centro (Lumiara)) · equipamento 3381');
    expect(screen.getByText('101')).toBeInTheDocument();
    expect(screen.getByText('3381')).toBeInTheDocument();
    expect(screen.queryByText('localidade não cadastrada, confirmar')).not.toBeInTheDocument();
  });

  it('sinaliza "localidade não cadastrada, confirmar" quando a localidade não existe', () => {
    render(
      <TransformadorInterpretacao
        interpretacao={interpretacao({
          numero: '9993381',
          codigoLocalidade: '999',
          localidade: null,
          localidadeCadastrada: false,
          alertas: ['Localidade não cadastrada, confirmar.'],
        })}
      />,
    );
    expect(screen.getByText('localidade não cadastrada, confirmar')).toBeInTheDocument();
    expect(screen.getByTestId('transformador-interpretacao')).toHaveTextContent('Localidade 999 · equipamento 3381');
  });

  it('mostra o erro quando o número é inválido', () => {
    render(
      <TransformadorInterpretacao
        interpretacao={interpretacao({
          valido: false,
          erro: 'O número do transformador precisa de ao menos 4 dígitos (3 da localidade e 1 do equipamento).',
          codigoLocalidade: null,
          numeroLocal: null,
          localidade: null,
          localidadeCadastrada: false,
        })}
      />,
    );
    expect(screen.getByRole('alert')).toHaveTextContent('precisa de ao menos 4 dígitos');
    expect(screen.queryByTestId('transformador-interpretacao')).not.toBeInTheDocument();
  });

  it('lista os alertas da API', () => {
    render(
      <TransformadorInterpretacao
        interpretacao={interpretacao({
          alertas: ['Circuito e conjunto identificados pelo cadastro, a confirmar.', 'Transformador de outro município.'],
        })}
      />,
    );
    expect(screen.getByText('Circuito e conjunto identificados pelo cadastro, a confirmar.')).toBeInTheDocument();
    expect(screen.getByText('Transformador de outro município.')).toBeInTheDocument();
  });

  it('mostra carregamento e erro de consulta', () => {
    const { rerender } = render(<TransformadorInterpretacao interpretacao={undefined} loading />);
    expect(screen.getByText('Interpretando número…')).toBeInTheDocument();
    rerender(<TransformadorInterpretacao interpretacao={undefined} error={new Error('Falha de rede')} />);
    expect(screen.getByRole('alert')).toHaveTextContent('Falha de rede');
  });
});
