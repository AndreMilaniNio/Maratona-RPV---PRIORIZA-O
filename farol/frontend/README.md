# Farol — Frontend

Interface web do **Farol**, sistema de gestão, priorização e despacho de Ordens de Serviço (OS) da rede elétrica.
Todo o cálculo de prioridade, prazos e permissões acontece na API (ASP.NET Core); o frontend apenas apresenta,
coleta os fatos e chama os endpoints.

## Pré-requisitos

- Node.js 20.19+ (testado com Node 24) e npm.
- API do Farol em execução (padrão `http://localhost:5080`, com CORS liberado para `http://localhost:5173`).

## Como executar

```bash
cd farol/frontend
cp .env.example .env          # ajuste VITE_API_URL se a API não estiver em http://localhost:5080
npm install
npm run dev                   # http://localhost:5173
```

Usuários demonstrativos (senha `Farol@Demo2026`, dados fictícios): `admin@farol.demo`, `supervisor@farol.demo`,
`despachante@farol.demo`, `atendente@farol.demo`, `atendente.vtq@farol.demo`, `chave@farol.demo`, `chave2@farol.demo`,
`equipe.lum03@farol.demo`.

### Scripts

| Script | O que faz |
|---|---|
| `npm run dev` | servidor de desenvolvimento Vite (porta 5173) |
| `npm run build` | `tsc -b && vite build` — checagem de tipos estrita + build de produção em `dist/` |
| `npm test` | `vitest run` — testes unitários e de componentes |
| `npm run typecheck` | apenas a checagem de tipos |
| `npm run gen:types` | regenera `src/types/api.generated.ts` a partir de `../docs/openapi.json` |

### Variáveis de ambiente

| Variável | Padrão | Uso |
|---|---|---|
| `VITE_API_URL` | `http://localhost:5080` | URL base da API (REST e hub SignalR `/hubs/operacao`). Embutida no bundle no build. |

### Docker

Build multi-estágio (Node → nginx) com fallback de SPA (`nginx.conf`):

```bash
docker build -t farol-frontend --build-arg VITE_API_URL=http://localhost:5080 .
docker run --rm -p 5173:80 farol-frontend
```

## Versões utilizadas

| Biblioteca | Versão |
|---|---|
| React / React DOM | 19.1.9 |
| TypeScript (modo estrito) | 5.9.2 |
| Vite / @vitejs/plugin-react | 7.1.7 / 5.0.4 |
| React Router | 7.9.3 |
| TanStack Query | 5.90.2 |
| React Hook Form / @hookform/resolvers / Zod | 7.63.0 / 5.2.2 / 3.25.76 |
| Tailwind CSS (+ @tailwindcss/vite) | 4.1.13 |
| Radix UI (dialog, tabs, select, popover, checkbox, tooltip, dropdown-menu, switch, label, slot) | 1.x / 2.x (ver `package.json`) |
| class-variance-authority / clsx / tailwind-merge | 0.7.1 / 2.1.1 / 3.3.1 |
| Leaflet / react-leaflet | 1.9.4 / 5.0.0 (tiles OpenStreetMap) |
| @microsoft/signalr | 9.0.19 |
| lucide-react | 0.544.0 |
| sonner (toasts) | 2.0.7 |
| Vitest / Testing Library (react, jest-dom, user-event) / jsdom | 3.2.4 / 16.3.0, 6.8.0, 14.6.1 / 26.1.0 |

## Arquitetura

```
src/
├── app/            router (guards por permissão), providers (Query, Auth, Tooltip, Toaster), layouts (cabeçalho + menu lateral), config (env, navegação)
├── pages/          uma pasta por tela (Login, Dashboard, NovaSolicitacao, PainelOperacional, DetalhesOS, Equipes, MapaOperacional, Historico, ConfiguracaoPontuacao, Configuracoes)
├── features/       solicitacoes, ordens-servico, priorizacao, equipes, despacho, geolocalizacao, rede-eletrica, clientes-uc, autenticacao, administracao
│   └── <feature>/{components,hooks,services,schemas,types}
├── components/     ui (primitivos estilo shadcn sobre Radix), forms, tables, maps, feedback, layout
├── services/       api (cliente HTTP, ProblemDetails, chaves de consulta) e realtime (SignalR)
├── hooks/  lib/  types/  styles/  test/
```

Decisões principais:

- **Tipos da API** gerados do OpenAPI (`scripts/gen-types.cjs` → `src/types/api.generated.ts`); enums trafegam como strings.
- **Chamadas HTTP** só em `features/*/services`; componentes de apresentação recebem dados por props; consultas e
  mutações via TanStack Query. Nenhuma regra de pontuação no React: a prévia do formulário usa `POST /api/solicitacoes/previa`
  e o simulador do Usuário Chave usa `POST /api/configuracoes/pontuacao/simular`.
- **Erros** RFC 7807 (`ProblemDetails`) exibidos com título, erros por campo (mapeados nos campos do formulário) e pendências (422).
  401 encerra a sessão e volta ao Login.
- **Sessão**: JWT em `sessionStorage`. A cidade escolhida no cabeçalho é salva no perfil (`PUT /api/me/preferencias`), não no navegador.
- **Atualização**: a fila, os indicadores e o mapa atualizam a cada 60 s (reconciliação) e também quando o hub SignalR
  `/hubs/operacao` emite `filaAlterada` (`Acompanhar(municipioId)`); "Atualizar agora" e "Última atualização" no painel;
  se a API cair, os dados atuais são mantidos com aviso de dados desatualizados.
- **Datas**: a API usa UTC; a interface exibe em `America/Sao_Paulo` (pt-BR).
- **Identidade visual** (seção 19): azul `#2457A6` / `#17365D`, fundo `#F4F7FB`, verde-musgo `#667A45`, texto `#243247`,
  bordas `#DCE3EC`. Cores de prioridade vêm da API e sempre acompanham texto e ícone. Faixa âmbar
  "Pontuação demonstrativa, não oficial" quando a versão de pontuação em vigor é demonstrativa; selo "Demonstrativo" em registros fictícios.
- **Mapa**: Leaflet + OpenStreetMap; OS por prioridade (críticas em losango), equipes por status (posição demonstrativa
  tracejada e rotulada), subestações; sem roteamento configurado, a distância é rotulada "linha reta — sem estimativa de tempo".

## Testes

`npm test` executa os testes Vitest + Testing Library (ver seção "Situação" abaixo para a lista).
