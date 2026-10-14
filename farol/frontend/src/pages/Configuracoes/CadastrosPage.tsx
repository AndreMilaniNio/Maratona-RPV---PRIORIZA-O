import { PageHeader } from '@/components/layout/PageHeader';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { MunicipiosTab } from '@/features/administracao/cadastros/components/municipios/MunicipiosTab';
import { LocalidadesTab } from '@/features/administracao/cadastros/components/localidades/LocalidadesTab';
export default function CadastrosPage() { return <div><PageHeader title="Cadastros da rede" description="Municípios e localidades usados para segmentar a operação e identificar transformadores." /><Tabs defaultValue="municipios"><TabsList><TabsTrigger value="municipios">Municípios</TabsTrigger><TabsTrigger value="localidades">Localidades</TabsTrigger></TabsList><TabsContent value="municipios"><MunicipiosTab /></TabsContent><TabsContent value="localidades"><LocalidadesTab /></TabsContent></Tabs></div>; }
