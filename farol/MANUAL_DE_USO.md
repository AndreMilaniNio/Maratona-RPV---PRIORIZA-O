# Manual de uso — PriorizaOS

## Abrir o sistema

Com os serviços locais ativos, abra **http://localhost:5173** no navegador. O modo local é de operador único: não há login e as ações ficam registradas como realizadas pelo operador do sistema.

No cabeçalho, escolha a cidade antes de registrar, consultar ou configurar uma operação. A escolha limita a fila, o mapa, as equipes e os casos municipais.

## Fluxo diário sugerido

1. Abra **Nova solicitação**.
2. Escolha a cidade, tipo de ocorrência, manutenção e local da ocorrência.
3. Informe a UC ou marque que o solicitante não a conhece, com o motivo e uma forma de localizar o local.
4. Responda aos **Casos configurados** e descreva a ocorrência.
5. Clique em **Registrar e gerar OS**. A API cria a solicitação, gera a OS e calcula sua classificação.
6. Acesse **Painel operacional** para acompanhar a OS na fila e abrir seus detalhes.
7. Use **Equipes** e o detalhe da OS para organizar o atendimento.

## Decidir a prioridade de uma OS

O cálculo automático continua sendo a referência, mas o operador autorizado pode tomar uma decisão manual:

1. No **Painel operacional**, clique no número da OS ou em **Detalhes**.
2. Na tela da OS, encontre **Revisar prioridade manualmente**.
3. Escolha a prioridade desejada. Para voltar ao cálculo automático, escolha **Usar prioridade calculada**.
4. Escreva a justificativa obrigatória.
5. Clique em **Aplicar decisão**.

A decisão manual é gravada no histórico e na auditoria; ela não altera os pontos nem apaga a classificação originalmente calculada.

## Formulário e pontuação

Em **Formulário e pontuação**, selecione uma cidade e use o **Construtor de Casos** para criar perguntas próprias. Cada caso possui alternativas e uma opção de desconhecido.

Depois de salvar o caso:

1. Crie um rascunho.
2. Habilite o caso e defina os pontos de cada alternativa.
3. Ajuste as faixas de prioridade se necessário.
4. Salve e publique, registrando a justificativa.

Somente uma versão publicada passa a valer para novas OS. Casos municipais aparecem somente para a cidade configurada.

## Demais telas

| Tela | Uso |
|---|---|
| Dashboard | Indicadores e OS no topo da fila. |
| Painel operacional | Filtros, busca, ordenação e acesso a cada OS. |
| Mapa operacional | Visualiza OS, equipes e subestações com coordenadas. |
| Equipes | Cadastra, edita, consulta capacidade e muda o status operacional. |
| Cadastros da rede | Mantém municípios e localidades de transformadores. |
| Códigos e prazos | Consulta prioridades e prazos configurados. |
| Regras de prioridade | Consulta regras de precedência que podem elevar uma OS. |
| Usuários e permissões | Consulta os vínculos e perfis do ambiente local. |
| Histórico / auditoria | Consulta as alterações registradas no servidor. |

## Cuidados

- Dados demonstrativos não representam regras oficiais.
- Não exponha a API local sem autenticação em rede pública.
- Sempre justifique revisões manuais, mudança de status e publicação de uma versão de pontuação.
