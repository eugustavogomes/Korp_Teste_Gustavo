# Sistema de Emissão de Notas Fiscais

## Tecnologias Utilizadas

### Backend

- ASP.NET Core 8.0 Web API
- Entity Framework Core
- PostgresSQL
- Polly (resiliência)
- FluentValidation
- Serilog

### Frontend

- Angular 17
- RxJS
- Angular Material
- TypeScript

## Como Executar

### Com Docker

bash
docker-compose up


### Sem Docker

[Instruções detalhadas]

## Arquitetura

[Diagrama da arquitetura de microsserviços]

## Funcionalidades Implementadas

- ✅ Cadastro de Produtos
- ✅ Cadastro de Notas Fiscais
- ✅ Impressão com baixa de estoque
- ✅ Tratamento de falhas
- ✅ Resiliência com retry e circuit breaker
- ✅ [Opcional] Tratamento de concorrência
- ✅ [Opcional] Idempotência

## Detalhamento Técnico

### Ciclos de Vida Angular

- OnInit, OnDestroy, OnChanges

### RxJS

- BehaviorSubject para cache de produtos
- Operators: map, tap, catchError, retry, finalize

### LINQ (C#)

- Queries: Where, Select, Include
- Async: FirstOrDefaultAsync, ToListAsync

### Tratamento de Erros

- Custom exceptions
- Global error handler
- HTTP status codes apropriados
- Logging estruturado com Serilog

