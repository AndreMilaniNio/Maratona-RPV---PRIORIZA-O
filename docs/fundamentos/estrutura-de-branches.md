# Estrutura das Branches Git

## Objetivo

Este documento define a finalidade das branches do repositório e o fluxo esperado para levar alterações individuais até a produção.

## Branches principais

| Branch              | Finalidade                                                                    |
| ------------------- | ----------------------------------------------------------------------------- |
| `main`              | Versão estável considerada pronta para produção.                              |
| `release`           | Integração e validação das alterações selecionadas para a próxima publicação. |
| `feature/<usuario>` | Trabalho individual de cada integrante, isolado da versão estável.            |

## Branches individuais atuais

- `feature/erick`
- `feature/andre`
- `feature/nathan`
- `feature/alexandre`
- `feature/nicoly`

Use nomes curtos, em minúsculas, sem espaços ou acentos. Para uma atividade específica, pode-se usar um nome mais descritivo, por exemplo `feature/andre-validacao-rpv`.

## Fluxo de trabalho

```text
feature/<usuario> -> release -> main
```

1. Desenvolva e teste a alteração na branch individual.
2. Abra um Pull Request da branch individual para `release`.
3. Revise e valide em `release` as alterações reunidas para a publicação.
4. Quando a versão estiver aprovada, abra um Pull Request de `release` para `main`.
5. Após a integração em `main`, a alteração passa a compor a versão estável de produção.

Não faça alterações diretamente em `main`. Use Pull Requests para integrar código, permitir revisão e registrar decisões. Corrija problemas encontrados durante a validação antes de promover a versão para `main`.

## Base das branches

As branches individuais e `release` existentes foram criadas a partir de `main`. Para novas atividades, crie a branch individual a partir da versão estável mais recente de `main`; integre-a em `release` por Pull Request. Antes de preparar uma publicação, mantenha `release` atualizada com as alterações aprovadas.

## Comandos úteis

```bash
# Atualizar as referências do repositório remoto
git fetch origin

# Criar uma branch individual a partir de main
git switch main
git pull origin main
git switch -c feature/seu-nome-descricao

# Publicar a branch individual no remoto
git push -u origin feature/seu-nome-descricao
```

Os Pull Requests de integração devem ter como destino `release`; a promoção aprovada deve ter como destino `main`.
