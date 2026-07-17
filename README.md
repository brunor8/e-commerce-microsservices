# Catalog API — E-Commerce Microservices

API REST para gerenciamento de catálogo de produtos (produtos e categorias), construída como o primeiro microsserviço de um sistema de e-commerce distribuído. Projeto desenvolvido como vitrine técnica, aplicando Clean Architecture, DDD, CQRS e boas práticas de engenharia de software sênior em .NET.

## Stack Técnica

- **.NET 10** / C#
- **PostgreSQL** (via Docker)
- **Entity Framework Core** (Npgsql provider)
- **MediatR** — CQRS e pipeline behaviors
- **FluentValidation** — validação de entrada
- **Scalar** — documentação interativa da API (OpenAPI nativo do .NET)
- **Docker & Docker Compose** — orquestração local
- **Health Checks** — incluindo verificação real de conectividade com o banco

## Arquitetura

O projeto segue **Clean Architecture**, com dependências apontando sempre para dentro, em direção ao Domain:

```
Domain ← Application ← Infrastructure
                      ← API
```

```
Catalog/
  src/
    Catalog.Domain/          # Entidades, Value Objects, regras de negócio puras
    Catalog.Application/     # Casos de uso (CQRS), DTOs, validação, orquestração
    Catalog.Infrastructure/  # EF Core, repositórios, mapeamento de persistência
    Catalog.API/              # Controllers, middlewares, configuração HTTP
  Tests/
    Catalog.Tests/            # (em construção)
  Dockerfile
  docker-compose.yml
  Catalog.slnx
```

### Domain

Modelo rico, não anêmico — entidades protegem suas próprias invariantes e expõem comportamento em vez de apenas dados:

- **`Product`** — entidade central, com métodos como `AddStock`, `RemoveStock`, `Update`, `Deactivate`. Nenhum setter público; todas as mudanças de estado passam por regras de negócio explícitas.
- **`Category`** — suporta ativação/desativação (base para soft delete).
- **`Money`** — Value Object imutável para preço + moeda, com igualdade por valor.
- **`Sku`** — Value Object que valida formato (letras maiúsculas, números e hífen) e encapsula a regra de negócio do identificador de produto.
- **`DomainException`** — exceção específica para violações de regra de negócio.
- Interfaces de repositório (`IProductRepository`, `ICategoryRepository`) definidas aqui, implementadas na Infrastructure — inversão de dependência aplicada na prática.

### Application

Implementa **CQRS** com **MediatR**, separando comandos (escrita) de queries (leitura):

- **Commands**: `CreateProduct`, `UpdateProduct`, `DeleteProduct`, `AddStock`, `CreateCategory`, `DeleteCategory`
- **Queries**: `GetAllProducts`, `GetProductById`, `GetAllCategories`
- **Validators** (FluentValidation) para cada Command, isolados e testáveis independentemente
- **`ValidationBehavior`** — Pipeline Behavior do MediatR que intercepta automaticamente todo Command/Query, executa a validação correspondente e bloqueia a execução do Handler em caso de erro, sem que nenhum Handler precise chamar validação manualmente
- DTOs próprios (`ProductDto`, `CategoryDto`) desacoplando o contrato externo das entidades de Domain

### Infrastructure

- **`CatalogDbContext`** com mapeamento via Fluent API (`IEntityTypeConfiguration<T>`)
- Mapeamento de Value Objects: `Sku` via `HasConversion`, `Money` via `OwnsOne`
- Enum `ProductStatus` persistido como texto (mais legível e resiliente a reordenação)
- `AsNoTracking()` em queries de leitura pura, para performance
- Regra de negócio "SKU único" e "categoria com produtos vinculados não pode ser removida" resolvidas no nível de Application, consultando repositórios

### API

- Controllers finos — sem lógica de negócio, apenas orquestram `IMediator.Send()`
- **Middleware global de tratamento de exceções**, convertendo automaticamente:
  - `NotFoundException` → `404`
  - `ValidationException` (FluentValidation) → `400`, com lista estruturada de erros por campo
  - `InvalidOperationException` → `409`
  - Exceções não mapeadas → `500`, com log estruturado
- **Health Checks** em `/health`, incluindo teste real de conectividade com o PostgreSQL (não apenas "a API está de pé")
- Documentação interativa via **Scalar** (`/scalar/v1`), usando o gerador OpenAPI nativo do .NET (substituto atual do Swashbuckle/Swagger nos templates recentes)

## Decisões de Design Relevantes

- **Soft delete em `Category`**: deletar fisicamente uma categoria quebraria a integridade referencial com produtos existentes. Em vez disso, `DeleteAsync` chama `category.Deactivate()`, e listagens filtram automaticamente por `IsActive`.
- **Bloqueio de exclusão de categoria com produtos vinculados**: regra de negócio explícita no `DeleteCategoryCommandHandler`, evitando desativação acidental de categorias em uso.
- **Value Objects com correção de query EF Core**: `Sku` e `Money` são mapeados via conversão, o que exige cuidado ao escrever LINQ — comparações devem ser feitas contra o objeto Value Object inteiro (`p.Sku == new Sku(valor)`), não contra propriedades internas (`p.Sku.Value == valor`), pois o EF Core não traduz acesso a membros de tipos convertidos.
- **Gestão de segredos**: nenhuma credencial versionada no repositório.
  - Localmente: **User Secrets** do .NET (`dotnet user-secrets`)
  - Via Docker: arquivo `.env` (ignorado pelo Git), com `.env.example` documentando as variáveis esperadas
- **FluentValidation** escolhido no lugar de Data Annotations por permitir validação isolada, testável, com suporte nativo a regras condicionais/assíncronas e integração direta com o pipeline do MediatR.

## Como Rodar Localmente

### Pré-requisitos
- Docker Desktop (com integração WSL2, se estiver no Windows)
- .NET 10 SDK (apenas se for rodar fora de container)

### Passos

1. Clone o repositório e copie o arquivo de variáveis de ambiente:
   ```bash
   cp .env.example .env
   ```
   Ajuste os valores conforme necessário.

2. Suba a aplicação completa (API + PostgreSQL):
   ```bash
   docker compose up --build
   ```

3. Aplique as migrations (primeira execução):
   ```bash
   dotnet ef database update --project src/Catalog.Infrastructure --startup-project src/Catalog.API
   ```

4. Acesse:
   - **Documentação interativa**: http://localhost:5000/scalar/v1
   - **Health check**: http://localhost:5000/health

## Roadmap — Próximas Implementações

Este projeto está em desenvolvimento ativo. Os próximos passos planejados, em ordem de prioridade:

- [ ] **Testes automatizados** — testes unitários para Domain e Handlers (xUnit + mocking), testes de integração da API com banco real via Testcontainers
- [ ] **CI/CD** — pipeline no GitHub Actions rodando build, testes e lint a cada push/PR
- [ ] **Novos microsserviços** — expansão da arquitetura para um cenário real de microsserviços:
  - `Orders` — criação e gestão de pedidos, consumindo o Catalog via HTTP/gRPC
  - `Payments` — simulação de processamento de pagamento
  - Comunicação assíncrona entre serviços via **RabbitMQ** (padrão de eventos, ex.: `OrderCreated`)
- [ ] **API Gateway** — unificação do acesso aos serviços via YARP ou Ocelot
- [ ] **Observabilidade** — logging estruturado (Serilog) e tracing distribuído (OpenTelemetry), essencial para depurar requisições que atravessam múltiplos serviços
- [ ] **Autenticação e Autorização** — JWT, com controle de acesso por roles
- [ ] **Paginação e filtros** — evolução das queries de listagem (`GetAllProducts`, `GetAllCategories`) para suportar paginação, filtros por categoria/status e ordenação
- [ ] **Cache** — Redis para consultas de leitura frequente (ex.: listagem de produtos)

## Autor

Bruno Rocha Rodrigues

## Objetivo

Projeto desenvolvido como portfólio técnico, com foco em demonstrar práticas de arquitetura e engenharia de software aplicadas a um cenário realista de backend .NET.