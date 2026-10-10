# Progresso do MVP — Farol / PriorizaOS

Atualizado em 10/10/2026 (America/Sao_Paulo).

## Atualização local mais recente

- .NET 10.0.401 e PostgreSQL 18 estão instalados; a API está ativa em `http://localhost:5080` e o frontend em `http://localhost:5173`.
- O Construtor de Casos cria e edita perguntas, alternativas e a opção de desconhecido. A edição preserva o código interno do caso.
- O catálogo recebe a cidade selecionada. O caso demonstrativo **Chamado para o setor de TI** foi publicado para Lumiara, com 20/10/5/0 pontos, e não é exibido em Cerro Anil.
- Uma prévia enviada à API confirmou que a alternativa **Analista** entrou na explicação de classificação com `+20` pontos.
- Validações concluídas: `dotnet build Farol.sln --no-restore` (0 erros), `dotnet test tests/Farol.UnitTests --no-build --no-restore` (63 testes) e `npm.cmd run build` (sucesso; apenas avisos externos do SignalR).
- Todas as rotas visíveis do menu foram concluídas com dados do backend: dashboard, painel operacional, mapa, equipes, detalhe de equipe, detalhe da OS, cadastros, prioridades e prazos, regras de prioridade, usuários e auditoria.
- O painel usa a fila e os indicadores do servidor; o mapa exibe OS, equipes e subestações; páginas administrativas consultam os registros persistidos.
- Adicionada revisão manual de prioridade no detalhe da OS: o operador escolhe uma prioridade ou restaura a calculada, informa justificativa obrigatória e a API registra histórico e auditoria.
- Adicionados `MANUAL_DE_USO.md` e `MANUAL_TECNICO_E_TELAS.md`, com o fluxo de uso, papel de cada tela e estado dos serviços locais.

## Estado verificado

| Fluxo | Estado | Evidência |
|---|---|---|
| Estrutura API, EF Core e PostgreSQL | Parcial / não executado | A solução `backend/Farol.sln`, entidades, migration inicial e serviços existem; este ambiente não possui `dotnet` nem Docker. |
| Cadastro de solicitação/OS e persistência | Parcial / não executado | `SolicitacaoService`, `MontadorOrdemServico`, entidades e testes de integração existem, mas não foi possível subir PostgreSQL. |
| Cálculo, precedência, ranking e histórico | Parcial / não executado | `MotorPriorizacao`, `ServicoClassificacao`, `PoliticaOrdenacao`, resultados imutáveis e testes unitários já existem; compilação .NET está bloqueada pelo SDK ausente. |
| Isolamento municipal | Parcial / não executado | `MunicipioId` é obrigatório nas entidades e consultas; há testes `CidadesEPermissoesTests`, ainda não executados. |
| Tela de configuração de pontos/faixas | Funcional no frontend | Página React agora usa a API para criar/salvar rascunho, editar pontos/faixas, publicar e consultar versões; `npm run build` passou. |
| Formulário municipal totalmente configurável | Falta | A estrutura atual ainda é catálogo de critérios administrados + pontos por versão. Falta permitir ao gestor criar, editar, remover e reordenar perguntas e alternativas por município, com fotografia por versão. |
| Teste de aceite PostgreSQL ponta a ponta | Bloqueado | Não há SDK .NET, Docker nem banco PostgreSQL disponível no ambiente. |

## Alterações desta sessão

- Ativado o modo local de operador único (`Farol:ModoOperadorUnico=true`): a API libera as políticas sem JWT, mas preserva uma identidade interna de administrador para auditoria, persistência e escopo municipal.
- O seed cria o operador técnico somente quando necessário; não há credencial exposta ao usuário final.
- A inicialização do frontend consulta diretamente `/api/me`, não redireciona para login e a rota `/login` volta ao painel.
- Adicionado `frontend/src/services/api/pontuacaoService.ts`, centralizando os endpoints de configuração de pontuação.
- `backend/src/Farol.Application/Services/PontuacaoService.cs` agora exige o escopo municipal do usuário nas operações municipais de leitura, rascunho, simulação, impacto, publicação e aprovação; recusas usam o mecanismo de auditoria já existente em `EscopoMunicipio`.
- Substituído o placeholder `pages/ConfiguracaoPontuacao/ConfiguracaoPontuacaoPage.tsx` por uma tela conectada à API:
  - cria rascunho por escopo municipal;
  - altera critérios habilitados, pontos e confirmação;
  - altera faixas de classificação;
  - mostra pendências/alertas retornados pelo backend;
  - publica com justificativa e política `SomenteNovas` ou `ReclassificarAbertas`;
  - mostra histórico de versões.

## Comandos executados

| Comando | Resultado |
|---|---|
| `dotnet build Farol.sln --no-restore` | Bloqueado: comando `dotnet` não instalado. |
| `docker --version` | Bloqueado: Docker não instalado. |
| `npm.cmd ci` | Passou; dependências do frontend instaladas conforme lockfile. |
| `npm.cmd run build` | Passou; TypeScript e Vite concluíram a build. Avisos não bloqueantes vieram de comentários `PURE` no pacote SignalR. |
| `npm.cmd test` | Passou (testes Vitest disponíveis: schema de solicitação, interpretação de transformador e menu lateral). |
| `dotnet build Farol.sln --no-restore` | Passou com 0 erros; há 8 avisos preexistentes de nulidade em `AdminService`. |
| `dotnet test tests/Farol.UnitTests --no-build` | Passou: 63 testes. |
| `dotnet test tests/Farol.ArchitectureTests --no-build` | Passou: 6 testes. |

> O SDK .NET 10.0.401 foi instalado. Ainda falta PostgreSQL para subir a API e executar os testes de integração.

## Decisões técnicas

- O frontend não calcula prioridade e não guarda formulário/OS em `localStorage`; ele envia mudanças à API existente.
- A tela usa o município selecionado no contexto da sessão. Quando houver configuração global herdada, informa isso explicitamente e permite iniciar uma versão municipal.
- Versões e histórico são exibidos a partir do backend; não são reconstruídos no cliente.
- Nenhuma regra demonstrativa foi tratada como oficial.

## Pendências ordenadas

1. Instalar .NET SDK compatível com a solução e disponibilizar PostgreSQL/Docker; executar build, testes unitários, arquitetura e integração.
2. Evoluir o domínio para um formulário por município, inicialmente vazio, com perguntas/alternativas editáveis e ordenáveis pelo gestor, e snapshot imutável por versão.
3. Adaptar o formulário de nova solicitação para buscar o formulário publicado do município e persistir respostas vinculadas à sua versão.
4. Criar migration segura, testes de isolamento Cataguases/Leopoldina e o aceite ponta a ponta descrito no prompt.
5. Implementar as demais telas ainda marcadas como `Em construção` somente quando não forem cobertas pelo fluxo central.

## Próxima ação exata

Com o SDK .NET e PostgreSQL disponíveis, rodar `dotnet build Farol.sln`, depois `dotnet test tests/Farol.UnitTests`, `dotnet test tests/Farol.ArchitectureTests` e os testes de integração com `FAROL_TEST_CONNECTION`; corrigir qualquer falha antes de introduzir a migration do formulário municipal versionado.
