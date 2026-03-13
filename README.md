# NsTech Order Service

Este é um serviço de gestão de pedidos com itens e validação de estoque, desenvolvido como parte de um teste técnico para Arquiteto Backend Sênior.

## Tecnologias
- .NET 10
- ASP.NET Minimal API
- EF Core com PostgreSQL
- MediatR (CQRS/Vertical Slices)
- FluentValidation
- JWT Authentication
- xUnit & FluentAssertions
- Docker & Docker Compose

## Estrutura do Projeto
O projeto segue uma arquitetura limpa separada em:
- **Domain**: Entidades, Enums e Interfaces de Repositório.
- **Application**: Lógica de negócio organizada por Features (Vertical Slices) usando MediatR.
- **Infrastructure**: Implementação do EF Core, Repositórios e Configurações de Banco.
- **Api**: Endpoints Minimal API e configurações de Middleware.

## Como Executar

### Via Docker (Recomendado)
Certifique-se de ter o Docker instalado e execute:
```bash
docker compose up --build
```
A API estará disponível em `http://localhost:8080`.

### Localmente
1. Tenha um PostgreSQL rodando.
2. Configure a connection string em `src/NsTech.Api/appsettings.json`.
3. Execute as migrações:
```bash
dotnet ef database update --project src/NsTech.Infrastructure --startup-project src/NsTech.Api
```
4. Rode a aplicação:
```bash
dotnet run --project src/NsTech.Api
```

## Como Testar
Para rodar os testes unitários e garantir a cobertura:
```bash
dotnet test
```

## Decisões Técnicas

### 1. Estratégia de Concorrência
Para evitar estoque negativo e inconsistências em ambientes de alta concorrência, foi adotado o **Controle de Concorrência Otimista**.
- A entidade `Product` possui uma propriedade `Version` (mapeada para a coluna de sistema `xmin` no PostgreSQL).
- Quando o estoque é reservado (na confirmação do pedido), o EF Core valida se a versão do produto não foi alterada desde a leitura.
- Em caso de conflito, a API retorna um status `409 Conflict`.

### 2. Idempotência
Os endpoints de **Confirmação** e **Cancelamento** de pedido são idempotentes. No domínio, as entidades validam o estado atual antes de aplicar a transição. Se o pedido já estiver no estado desejado, a operação retorna sucesso sem erro e sem repetir os efeitos colaterais (como liberação/reserva de estoque).

### 3. Vertical Slices
A camada de `Application` foi organizada por features. Cada feature contém seu Command/Query e Handler, facilitando a manutenção e a escalabilidade do sistema.

### 4. Autenticação JWT
A API está protegida por JWT. Para obter um token de teste (demo), use as credenciais:
- **User**: `admin`
- **Password**: `admin`
No endpoint `POST /auth/token?username=admin&password=admin`.

## Endpoints Principais
- `POST /auth/token`: Gera token JWT.
- `POST /orders`: Cria um novo pedido (status Placed).
- `POST /orders/{id}/confirm`: Confirma o pedido e reserva estoque.
- `POST /orders/{id}/cancel`: Cancela o pedido e libera estoque.
- `GET /orders/{id}`: Detalhes do pedido.
- `GET /orders`: Listagem paginada com filtros.
- `POST /products`: Cadastro de produtos (CRUD).
- `GET /products`: Listagem de produtos.
