# OrderFlow Messaging

OrderFlow Messaging é uma biblioteca .NET para mensageria baseada em eventos, com foco em robustez, idempotência, retry, extensibilidade e infra desacoplada.

Ela foi projetada para funcionar como NuGet Package, permitindo que aplicações publiquem e consumam eventos usando RabbitMQ, sem acoplamento direto à infraestrutura.

## Principais Features

✅ Publish / Subscribe baseado em eventos

✅ RabbitMQ como broker

✅ Retry automático com Polly

✅ Idempotência de mensagens

✅ Dead Letter Queue (DLQ)

✅ Serialização desacoplada

✅ Integração com Microsoft.Extensions.DependencyInjection

✅ Totalmente testável

✅ Containers Docker independentes

✅ Pronto para uso como NuGet Package

## Arquitetura
A solução é dividida em camadas bem definidas:
```python
OrderFlow
│
├── Orderflow.Messaging.Abstractions
│   └── Contratos (IMessageBus, IMessageConsumer, etc)
│
├── OrderFlow.Messaging.Core
│   ├── Retry (Polly)
│   ├── Idempotência
│   ├── Serialização
│   └── Processamento genérico
│
├── OrderFlow.Messaging.RabbitMQ
│   ├── Implementação do bus
│   ├── Conexão RabbitMQ
│   ├── Exchange / Queue / DLQ
│   └── Integração com Core
│
├── OrderFlow.Contracts.Events
│   └── Eventos de domínio (ex: OrderCreatedEvent)
│
├── samples
│   ├── OrderFlow.Sample.Publisher
│   └── OrderFlow.Sample.Consumer
│
└── tests
    └── Integration Tests (RabbitMQ)
```

## Requisitos

- .NET 8+

- Docker

- Docker Compose

## 🐳 Subindo o ambiente com Docker

1- Arquivo .env

```python
RABBITMQ_DEFAULT_USER=guest
RABBITMQ_DEFAULT_PASS=guest
```

2- docker-compose.yml

```python

services:
  rabbitmq:
    image: rabbitmq:3.13-management
    container_name: orderflow-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_DEFAULT_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_DEFAULT_PASS}
    volumes:
      - ./docker/rabbitmq/enabled_plugins:/etc/rabbitmq/enabled_plugins
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  consumer:
    build:
      context: .
      dockerfile: OrderFlow.Sample.Consumer/Dockerfile
    environment:
      RabbitMq__Host: rabbitmq
      RabbitMq__Port: 5672
      RabbitMq__Username: ${RABBITMQ_DEFAULT_USER}
      RabbitMq__Password: ${RABBITMQ_DEFAULT_PASS}
    depends_on:
      rabbitmq:
        condition: service_healthy

  publisher:
    build:
      context: .
      dockerfile: OrderFlow.Sample.Publisher/Dockerfile
    environment:
      RabbitMq__Host: rabbitmq
      RabbitMq__Port: 5672
      RabbitMq__Username: ${RABBITMQ_DEFAULT_USER}
      RabbitMq__Password: ${RABBITMQ_DEFAULT_PASS}
    depends_on:
      rabbitmq:
        condition: service_healthy

volumes:
  sqlserver_data:

```

3- Subir tudo

```python
docker-compose up -d --build
```

## Publisher – Exemplo de uso
```python
services.AddRabbitMqMessaging(options =>
{
    options.Host = "rabbitmq";
    options.Port = 5672;
    options.Username = "guest";
    options.Password = "guest";

    options.ExchangeName = "orderflow.exchange";
    options.RoutingKey = "order.created";
});
```

## Publicando um evento:
```python
await bus.PublishAsync(new OrderCreatedEvent
{
    OrderId = Guid.NewGuid(),
    CreatedAt = DateTime.UtcNow
});
```
## Consumer – Exemplo de uso
```python
public class OrderCreatedConsumer 
    : IMessageConsumer<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent message)
    {
        Console.WriteLine($"Order received: {message.OrderId}");
        return Task.CompletedTask;
    }
}
```

## Registrando no Host
```python
services.AddRabbitMqMessaging(options =>
{
    options.Host = "rabbitmq";
    options.Queue = "orderflow.order-created.queue";
    options.RoutingKey = "order.created";
});

services.AddScoped<OrderCreatedConsumer>();

services.AddHostedService(provider =>
{
    var bus = provider.GetRequiredService<IMessageBus>();
    bus.Subscribe<OrderCreatedEvent, OrderCreatedConsumer>();
    return new BackgroundServiceWrapper();
});
```

## Retry com Polly


- Retry automático configurável

- Backoff simples por segundos

- Integrado ao pipeline de consumo
```python
options.RetryCount = 3;
options.RetryDelaySeconds = 2;
```

## Idempotência

A biblioteca evita o reprocessamento de mensagens duplicadas.

Implementação atual:
```python
IMessageProcessedStore

Implementação InMemoryMessageProcessedStore

services.AddSingleton<IMessageProcessedStore, InMemoryMessageProcessedStore>();
```

## Pode ser facilmente estendido para:
- SQL Server

- Redis

- MongoDB

## Dead Letter Queue (DLQ)
Mensagens que falham após todos os retries são enviadas automaticamente para:
```python
{Queue}.dlq
```
## Testes
Tipos de testes implementados:

✅ Smoke Test (publish + consume)

✅ Retry Test (falha proposital + retry)

✅ Testes de integração com RabbitMQ real

Executar testes
```python
dotnet test
```

## NuGet Packages

Os seguintes projetos são preparados para publicação:

- Orderflow.Messaging.Abstractions

- OrderFlow.Messaging.Core

- OrderFlow.Messaging.RabbitMQ

- OrderFlow.Contracts.Events

Gerar pacotes
```python
dotnet pack -c Release
```
## Filosofia do Projeto
❌ Sem acoplamento direto ao broker

✅ Infra como detalhe

✅ Mensageria orientada a eventos

✅ Foco em produção real

✅ Código extensível e testável

## Próximos Passos (Extensões possíveis)

- Persistência real de idempotência

- Observabilidade (metrics / tracing)

- Outbox Pattern

- Suporte a outros brokers

- Publicação oficial no NuGet.org

## Autor

Kauê Caobianco Fernandes
Desenvolvedor .NET
- Projeto criado como prova técnica e base para biblioteca reutilizável