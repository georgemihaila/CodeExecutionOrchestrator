# CodeExecutionOrchestrator

A remote code execution / distributed action orchestration system built around a **Server ⇄ Agent ⇄ Client** architecture in .NET Core. A central **Server** tracks a fleet of lightweight **Agent** web services that register themselves on the network; any **client process** can embed an agent (via the `CodeExecutionOrchestrator` library) and expose local C# delegates (methods/functions) as remotely invocable "actions". The server can then invoke those actions on all reachable agents over HTTP — with or without input parameters and with or without a return value — and collects results through an asynchronous callback mechanism.

The project demonstrates a small-scale "orchestrator" pattern: agents self-register with the server, the server health-checks them every 3 seconds, and it fans out four kinds of invocations to every reachable agent in parallel: **NINO** (no input, no output), **SINO** (some input, no output), **NISO** (no input, some output) and **SISO** (some input, some output). Results from the output-producing actions are stored server-side under callback IDs and can be retrieved by clients.

## Features / What it can do

- **Agent registration** — agents POST to the server's `register?port=...` endpoint; the server records their IP (from the connection) and port, assigns a random name (`AGENT_xxxxx`) and environment (`ENV_0`/`ENV_1`), and returns an identity.
- **Agent health monitoring** — a hosted `BackgroundService` pings every agent's `/api/up` endpoint every 3 seconds, marks unreachable agents, and after 3 consecutive failures removes them from the registry.
- **Remote action dispatch (4 flavors)** — the server exposes four POST endpoints that fan a named action out to every reachable agent in parallel:
  - `NoInputNoOutputTest` (`/api/NINO`)
  - `SomeInputNoOutputTest` (`/api/SINO`) — accepts an `InstanceDescriptor[]` body
  - `NoInputSomeOutputTest` (`/api/NISO`) — returns callback IDs (HTTP 201)
  - `SomeInputSomeOutputTest` (`/api/SISO`) — accepts parameters, returns callback IDs
- **Asynchronous callback / result retrieval** — agents POST results back to the server's `InvokeCallback?id=...` endpoint; results are stored under callback IDs and can be polled via `Callbacks`, `Callback?id=...`, `PendingCallbacks`.
- **Embeddable agent library** — the `CodeExecutionOrchestrator` client library (`RemoteAgent`) lets a host process register `Action`/`Func` delegates against action names (matched case-insensitively after trimming) and then start an in-process ASP.NET agent on a chosen (or OS-assigned) port. It supports:
  - `On("name", Action)`, `On<T>(...)`, `On<T1,T2>(...)` — no-return handlers
  - `On<TRet>(...)`, `On<T1,TRet>(...)`, `On<T1,T2,TRet>(...)` — returning handlers (results are always returned to the server so it doesn't hang)
  - Async delegates (`Task`/`Task<T>` returning) are awaited via dynamic invocation
- **SSDP-style identity endpoint** — agents expose `/api/identity` and `/api/up` (`"client up"`); the server health check expects exactly `"client up"`.
- **Reference test program** — a demo console app registering example handlers (`NINOTest`, `SINOTest`, `SISOTest`, `NISOTest`, `LongNISOTest`, multi-parameter variants) and running an embedded agent.

## Project structure

```
CodeExecutionOrchestrator.sln                  - Solution (VS 2019 / format 12.00)
├── CodeExecutionOrchestrator.Core/            - Shared model & helper library
│   ├── AgentInfo.cs                           - Name / Environment / Address of an agent
│   ├── InstanceDescriptor.cs                  - (Type, Instance) pair used for typed parameters/results
│   ├── HttpHelper.cs                          - HTTP GET/POST helpers (Newtonsoft.Json serialization)
│   ├── Actions/
│   │   ├── IRemoteAction.cs                   - Marker interface IRemoteAction<T> (Name)
│   │   ├── RemoteActionInvocationArgs.cs      - EventArgs carrying action name + InstanceDescriptor[] params
│   │   └── TestAction.cs                      - Example action ("test")
├── CodeExecutionOrchestrator.Agent/           - The agent host (ASP.NET Core Web API)
│   ├── Program.cs                             - Two Main overloads; one takes a port and binds http://*:port
│   ├── Startup.cs                             - Reads ServerAddress, self-registers with the server
│   ├── Controllers/APIController.cs           - /api/NINO, /api/SINO, /api/NISO, /api/SISO, /api/Identity, /api/Up
│   └── Infrastructure/
│       ├── APIControllerIOC.cs                - Static event "bus" for NINO/SINO/NISO/SISO invoked + callbacks
│       ├── ActionCallbackHandler.cs           - Assigns callback IDs and POSTs results back to the server
│       └── AgentConfiguration.cs              - Static AgentInfo / ServerAddress / Port holder
├── CodeExecutionOrchestrator.Server/          - The orchestrator (ASP.NET Core Web API)
│   ├── Program.cs / Startup.cs                - Standard generic-host bootstrap
│   ├── Controllers/APIController.cs           - /api/Up, dispatch endpoints, /api/Register, /api/Agents, callback endpoints
│   └── Infrastructure/
│       ├── Agent.cs                           - Agent : AgentInfo + Status (Reachable/Unreachable), LastUpdate, health check
│       └── AgentMonitoringBackgroundTask.cs   - BackgroundService pinging agents every 3s, removing dead ones
├── CodeExecutionOrchestrator/                 - Embeddable client library
│   ├── RemoteAgent.cs                         - Register delegates + Run() the embedded agent
│   ├── RemoteAgentBuilder.cs                  - FromConfiguration(...) factory
│   └── RemoteAgentConfiguration.cs            - Port (explicit or OS-assigned via ephemeral TcpListener)
└── CodeExecutionOrchestrator.Test/            - Console demo (registers sample actions, runs an agent)
```

## Tech stack

| Area | Choice |
|---|---|
| Language / runtime | C# on **.NET Core 3.1** (`netcoreapp3.1`) |
| Web framework | ASP.NET Core 3.1 (SDK `Microsoft.NET.Sdk.Web` / `Microsoft.NET.Sdk`) |
| JSON | Newtonsoft.Json **12.0.3**, `Microsoft.AspNetCore.Mvc.NewtonsoftJson` **3.1.0** |
| Persistence (server project only) | Entity Framework **6.4.4** (classic), `Microsoft.EntityFrameworkCore` / SqlServer **5.0.0-preview.6.20312.4** (declared but not wired into any DbContext in the code) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` **3.1.6** |
| Solution | Visual Studio 2019 solution file (`VisualStudioVersion 16.0`), Debug/Release, AnyCPU/x86 |

## Build & run

Requires the .NET Core 3.1 SDK. From the solution root:

```bash
# Restore and build the whole solution
dotnet build CodeExecutionOrchestrator.sln

# Run the server (orchestrator)
dotnet run --project CodeExecutionOrchestrator.Server

# Run an agent standalone (registers itself with the server on startup)
dotnet run --project CodeExecutionOrchestrator.Agent

# Run the client demo (embeds an agent in-process and waits for key input)
dotnet run --project CodeExecutionOrchestrator.Test
```

Notes on building:

- The project targets `netcoreapp3.1`; the solution also defines x86 platform entries (the Test project declares `AnyCPU;x86`).
- A `PostBuild` MSBuild target copies each project's output directory to `<SolutionDir>\output` using the Windows `copy /Y` command — on Linux/macOS this post-build step will not work as written (the build itself is fine).

## Usage notes / configuration

- **Server address** is configured via `appsettings.json`:
  - Agent: `ServerAddress` = `http://localhost:60276/api/` (a commented-out alternative shows `http://10.2.0.7:60277/api/`).
  - The server's own `appsettings.json` carries a `ServerAddress` entry pointing at `http://10.2.0.7:60277/api/` (currently not consumed by server code).
- **Flow:** start the Server first → start one or more Agents (or a client that embeds a `RemoteAgent`) → agents self-register → hit the server's dispatch endpoints to trigger actions on all reachable agents.
- **Callback semantics:** `NISO`/`SISO` return a callback `id` immediately; the agent asynchronously executes the action and POSTs the result to `InvokeCallback`. Poll `Callbacks` / `Callback?id=...` / `PendingCallbacks` to collect results. The `RemoteAgent` code deliberately always sends a result back "so the server doesn't hang".
- **Health check:** the server calls `GET <agent>/api/up` and accepts only the literal response `client up`.
- **HTTP security:** `HttpHelper` disables server-certificate validation in `DEBUG` builds (`ServerCertificateValidationCallback` always returns `true`).

## License

**No LICENSE file is present in this repository.** License status could not be determined from the repository contents.
