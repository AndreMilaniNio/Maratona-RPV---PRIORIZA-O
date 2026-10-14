# Prompt — Instalação e configuração do PriorizaOS / Farol

Copie o texto abaixo e envie ao Codex quando precisar preparar o projeto em uma nova máquina.

```text
Estou no projeto Maratona-RPV---PRIORIZA-O. Prepare o ambiente para executar o sistema Farol/PriorizaOS localmente no Windows.

Objetivo final:
- Frontend disponível em http://localhost:5173
- API disponível em http://localhost:5080
- Swagger disponível em http://localhost:5080/swagger
- Verificação da API em http://localhost:5080/api/saude
- Banco PostgreSQL disponível na porta 5432
- Aplicação abrindo sem tela de login, em modo de operador único.

Siga estas regras:
1. Primeiro verifique o que já está instalado. Não reinstale uma dependência que já esteja disponível e compatível.
2. Informe, antes de cada instalação, o que será instalado e peça a autorização necessária para downloads ou alterações fora da pasta do projeto.
3. Prefira Docker Compose. Se Docker não estiver instalado ou não puder ser usado, prepare a execução local com PostgreSQL, .NET e Node.js.
4. Não grave senhas reais, tokens ou chaves privadas em arquivos versionados. O arquivo .env deve permanecer ignorado pelo Git.
5. Ao terminar, inicie os serviços, valide as URLs informadas acima e apresente os comandos de parada.

Pré-requisitos:
- Git (para obter o projeto);
- Docker Desktop com Docker Compose (método recomendado);
- Alternativa sem Docker: PostgreSQL 15 ou superior, .NET SDK 10 e Node.js 20 ou superior (npm incluso).

Método recomendado — Docker Compose:
1. Entre na pasta `farol`.
2. Copie `.env.example` para `.env`.
3. No `.env`, configure no mínimo:
   POSTGRES_DB=farol
   POSTGRES_USER=farol
   POSTGRES_PASSWORD=<uma senha local forte>
   JWT_CHAVE=<uma chave aleatória com ao menos 32 caracteres>
   FAROL_DEMO=true
   FAROL_DEMO_SENHA=<senha local de demonstração com ao menos 10 caracteres>
   VITE_API_URL=http://localhost:5080
   FAROL_CEP_PROVEDOR=Demonstrativo
4. Não é obrigatório preencher FAROL_ADMIN_EMAIL e FAROL_ADMIN_SENHA para a demonstração local em modo de operador único.
5. Suba os serviços com `docker compose up --build`.
6. As migrações e a base fictícia devem ser aplicadas automaticamente pela API.

Alternativa — execução local sem Docker (PowerShell):
1. Garanta que o PostgreSQL esteja em execução e crie o banco `farol` para o usuário que será utilizado.
2. Configure apenas para a sessão atual do PowerShell:
   $env:ConnectionStrings__Farol='Host=localhost;Port=5432;Database=farol;Username=farol;Password=<senha-local>'
   $env:Jwt__Chave='<chave-aleatoria-com-no-minimo-32-caracteres>'
   $env:Farol__Demo__Habilitado='true'
   $env:Farol__Demo__SenhaUsuarios='<senha-demo-com-no-minimo-10-caracteres>'
   $env:Farol__Cep__Provedor='Demonstrativo'
   $env:ASPNETCORE_URLS='http://localhost:5080'
3. Em um terminal, execute:
   cd farol/backend
   dotnet restore
   dotnet run --project src/Farol.API
4. Em outro terminal, execute:
   cd farol/frontend
   Copy-Item .env.example .env
   npm.cmd install
   npm.cmd run dev
5. Se PowerShell bloquear `npm`, use sempre `npm.cmd` em vez de `npm`.

Configurações obrigatórias do projeto:
- `farol/frontend/.env`: `VITE_API_URL=http://localhost:5080`.
- API: CORS deve permitir `http://localhost:5173`.
- API: `Farol:ModoOperadorUnico=true` para não exibir login na demonstração local.
- API: `Farol:Banco:InicializarAoSubir=true` para aplicar as migrações ao iniciar.
- API: `Farol:Demo:Habilitado=true` para carregar os dados fictícios, OS e UCs de demonstração.
- O banco e os dados de demonstração são locais e fictícios; não usar em produção.

Validação final obrigatória:
1. Confirme que `http://localhost:5173` retorna HTTP 200.
2. Confirme que `http://localhost:5080/api/saude` retorna HTTP 200 e JSON com `status: "ok"`.
3. Confirme que `http://localhost:5080/api/ordens-servico` retorna HTTP 200 e contém OS de demonstração.
4. Confirme que o frontend pode acessar a API, com CORS para a origem `http://localhost:5173`.
5. Apresente qualquer erro restante de forma objetiva, com a causa e o próximo comando seguro para resolvê-lo.

Para encerrar:
- Docker: pressione Ctrl+C no terminal do Compose; para remover os containers, use `docker compose down` dentro de `farol`.
- Execução local: pressione Ctrl+C nos terminais da API e do Vite. Não apague o banco sem confirmação explícita.
```

## Observações rápidas

- A rota correta de saúde é `/api/saude`; `/health` não é uma rota do projeto.
- O frontend usa Vite na porta `5173` e a API deve usar a porta `5080` neste ambiente.
- Para testar com dados existentes, a UC `983750` corresponde ao endereço demonstrativo `Rua 398, nº 8440 — Jardim Europa`, CEP `00130398`.
