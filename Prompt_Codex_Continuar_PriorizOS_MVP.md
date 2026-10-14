# CODEX — CONTINUIDADE DO PRIORIZAOS / FAROL (MVP FUNCIONAL)

> **Instrução principal:** este projeto foi iniciado no Claude e já contém código. **Não crie outro projeto, não comece do zero e não substitua implementações existentes sem necessidade comprovada.** Seu trabalho é descobrir o estado real do repositório, corrigir o que não funciona e concluir o MVP usando a base atual.

## 1. Contexto e arquivos

- **Repositório existente:** `Maratona-RPV---PRIORIZA-O-main`, com a aplicação em `farol/`.
- **Documento de domínio:** `Projeto_PriorizaOS_Requisitos_Completos.md` (ou o TXT `prompt_projeto_v1_corrigido(3).txt`, caso seja o arquivo disponível). É referência para conceitos, campos, segurança e arquitetura, **não uma ordem para implementar todas as funcionalidades agora**.
- **Este documento:** define a **continuidade**, as decisões atualizadas e o escopo do MVP. **Prevalece em conflitos** com o documento antigo.
- O histórico privado da conversa com o Claude pode não estar disponível. **O código, as migrações, os testes, os READMEs, o histórico Git e os arquivos de progresso são a fonte da verdade sobre onde o desenvolvimento parou.** Não suponha que algo funciona só porque existe uma classe ou uma tela.

## 2. Descubra onde o Claude parou — sem refazer o projeto

Antes de alterar qualquer arquivo:

1. Inspecione a raiz e `farol/`; leia `farol/README.md`, os arquivos relevantes em `docs/`, a configuração de execução e eventuais `TODO`, `PROGRESSO`, `AGENTS.md` e instruções locais.
2. Se houver histórico Git, verifique status, branch, últimos commits e mudanças não commitadas. **Não descarte, reverta, sobrescreva ou faça reset de trabalho existente.**
3. Mapeie os componentes **já existentes**: frontend React/TypeScript/Vite, API ASP.NET Core, entidades/serviços de domínio, Entity Framework/PostgreSQL, migrations, autenticação, priorização, OS, formulários, histórico e testes.
4. Tente executar os comandos previstos pelo projeto (`dotnet build`, `dotnet test`, instalação/build do frontend e, se disponível, `docker compose`), adaptando-se ao ambiente. Não invente resultados. Se faltar Docker, .NET, banco ou credenciais, documente o bloqueio e prossiga nas partes verificáveis.
5. Classifique cada fluxo como **funcional e testado**, **parcial**, **ausente** ou **bloqueado**. Cite arquivos/rotas e evidências concretas. Faça um diagnóstico breve, não uma auditoria interminável.
6. Identifique o **menor conjunto de alterações** que conecta os módulos já prontos e conclui o MVP. Reutilize o que existir.

**Após o diagnóstico, comece imediatamente a implementar as pendências de maior impacto, sem aguardar uma nova confirmação.** Não pare apenas no planejamento.

## 3. Objetivo do MVP (funcionamento antes de aparência)

Entregar um fluxo integrado e persistente:

**Gestor configura formulário do município → publica → usuário cadastra solicitação/OS e responde → backend calcula prioridade → OS aparece no ranking correto → alterações recalculam quando aplicável → histórico explica o resultado.**

Requisitos do fluxo:

- **Municípios:** toda OS e todo formulário pertence a um município; consultas, ranking e permissões devem respeitar o município também no backend. Usar Cataguases e Leopoldina apenas como exemplos demonstrativos.
- **Formulários criados pelo gestor:** o próprio gestor cria um formulário inicialmente vazio, adiciona, edita, remove e reordena perguntas e alternativas, define pontos por resposta, testa/simula, salva rascunho e publica. **Não exigir soma de 100 pontos e não exigir avaliador/Usuário Chave ou segunda aprovação.** Formulários de municípios diferentes são independentes.
- **Versões:** alterar formulário publicado cria nova versão sem apagar versões/respostas anteriores; política explícita para aplicar mudanças a novas OS e/ou reclassificar abertas.
- **Cadastro:** preservar a distinção entre solicitação e OS se já existir no domínio; gerar identificador único no servidor, registrar município, descrição, manutenção corretiva/preventiva, tipo de ocorrência, localização, impacto, segurança e respostas ao formulário. Permitir UC desconhecida. Persistir em PostgreSQL.
- **Prioridade:** calcular **no backend**, com soma das respostas configuradas, classificação/faixas configuráveis e justificativa detalhada. **Regras de segurança/precedência ficam separadas da pontuação** e não podem ser anuladas por soma comum. Não inventar critérios, pesos, prazos ou regras oficiais da concessionária: usar apenas exemplos claramente rotulados **DEMONSTRATIVOS / NÃO OFICIAIS** quando necessário.
- **Ranking:** ordenação determinística por município, precedência, classificação, pontos e desempates documentados; filtros básicos, consulta dos detalhes, atualização após cadastro/edição, sem reiniciar o sistema.
- **Auditoria:** registrar criação/edição de OS, respostas, versão do formulário, publicação, resultado de cálculo e reclassificações; preservar histórico e justificativas.
- **Acesso:** reutilizar autenticação e permissões existentes; não introduzir atalhos que exponham dados entre municípios.

## 4. Preservar a arquitetura e o trabalho existente

- **Manter o stack encontrado no repositório:** C#/ASP.NET Core, Entity Framework Core, PostgreSQL e React/TypeScript/Vite, respeitando a estrutura atual de `farol/`.
- Preferir **corrigir e completar serviços, controllers, DTOs, componentes, migrations e testes existentes** a criar versões paralelas.
- Não renomear o projeto, reestruturar tudo, trocar banco, recriar schema ou apagar dados sem necessidade técnica explícita.
- Não remover módulos existentes de **equipes, despacho, mapa ou geolocalização**. Se já funcionarem, preservá-los; se estiverem incompletos e não bloquearem o fluxo central, registrar para a próxima fase.
- **Fora da prioridade atual:** refazer identidade visual, animações, roteirização, GPS ao vivo, integrações externas oficiais e despacho automático. Interface simples e funcional é suficiente.
- Nunca substituir o backend por `localStorage`, arrays em memória ou telas com botões falsos.

## 5. Ordem de trabalho orientada pelo estado real

1. **Recuperar execução:** corrigir erros de build, inicialização, migrations, conexão frontend/API/banco e configuração do ambiente.
2. **Aproveitar cadastro e ranking atuais:** verificar se a OS realmente é gravada, classificada e exibida; consertar o que estiver quebrado.
3. **Adaptar a configuração de prioridade existente:** onde o projeto ainda usar critérios fixos e perfil `Usuário Chave`, evoluir para **formulários inteiramente configuráveis pelo gestor** com publicação/versionamento por município. Migrar de forma segura, preservando dados e compatibilidade quando possível.
4. **Integrar tudo:** respostas do formulário ativo alimentam o motor de cálculo existente ou sua evolução; resultado e explicação chegam ao ranking e detalhes.
5. **Testar e finalizar:** testes unitários do motor, integração de persistência, isolamento por município e fluxo completo de ponta a ponta; corrigir erros descobertos.

Essa ordem pode ser ajustada após o diagnóstico. **Não implemente novamente o que já estiver comprovadamente pronto.**

## 6. Teste de aceite obrigatório

Demonstre, com dados fictícios:

1. Gestor cria **do zero** um formulário para Cataguases, com perguntas/opções/pontos cuja soma não é 100; publica sozinho.
2. Cadastra uma OS de Cataguases, responde ao formulário e confirma persistência no PostgreSQL.
3. Backend calcula a prioridade e mostra os fatores que influenciaram a decisão; a OS aparece no ranking de Cataguases.
4. Edita a OS ou suas respostas; o cálculo/ranking muda quando aplicável e o histórico registra a mudança.
5. Cria outro formulário para Leopoldina; as regras, OS e ranking de Cataguases não se misturam aos de Leopoldina.
6. Publica nova versão de formulário sem perder os resultados e versões anteriores.
7. Reinicia a aplicação; os dados continuam disponíveis.
8. Testa que uma ocorrência com risco crítico não perde precedência apenas porque outra acumulou mais pontos.

Não declare uma etapa pronta se o fluxo correspondente não foi testado; diferencie teste automatizado, verificação manual e pendência por limitação do ambiente.

## 7. Relatório e retomada entre sessões

- No começo, apresente uma tabela **Pronto / Parcial / Falta / Bloqueado**, curta e baseada em evidências, com a **primeira tarefa escolhida**.
- Depois implemente. Ao final de cada bloco significativo, informe **o que alterou, quais comandos/testes executou, resultado e próximo passo**, sem relatórios longos.
- Crie ou atualize `farol/PROGRESSO_MVP.md` com: estado real do projeto, arquivos alterados, comandos de execução, testes que passaram/falharam, pendências ordenadas, decisões técnicas e **próxima ação exata**. Assim outra sessão do Codex continua sem recomeçar.
- Atualize o README apenas onde necessário para tornar a execução reproduzível; nunca exponha credenciais.
- Se atingir o limite da sessão, deixe o código em estado consistente e o progresso documentado. **Não afirme que concluiu o MVP se ainda faltarem integrações.**

## 8. Comece agora

**Abra o projeto existente em `farol/`, descubra o ponto real em que o Claude parou, valide o que já existe e continue o desenvolvimento até concluir o MVP funcional. Não crie um projeto novo. Não pare no diagnóstico.**
