# Implantação: Supabase + Render + Vercel

O ambiente de produção foi preparado com esta divisão:

```text
Navegador -> Vercel (React/Vite) -> Render (ASP.NET Core) -> Supabase (PostgreSQL)
```

O frontend não usa credenciais do Supabase. Somente a API no Render recebe a
cadeia de conexão do banco, o que preserva o banco fora do navegador.

## 1. Supabase

O projeto de produção é `farol-rpv`, na região `sa-east-1`. No painel do
Supabase, obtenha a cadeia de conexão **Session pooler** e forneça-a apenas ao
Render, com SSL obrigatório. A variável deve ter o nome:

```text
ConnectionStrings__Farol
```

Não aplique manualmente `database/migracoes.sql`: a API executa as migrações
EF Core pendentes ao iniciar. Assim, a tabela de histórico
`__ef_migracoes` permanece consistente com o código.

## 2. Render (API)

O arquivo `render.yaml` deve estar na raiz do repositório (`farol/`). Na criação
do Blueprint, preencha os campos solicitados:

| Variável | Valor |
| --- | --- |
| `ConnectionStrings__Farol` | cadeia Session pooler do Supabase, com `Ssl Mode=Require` |
| `Farol__Cors__Origens__0` | URL de produção da Vercel, sem barra final |

O Render gera `Jwt__Chave` e desliga o modo de operador único e a carga de
dados demonstrativos. A API atende na porta atribuída pelo Render e tem health
check em `/api/saude`.

## 3. Vercel (frontend)

No projeto da Vercel, defina **Root Directory** como `frontend`. O arquivo
`frontend/vercel.json` compila o Vite e preserva as rotas do React Router.

Crie a variável de ambiente de produção e preview:

```text
VITE_API_URL=https://<url-da-api-no-render>
```

`VITE_API_URL` é pública por natureza e não pode conter segredo. Depois de
salvá-la, faça novo deploy para que a URL seja incorporada ao bundle.

## Ordem de ativação

1. Publique este diretório em um repositório Git (GitHub, GitLab ou Bitbucket).
2. Crie o Blueprint no Render a partir de `render.yaml` e informe a conexão do
   Supabase.
3. Copie a URL HTTPS resultante da API para `VITE_API_URL` na Vercel.
4. Copie a URL da Vercel para `Farol__Cors__Origens__0` no Render e faça o
   redeploy da API.
5. Verifique `https://<api>/api/saude` e faça login pelo frontend.
