Converto abaixo o conteúdo do PDF para **Markdown estruturado**, mantendo títulos, listas e seções para você poder usar diretamente em um **README.md ou documentação**.

---

# Teste Técnico — Arquiteto Backend Sênior (3 dias)

**Objetivo:**
Avaliar arquitetura, qualidade de código, domínio, testes, performance, boas práticas e entrega “pronta para produção” (no essencial). 

**Tempo sugerido:**
Até **3 dias corridos**.

---

# Como entregar

1. Implemente a solução conforme o enunciado.
2. Ao final, envie:

* Link do repositório
* Instruções para rodar (**README**)
* Observações / decisões técnicas
  (no README ou em `/docs/decisions.md`)

---

# Stack e restrições

* **.NET:** preferencialmente **.NET 10**
* **Linguagem:** C#
* **Persistência:** EF Core com migrations
* **Banco:** Postgres (via Docker)
* **API:** ASP.NET **Minimal API**
* **Async/await** end-to-end
* **Arquitetura:** separar claramente

  * Domain
  * Application
  * Infrastructure
  * API
* **Testes:** xUnit
* **Docker:** `docker compose up` deve subir **API + banco**

### Opcional (desejável)

* **Mediator**
* **CQRS** (Command / Query separados)
* **Behaviors**

  * FastFail
  * Problem Details em tratamento global

---

# Domínio: Order Service

Você irá construir uma **API REST para gestão de pedidos com itens e validação de estoque**.

---

# Entidades (mínimo sugerido)

## Order

* Id
* CustomerId
* Status

  * Draft
  * Placed
  * Confirmed
  * Canceled
* Currency
* Items
* Total
* CreatedAt

## OrderItem

* ProductId
* UnitPrice
* Quantity

## Product / Stock

Modelo simples:

* ProductId
* UnitPrice
* AvailableQuantity

Você tem liberdade para **ajustar o modelo**, desde que as regras sejam atendidas.

---

# Requisitos funcionais (MUST)

## 1) CRUD simples de Produto

* Domínio **anêmico**

---

# 2) Criar pedido

```
POST /orders
```

### Payload mínimo

```json
{
  "customerId": "string",
  "currency": "string",
  "items": [
    { "productId": "guid", "quantity": 1 }
  ]
}
```

### Regras

* Não pode criar pedido **sem itens**
* Quantidade deve ser **> 0**
* Produto deve existir
* Não pode exceder **estoque disponível**
* Total do pedido =
  `sum(unitPrice * quantity)`

### Estado inicial

Pedido nasce como:

```
Placed
```

ou

```
Draft → Placed
```

(se justificar)

---

# 3) Confirmar pedido (idempotente)

```
POST /orders/{id}/confirm
```

### Regras

* Só confirma pedido em **Placed**
* Confirmação deve **reservar ou baixar estoque**
* Se chamado **2x**, deve retornar **mesmo resultado**

### Estado

```
Placed → Confirmed
```

---

# 4) Cancelar pedido (idempotente)

```
POST /orders/{id}/cancel
```

### Regras

* Pode cancelar pedidos em:

  * Placed
  * Confirmed
* Cancelamento deve **liberar estoque reservado**
* Endpoint deve ser **idempotente**

### Estado

```
Placed / Confirmed → Canceled
```

---

# 5) Consultar pedido

```
GET /orders/{id}
```

Deve retornar:

* Pedido
* Itens
* DTO adequado

---

# 6) Listar pedidos

```
GET /orders
```

### Query parameters

```
customerId=
status=
from=
to=
page=
pageSize=
```

### Regras

* **Paginação obrigatória**
* Filtros:

  * customerId
  * status
  * intervalo de datas

---

# Requisitos não funcionais (MUST)

## Vertical Slices + SOLID

Separação:

* Domain
* Application
* Infrastructure

Porém a organização deve ser feita **por feature (Vertical Slices)**.

---

# Concorrência

A confirmação do pedido deve ser **segura sob concorrência** e **não permitir estoque negativo**.

Explique no README a estratégia adotada.

### Exemplos aceitos

**1 — Controle otimista**

* ConcurrencyToken / RowVersion
* Retornar **409 Conflict**

ou

**2 — Atualização atômica no banco**

Exemplo:

```
UPDATE stock
SET available = available - quantity
WHERE available >= quantity
```

Validando **linhas afetadas**.

---

# CancellationToken

Endpoints, handlers e repositórios devem aceitar:

```
CancellationToken
```

E propagar para operações async:

* EF Core
* I/O

---

# Testes (TDD friendly)

* Cobertura de:

  * regras de negócio
  * casos de borda
  * cenários importantes
* Rodar com:

```
dotnet test
```

---

# Segurança

Mínimo esperado:

* **Autenticação JWT**

---

# Performance

* Modelagem eficiente
* Queries eficientes

---

# Operacional

```
docker compose up
```

Deve subir:

* API
* Banco

### Migrations

Aplicadas automaticamente.

### README deve conter

* Como rodar
* Como testar
* Como usar

---

# Endpoints obrigatórios

```
POST /auth/token

POST /orders
POST /orders/{id}/confirm
POST /orders/{id}/cancel

GET /orders/{id}
GET /orders
```

---

# O que avaliamos

* Arquitetura
* Separação de responsabilidades
* DDD prático

  * invariantes
  * modelagem
  * value objects
* SOLID
* Clean Code
* Testes
* EF Core

  * modelagem
  * migrations
  * queries
  * performance
* Segurança
* Entrega

  * Docker
  * README
  * experiência de rodar

Desejável:

* Conhecimento em **Dapper**

---

# Checklist mínimo

* [ ] API roda local e via Docker
* [ ] Migrations aplicadas automaticamente
* [ ] Endpoints MUST implementados
* [ ] JWT + autorização básica funcionando
* [ ] `dotnet test` passando
* [ ] README com passo a passo

---

# Próximas etapas

Após a entrega:

* Avaliação do projeto
* Conversa técnica sobre:

  * decisões arquiteturais
  * desafios
  * aprendizados

---

Se quiser, também posso te entregar uma **versão melhorada desse Markdown para README profissional**, já com:

* **arquitetura sugerida**
* **estrutura de pastas Vertical Slice**
* **estratégia de concorrência**
* **exemplo de Docker Compose**
* **exemplo de JWT**

Isso te colocaria **muito acima da média nesse teste técnico**.
