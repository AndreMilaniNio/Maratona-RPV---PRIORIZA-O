# Farol

Sistema integrado de gestão, priorização e despacho de Ordens de Serviço (OS) da rede elétrica, segmentado por cidade.
Uma solicitação registrada na tela **Nova Solicitação** gera uma OS no PostgreSQL, é classificada no servidor (regras de segurança + pontuação definida pelo **Usuário Chave**) e aparece automaticamente na fila do **Painel Operacional**, de onde é despachada para a equipe mais adequada.

> **Aviso:** todos os dados carregados no modo demonstrativo são fictícios (derivados de `Experimento_Dados_Ficticios - BASE CLIENTES 1.xlsx`), e todas as pontuações, faixas, prazos e regras de precedência são **demonstrativos, não oficiais**. O sistema não está pronto para operação crítica antes da validação pela operação e da integração com os sistemas oficiais.

- Documento de design (decisões, contratos, esquema, fluxos): [`.design/farol.md`](.design/farol.md)
- Regras de negócio e premissas: [`docs/regras-de-negocio.md`](docs/regras-de-negocio.md)
- Contrato OpenAPI exportado: [`docs/openapi.json`](docs/openapi.json) (Swagger em `/swagger` com a API rodando)

## Estrutura

```text
farol/
├── backend/
│   ├── src/
│   │   ├── Farol.Domain/          entidades, enums, regras puras (transformador, coordenadas, máquina de estados)
│   │   ├── Farol.Application/     DTOs, validações, motor de priorização, serviços de caso de uso
│   │   ├── Farol.Infrastructure/  EF Core/PostgreSQL, Identity/JWT, provedores de CEP e rotas, seed
│   │   └── Farol.API/             controllers REST, SignalR, middleware, Swagger
│   ├── tests/
│   │   ├── Farol.UnitTests/         motor, ordenação, prazos, faixas, validações, domínio
│   │   ├── Farol.IntegrationTests/  API real + PostgreSQL real (fluxos, concorrência, permissões)
│   │   └── Farol.ArchitectureTests/ direção das dependências entre camadas
│   └── Dockerfile
├── frontend/                React + TypeScript + Vite (ver frontend/README.md)
├── database/
│   ├── seed/base_clientes_ficticia.csv   subconjunto da planilha fictícia (embutido no seed)
│   └── migracoes.sql                     script SQL idempotente das migrações EF Core
├── docs/
├── tests/                   orientações dos testes ponta a ponta
├── docker-compose.yml
├── .env.example
└── README.md
```

## Arquitetura e decisões técnicas

| Tema | Decisão |
|---|---|
| Backend | .NET 10 (LTS), ASP.NET Core Web API, EF Core 10 + Npgsql 10, Identity + JWT, FluentValidation 12, SignalR, Swashbuckle 10 |
| Banco | PostgreSQL 18 (compatível com 15+), nomes em snake_case, instantes em UTC (`timestamptz`), concorrência otimista via `xmin` |
| Camadas | Domain ← Application ← Infrastructure ← API (verificado por testes de arquitetura) |
| Priorização | Motor **puro** (`MotorPriorizacao`) usado pela criação, prévia, simulador, impacto e reclassificação — não há regra no frontend |
| Histórico | Cada cálculo é um resultado imutável vinculado à versão de pontuação; nada é sobrescrito |
| Cidades | `municipio_id` obrigatório em solicitação e OS; escopo aplicado no servidor em toda consulta |
| Numeração | contador por cidade/ano em `numeracao_sequencias` com upsert atômico + índice único |
| Concorrência | despacho com bloqueio de linha (OS e equipe) + índice único parcial de despacho ativo; rascunho de pontuação com `xmin` |
| Tempo real | SignalR (`/hubs/operacao`, grupos por cidade) + atualização periódica de 60 s como reconciliação |
| Geografia | latitude/longitude + haversine. PostGIS foi avaliado e adiado: só compensa quando houver geometria oficial de trechos |
| Integrações | interfaces com provedores configuráveis; sem configuração, nada é inventado (CEP local, sem tempo de rota) |

## Pré-requisitos

- .NET SDK 10
- Node.js 20+ (testado com 24) e npm
- PostgreSQL 15+ local **ou** Docker com Docker Compose

## Configuração do ambiente

Copie `.env.example` para `.env` e preencha. Variáveis usadas pela API (podem vir do `.env` via compose ou do ambiente):

| Variável | Uso |
|---|---|
| `ConnectionStrings__Farol` | conexão PostgreSQL (`Host=…;Port=…;Database=farol;Username=…;Password=…`) |
| `Jwt__Chave` | chave do JWT, ≥ 32 caracteres |
| `FAROL_ADMIN_EMAIL` / `FAROL_ADMIN_SENHA` | cria o administrador inicial na primeira subida (não há senha padrão) |
| `Farol__Demo__Habilitado` / `Farol__Demo__SenhaUsuarios` | carrega a base fictícia, equipes, usuários `*@farol.demo` e 12 OS de cenário |
| `Farol__Roteamento__UrlBase` | serviço de rotas compatível com OSRM (opcional) |
| `Farol__Cep__Provedor` / `Farol__Cep__UrlModelo` | `Demonstrativo` (base local) ou `Http` com URL contendo `{cep}` |
| `Farol__Pontuacao__ExigirAprovacao` | publicação de pontuação exige aprovação de um segundo usuário |
| `Farol__Uc__Formato`, `Farol__Transformador__TamanhoMaximo`, `Farol__Numeracao__*` | formatos configuráveis |
| `Farol__Reclassificacao__IntervaloSegundos` | intervalo da reclassificação automática (padrão 60) |

Portas: API **5080**, frontend **5173**, PostgreSQL **5432**.

## Execução com Docker Compose

```bash
cp .env.example .env    # preencha POSTGRES_PASSWORD, JWT_CHAVE e FAROL_DEMO_SENHA
docker compose up --build
```

- Frontend: http://localhost:5173 · API: http://localhost:5080 · Swagger: http://localhost:5080/swagger
- As migrações rodam automaticamente na subida da API (`Farol:Banco:InicializarAoSubir`).

## Execução local (sem Docker)

### PostgreSQL
Use uma instância existente ou crie um banco: `createdb -U <usuario> farol`.

### Migrações
A API aplica as migrações ao subir. Para aplicar manualmente:

```bash
dotnet tool install --global dotnet-ef
cd backend
dotnet ef database update -p src/Farol.Infrastructure -s src/Farol.API
# ou execute database/migracoes.sql (idempotente) no psql
```

### Backend
```bash
cd backend
export ConnectionStrings__Farol="Host=localhost;Port=5432;Database=farol;Username=farol;Password=<senha>"
export Jwt__Chave="<chave com 32+ caracteres>"
export Farol__Demo__Habilitado=true Farol__Demo__SenhaUsuarios="<senha demo>"
export ASPNETCORE_URLS=http://localhost:5080
dotnet run --project src/Farol.API
```
(No PowerShell: `$env:Jwt__Chave="..."`.)

### Frontend
```bash
cd frontend
cp .env.example .env    # VITE_API_URL=http://localhost:5080
npm install
npm run dev             # http://localhost:5173
```

### Usuários demonstrativos
Com o modo demonstrativo, todos usam a senha definida em `Farol__Demo__SenhaUsuarios`:

| E-mail | Perfil | Cidades |
|---|---|---|
| admin@farol.demo | Administrador | todas |
| supervisor@farol.demo | Supervisor | todas |
| despachante@farol.demo | Despachante | Lumiara, Cerro Anil |
| atendente@farol.demo | Atendente | Lumiara, Cerro Anil |
| atendente.vtq@farol.demo / despachante.vtq@farol.demo | Atendente / Despachante | Vale Turquesa |
| chave@farol.demo / chave2@farol.demo | Usuário Chave (titular e substituto) | todas |
| equipe.lum03@farol.demo | Equipe de campo (LUM-03) | Lumiara |

## Testes

```bash
cd backend
dotnet test tests/Farol.UnitTests
dotnet test tests/Farol.ArchitectureTests
# Integração: banco DESCARTÁVEL — é apagado e recriado a cada execução
export FAROL_TEST_CONNECTION="Host=localhost;Port=5432;Database=farol_test;Username=farol;Password=<senha>"
dotnet test tests/Farol.IntegrationTests

cd ../frontend && npm test
```

Cobertura principal: determinismo e explicabilidade do cálculo; precedência que pontos não neutralizam; desconhecido ≠ zero; desempates; corretiva × preventiva com exceções; calendário útil e feriados; faixas com lacuna/sobreposição; opções sem pontos bloqueando publicação; rascunho concorrente; versão da cidade prevalecendo; nova versão sem alterar histórico; simulador = produção; criação transacional, idempotência e unicidade de números sob concorrência; "não sabe a UC"; transformador com zeros à esquerda e localidade não cadastrada; coordenadas fora do Brasil e CEP único; escopo por cidade e auditoria de acesso negado; despacho concorrente, exceção autorizada, apoio intermunicipal, ciclo completo; transições de status; revisão manual; unificação; reclassificação por tempo.

## Configurar o mapa

O mapa usa Leaflet com tiles do OpenStreetMap. Tempo de deslocamento só aparece com um serviço de rotas configurado (`Farol__Roteamento__UrlBase`, compatível com OSRM — p.ex. um contêiner `osrm/osrm-backend` com o recorte regional). Sem ele, o sistema mostra apenas distância em linha reta, rotulada como tal. Posições de equipes do modo demonstrativo são rotuladas "posição demonstrativa"; a posição pode ser atualizada por `PUT /api/equipes/{id}/localizacao` (manual ou futura integração GPS).

## Configurar as regras de prioridade

- **Pontos e faixas**: Usuário Chave → tela *Configuração de pontuação* (global ou por cidade): editar rascunho, simular, ver impacto, publicar com vigência e escolher se reclassifica as OS abertas.
- **Regras de precedência (camada A)** e **estrutura de critérios**: Administrador → *Regras de prioridade*.
- **Códigos, prazos, calendário e feriados**: Administrador → *Códigos e prazos*.

## Limitações conhecidas

- Sem integração com SGM, cadastro comercial de UCs, GIS da rede e GPS das equipes: há interfaces e dados demonstrativos.
- Trechos de rede não têm geometria (sem PostGIS); a localização é por ponto.
- Feriados móveis/municipais precisam ser cadastrados.
- O motor de critérios é de critérios fixos (versão 1); critérios personalizados são respondidos pelo atendente no formulário.
- Anexos ficam no banco (até 5 MB); para volume real, mover para armazenamento de objetos.

## Integração futura com sistemas corporativos

| Sistema | Ponto de integração |
|---|---|
| SGM (códigos e prazos oficiais) | cadastro de `prioridades` via `/api/admin/prioridades`; regras via `/api/admin/regras-precedencia` |
| Cadastro comercial (UC/cliente) | substituir a consulta local em `CadastroRedeService.UnidadeConsumidoraAsync` por um provedor externo |
| GIS da rede | carga de municípios, localidades, subestações, conjuntos e transformadores pelos endpoints `/api/admin/*` ou ETL nas tabelas correspondentes |
| GPS / app das equipes | `PUT /api/equipes/{id}/localizacao` com `origem: Gps` |
| CEP | `Farol__Cep__Provedor=Http` + `Farol__Cep__UrlModelo` |
| Rotas | `Farol__Roteamento__UrlBase` (OSRM) |
| Canais de entrada (URA, portal) | `POST /api/solicitacoes` com `canal: Integracao` e `chaveIdempotencia` |
