# HelpDesk API

API REST para gerenciamento de chamados de suporte técnico, desenvolvida em ASP.NET Core 8 com Entity Framework Core e PostgreSQL.

## Tecnologias

- ASP.NET Core 8
- Entity Framework Core
- Npgsql PostgreSQL
- JWT com autenticação
- Swagger/OpenAPI
- BCrypt para hash de senhas

## Estrutura principal

- HelpDesk.Api/Controllers
- HelpDesk.Api/Services
- HelpDesk.Api/Data
- HelpDesk.Api/Models
- HelpDesk.Api/DTOs

## Funcionalidades

- Autenticação e autorização por perfil
- Cadastro e login de usuários
- Gestão de categorias
- Criação, atualização e consulta de chamados
- Atribuição de técnico
- Comentários em chamados
- Validação de regras de negócio

## Perfis

- Usuario
- Tecnico
- Administrador

## Credenciais iniciais

Ao iniciar a aplicação com banco configurado, serão criados usuários de exemplo:

- Administrador: admin@helpdesk.com / admin123
- Técnico: tecnico@helpdesk.com / tecnico123
- Usuário: usuario@helpdesk.com / usuario123

## Execução local

A API está configurada para funcionar com SQLite por padrão em ambiente local, e também aceita PostgreSQL configurando `DatabaseProvider` e a connection string correta.

### Opção 1: SQLite (mais simples)

1. Execute:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd HelpDesk.Api
dotnet restore
dotnet build
dotnet run
```

2. Acesse:

- Swagger: http://localhost:5003/swagger
- API: http://localhost:5003/api

### Opção 2: PostgreSQL

1. Ajuste `DatabaseProvider` para `PostgreSQL` e configure `DefaultConnection` com a sua string do PostgreSQL.
2. Execute:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd HelpDesk.Api
dotnet restore
dotnet build
dotnet run
```

## Observações

- A autenticação usa JWT.
- O projeto foi pensado para fins de estudo e portfólio.
- Foi desenvolvido em etapas progressivas para facilitar aprendizado.

## Frontend

O dashboard React está em `helpdesk-web` e consome a API por meio do proxy do Vite.

```bash
cd helpdesk-web
npm install
npm run dev -- --host 127.0.0.1 --port 5175
```

Abra `http://localhost:5175`. O proxy do Vite encaminha as requisições para a API em `http://localhost:5003`.
