import { describe, expect, it } from 'vitest';
import type { CatalogoDto, CriterioFormularioDto } from '@/types/api';
import {
  MSG_MEIO_LOCALIZAR,
  criarSolicitacaoSchema,
  paraNovaSolicitacaoRequest,
  solicitacaoSchema,
  valoresIniciais,
  type SolicitacaoFormValues,
} from './solicitacaoSchema';

let idSeq = 1;
function criterio(codigo: string, opcoes: string[], desconhecida: string | null, multipla = false): CriterioFormularioDto {
  return {
    id: idSeq++,
    codigo,
    nome: codigo,
    descricao: null,
    multiplaEscolha: multipla,
    opcoes: opcoes.map((o) => ({ id: idSeq++, codigo: o, rotulo: o, representaDesconhecido: o === desconhecida })),
  };
}

const catalogo: CatalogoDto = {
  municipios: [],
  tiposOcorrencia: [],
  classes: [],
  recursos: [],
  qualificacoes: [],
  prioridades: [],
  criteriosFixos: [
    criterio('PESSOAS_AFETADAS', ['ATE_10', 'DE_11_A_100', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('UCS_AFETADAS', ['ATE_10', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('SERVICO_ESSENCIAL', ['HOSPITAL', 'NAO_IDENTIFICADO'], 'NAO_IDENTIFICADO'),
    criterio('SITUACAO_CLIENTE', ['LIGADO', 'DESLIGADO', 'NAO_INFORMADA'], 'NAO_INFORMADA'),
    criterio('RISCO_SEGURANCA', ['RISCO_CHOQUE', 'SEM_RISCO_ADICIONAL', 'DESCONHECIDA'], 'DESCONHECIDA', true),
    criterio('CONDICAO_FORNECIMENTO', ['SEM_INTERRUPCAO', 'TOTAL', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('REDUNDANCIA', ['SIM', 'NAO', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('FONTE_RESERVA', ['SIM', 'NAO', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('EQUIPE_ESPECIALIZADA', ['SIM', 'NAO', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('EQUIPAMENTO_AFETADO', ['POSTE', 'NAO_IDENTIFICADO'], 'NAO_IDENTIFICADO'),
    criterio('ABRANGENCIA', ['PONTO_UNICO', 'DESCONHECIDA'], 'DESCONHECIDA'),
    criterio('NIVEL_REDE', ['CIRCUITO', 'NAO_IDENTIFICADO'], 'NAO_IDENTIFICADO'),
  ],
  criteriosPersonalizados: [criterio('PERS', ['A', 'NAO_SEI'], 'NAO_SEI')],
  rotulosSituacao: null,
  configuracao: null,
};

/** Formulário válido em tudo, exceto a parte de UC/localização sob teste. */
function base(over: (v: SolicitacaoFormValues) => void): SolicitacaoFormValues {
  const v = valoresIniciais(catalogo, 1);
  v.tipoOcorrenciaId = 1;
  v.tipoManutencao = 'Corretiva';
  v.descricao = 'teste';
  over(v);
  return v;
}

function mensagens(v: SolicitacaoFormValues, schema = solicitacaoSchema) {
  const r = schema.safeParse(v);
  if (r.success) return {} as Record<string, string[]>;
  return r.error.issues.reduce<Record<string, string[]>>((acc, i) => {
    const k = i.path.join('.');
    (acc[k] ??= []).push(i.message);
    return acc;
  }, {});
}

describe('valoresIniciais', () => {
  it('usa a opção explícita de desconhecido como padrão de todo critério de impacto', () => {
    const v = valoresIniciais(catalogo, 2);
    expect(v.municipioId).toBe(2);
    expect(v.impacto.pessoasAfetadas).toBe('DESCONHECIDA');
    expect(v.impacto.ucsAfetadas).toBe('DESCONHECIDA');
    expect(v.impacto.servicoEssencial).toBe('NAO_IDENTIFICADO');
    expect(v.impacto.situacaoCliente).toBe('NAO_INFORMADA');
    expect(v.impacto.condicoesSeguranca).toEqual(['DESCONHECIDA']);
    expect(v.impacto.nivelRede).toBe('NAO_IDENTIFICADO');
    expect(v.impacto.quantidadePessoas).toBeNull();
    expect(v.respostasPersonalizadas).toHaveLength(1);
    expect(v.respostasPersonalizadas[0].opcaoId).not.toBeNull();
  });
});

describe('solicitacaoSchema — "Solicitante fora da própria residência / não sabe a UC"', () => {
  it('sem a opção marcada, exige ao menos uma UC', () => {
    const erros = mensagens(base(() => {}));
    expect(erros.ucs?.[0]).toMatch(/ao menos uma UC/);
  });

  it('sem a opção marcada e com UC informada, é válido', () => {
    expect(solicitacaoSchema.safeParse(base((v) => (v.ucs = [{ numero: '137390' }]))).success).toBe(true);
  });

  it('com a opção marcada, a UC é opcional mas o motivo é obrigatório', () => {
    const erros = mensagens(
      base((v) => {
        v.ucNaoInformada = true;
        v.localizacao.cep = '00110-404';
      }),
    );
    expect(erros.ucs).toBeUndefined();
    expect(erros.motivoUcNaoInformada?.[0]).toMatch(/por que a UC não foi informada/);
  });

  it('com a opção marcada, exige rua, CEP ou coordenadas', () => {
    const erros = mensagens(
      base((v) => {
        v.ucNaoInformada = true;
        v.motivoUcNaoInformada = 'Solicitante na rua';
      }),
    );
    expect(erros.localizacao).toEqual([MSG_MEIO_LOCALIZAR]);
  });

  const comOpcao = (f: (v: SolicitacaoFormValues) => void) =>
    base((v) => {
      v.ucNaoInformada = true;
      v.motivoUcNaoInformada = 'Solicitante na rua';
      f(v);
    });

  it('aceita somente o CEP', () => {
    expect(solicitacaoSchema.safeParse(comOpcao((v) => (v.localizacao.cep = '00110-404'))).success).toBe(true);
  });

  it('aceita somente as coordenadas', () => {
    const r = solicitacaoSchema.safeParse(
      comOpcao((v) => {
        v.localizacao.latitude = -21.53;
        v.localizacao.longitude = -42.64;
      }),
    );
    expect(r.success).toBe(true);
  });

  it('aceita somente a rua', () => {
    expect(solicitacaoSchema.safeParse(comOpcao((v) => (v.localizacao.logradouro = 'Rua das Flores'))).success).toBe(true);
  });

  it('coordenada incompleta não conta como meio de localização', () => {
    const erros = mensagens(comOpcao((v) => (v.localizacao.latitude = -21.53)));
    expect(erros.localizacao).toEqual([MSG_MEIO_LOCALIZAR]);
  });

  it('valida o formato da UC quando configurado', () => {
    const schema = criarSolicitacaoSchema({ formatoUc: '^\\d{6,10}$' });
    const erros = mensagens(base((v) => (v.ucs = [{ numero: '12a' }])), schema);
    expect(erros['ucs.0.numero']).toEqual(['Formato de UC inválido.']);
  });
});

describe('paraNovaSolicitacaoRequest', () => {
  it('monta o payload com CEP só com dígitos, origem da coordenada e sem campos de UI', () => {
    const v = base((x) => {
      x.ucNaoInformada = true;
      x.motivoUcNaoInformada = ' fora de casa ';
      x.localizacao.cep = '00110-404';
      x.localizacao.latitude = -21.5;
      x.localizacao.longitude = -42.6;
      x.localizacao.origemCoordenada = 'Mapa';
    });
    const req = paraNovaSolicitacaoRequest(v, 'chave');
    expect(req.chaveIdempotencia).toBe('chave');
    expect(req.localizacao?.cep).toBe('00110404');
    expect(req.localizacao?.origemCoordenada).toBe('Mapa');
    expect(req.motivoUcNaoInformada).toBe('fora de casa');
    expect(req.impacto?.condicoesSeguranca).toEqual(['DESCONHECIDA']);
    expect(req.respostasPersonalizadas).toHaveLength(1);
    expect(req).not.toHaveProperty('classeConfirmada');
  });
});
