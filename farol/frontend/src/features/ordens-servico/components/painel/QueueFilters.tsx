import * as React from 'react';
import { FilterX, Search, SlidersHorizontal } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { CheckboxField } from '@/components/ui/checkbox';
import { useCatalogo, criterioPorCodigo } from '@/hooks/useCatalogo';
import { useConjuntosFiltro, useEquipesFiltro, useSubestacoesFiltro } from '@/features/ordens-servico/hooks/useFila';
import { contarFiltrosAtivos, type FiltroPainel } from '@/features/ordens-servico/schemas/filtroFila';
import { SITUACAO_CLIENTE_LABEL, STATUS_OS_LABEL, TIPO_MANUTENCAO_LABEL } from '@/lib/labels';
import { StatusOrdemServicoValores, TipoManutencaoValores, type CatalogoDto, type StatusOrdemServico, type TipoManutencao } from '@/types/api';
import { DateFilter, MultiSelectFilter, SelectFilter, TextFilter, type OpcaoFiltro } from './FilterControls';

export interface QueueFiltersProps {
  filtro: FiltroPainel;
  onAlterar: (patch: Partial<FiltroPainel>) => void;
  onLimpar: () => void;
  municipioId: number | null;
  /** Controles extras à direita da barra (colunas, mapa…). */
  extras?: React.ReactNode;
}

function opcoesCriterio(cat: CatalogoDto | undefined, codigo: string): OpcaoFiltro[] {
  return (criterioPorCodigo(cat, codigo)?.opcoes ?? []).map((o) => ({ valor: o.codigo ?? '', rotulo: o.rotulo ?? o.codigo ?? '' })).filter((o) => o.valor);
}

const idStr = (n: number | null) => (n == null ? '' : String(n));
const strId = (s: string) => (s ? Number(s) : null);

/** Barra de busca + painel recolhível com os filtros combináveis da seção 5.3. */
export function QueueFilters({ filtro, onAlterar, onLimpar, municipioId, extras }: QueueFiltersProps) {
  const [aberto, setAberto] = React.useState(false);
  const ativos = contarFiltrosAtivos(filtro);
  const temAlgo = ativos > 0 || !!filtro.busca || !!filtro.ordenarPor;

  return (
    <div className="rounded-md border border-line bg-white">
      <div className="flex flex-wrap items-center gap-2 px-3 py-2">
        <BuscaRapida value={filtro.busca} onChange={(busca) => onAlterar({ busca })} />
        <Button variant={aberto ? 'outline' : 'secondary'} size="sm" onClick={() => setAberto((v) => !v)} aria-expanded={aberto} aria-controls="painel-filtros">
          <SlidersHorizontal /> Filtros{ativos > 0 && <span className="rounded bg-primary px-1 text-xs text-white">{ativos}</span>}
        </Button>
        {temAlgo && (
          <Button variant="ghost" size="sm" onClick={onLimpar}>
            <FilterX /> Limpar filtros
          </Button>
        )}
        <div className="ml-auto flex flex-wrap items-center gap-2">{extras}</div>
      </div>
      {aberto && (
        <div id="painel-filtros" className="border-t border-line px-3 py-3">
          <CamposFiltro filtro={filtro} onAlterar={onAlterar} municipioId={municipioId} />
        </div>
      )}
    </div>
  );
}

function BuscaRapida({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const [local, setLocal] = React.useState(value);
  React.useEffect(() => setLocal(value), [value]);
  const ref = React.useRef(onChange);
  ref.current = onChange;
  React.useEffect(() => {
    if (local.trim() === value) return;
    const t = setTimeout(() => ref.current(local.trim()), 450);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [local]);
  return (
    <div className="relative w-full sm:w-80">
      <Search className="pointer-events-none absolute left-2 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" aria-hidden />
      <Input
        type="search"
        aria-label="Pesquisar na fila (número, endereço, ocorrência)"
        placeholder="Pesquisar número, endereço ou ocorrência…"
        className="pl-8"
        value={local}
        onChange={(e) => setLocal(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter') ref.current(local.trim());
        }}
      />
    </div>
  );
}

function CamposFiltro({ filtro, onAlterar, municipioId }: { filtro: FiltroPainel; onAlterar: (p: Partial<FiltroPainel>) => void; municipioId: number | null }) {
  const { data: cat } = useCatalogo();
  const equipes = useEquipesFiltro(municipioId);
  const subestacoes = useSubestacoesFiltro(municipioId);
  const conjuntos = useConjuntosFiltro(municipioId != null ? filtro.subestacaoId : null);

  const prioridades: OpcaoFiltro[] = (cat?.prioridades ?? []).map((p) => ({ valor: String(p.id), rotulo: `${p.codigo} · ${p.nome}` }));
  const status: OpcaoFiltro[] = StatusOrdemServicoValores.map((s) => ({ valor: s, rotulo: STATUS_OS_LABEL[s] }));
  const tiposOcorrencia: OpcaoFiltro[] = (cat?.tiposOcorrencia ?? []).filter((t) => t.ativo).map((t) => ({ valor: String(t.id), rotulo: t.nome ?? '' }));
  const risco = [{ valor: 'QUALQUER', rotulo: 'Qualquer risco à segurança' }, ...opcoesCriterio(cat, 'RISCO_SEGURANCA')];
  const essencial = [{ valor: 'QUALQUER', rotulo: 'Qualquer serviço essencial' }, ...opcoesCriterio(cat, 'SERVICO_ESSENCIAL')];
  const situacoes = Object.entries(cat?.rotulosSituacao ?? SITUACAO_CLIENTE_LABEL).map(([valor, rotulo]) => ({ valor, rotulo }));

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-6">
      <MultiSelectFilter
        id="f-prioridade"
        label="Prioridade"
        value={filtro.prioridadeIds.map(String)}
        onChange={(v) => onAlterar({ prioridadeIds: v.map(Number) })}
        opcoes={prioridades}
        todos="Todas"
      />
      <MultiSelectFilter
        id="f-status"
        label="Status"
        value={filtro.status}
        onChange={(v) => onAlterar({ status: v as StatusOrdemServico[] })}
        opcoes={status}
        todos="Pendentes (padrão)"
      />
      <SelectFilter
        id="f-manutencao"
        label="Tipo de manutenção"
        value={filtro.tipoManutencao}
        onChange={(v) => onAlterar({ tipoManutencao: v as TipoManutencao | '' })}
        opcoes={TipoManutencaoValores.map((t) => ({ valor: t, rotulo: TIPO_MANUTENCAO_LABEL[t] }))}
      />
      <SelectFilter
        id="f-ocorrencia"
        label="Tipo de ocorrência"
        value={idStr(filtro.tipoOcorrenciaId)}
        onChange={(v) => onAlterar({ tipoOcorrenciaId: strId(v) })}
        opcoes={tiposOcorrencia}
      />
      <SelectFilter id="f-prazo" label="Prazo" value={filtro.prazo} onChange={(v) => onAlterar({ prazo: v as FiltroPainel['prazo'] })}
        opcoes={[
          { valor: 'vencido', rotulo: 'Vencido' },
          { valor: 'proximo', rotulo: 'Vence nas próximas 2 h' },
        ]}
      />
      <SelectFilter id="f-risco" label="Risco de segurança" value={filtro.risco} onChange={(v) => onAlterar({ risco: v })} opcoes={risco} />
      <TextFilter id="f-bairro" label="Bairro" value={filtro.bairro} onChange={(v) => onAlterar({ bairro: v })} />
      <TextFilter id="f-logradouro" label="Rua / logradouro" value={filtro.logradouro} onChange={(v) => onAlterar({ logradouro: v })} />
      <TextFilter id="f-trecho" label="Trecho" value={filtro.trecho} onChange={(v) => onAlterar({ trecho: v })} />
      <SelectFilter id="f-pessoas" label="Pessoas afetadas" value={filtro.faixaPessoas} onChange={(v) => onAlterar({ faixaPessoas: v })} opcoes={opcoesCriterio(cat, 'PESSOAS_AFETADAS')} todos="Todas" />
      <SelectFilter id="f-ucs" label="UCs afetadas" value={filtro.faixaUcs} onChange={(v) => onAlterar({ faixaUcs: v })} opcoes={opcoesCriterio(cat, 'UCS_AFETADAS')} todos="Todas" />
      <SelectFilter id="f-essencial" label="Instalação / serviço essencial" value={filtro.servicoEssencial} onChange={(v) => onAlterar({ servicoEssencial: v })} opcoes={essencial} />
      <SelectFilter id="f-equipamento" label="Equipamento" value={filtro.equipamento} onChange={(v) => onAlterar({ equipamento: v })} opcoes={opcoesCriterio(cat, 'EQUIPAMENTO_AFETADO')} />
      <SelectFilter
        id="f-equipe"
        label="Equipe atribuída"
        value={idStr(filtro.equipeId)}
        onChange={(v) => onAlterar({ equipeId: strId(v) })}
        opcoes={(equipes.data ?? []).map((e) => ({ valor: String(e.id), rotulo: `${e.codigo ?? ''} — ${e.nome ?? ''}` }))}
        todos={equipes.isLoading ? 'Carregando…' : 'Todas'}
      />
      <DateFilter id="f-de" label="Aberta de" value={filtro.abertaDe} onChange={(v) => onAlterar({ abertaDe: v })} />
      <DateFilter id="f-ate" label="Aberta até" value={filtro.abertaAte} onChange={(v) => onAlterar({ abertaAte: v })} />
      <SelectFilter
        id="f-circuito"
        label="Circuito (subestação)"
        value={idStr(filtro.subestacaoId)}
        onChange={(v) => onAlterar({ subestacaoId: strId(v), conjuntoId: null })}
        opcoes={(subestacoes.data ?? []).map((s) => ({ valor: String(s.id), rotulo: `${s.codigo ?? ''}${s.nome ? ` — ${s.nome}` : ''}` }))}
        todos={municipioId == null ? 'Selecione uma cidade' : 'Todos'}
        disabled={municipioId == null}
      />
      <SelectFilter
        id="f-conjunto"
        label="Conjunto elétrico"
        value={idStr(filtro.conjuntoId)}
        onChange={(v) => onAlterar({ conjuntoId: strId(v) })}
        opcoes={(conjuntos.data ?? []).map((c) => ({ valor: String(c.id), rotulo: c.numero ?? String(c.id) }))}
        todos={filtro.subestacaoId == null ? 'Selecione um circuito' : 'Todos'}
        disabled={municipioId == null || filtro.subestacaoId == null}
      />
      <TextFilter id="f-trafo" label="Transformador" value={filtro.transformador} onChange={(v) => onAlterar({ transformador: v })} placeholder="Número (início)" />
      <SelectFilter
        id="f-classe"
        label="Classe do cliente"
        value={idStr(filtro.classeId)}
        onChange={(v) => onAlterar({ classeId: strId(v) })}
        opcoes={(cat?.classes ?? []).filter((c) => c.ativo).map((c) => ({ valor: String(c.id), rotulo: c.nome ?? '' }))}
        todos="Todas"
      />
      <SelectFilter id="f-situacao" label="Situação do cliente" value={filtro.situacaoCliente} onChange={(v) => onAlterar({ situacaoCliente: v })} opcoes={situacoes} todos="Todas" />
      <TextFilter id="f-uc" label="UC" value={filtro.uc} onChange={(v) => onAlterar({ uc: v })} placeholder="Número da UC" />
      <div className="flex items-end pb-1.5">
        <CheckboxField id="f-encerradas" label="Incluir encerradas" checked={filtro.incluirEncerradas} onCheckedChange={(v) => onAlterar({ incluirEncerradas: v })} />
      </div>
    </div>
  );
}
