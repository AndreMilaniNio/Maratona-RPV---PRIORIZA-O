# Testes do Farol

Os testes ficam junto de cada aplicação:

| Tipo | Local | Comando |
|---|---|---|
| Unitários (motor, ordenação, prazos, faixas, domínio, validações) | `backend/tests/Farol.UnitTests` | `dotnet test` |
| Integração (API real + PostgreSQL real) | `backend/tests/Farol.IntegrationTests` | `FAROL_TEST_CONNECTION=... dotnet test` |
| Arquitetura (dependências entre camadas, autorização) | `backend/tests/Farol.ArchitectureTests` | `dotnet test` |
| Interface (componentes e regras de formulário) | `frontend/src/**/*.test.ts(x)` | `npm test` |

Os testes de integração **apagam e recriam** o banco indicado em `FAROL_TEST_CONNECTION` a cada execução — use um banco descartável.
