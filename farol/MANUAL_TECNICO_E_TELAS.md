# Manual técnico e mapa das telas — PriorizaOS

## O que está em execução localmente

| Componente | Endereço | O que faz |
|---|---|---|
| Frontend React/Vite | `http://localhost:5173` | Exibe as telas e envia comandos à API. |
| API ASP.NET Core | `http://localhost:5080` | Aplica regras, persiste dados e expõe Swagger/API. |
| PostgreSQL | Banco local `farol` | Guarda solicitações, OS, pontuação, equipes e auditoria. |

O frontend não calcula prioridade localmente. Toda classificação, publicação de versão, despacho e revisão manual são executados pelo backend e persistidos no PostgreSQL.

## O que acontece em cada janela

### Dashboard

Consulta indicadores e as primeiras OS da fila. Os cartões usam a API de indicadores, e cada OS abre seu detalhe.

### Nova solicitação

Carrega o catálogo da cidade. Ao enviar, registra a solicitação, monta a OS, extrai fatos, aplica a versão de pontuação vigente e devolve número, prioridade, pontos e alertas.

### Painel operacional

Consulta a fila paginada com filtros de texto, status, ocorrência, prazo, rede e equipe. A ordenação e a posição são determinadas pelo servidor. Abrir uma OS leva ao detalhe; o despacho segue a partir dessa área operacional.

### Detalhes da OS

Mostra dados persistidos: classificação explicável, localização, descrição, despacho e histórico. A seção **Revisar prioridade manualmente** chama `POST /api/ordens-servico/{id}/reclassificar`; exige justificativa e registra auditoria.

### Mapa operacional

Consulta `GET /api/mapa/operacoes` no recorte municipal. Desenha apenas itens com coordenadas; OS sem coordenadas são avisadas abaixo do mapa.

### Equipes e detalhe de equipe

Consulta capacidade, status, integrantes, qualificações, recursos e OS atribuídas. A tela de detalhe atualiza o status pela API e a alteração atualiza fila e indicadores.

### Formulário e pontuação

Mantém versões de critérios e faixas. O Construtor de Casos grava perguntas personalizadas; o rascunho define quais entram na pontuação e a publicação torna uma versão vigente.

### Cadastros da rede

Mantém municípios e localidades. A localidade compõe os três primeiros dígitos do identificador do transformador conforme o modelo do projeto.

### Códigos e prazos, regras, usuários e histórico

Essas janelas consultam diretamente as configurações administrativas e a auditoria persistida. Elas permitem acompanhar o estado real do ambiente sem usar dados simulados no navegador.

## Estado atual e limites conhecidos

- Modo local de operador único ativo: adequado para demonstração, não para exposição pública.
- Os valores demonstrativos existentes devem ser substituídos por regras aprovadas pela operação.
- Administração completa de prioridades, regras e usuários deve ser feita pela API administrativa enquanto a operação decide o fluxo de aprovação final; as telas atuais exibem os dados cadastrados.
- O escopo municipal é aplicado ao catálogo de casos, fila, mapa, equipes e configurações de pontuação.

## Verificações finais executadas

- API de saúde local respondendo.
- Frontend compilado com TypeScript e Vite sem erros.
- Build do backend concluído.
- Testes unitários e de arquitetura já existentes concluídos anteriormente; esta entrega não adicionou uma nova suíte de testes.
