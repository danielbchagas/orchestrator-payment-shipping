# Exemplo de máquina de estados

## Requisitos

- .NET 8.0
- ASP.NET Core
- C# 12.0

## Estrutura do Projeto

```bash
Samples.Orchestrator/
│
├── Domain/
│   └── Events/
|       ├── Initial/
|       ├   ├── Initial.cs
│       ├── Payment/
│       │   ├── Accepted.cs
│       │   ├── Cancelled.cs
│       │   ├── DeadLetter.cs
│       │   └── Submitted.cs
│       │
│       └── Shipping/
│           ├── Accepted.cs
│           ├── Cancelled.cs
│           ├── DeadLetter.cs
│           └── Submitted.cs
│
├── Infrastructure/
│   ├── Database/
│   │   ├── OrderStateDbContext.cs
│   │   └── OrderStateMap.cs
│   │
│   ├── Extensions/
│   │   └── MasstransitExtensions.cs
│   │
│   ├── Migrations/
│   │   ├── 20241026125247_Initial.cs
│   │   ├── 20241026125247_Initial.Designer.cs
│   │   └── OrderStateDbContextModelSnapshot.cs
│   │
│   └── StateMachine/
│       ├── OrderState.cs
│       └── OrderStateMachine.cs
│
├── appsettings.json
├── appsettings.Development.json
├── http-client.env.json
├── Program.cs
├── Samples.Orchestrator.http
└── Properties/
