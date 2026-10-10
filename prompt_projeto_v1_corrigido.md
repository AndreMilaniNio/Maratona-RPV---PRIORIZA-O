# PROJETO: Sistema Integrado de Gestão, Priorização e Despacho de Ordens de Serviço Elétricas (Versão 1: pontuação definida pelo Usuário Chave)

> **Versão 1: critérios fixos, com pontuação definida pelo Usuário Chave.**
> Esta versão mantém a lista de critérios do projeto original (seção 7) e acrescenta uma tela própria para o Usuário Chave definir quantos pontos vale cada opção e quais faixas de pontos correspondem a cada prioridade. Os critérios em si só mudam por cadastro de um administrador.

## 0. Registro de alterações e premissas desta versão

### 0.1. O que foi alterado em relação ao projeto original

1. **Separação por cidade (município):** toda OS pertence a uma cidade. Fila, indicadores, equipes, despacho, mapa e permissões são separados por cidade (seção 3A).
2. **Tela nova para o Usuário Chave:** pessoa responsável por definir quantos pontos tem cada prioridade (seção 7A).
3. **Circuito:** passa a representar a subestação e o local da subestação (seção 4.2.3).
4. **Conjunto elétrico:** número do conjunto elétrico de cada subestação (seção 4.2.3).
5. **Transformador:** número do transformador ao qual o cliente está ligado. Os 3 primeiros dígitos identificam a localidade e os demais, o número do equipamento na localidade (seções 3 e 4.2.3).
6. **Situação do cliente:** ligado ou desligado (seção 4.5).
7. **Classes de cliente:** poder público, rural, essencial e residencial (seção 4.5).
8. **Busca de UC:** com a opção de informar que o solicitante não sabe a UC quando não está na própria residência (seção 4.2.1).
9. **Rua, CEP e endereço**, com registro de coordenadas e entrega das coordenadas à equipe (seções 4.2.2 e 5.5).

### 0.2. Premissas adotadas (confirmar com a operação)

Estas premissas foram assumidas para não bloquear o desenvolvimento. O agente deve implementá-las como configuráveis e listá-las ao final da entrega como pontos a confirmar.

- **Situação do cliente (ligado/desligado):** interpretada como a situação da unidade consumidora do cliente no momento do registro. É um campo distinto da "condição de fornecimento", que descreve a interrupção observada na rede. Os rótulos devem ser configuráveis.
- **Classes de cliente:** foram usadas as quatro informadas (poder público, rural, essencial e residencial). A lista é administrável, para que a operação inclua outras classes sem alterar o código. A lista "Tipo de local ou serviço afetado" do projeto original foi substituída pela classe do cliente e por um detalhamento de serviço essencial; comercial, industrial e área pública deixam de ser opções iniciais, mas podem ser cadastrados como classes.
- **Enviar coordenadas:** tratado de duas formas complementares: o atendente registra as coordenadas recebidas do solicitante, e o sistema entrega as coordenadas da OS à equipe no despacho.
- **Separação por cidade:** cada OS pertence a um município e a fila é segmentada por município. A visão consolidada de todas as cidades fica restrita a perfis autorizados.
- **Formatos não informados:** o formato do número da UC, o formato do CEP além dos 8 dígitos e o tamanho do número do transformador além dos 3 dígitos da localidade não foram fornecidos. Devem ser configuráveis, sem presumir formato oficial.
- **Pontos e faixas:** nenhum valor de pontos foi fornecido. Enquanto o Usuário Chave não publicar a primeira versão, o sistema opera com um conjunto demonstrativo identificado como não oficial.
- **Precedência e segurança:** as regras da camada A (seção 7.3) continuam separadas da pontuação. O Usuário Chave define pontos; regras de precedência seguem sob aprovação de administrador.

---

## 1. Objetivo do projeto

Desenvolva uma aplicação web completa para gestão de manutenção de redes elétricas, com foco na abertura de solicitações, geração automática de Ordens de Serviço (OS), classificação de prioridades por regras de negócio, distribuição de serviços para equipes de eletricistas e acompanhamento geográfico das operações.

A aplicação deve ser inspirada nas boas práticas de sistemas de Gestão de Manutenção (SGM), especialmente na organização de ordens de serviço, classificação de prioridades, manutenção corretiva e preventiva, prazos de atendimento, rastreabilidade e despacho de equipes.

**Importante:** não presuma conhecer os códigos, os pesos ou os prazos oficiais do SGM utilizado pela empresa. Caso a documentação oficial não seja fornecida, implemente uma estrutura configurável, com dados demonstrativos claramente identificados. Não apresente regras fictícias como regras oficiais do SGM.

O sistema deve ser integrado de ponta a ponta. Uma solicitação registrada na primeira tela deve gerar uma OS persistida no banco de dados, passar pelo mecanismo de classificação e aparecer automaticamente na fila de prioridades da segunda tela.

O sistema deve ser **segmentado por cidade (município)**. Cada solicitação e cada OS pertencem a exatamente um município, e toda a operação (fila de prioridades, indicadores, equipes, despacho, mapa e permissões) deve poder ser separada por cidade, conforme a seção 3A.

O objetivo central é responder a quatro perguntas:

1. Qual serviço deve ser atendido primeiro?
2. Por que esse serviço tem essa prioridade?
3. Qual equipe está apta e disponível para executá-lo?
4. Qual equipe representa a melhor opção considerando localização, deslocamento, capacidade e prioridade?

Não desenvolva somente um protótipo visual. Implemente frontend, backend, banco de dados, regras de negócio, persistência, validações, integração entre telas e testes.

A pontuação de cada prioridade não é definida pelo desenvolvedor nem pelo atendente. Ela é definida por uma pessoa responsável na operação, o **Usuário Chave**, por meio de uma tela própria (seção 7A). O código deve fornecer o mecanismo; os valores pertencem à operação.

---

## 2. Tecnologias obrigatórias

### Backend

- C#.
- ASP.NET Core Web API, preferencialmente com a versão LTS do .NET vigente e compatível com o ambiente.
- Entity Framework Core.
- PostgreSQL.
- API REST com contratos DTO explícitos.
- Swagger/OpenAPI.
- FluentValidation ou mecanismo equivalente de validação.
- Autenticação e autorização com ASP.NET Core Identity e JWT ou solução equivalente.
- SignalR ou mecanismo equivalente para notificações em tempo real, caso seja utilizado além da atualização periódica.
- Testes automatizados com xUnit.

### Frontend

- React.
- TypeScript em modo estrito.
- Vite.
- React Router.
- TanStack Query para gerenciamento de consultas, cache, atualização e invalidação de dados.
- React Hook Form com Zod para formulários e validações.
- Biblioteca de componentes acessíveis, preferencialmente shadcn/ui com Tailwind CSS, ou solução equivalente.
- Leaflet com OpenStreetMap para o mapa, utilizando serviços de geocodificação e roteamento configuráveis.

Escolha versões estáveis e compatíveis entre si. Registre as versões utilizadas na documentação.

### Banco de dados

PostgreSQL como banco principal.

O banco deve ser persistente, relacional e normalizado. Não utilize arrays em memória, localStorage ou dados mockados como substitutos do backend real.

Mocks são permitidos exclusivamente para testes automatizados e para um modo demonstrativo claramente identificado.

### Organização do repositório

Mantenha frontend e backend separados em pastas independentes, dentro de um único repositório:

```text
gestao-manutencao-eletrica/
├── backend/
├── frontend/
├── database/
├── docs/
├── tests/
├── docker-compose.yml
├── .env.example
├── .gitignore
└── README.md
```

O projeto deve permitir executar frontend, backend e PostgreSQL de maneira organizada, preferencialmente com Docker Compose para o ambiente de desenvolvimento.

---

## 3. Conceitos fundamentais do domínio

Implemente as seguintes entidades, mantendo seus conceitos separados.

### Solicitação

É o registro inicial da ocorrência, preenchido por quem recebe ou registra o chamado.

Contém a descrição original, a origem do contato, o endereço informado, os impactos relatados e as informações iniciais.

### Ordem de Serviço (OS)

É o registro operacional gerado a partir da solicitação.

Deve possuir identificador próprio, número de OS único, classificação, tipo de manutenção, prioridade, pontuação, prazo, localização, status e histórico.

A solicitação não deve se confundir com a OS. Uma solicitação pode gerar uma OS ou ser vinculada a uma ocorrência já existente quando for constatado que se trata do mesmo evento, mediante validação e registro de auditoria.

### Trecho da rede

Representa o segmento operacional da rede elétrica relacionado à ocorrência.

Deve permitir associar endereço, rua, pontos de referência, coordenadas, trecho elétrico identificado e equipamentos envolvidos, quando esses dados estiverem disponíveis.

Não invente identificadores de trechos ou equipamentos reais. Quando não houver integração com o cadastro oficial da rede, permita registrar a localização aproximada e sinalize que o trecho precisa de confirmação.

### Equipe

Representa um eletricista individual ou uma equipe formada por dois ou mais profissionais.

Deve conter integrantes, competências, qualificações, disponibilidade, status operacional, localização conhecida e histórico de atribuições.

### Despacho

É a atribuição formal de uma OS a uma equipe, registrando responsável, data, horário, posição conhecida da equipe, previsão de deslocamento, aceite e histórico de alterações.

### Município (cidade)

É o recorte territorial e organizacional da operação. Cada solicitação, OS, equipe e usuário está associado a um ou mais municípios, conforme a seção 3A.

### Subestação e circuito

O **circuito** identifica a **subestação** que alimenta o ponto da rede e o **local da subestação**. Deve ser cadastrado com nome ou código da subestação, município e localização (endereço e/ou coordenadas), quando disponível.

### Conjunto elétrico

É o **número do conjunto elétrico** de cada subestação. Uma subestação pode ter mais de um conjunto. O conjunto sempre pertence a uma subestação e só pode ser selecionado depois do circuito.

### Transformador

Identifica o transformador ao qual o cliente está ligado. O número segue esta regra:

- Os **3 primeiros dígitos** identificam a **localidade**.
- Os **dígitos restantes** identificam o **número do equipamento dentro dessa localidade**.

Exemplo meramente ilustrativo: no número `1234567`, a localidade seria `123` e o número do equipamento local seria `4567`.

Armazene o número completo como texto, para preservar zeros à esquerda, junto com o código da localidade e o número local derivados. Não converta o número para tipo numérico. O cadastro de localidades (código de 3 dígitos, nome e município) deve ser administrável; não invente códigos de localidade.

### Unidade Consumidora (UC) e cliente

A UC é o ponto de fornecimento vinculado a um cliente. Registra número da UC, classe, situação, endereço da UC e transformador ao qual está ligada, quando disponível. O solicitante pode não saber a UC (seção 4.2.1), e isso não deve impedir o registro da ocorrência.

### Classe do cliente

Classificação do cliente ou da UC afetada. Valores iniciais: poder público, rural, essencial e residencial. A lista é administrável.

### Usuário Chave

Pessoa responsável, na operação, por definir quantos pontos vale cada opção de cada critério e quais faixas de pontos correspondem a cada prioridade. Possui perfil próprio e uma tela exclusiva (seção 7A).

---

## 3A. Segmentação por cidade (regra transversal)

A separação por cidade vale para todo o sistema e deve ser aplicada no backend, não apenas na interface.

### 3A.1. Cadastro e vínculo

- Cadastro administrável de municípios: nome, UF, código oficial (IBGE) quando conhecido, coordenadas de referência e situação (ativo ou inativo).
- Toda solicitação e toda OS possuem município obrigatório (chave estrangeira).
- Usuários são vinculados a um ou mais municípios. Equipes possuem um município-base e podem ter municípios adicionais autorizados.
- Município com OS vinculada não pode ser excluído, apenas inativado.

### 3A.2. Fila e indicadores por cidade

- Seletor de cidade no cabeçalho, visível em todas as telas operacionais, que lembra a última escolha do usuário (no perfil do usuário no backend ou na URL; não em localStorage como substituto de dados).
- A fila, a posição na fila, os indicadores (seção 5.1), o mapa e os filtros respeitam a cidade selecionada.
- A visão "Todas as cidades" é restrita a perfis autorizados (por exemplo, Supervisor e Administrador), exibe a coluna Município e mantém a posição na fila calculada dentro da cidade de cada OS.
- A ordenação dentro de cada cidade segue a política da seção 7.4.

### 3A.3. Permissões por cidade

- O atendente registra ocorrências apenas nas cidades às quais está vinculado.
- O despachante vê e despacha apenas OS e equipes das cidades às quais está vinculado.
- A API filtra por município em toda consulta. Não confie no frontend.
- Tentativa de acessar OS de município não autorizado retorna erro de autorização e é registrada na auditoria.

### 3A.4. Equipes e apoio entre cidades

- A recomendação de equipe prioriza equipes do mesmo município da OS.
- Equipes de outros municípios só aparecem como "apoio intermunicipal", identificadas visualmente, e exigem confirmação e justificativa.

### 3A.5. Numeração

- O número da OS e o da solicitação incluem a identificação do município (por exemplo, prefixo do município, ano e sequência), em formato configurável.
- A unicidade global é garantida por índice único no banco, com geração no servidor e controle de concorrência. O formato é uma sugestão, não uma regra oficial.

### 3A.6. Correção de município

- A troca do município de uma OS só pode ser feita por usuário autorizado, com justificativa obrigatória e auditoria. Após a troca, fila e permissões são recalculadas e o histórico é preservado.

### 3A.7. Pontuação por cidade

- A pontuação definida pelo Usuário Chave (seção 7A) é global por padrão. O Usuário Chave pode definir pontuação específica para uma cidade, que prevalece sobre a global naquela cidade, com indicação visível na tela e no detalhamento da OS.

---

## 4. Primeira tela: abertura de solicitação e geração da OS

Crie uma página específica para registrar solicitações.

Nome sugerido: **Nova Solicitação / Abertura de OS**.

O formulário deve ser organizado em seções visuais, com linguagem profissional e clara, sem excesso de campos apresentados de uma só vez.

O princípio é que o solicitante informe os fatos observados, enquanto o sistema calcula automaticamente os campos derivados e as classificações operacionais.

### 4.1. Identificação da solicitação

Campos e informações:

- Número da solicitação, gerado automaticamente.
- Data e hora do registro, geradas pelo servidor.
- Usuário responsável pelo registro, identificado pela autenticação.
- Canal de entrada: telefone, sistema interno, atendimento presencial ou integração.
- Origem do chamado e protocolo externo, quando aplicáveis.
- Número da OS, gerado automaticamente após a criação.
- Status atual da solicitação e da OS.
- Município (cidade) responsável pela OS, obrigatório e limitado às cidades permitidas ao usuário (seção 3A).

Os números devem ser únicos e seguros em cenários de registros simultâneos. Implemente geração no servidor com garantia de unicidade no banco de dados.

Não utilize apenas um contador mantido no frontend.

O número da OS deve incluir a identificação do município (por exemplo, prefixo do município, ano e sequência), em formato configurável, mantendo a unicidade global no banco de dados.

### 4.2. Cliente, UC e localização da ocorrência

Esta seção reúne a identificação do cliente, o endereço e a posição da ocorrência na rede elétrica. O atendente deve conseguir registrar a ocorrência mesmo com informações incompletas. O que não for conhecido deve ficar identificado como "não informado", nunca como valor inventado.

#### 4.2.1. Busca de UC (unidade consumidora)

- Campo de busca por número da UC. Quando houver integração com o cadastro comercial da concessionária, exibir os dados necessários ao atendimento: classe, situação, endereço da UC, transformador, circuito e conjunto. Sem integração, aceitar a digitação manual do número e marcar a UC como "não validada no cadastro".
- O formato do número da UC (tamanho, dígitos verificadores) deve ser configurável. Não presuma um formato oficial.
- Opção explícita: **"Solicitante fora da própria residência / não sabe a UC"**. Quando marcada:
  - o campo UC deixa de ser obrigatório;
  - o sistema registra que a UC não foi informada e o motivo;
  - a localização passa a depender de rua, CEP e endereço e/ou coordenadas (seção 4.2.2), e ao menos um meio de localizar a ocorrência torna-se obrigatório;
  - o sistema não tenta adivinhar a UC.
- Se a UC encontrada tiver endereço diferente do local informado da ocorrência, exibir um alerta e armazenar os dois. O local da ocorrência informado e confirmado prevalece sobre o endereço cadastrado da UC.
- Permitir vincular mais de uma UC à solicitação quando o solicitante relatar mais de um cliente afetado.
- Respeitar a LGPD: exibir somente os dados necessários, mascarar dados pessoais quando possível e registrar em auditoria cada consulta de UC.
- Dados de UC demonstrativos devem ser claramente identificados como demonstrativos.

#### 4.2.2. Rua, CEP, endereço e coordenadas

Campos:

- Rua ou logradouro, informado pelo solicitante, com sugestão quando houver base geográfica.
- Número ou referência de localização, quando disponível.
- Bairro.
- CEP, com máscara e validação de formato (8 dígitos). Consultar o endereço pelo CEP por meio de um serviço configurável, sem presumir provedor. Em municípios com CEP único, o CEP não identifica a rua, e a rua deve ser preenchida manualmente. Se a consulta falhar, permitir o preenchimento manual.
- Município, selecionado a partir do cadastro de municípios (seção 3A). Alertar se o município derivado do CEP ou do transformador divergir do município selecionado.
- Endereço completo, montado a partir dos campos acima e editável.
- Ponto de referência.
- Observações de localização.

**Coordenadas (enviar e registrar coordenadas):**

- O atendente pode informar latitude e longitude recebidas do solicitante (por exemplo, a localização compartilhada por aplicativo de mensagens). Aceitar graus decimais e a colagem de um texto ou link de mapa que contenha o par de coordenadas.
- O atendente também pode marcar o ponto no mapa ou usar a geocodificação do endereço, cujo resultado deve ser marcado como aproximado.
- A geolocalização do dispositivo do atendente só deve ser oferecida quando o registro for feito no local da ocorrência.
- Validar os intervalos (latitude entre -90 e 90, longitude entre -180 e 180). Alertar quando o ponto estiver fora do território brasileiro ou fora do município selecionado (quando houver geometria), sem bloquear o registro apenas por esse alerta.
- Armazenar a origem das coordenadas (informada pelo solicitante, marcada no mapa, geocodificada ou do dispositivo), a precisão, o sistema de referência (WGS84) e a data e hora do registro.
- Coordenadas informadas ou confirmadas prevalecem sobre coordenadas geocodificadas.
- As coordenadas devem ser exibidas na OS e entregues à equipe no despacho (seção 5.5).

#### 4.2.3. Rede elétrica: circuito, conjunto e transformador

- **Circuito:** subestação e local da subestação. Seleção a partir do cadastro de subestações, exibindo o nome ou código da subestação e a sua localização.
- **Conjunto elétrico:** número do conjunto elétrico da subestação. A lista de conjuntos depende do circuito selecionado.
- **Transformador:** número do transformador ao qual o cliente está ligado. Ao digitar, exibir a interpretação do número: os 3 primeiros dígitos como localidade (com o nome da localidade, se cadastrada) e os dígitos restantes como número do equipamento na localidade. O atendente pode corrigir o número antes de salvar.
  - Aceitar somente dígitos, com mínimo de 4 dígitos (3 da localidade e ao menos 1 do equipamento). O tamanho máximo deve ser configurável.
  - Preservar zeros à esquerda (armazenar como texto).
  - Se o código da localidade não estiver cadastrado, aceitar o número e sinalizar "localidade não cadastrada, confirmar".
  - Se o transformador existir no cadastro da rede, sugerir o circuito e o conjunto correspondentes, marcados como "identificado pelo cadastro, a confirmar".
- Trecho da rede elétrica afetado, quando identificado.
- Equipamento envolvido, como transformador, poste, chave ou outro equipamento cadastrado.
- Identificador patrimonial ou operacional do equipamento, quando disponível.
- Chave elétrica relacionada, quando identificada.

Quando houver cadastro oficial da rede, permita selecionar o trecho e o equipamento a partir do logradouro e das referências disponíveis. Sem cadastro oficial, o circuito, o conjunto e o transformador podem ser informados manualmente e devem ser marcados como "informados, sem confirmação no cadastro". Não invente identificadores de trechos ou equipamentos reais.

#### 4.2.4. Distinção entre tipos de localização

O sistema deve distinguir claramente:

- Endereço informado pelo solicitante.
- Endereço geocodificado.
- Coordenadas informadas, marcadas no mapa ou geocodificadas.
- Circuito, conjunto e transformador informados.
- Circuito, conjunto e transformador identificados pelo cadastro.
- Trecho elétrico identificado.
- Trecho confirmado operacionalmente.

Não deduza uma chave elétrica, um circuito, um conjunto ou um segmento de rede exclusivamente a partir do nome da rua.

Se não houver dados suficientes, mantenha a ocorrência registrada e sinalize a necessidade de confirmação da localização.

### 4.3. Tipo de manutenção

Campo obrigatório, com opções:

- Corretiva.
- Preventiva.

O tipo deve influenciar as regras de ordenação, conforme definido na seção de priorização.

Toda OS deve possuir um tipo de manutenção claramente identificado.

A manutenção corretiva deve ter precedência sobre a preventiva em condições comparáveis, sem permitir que essa regra reduza a prioridade de uma situação preventiva com risco crítico de segurança ou prazo regulatório iminente. Essas exceções devem ser formalizadas nas regras de negócio.

### 4.4. Tipo de ocorrência

Campo obrigatório, com opções configuráveis, como:

- Interrupção total de energia.
- Interrupção parcial de energia.
- Transformador danificado.
- Galho ou objeto em contato com a rede.
- Cabo rompido ou caído.
- Poste danificado.
- Equipamento elétrico com falha.
- Curto-circuito ou princípio de incêndio.
- Risco de choque elétrico.
- Falha em chave ou equipamento de manobra.
- Manutenção programada.
- Inspeção preventiva.
- Outros.

A lista deve ser administrável por usuários autorizados, sem necessidade de alterar o código-fonte.

As opções devem estar associadas a critérios de classificação configuráveis. Não atribua uma pontuação definitiva apenas com base no nome da ocorrência.

### 4.5. Impacto e gravidade

Implemente campos estruturados, com opções objetivas.

**Quantidade estimada de pessoas ou unidades consumidoras afetadas:**

- Até 10.
- De 11 a 100.
- De 101 a 500.
- De 501 a 999.
- 1.000 ou mais.
- Desconhecida.

Não confunda quantidade de pessoas com quantidade de unidades consumidoras. Registre separadamente essas métricas quando ambas estiverem disponíveis.

**Classe do cliente:**

- Poder público.
- Rural.
- Essencial.
- Residencial.
- Não identificada.

A lista de classes é administrável por usuários autorizados, sem alteração de código-fonte, para que a operação possa incluir outras classes. Quando a UC é identificada no cadastro, a classe vem do cadastro e deve ser confirmada pelo atendente. Permita registrar mais de uma classe quando a ocorrência afetar clientes de classes diferentes.

**Serviço essencial afetado** (quando a classe for "Essencial" ou quando o atendente identificar um serviço essencial):

- Hospital ou serviço de saúde essencial.
- Serviço de emergência ou segurança pública.
- Infraestrutura crítica.
- Outro serviço essencial.
- Não identificado.

**Situação do cliente:**

- Ligado.
- Desligado.
- Não informada.

Indica se a unidade consumidora do cliente está ligada ou desligada no momento do registro. Esse campo é diferente da "condição de fornecimento" abaixo, que descreve a interrupção observada na rede. "Não informada" não deve ser tratada como situação de baixo risco.

**Condições de segurança observadas:**

- Risco de choque elétrico.
- Cabo energizado exposto ou caído.
- Incêndio ou risco de incêndio.
- Poste ou estrutura com risco de queda.
- Risco à circulação de pessoas ou veículos.
- Sem risco adicional identificado.
- Situação desconhecida.

Permita marcar mais de uma condição de segurança.

A ausência de um risco identificado não significa necessariamente que não exista risco. Os campos devem diferenciar “não identificado” de “confirmadamente inexistente”.

**Condição de fornecimento:**

- Sem interrupção.
- Interrupção parcial.
- Interrupção total.
- Interrupção desconhecida.

**Outros indicadores:**

- Quantidade de equipamentos afetados, quando conhecida.
- Existência de redundância ou alimentação alternativa confirmada.
- Existência de fonte de energia de reserva confirmada no local crítico.
- Duração estimada da interrupção, quando conhecida.
- Necessidade de equipe especializada.
- Necessidade de recursos ou equipamentos especiais.
- Data e horário-limite, quando a atividade tiver prazo operacional definido.

Informações desconhecidas não devem ser automaticamente interpretadas como situações de baixo risco.

### 4.6. Descrição livre

Inclua um campo de texto amplo chamado “Descrição da ocorrência”.

Esse campo deve permitir que o atendente registre livremente o que foi informado pelo solicitante, por exemplo:

“Cliente informa que um galho caiu sobre a rede na Rua das Flores, provocando interrupção de energia em parte da região. Há relatos de faíscas próximas ao poste.”

O texto livre deve complementar os campos estruturados, nunca substituí-los.

Não permita que comentários livres alterem silenciosamente a pontuação. Se futuramente houver análise automática de texto, ela deve apresentar sugestões explicáveis e exigir confirmação conforme as regras da operação.

### 4.7. Campos automáticos e campos bloqueados

O usuário que registra a solicitação não deve escolher manualmente a pontuação, o código de prioridade calculado, a posição da OS na fila ou o prazo derivado das regras.

Esses campos devem ser gerados pelo backend.

O formulário deve exibir uma prévia da classificação quando houver dados suficientes, mas deixar claro que a classificação definitiva é calculada e registrada pelo sistema.

A prioridade poderá ser revista por usuários autorizados, com justificativa obrigatória e trilha de auditoria.

A pontuação de cada opção e as faixas de cada prioridade são definidas pelo Usuário Chave (seção 7A). O formulário nunca deve exibir campos para o atendente digitar pontos.

### 4.8. Botão de criação

Ao clicar em “Registrar solicitação e gerar OS”, o sistema deve:

1. Validar os campos obrigatórios, incluindo o município, e a permissão do usuário para registrar ocorrências naquele município.
2. Validar as combinações de respostas.
3. Registrar a solicitação no banco.
4. Gerar um número único para a OS.
5. Calcular a classificação inicial e os prazos aplicáveis.
6. Persistir os critérios, os pesos, a pontuação e a versão das regras utilizadas.
7. Criar o histórico inicial.
8. Confirmar a operação ao usuário.
9. Atualizar a fila da segunda tela.
10. Disponibilizar o número da OS e um acesso aos seus detalhes.

As operações devem respeitar transações de banco de dados, evitando solicitações duplicadas ou OS sem vínculo válido.

---

## 5. Segunda tela: painel de prioridades e despacho

Nome sugerido: **Painel Operacional de Ordens de Serviço**.

Essa deve ser a principal tela de operação, apresentando uma lista organizada, legível e atualizada das OS pendentes.

O responsável deve conseguir entender rapidamente o que precisa ser atendido, por que a ocorrência está naquela posição e quais equipes podem ser designadas.

### 5.1. Cabeçalho e indicadores

Exiba indicadores resumidos:

- Total de OS abertas.
- OS críticas ou urgentes.
- OS aguardando despacho.
- OS em atendimento.
- OS com prazo vencido.
- Equipes disponíveis.
- Equipes em deslocamento.
- Equipes executando serviços.

Os indicadores devem ser calculados a partir do banco de dados, considerando os filtros e os estados definidos para cada métrica.

Os indicadores devem refletir a cidade selecionada no cabeçalho (seção 3A). A visão "Todas as cidades" fica restrita a perfis autorizados e deve apresentar os indicadores consolidados e por cidade.

### 5.2. Tabela principal

A tabela deve possuir, no mínimo, as seguintes colunas:

- Posição na fila (calculada dentro da cidade selecionada).
- Número da OS.
- Código e nome da prioridade.
- Pontuação total.
- Tipo de manutenção.
- Tipo de ocorrência.
- Endereço ou trecho afetado.
- Município (visível na visão consolidada).
- UC, ou indicação de "UC não informada".
- Classe do cliente.
- Circuito (subestação), conjunto elétrico e transformador.
- Quantidade de pessoas ou unidades consumidoras afetadas.
- Indicador de risco.
- Data e hora de abertura.
- Prazo de atendimento.
- Tempo restante ou atraso.
- Status da OS.
- Equipe atribuída, quando existente.
- Ações disponíveis.

Permita ocultar e exibir as colunas secundárias, preservando as essenciais.

Utilize indicadores visuais discretos para diferenciar prioridades.

O número da OS deve ser clicável e abrir um modal com os detalhes completos.

A tabela deve permitir paginação ou carregamento incremental, ordenação, pesquisa e filtros.

### 5.3. Filtros

Disponibilize filtros por:

- Código e nível de prioridade.
- Tipo de manutenção.
- Tipo de ocorrência.
- Status.
- Bairro, rua e trecho.
- Município (cidade). Por padrão, a cidade selecionada no cabeçalho.
- Faixa de pessoas afetadas.
- Faixa de unidades consumidoras afetadas.
- Risco de segurança.
- Instalação crítica afetada.
- Equipamento.
- Equipe atribuída.
- Período de abertura.
- Prazo vencido ou próximo do vencimento.
- Circuito (subestação), conjunto elétrico e transformador.
- Classe do cliente.
- Situação do cliente (ligado ou desligado).
- UC.

Os filtros devem ser combináveis e funcionar no backend quando houver volume relevante de dados.

A aplicação deve permitir limpar os filtros e retornar à fila operacional padrão.

### 5.4. Modal de detalhes da OS

Ao clicar no número da OS, abrir um modal ou painel lateral com:

- Identificação da solicitação.
- Número da OS.
- Descrição original.
- Tipo de manutenção.
- Classificação atual.
- Código de prioridade.
- Pontuação total.
- Detalhamento da pontuação por critério.
- Regras que influenciaram a prioridade.
- Endereço e trecho identificado.
- Município, UC (ou indicação de "UC não informada"), classe e situação do cliente.
- Circuito (subestação e local), conjunto elétrico e transformador (número completo, localidade e número local), com indicação de "informado" ou "identificado pelo cadastro".
- Coordenadas, com origem (informada, marcada no mapa ou geocodificada) e precisão.
- Mapa da ocorrência.
- Impacto estimado.
- Riscos registrados.
- Equipamentos relacionados.
- Histórico de alterações.
- Data e prazo aplicáveis.
- Equipe designada.
- Histórico de despachos.
- Status atual.
- Justificativas de alteração manual da prioridade.

O usuário deve conseguir entender por que uma OS está acima de outra, sem precisar consultar o código-fonte.

### 5.5. Disponibilizar para equipe

Inclua um botão “Disponibilizar para equipe” nas OS que ainda não tenham uma atribuição ativa.

Ao clicar, abrir um modal ou painel lateral com as equipes disponíveis e compatíveis.

Para cada equipe, apresentar:

- Identificação da equipe.
- Integrantes.
- Qualificações e competências.
- Status operacional.
- Localização conhecida e horário da última atualização.
- Distância estimada até o local da ocorrência.
- Tempo estimado de deslocamento, quando houver serviço de roteamento disponível.
- Recursos e equipamentos disponíveis.
- Compatibilidade com o serviço.
- Justificativa da recomendação.

O sistema deve destacar a equipe recomendada, mas permitir a seleção de outra equipe autorizada.

A recomendação deve considerar a compatibilidade técnica e as regras de segurança antes de considerar a distância.

Uma equipe mais próxima não deve ser recomendada se não estiver habilitada para realizar o serviço.

Por padrão, listar apenas equipes do município da OS e de municípios autorizados. Equipes de outras cidades só podem aparecer como "apoio intermunicipal", identificadas visualmente e com exigência de confirmação e justificativa (seção 3A.4).

Ao confirmar a designação:

1. Validar novamente a disponibilidade da equipe.
2. Verificar se a OS continua apta ao despacho.
3. Registrar a atribuição no banco.
4. Atualizar os estados da OS e da equipe conforme a transição permitida.
5. Registrar usuário, data, horário e justificativa quando aplicável.
6. Atualizar a fila e o mapa.
7. Impedir atribuições simultâneas incompatíveis.

Implemente controle de concorrência para evitar que duas pessoas designem a mesma equipe a ordens incompatíveis ao mesmo tempo.

Ao confirmar a designação, disponibilizar à equipe as coordenadas da OS (quando houver), o endereço, o CEP, a UC (quando houver), o circuito, o conjunto e o transformador, com ações para copiar as coordenadas e abrir a localização em um aplicativo de mapas. Se a OS não tiver coordenadas, informar que a localização é aproximada ou depende de confirmação.

---

## 6. Mapa georreferenciado das operações

O painel operacional deve conter um mapa interativo, integrado à tabela.

Utilize Leaflet e OpenStreetMap ou outra solução equivalente configurável.

O mapa deve exibir:

- Localização das OS com coordenadas válidas.
- Trechos afetados quando houver geometria disponível.
- Localização conhecida das equipes.
- Equipes disponíveis.
- Equipes em deslocamento.
- Equipes em execução.
- Marcadores distintos para ocorrências críticas.
- Seleção de uma OS para visualizar seu destino.
- Seleção de uma equipe para visualizar sua posição.
- Possibilidade de comparar equipe e ocorrência.
- Distância e estimativa de deslocamento, quando suportadas.
- Legenda dos marcadores.
- Subestações (circuitos) com localização cadastrada, quando houver.
- Recorte pela cidade selecionada no cabeçalho.

Ao abrir o modal de despacho, destaque no mapa o destino da OS e as equipes candidatas.

Se houver serviço de roteamento, apresente a rota estimada. Não calcule tempo de viagem apenas por distância em linha reta.

### Limitações de integração

A localização em tempo real depende de uma fonte legítima de coordenadas.

Implemente a arquitetura para integrar futuramente aplicativos móveis das equipes, GPS corporativo ou outra fonte autorizada.

Caso não exista integração de GPS, utilize somente a última posição cadastrada ou dados demonstrativos claramente identificados. Não simule uma localização como se fosse real.

A base de trechos, postes, chaves e circuitos deve ser integrada ao cadastro oficial da rede quando disponibilizado pela concessionária.

---

## 7. Mecanismo de priorização por pontos

Esta é uma das partes mais importantes do projeto.

Implemente um mecanismo de regras de negócio no backend que determine a classificação de cada OS de forma reproduzível, auditável e configurável.

A pontuação deve ser calculada no servidor. O frontend apenas apresenta os resultados.

### 7.1. Critérios possíveis

Considere critérios como:

1. Risco à vida e à segurança pública.
2. Risco de choque elétrico.
3. Cabos expostos, incêndios ou estruturas instáveis.
4. Impacto em hospitais e outros serviços essenciais.
5. Quantidade estimada de pessoas afetadas.
6. Quantidade de unidades consumidoras afetadas.
7. Extensão da interrupção.
8. Tipo e gravidade da ocorrência.
9. Equipamento afetado.
10. Abrangência geográfica.
11. Tempo decorrido desde a abertura.
12. Proximidade do vencimento do prazo.
13. Tipo de manutenção.
14. Existência de redundância confirmada.
15. Necessidade de equipe especializada.
16. Classe do cliente (poder público, rural, essencial, residencial e outras cadastradas).
17. Situação do cliente (ligado ou desligado).
18. Circuito (subestação) e conjunto elétrico afetados.
19. Transformador afetado e, quando disponível no cadastro, quantidade de unidades consumidoras ligadas a ele.
20. Outros critérios cadastrados por administradores autorizados, cujos pontos também são definidos pelo Usuário Chave.

Não atribua pontos definitivos sem uma tabela de regras validada. Os pontos de cada opção são definidos e publicados pelo Usuário Chave, na tela descrita na seção 7A. O código-fonte não deve conter pontuações fixas apresentadas como definitivas.

Cada critério deve possuir:

- Identificador.
- Nome.
- Descrição.
- Peso ou pontuação.
- Opções de resposta.
- Regra de aplicação.
- Vigência.
- Versão.
- Usuário responsável pela configuração.
- Data de criação e alteração.
- Indicação de habilitado ou desabilitado.
- Escopo de aplicação (todas as cidades ou municípios específicos).

### 7.2. Pontuação explicável

Para cada OS, armazene e apresente:

- Pontuação por critério.
- Pontuação total.
- Regras de precedência aplicadas.
- Código de prioridade resultante.
- Motivo principal da classificação.
- Versão do conjunto de regras utilizado.
- Data e hora do cálculo.

Exemplo meramente ilustrativo:

OS 1024 — transformador danificado em hospital.

- Impacto em serviço essencial: +X pontos.
- Quantidade estimada de pessoas afetadas: +Y pontos.
- Gravidade da ocorrência: +Z pontos.
- Risco de segurança: classificado conforme a regra específica.
- Tempo de espera: contribuição calculada pela regra vigente.

Pontuação ilustrativa: X + Y + Z + demais critérios aplicáveis.

Os valores X, Y e Z devem ser substituídos por configurações aprovadas pela operação. Não utilize esse exemplo como uma regra oficial.

### 7.3. Não permitir que pontos neutralizem riscos críticos

Uma simples soma pode produzir resultados perigosos. Por exemplo, uma ocorrência comum poderia acumular muitos pontos e superar uma ocorrência com risco iminente à vida.

Por isso, implemente duas camadas:

**Camada A — regras de precedência e segurança**

Identifica situações que exigem tratamento especial, de acordo com critérios aprovados pela área operacional e de segurança.

**Camada B — pontuação ponderada**

Entre ocorrências comparáveis, utiliza a soma dos critérios configurados para ordenar as OS.

A ordenação final deve seguir uma política explícita e testada. Não esconda regras de precedência dentro de números arbitrários.

A regra de precedência deve ser configurável e documentada, preservando os procedimentos de emergência vigentes.

### 7.4. Ordem de classificação

O sistema deve:

1. Aplicar regras de segurança e precedência.
2. Aplicar a classificação operacional validada.
3. Calcular a pontuação ponderada.
4. Determinar o código de prioridade.
5. Calcular o prazo aplicável.
6. Ordenar as OS pendentes.
7. Utilizar critérios de desempate estáveis e documentados.

Entre OS com prioridade equivalente, os desempates podem considerar vencimento do prazo, tempo de espera e número sequencial, conforme política validada.

### 7.5. Reclassificação automática

A prioridade pode mudar quando houver:

- Nova solicitação vinculada à mesma ocorrência.
- Atualização da quantidade de pessoas afetadas.
- Identificação de um risco anteriormente desconhecido.
- Alteração confirmada do estado do equipamento.
- Aproximação ou vencimento do prazo.
- Mudança relevante de outros critérios.
- Atualização administrativa das regras, respeitando a política de vigência.

Toda reclassificação deve manter histórico.

Mudanças de configuração não devem alterar silenciosamente o histórico anterior. Registre a versão das regras aplicada e defina explicitamente quando as novas regras passam a valer para as OS já abertas.

---

## 7A. Tela do Usuário Chave: definição de pontos

O **Usuário Chave** é a pessoa responsável, na operação, por definir quantos pontos vale cada opção de cada critério e quantos pontos enquadram uma OS em cada prioridade. Esta é uma tela nova, separada das telas de atendimento e despacho.

### 7A.1. Perfil e acesso

- Perfil próprio, "Usuário Chave", atribuído por um Administrador. Recomenda-se ao menos um titular e um substituto.
- Permissões separadas para editar rascunhos e para publicar versões. A publicação pode exigir a aprovação de um segundo usuário (configurável).
- Os demais perfis (atendente, despachante, supervisor) podem consultar a pontuação publicada em modo somente leitura, para entender o resultado de cada OS.
- O Usuário Chave não altera as regras de precedência e segurança da camada A (seção 7.3), a estrutura dos critérios, as permissões de usuários nem os resultados de OS já calculadas. Essas alterações seguem os perfis e procedimentos já definidos.

### 7A.2. Abas da tela

1. **Pontos por critério**
   - Lista agrupada por critério (seção 7.1), com as opções de resposta de cada um e um campo de pontos por opção.
   - Mostrar, para cada opção, a origem do valor (demonstrativo ou definido pelo Usuário Chave), a data e o usuário da última alteração.
   - Permitir habilitar ou desabilitar critérios e definir a vigência.
   - Exibir a pontuação mínima e máxima possível do conjunto.
2. **Faixas por prioridade**
   - Para cada código de prioridade (seção 8), definir a pontuação mínima e máxima que o enquadra.
   - Representação visual das faixas, validando que não haja lacunas nem sobreposições.
   - Exibir, somente para leitura, as regras de precedência da camada A que podem se sobrepor à pontuação.
3. **Simulador**
   - Formulário com os mesmos campos da Nova Solicitação, para montar uma ocorrência hipotética.
   - Mostrar o detalhamento por critério, a pontuação total, o código de prioridade e as regras de precedência aplicadas, usando as regras em rascunho.
   - O cálculo deve ser feito pelo mesmo mecanismo do backend usado em produção, sem reimplementar regras no frontend.
4. **Impacto**
   - Antes de publicar, listar quais OS abertas mudariam de prioridade e quantas são, por cidade.
5. **Versões e publicação**
   - Diferenciar rascunho e versão publicada.
   - A publicação exige justificativa, data e hora de vigência e a escolha explícita entre aplicar somente às novas OS ou reclassificar também as OS abertas (seção 7.5).
   - Histórico de versões com autor, data, justificativa e diferenças, com comparação entre duas versões.
   - Restaurar uma versão anterior cria uma nova versão, sem apagar o histórico.

### 7A.3. Validações

- Os pontos devem ser números inteiros não negativos, com limite máximo configurável.
- Toda opção de um critério habilitado deve ter pontos definidos explicitamente. "Sem pontos definidos" é diferente de zero e impede a publicação.
- Opções como "Desconhecida", "Não identificado" e "Não informada" devem ter tratamento explícito (pontos definidos e, se necessário, marcação de "requer confirmação"), para que a ausência de informação não seja tratada como risco zero.
- Alertar, sem bloquear, quando a pontuação for incoerente com a gravidade (por exemplo, uma opção de serviço essencial com menos pontos que uma opção residencial).
- Controle de concorrência na edição: se duas pessoas editarem o mesmo rascunho, a segunda gravação deve ser rejeitada com aviso.
- Toda alteração gera auditoria com os valores anteriores e novos.

### 7A.4. Escopo por cidade

- A pontuação é global por padrão. O Usuário Chave pode criar pontuações específicas para uma cidade, que prevalecem sobre as globais naquela cidade, com indicação visível na tela e no detalhamento da OS (seção 3A.7).

### 7A.5. Estado inicial

- Enquanto o Usuário Chave não tiver publicado a primeira versão, o sistema usa um conjunto demonstrativo e exibe o aviso "Pontuação demonstrativa, não oficial" na abertura de solicitação, no painel e no detalhe da OS.
- Os valores demonstrativos não devem ser apresentados como regras oficiais.

---

## 8. Códigos de prioridade e prazos de atendimento

Implemente uma tabela configurável de prioridades, com código, nome, descrição, critérios de enquadramento, prazo de atendimento e regras de escalonamento.

Os nomes, códigos e prazos devem ser configuráveis para refletir a norma operacional validada, inclusive a tabela oficial do SGM, se for fornecida.

A pontuação mínima e máxima que enquadra a OS em cada código (faixas) é definida pelo Usuário Chave na tela da seção 7A. Os prazos de cada código seguem sob administração de códigos e prazos, por usuários autorizados.

Exemplos de categorias que podem ser cadastradas, sem afirmar que correspondem aos códigos oficiais:

- Urgente.
- Emergente.
- Alta.
- Média.
- Baixa.

Não fixe prazos como cinco dias para uma categoria sem confirmação documental.

Cada categoria deve poder definir:

- Código.
- Nome.
- Descrição.
- Faixa de pontuação (mínima e máxima) que enquadra a OS na categoria, definida pelo Usuário Chave.
- Prazo máximo de atendimento.
- Unidade do prazo.
- Calendário aplicável.
- Regra para finais de semana e feriados.
- Tratamento de situações críticas.
- Condições de escalonamento.
- Vigência.
- Ativação ou desativação.

Diferencie os seguintes conceitos:

- Prazo para triagem.
- Prazo para despacho.
- Prazo para início do atendimento.
- Prazo para restabelecimento, quando aplicável.
- Prazo para conclusão da OS.

Não trate todos como o mesmo SLA.

Os prazos devem respeitar as regras oficiais aplicáveis e os procedimentos da concessionária. Até que essas informações sejam validadas, utilize configurações demonstrativas identificadas como não oficiais.

A tela deve mostrar claramente o tempo restante, o prazo vencido e os alertas correspondentes.

---

## 9. Regras para manutenção corretiva e preventiva

O sistema deve distinguir manutenção corretiva e preventiva em todas as etapas.

A política inicial deve dar precedência às OS corretivas sobre as preventivas em condições comparáveis, sem desconsiderar riscos críticos de segurança, obrigações regulatórias ou prazos preventivos que exijam tratamento especial.

Não basta colocar todas as corretivas automaticamente acima de todas as preventivas sem considerar essas exceções.

Para a manutenção preventiva, considere:

- Data programada.
- Prazo-limite.
- Equipamento ou trecho.
- Criticidade do ativo.
- Consequências do adiamento.
- Recursos necessários.
- Necessidade de desligamento programado.
- Dependências operacionais.

Para a manutenção corretiva, considere:

- Falha identificada.
- Impacto atual.
- Risco à segurança.
- Abrangência da interrupção.
- Serviço essencial afetado.
- Prazo operacional aplicável.

A classificação deve ser explicável e consistente com a política aprovada.

---

## 10. Atualização automática e manual

A fila de prioridades deve ser atualizada automaticamente a cada 60 segundos.

Inclua também um botão “Atualizar agora”.

A atualização deve consultar o backend e refletir:

- Novas OS.
- Alterações nos critérios.
- Mudanças de prioridade.
- Prazos vencidos.
- Mudanças de status.
- Designações de equipe.
- Cancelamentos.
- Conclusões.
- Mudanças relevantes na disponibilidade das equipes.

Evite recarregar a página inteira.

Utilize TanStack Query com atualização periódica, tratamento de erros e indicação de última atualização.

Caso SignalR seja implementado, use-o para propagar alterações relevantes em tempo real, mantendo a atualização periódica como mecanismo de reconciliação.

Se o backend estiver indisponível, preserve a tela atual, informe que os dados podem estar desatualizados e permita nova tentativa.

A atualização da lista não deve modificar indevidamente os filtros, a página selecionada ou o modal atualmente aberto.

---

## 11. Gestão de equipes e estados operacionais

Implemente uma estrutura de cadastro e consulta de equipes.

Cada equipe deve conter:

- Identificador.
- Nome ou código operacional.
- Município-base e municípios adicionais em que está autorizada a atuar.
- Integrantes.
- Habilidades e qualificações.
- Recursos e equipamentos.
- Status.
- Localização e data da última atualização.
- OS atribuídas.
- Capacidade operacional.
- Histórico de atividades.

Estados possíveis, sujeitos a validação:

- Disponível.
- A caminho.
- Em atendimento.
- Indisponível.
- Em pausa.
- Em deslocamento para outra atividade.

O estado da equipe deve ser consistente com suas atribuições e a política de disponibilidade.

Não permita designar uma equipe indisponível ou incompatível com os requisitos da OS sem uma exceção formalmente autorizada.

Quando uma equipe aceitar uma OS, registrar o aceite e atualizar os estados correspondentes.

Quando o serviço terminar, permitir o registro da conclusão e a liberação da equipe, respeitando as transições de status.

---

## 12. Status e ciclo de vida da OS

Implemente uma máquina de estados documentada.

Estados iniciais sugeridos:

- Aberta.
- Em triagem.
- Aguardando despacho.
- Equipe designada.
- Equipe a caminho.
- Em execução.
- Aguardando recurso ou autorização.
- Suspensa.
- Concluída.
- Cancelada.

A lista pode ser ajustada às normas operacionais.

Defina transições válidas e suas permissões.

Exemplos:

- Uma OS concluída não pode retornar diretamente para execução sem uma ação autorizada e auditada.
- Uma OS cancelada exige motivo.
- Uma OS em execução não pode ser atribuída simultaneamente a duas equipes incompatíveis.
- A mudança de equipe deve preservar o histórico do despacho anterior.
- Uma ocorrência crítica deve seguir o procedimento operacional de escalonamento.

Não permita que o frontend seja a única camada responsável por impedir transições inválidas.

---

## 13. Unificação de ocorrências e prevenção de duplicidade

Quando diversas pessoas informarem o mesmo problema na mesma região, o sistema deve ajudar a identificar possíveis duplicidades.

Considere:

- Proximidade geográfica.
- Mesmo trecho.
- Mesmo equipamento.
- Mesma UC, mesmo transformador, mesmo circuito ou mesmo conjunto elétrico.
- Mesmo município.
- Intervalo temporal.
- Tipo de ocorrência.
- Coincidência da interrupção relatada.

Apresente uma sugestão de possível duplicidade, sem unir ocorrências automaticamente apenas porque estão na mesma rua.

Quando a unificação for confirmada por usuário autorizado:

- Preserve a solicitação original de cada contato.
- Registre os vínculos entre solicitações.
- Evite criar ordens redundantes para a mesma intervenção, quando isso for confirmado.
- Atualize as métricas de impacto.
- Recalcule a prioridade, se necessário.
- Preserve todo o histórico e a rastreabilidade.

A aplicação deve distinguir múltiplos relatos de um único evento de falhas diferentes ocorridas no mesmo logradouro.

---

## 14. Arquitetura do backend

Organize o backend por responsabilidades, com separação entre domínio, aplicação, infraestrutura e API.

Estrutura sugerida:

```text
backend/
├── src/
│   ├── Maintenance.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   ├── Rules/
│   │   ├── Events/
│   │   └── Exceptions/
│   │
│   ├── Maintenance.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── UseCases/
│   │   ├── Validators/
│   │   ├── Prioritization/
│   │   └── Mapping/
│   │
│   ├── Maintenance.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Context/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Authentication/
│   │   ├── Geolocation/
│   │   ├── Routing/
│   │   └── ExternalServices/
│   │
│   └── Maintenance.API/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Authorization/
│       ├── Hubs/
│       ├── Filters/
│       ├── Extensions/
│       └── Program.cs
│
└── tests/
    ├── Maintenance.UnitTests/
    ├── Maintenance.IntegrationTests/
    └── Maintenance.ArchitectureTests/
```

Ajuste a estrutura se houver uma razão técnica clara, documentando a decisão.

Cada Controller deve ser responsável por um recurso ou conjunto coerente de operações. Não concentre toda a lógica em um único Controller.

Implemente serviços específicos para:

- Criação de solicitações.
- Geração de OS.
- Cálculo de prioridades.
- Cálculo de prazos.
- Gestão de equipes.
- Despacho.
- Geolocalização.
- Detecção de duplicidades.
- Histórico e auditoria.
- Cadastro da rede (municípios, localidades, subestações, conjuntos e transformadores).
- Consulta de UC e clientes, com integração configurável.
- Consulta de CEP e tratamento de coordenadas.
- Configuração de pontuação do Usuário Chave (rascunho, simulação, publicação e versões).

Regras de negócio devem ficar na camada apropriada, não em componentes React ou diretamente em Controllers.

Utilize injeção de dependência e interfaces onde isso favorecer testabilidade e separação de responsabilidades.

Não crie interfaces e classes vazias apenas para aparentar uma arquitetura sofisticada.

---

## 15. Arquitetura do frontend

Organize o frontend por funcionalidades e telas, com componentes reutilizáveis e separação de responsabilidades.

Estrutura sugerida:

```text
frontend/
├── src/
│   ├── app/
│   │   ├── router/
│   │   ├── providers/
│   │   ├── layouts/
│   │   └── config/
│   │
│   ├── pages/
│   │   ├── Login/
│   │   ├── Dashboard/
│   │   ├── NovaSolicitacao/
│   │   ├── PainelOperacional/
│   │   ├── DetalhesOS/
│   │   ├── Equipes/
│   │   ├── MapaOperacional/
│   │   ├── Historico/
│   │   ├── ConfiguracaoPontuacao/   # tela do Usuário Chave
│   │   └── Configuracoes/
│   │
│   ├── features/
│   │   ├── solicitacoes/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   ├── schemas/
│   │   │   └── types/
│   │   ├── ordens-servico/
│   │   ├── priorizacao/
│   │   ├── equipes/
│   │   ├── despacho/
│   │   ├── geolocalizacao/
│   │   ├── rede-eletrica/
│   │   ├── clientes-uc/
│   │   └── autenticacao/
│   │
│   ├── components/
│   │   ├── ui/
│   │   ├── forms/
│   │   ├── tables/
│   │   ├── maps/
│   │   ├── feedback/
│   │   └── layout/
│   │
│   ├── hooks/
│   ├── services/
│   │   ├── api/
│   │   └── realtime/
│   ├── lib/
│   ├── types/
│   ├── styles/
│   └── main.tsx
│
├── public/
├── package.json
├── tsconfig.json
└── vite.config.ts
```

Cada página deve ter seus próprios componentes, hooks, schemas e serviços quando necessário.

Evite criar componentes gigantes, duplicar lógica ou misturar chamadas HTTP, regras de negócio e apresentação no mesmo arquivo.

Utilize tipos TypeScript explícitos, estados de carregamento, estados vazios, tratamento de erros e feedback após operações.

---

## 16. Banco de dados

Modele, no mínimo, as seguintes entidades:

- Usuários.
- Perfis e permissões.
- Solicitações.
- Ordens de serviço.
- Histórico de status.
- Critérios de prioridade.
- Opções dos critérios.
- Conjuntos e versões de regras.
- Resultados de cálculo de prioridade.
- Categorias e códigos de prioridade.
- Regras de prazo e SLA.
- Municípios.
- Localidades (código de 3 dígitos, nome e município).
- Subestações (circuitos) e sua localização.
- Conjuntos elétricos, vinculados à subestação.
- Transformadores (número completo como texto, código da localidade, número do equipamento local, conjunto e circuito, quando conhecidos).
- Unidades consumidoras e clientes (UC, classe, situação, endereço e transformador).
- Classes de cliente.
- Vínculos entre usuários e municípios e entre equipes e municípios.
- Faixas de pontuação por código de prioridade.
- Trechos de rede.
- Equipamentos.
- Equipes.
- Integrantes das equipes.
- Qualificações.
- Localizações registradas.
- Despachos.
- Histórico de despachos.
- Anexos.
- Comentários.
- Relacionamentos entre solicitações duplicadas.
- Logs de auditoria.

Defina corretamente:

- Chaves primárias.
- Chaves estrangeiras.
- Restrições de unicidade.
- Índices.
- Campos obrigatórios.
- Datas e horários.
- Regras de exclusão e retenção.
- Migrações versionadas.

Use UTC para armazenar instantes no banco e converta para o fuso horário operacional na interface.

Avalie PostGIS caso a aplicação precise armazenar e consultar geometrias de trechos, proximidade e operações geoespaciais.

Não armazene apenas o endereço como texto se houver coordenadas disponíveis. Preserve tanto a referência textual quanto os dados geográficos e sua precisão.

Armazene o número do transformador como texto, com o código da localidade e o número local em colunas próprias, e crie restrição de unicidade para a combinação de código da localidade e número do equipamento local no cadastro da rede. Armazene o município em toda solicitação e OS, com índice para as consultas por cidade.

---

## 17. API REST

Crie endpoints organizados por recurso.

Exemplos de rotas:

```text
POST   /api/solicitacoes
GET    /api/solicitacoes/{id}

GET    /api/ordens-servico
GET    /api/ordens-servico/{id}
GET    /api/ordens-servico/{id}/prioridade
GET    /api/ordens-servico/{id}/historico

POST   /api/ordens-servico/{id}/reclassificar
POST   /api/ordens-servico/{id}/despachos
PATCH  /api/ordens-servico/{id}/status

GET    /api/equipes
GET    /api/equipes/disponiveis
GET    /api/equipes/{id}
GET    /api/equipes/{id}/localizacao

GET    /api/mapa/operacoes
GET    /api/trechos
GET    /api/equipamentos

GET    /api/configuracoes/prioridades
GET    /api/configuracoes/regras

GET    /api/municipios
GET    /api/municipios/{id}/subestacoes
GET    /api/subestacoes/{id}/conjuntos
GET    /api/transformadores?numero=
GET    /api/unidades-consumidoras?uc=
GET    /api/geocodificacao/cep/{cep}

GET    /api/configuracoes/pontuacao
PUT    /api/configuracoes/pontuacao/rascunho
POST   /api/configuracoes/pontuacao/simular
GET    /api/configuracoes/pontuacao/impacto
POST   /api/configuracoes/pontuacao/publicar
GET    /api/configuracoes/pontuacao/versoes
```

Essas rotas são uma referência inicial, não uma obrigação de manter exatamente os mesmos nomes.

Implemente paginação, filtros, ordenação permitida, validação de entrada e respostas HTTP coerentes.

Utilize DTOs para evitar expor diretamente as entidades do banco.

Documente os endpoints no Swagger e forneça exemplos de requisição e resposta.

---

## 18. Segurança, permissões e auditoria

Implemente autenticação e autorização.

Perfis iniciais sugeridos:

- Administrador.
- Atendente.
- Despachante ou responsável operacional.
- Supervisor.
- Equipe de campo.
- Usuário Chave.

As permissões devem controlar quem pode:

- Registrar solicitações.
- Consultar OS.
- Alterar classificações.
- Modificar critérios.
- Configurar prazos.
- Designar equipes.
- Modificar status.
- Encerrar ordens.
- Consultar dados de localização.
- Administrar usuários.
- Definir e publicar a pontuação (Usuário Chave).
- Acessar e atuar em cada município (permissões por cidade).

Registre auditoria para operações relevantes, incluindo alterações de prioridade, mudança de equipe, edição de critérios, cancelamentos e conclusões.

O registro deve incluir usuário, data, ação, entidade afetada, valores anteriores e posteriores quando aplicável, e justificativa.

Não permita que um usuário sem autorização altere diretamente os pesos ou as regras de prioridade.

Somente o Usuário Chave, pela tela da seção 7A, e administradores autorizados podem alterar a pontuação. Toda publicação deve ser auditada.

Não exponha credenciais, tokens ou informações sensíveis nos logs.

---

## 19. Design e identidade visual

O sistema deve apresentar aparência profissional, moderna e adequada a um centro de operação ou equipe de manutenção elétrica.

Utilize uma identidade visual predominantemente azul e branca, com verde-musgo em pequenos destaques.

Referências de paleta:

- Azul principal: #2457A6.
- Azul escuro: #17365D.
- Branco: #FFFFFF.
- Fundo neutro: #F4F7FB.
- Verde-musgo: #667A45.
- Texto principal: #243247.
- Bordas: #DCE3EC.

As cores de prioridade devem ser semanticamente distintas e acessíveis. Não dependa apenas das cores para comunicar criticidade; use também texto e ícones.

A interface não deve parecer um template genérico produzido automaticamente nem ser excessivamente minimalista.

Evite:

- Gradientes decorativos em excesso.
- Sombras exageradas.
- Cartões para absolutamente tudo.
- Ícones aleatórios.
- Animações desnecessárias.
- Grandes espaços vazios que reduzam a densidade operacional.
- Cores excessivamente saturadas.
- Informações pequenas demais nas tabelas.

Priorize:

- Hierarquia visual clara.
- Boa densidade de informação.
- Tabelas legíveis.
- Formulários divididos em seções.
- Painéis laterais e modais funcionais.
- Botões com rótulos claros.
- Estados de carregamento discretos.
- Feedback de sucesso e erro.
- Navegação lateral consistente.
- Cabeçalho com identificação do usuário.
- Responsividade para desktop e tablet.
- Acessibilidade por teclado e contraste adequado.

A tabela operacional deve ser o elemento central da segunda tela. O mapa deve complementar o trabalho, sem ocupar espaço excessivo quando não estiver sendo utilizado.

---

## 20. Páginas adicionais

Além das duas telas principais, implemente páginas de suporte ao funcionamento completo:

1. Login.
2. Dashboard operacional.
3. Nova solicitação.
4. Painel de prioridades.
5. Detalhes completos da OS.
6. Cadastro e consulta de equipes.
7. Mapa operacional.
8. Histórico de operações.
9. Administração das regras de prioridade.
10. Administração de códigos e prazos.
11. Cadastro de trechos e equipamentos, quando aplicável.
12. Usuários e permissões.
13. Tela do Usuário Chave (seção 7A).
14. Cadastro de municípios, localidades, subestações, conjuntos elétricos, transformadores e classes de cliente.

Não transforme essas páginas adicionais em um projeto paralelo. A prioridade é garantir primeiro o funcionamento completo do fluxo entre abertura, classificação, despacho e acompanhamento.

---

## 21. Testes obrigatórios

Implemente testes unitários, de integração e, quando aplicável, de interface.

Os testes de prioridade devem verificar:

- A mesma entrada produz o mesmo resultado para a mesma versão de regras.
- Os critérios configurados são aplicados corretamente.
- Situações críticas respeitam as regras de precedência.
- Uma OS não recebe prioridade baseada em dados inexistentes tratados incorretamente como risco zero.
- A mudança de um critério relevante recalcula a prioridade.
- A pontuação detalhada corresponde à pontuação total.
- Os desempates são determinísticos.
- A regra de manutenção corretiva e preventiva respeita as exceções definidas.
- Os prazos são calculados segundo o calendário configurado.
- Mudanças de versão das regras são rastreáveis.
- A pontuação publicada pelo Usuário Chave é a utilizada no cálculo.
- A alteração de pontos pela tela do Usuário Chave gera nova versão sem alterar resultados históricos.
- Opções sem pontos definidos impedem a publicação.
- Faixas de prioridade com lacuna ou sobreposição são rejeitadas.
- O simulador e o cálculo de produção devolvem o mesmo resultado para a mesma entrada.
- Usuário sem o perfil de Usuário Chave não consegue editar nem publicar pontuação.
- A pontuação específica de uma cidade prevalece sobre a global nessa cidade.

Os testes de integração devem verificar:

- Criação de solicitação e OS.
- Persistência no PostgreSQL.
- Unicidade dos números.
- Atualização da fila.
- Aplicação dos filtros.
- Designação de equipe.
- Prevenção de despacho duplicado.
- Transições válidas de status.
- Registro de auditoria.
- Tratamento de erros.
- Controle de permissões.
- Separação por cidade em filas, indicadores e despacho.
- Bloqueio de acesso a OS de município não autorizado.
- Busca de UC e registro com "não sabe a UC".
- Interpretação do número do transformador (localidade e número local), incluindo zeros à esquerda.
- Validação de CEP e de coordenadas (formato, intervalo e alertas).
- Dependência entre circuito e conjunto elétrico.

Os testes de interface devem verificar o fluxo completo desde o preenchimento do formulário até a visualização da OS na fila e a abertura do modal de despacho.

Inclua casos envolvendo hospital, cabo caído, interrupção residencial, ocorrência preventiva, localização desconhecida, OS vencida e equipe indisponível.

Inclua também casos envolvendo UC não informada, número de transformador inválido ou com localidade não cadastrada, coordenadas fora do território brasileiro, CEP único de município, OS de cidades diferentes na visão consolidada, apoio intermunicipal e usuário sem permissão para o município.

---

## 22. Documentação e execução

Entregue um README em português contendo:

- Visão geral do projeto.
- Arquitetura e decisões técnicas.
- Pré-requisitos.
- Configuração do ambiente.
- Variáveis de ambiente.
- Como executar PostgreSQL.
- Como executar migrações.
- Como iniciar o backend.
- Como iniciar o frontend.
- Como executar os testes.
- Como configurar o mapa.
- Como configurar as regras de prioridade.
- Como cadastrar códigos e prazos.
- Limitações conhecidas.
- Como integrar futuramente com sistemas corporativos.

Forneça um arquivo `.env.example` sem segredos reais.

Inclua scripts para facilitar a execução e documente as portas utilizadas.

Crie um usuário administrativo inicial somente por um mecanismo seguro e documentado, sem deixar senhas padrão permanentes em produção.

Utilize migrações do Entity Framework Core para versionar o esquema do banco.

---

## 23. Estratégia de implementação

Implemente em etapas, mantendo a aplicação executável em cada fase.

### Etapa 1 — Fundação

- Criar a estrutura de pastas.
- Configurar backend, frontend e PostgreSQL.
- Implementar autenticação.
- Definir as entidades principais.
- Criar as migrações.
- Documentar como executar o projeto.

### Etapa 2 — Abertura de solicitação

- Construir o formulário completo.
- Implementar validações.
- Gerar números únicos.
- Persistir solicitação e OS.
- Registrar histórico inicial.
- Criar testes de integração.
- Implementar o cadastro de municípios, localidades, subestações, conjuntos e transformadores.
- Implementar a busca de UC, a consulta de CEP e o registro de coordenadas.
- Aplicar a separação por cidade em solicitações e OS.

### Etapa 3 — Motor de prioridade

- Criar o modelo configurável de regras.
- Implementar a pontuação.
- Implementar regras de precedência.
- Configurar códigos e prazos demonstrativos.
- Implementar explicação dos resultados.
- Testar os cenários críticos.
- Implementar a tela do Usuário Chave (seção 7A), com simulador e versionamento.

### Etapa 4 — Painel operacional

- Construir a tabela.
- Implementar filtros.
- Implementar modal de detalhes.
- Implementar atualização a cada 60 segundos.
- Implementar atualização manual.
- Conectar todos os componentes à API real.
- Implementar o seletor de cidade e aplicar o recorte por cidade à tabela, aos indicadores e ao mapa.

### Etapa 5 — Equipes e despacho

- Criar cadastro de equipes.
- Implementar status e disponibilidade.
- Implementar recomendação de equipe.
- Criar o fluxo de designação.
- Tratar concorrência e auditoria.

### Etapa 6 — Mapa

- Integrar o mapa.
- Apresentar ocorrências e equipes com coordenadas disponíveis.
- Integrar geocodificação e roteamento configuráveis.
- Identificar visualmente dados reais, aproximados e demonstrativos.

### Etapa 7 — Validação final

- Executar testes.
- Corrigir erros.
- Verificar o fluxo completo.
- Revisar segurança.
- Atualizar a documentação.
- Entregar instruções para execução local.

---

## 24. Critérios de aceitação

O projeto somente será considerado concluído quando:

1. For possível registrar uma solicitação pela interface.
2. Uma OS for gerada e persistida no PostgreSQL.
3. O sistema calcular uma prioridade usando regras executadas no backend.
4. A classificação apresentar os critérios e pontos que justificam o resultado.
5. A OS aparecer na fila operacional sem cadastro manual adicional.
6. A lista puder ser filtrada e atualizada.
7. A atualização automática funcionar a cada 60 segundos.
8. O botão de atualização manual funcionar.
9. O modal apresentar os detalhes completos da OS.
10. O despacho permitir consultar equipes disponíveis e compatíveis.
11. A atribuição de equipe for persistida e auditada.
12. O mapa exibir dados geográficos disponíveis sem inventar localizações.
13. O sistema impedir atribuições incompatíveis e transições inválidas.
14. As regras de prioridade, os códigos e os prazos forem configuráveis.
15. Os testes principais forem executados com sucesso.
16. A aplicação puder ser iniciada seguindo o README.
17. Frontend e backend permanecerem organizados em pastas separadas.
18. Não existirem botões essenciais sem funcionalidade.
19. Os dados demonstrativos forem identificados e não confundidos com dados reais.
20. A documentação deixar claras as integrações necessárias para utilizar dados oficiais de rede, SGM e localização das equipes.
21. As solicitações e as OS forem separadas por cidade, e os usuários só acessarem as cidades permitidas.
22. O Usuário Chave conseguir definir os pontos de cada opção e as faixas de cada prioridade pela tela da seção 7A, sem alterar código.
23. A publicação de nova pontuação gerar versão rastreável, sem alterar o histórico das OS já calculadas.
24. A busca de UC funcionar e o registro com "não sabe a UC" for possível sem impedir a abertura da OS.
25. O número do transformador for interpretado em localidade e número local e armazenado como texto.
26. Circuito (subestação e local), conjunto elétrico, classe e situação do cliente puderem ser registrados e usados em filtros e no detalhe da OS.
27. Rua, CEP, endereço e coordenadas puderem ser registrados, e as coordenadas forem entregues à equipe no despacho.

---

## 25. Instruções finais ao agente

Antes de implementar regras operacionais definitivas, identifique quais informações dependem da documentação oficial do SGM e da concessionária.

Não bloqueie o desenvolvimento das funcionalidades genéricas por falta dessa documentação. Implemente um mecanismo configurável e utilize dados demonstrativos explicitamente identificados.

Não invente integrações, credenciais, dados de rede, localizações ou códigos oficiais.

Se uma integração externa não puder ser realizada sem credenciais ou acesso a um sistema corporativo, implemente a interface de integração, documente os requisitos e deixe claro o que depende do acesso externo.

Não entregue somente um plano, uma lista de arquivos ou uma coleção de componentes sem integração. Crie os arquivos, implemente a aplicação e valide o funcionamento de cada etapa.

Ao final, apresente:

- A estrutura final do repositório.
- As principais decisões arquiteturais.
- As regras de negócio implementadas.
- As funcionalidades concluídas.
- Os testes executados e seus resultados.
- As integrações ainda pendentes.
- As instruções exatas para iniciar a aplicação.
- As premissas adotadas e os pontos que ainda dependem de confirmação da operação (seção 0.2).

Priorize confiabilidade, rastreabilidade, segurança operacional, explicabilidade das prioridades e qualidade de código. A aplicação deve ser preparada para evoluir de um ambiente de desenvolvimento para um ambiente operacional real, sem afirmar que está pronta para operação crítica antes das validações necessárias.
