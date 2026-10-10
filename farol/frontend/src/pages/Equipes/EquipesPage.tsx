import * as React from 'react';
import { Plus } from 'lucide-react';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { LoadingState, ErrorState, EmptyState } from '@/components/feedback/states';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { useAuth } from '@/features/autenticacao/hooks/AuthProvider';
import { useEquipes } from '@/features/equipes/hooks/useEquipes';
import { EquipesTable } from '@/features/equipes/components/EquipesTable';
import { EquipeFormDialog } from '@/features/equipes/components/form/EquipeFormDialog';
import type { EquipeDto } from '@/types/api';

export default function EquipesPage() { const { municipioId, municipio } = useCidade(); const { usuario } = useAuth(); const equipes = useEquipes(municipioId); const [edicao, setEdicao] = React.useState<EquipeDto | null | undefined>(undefined);
  return <div className="space-y-3"><PageHeader title="Equipes" description={`Capacidade e disponibilidade em ${municipio?.nome ?? 'todas as cidades'}.`} actions={<Button onClick={() => setEdicao(null)}><Plus /> Nova equipe</Button>} />
    <Section title="Equipes cadastradas">{equipes.isLoading ? <LoadingState /> : equipes.error ? <ErrorState error={equipes.error} onRetry={() => void equipes.refetch()} /> : equipes.data?.length ? <EquipesTable equipes={equipes.data} equipeDoUsuarioId={usuario?.equipeId} onEditar={setEdicao} /> : <EmptyState title="Nenhuma equipe encontrada" description="Cadastre a primeira equipe para permitir o despacho." />}</Section>
    {edicao !== undefined && <EquipeFormDialog open onOpenChange={(aberto) => !aberto && setEdicao(undefined)} equipe={edicao} />}</div>; }
