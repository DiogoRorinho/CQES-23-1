# Gestor de Eventos CQES 23+1

Aplicação de consola para apoiar a gestão de eventos, inscrições e relatórios. O objetivo é permitir que um utilizador crie e consulte eventos, registe participantes, acompanhe inscrições e obtenha informação resumida sobre a ocupação dos eventos.

A aplicação é desenvolvida em C# e corre em **.NET 10**.

## O que a aplicação permite fazer

- Consultar um menu principal com acesso às áreas de Eventos, Inscrições e Relatórios.
- Criar, alterar, cancelar e listar eventos.
- Criar inscrições para eventos disponíveis.
- Validar dados introduzidos pelo utilizador, como campos obrigatórios e números positivos.
- Gerar bilhetes e comprovativos em PDF para inscrições.
- Listar inscritos por evento.
- Consultar a ocupação dos eventos.
- Tratar o cancelamento de eventos, identificando inscrições afetadas.
- Atualizar automaticamente estados de eventos e inscrições quando a data do evento passa.

## Como funciona

Ao iniciar a aplicação, é apresentado um menu principal na consola. A partir desse menu, o utilizador pode escolher a área onde pretende trabalhar:

- **Eventos**: gestão dos eventos disponíveis.
- **Inscrições**: registo de participantes nos eventos.
- **Relatórios**: consulta de informação sobre inscritos e ocupação.
- **Terminar**: encerra a aplicação.

Quando um evento é cancelado, a aplicação aciona o processo associado ao cancelamento e identifica as inscrições relacionadas com esse evento.

## Estado atual

A aplicação já tem os principais menus e fluxos encaminhados e inclui persistência em SQLite.

A base de dados é criada automaticamente no arranque da aplicação, usando os scripts SQL existentes na pasta `Dados/Sql/`. Quando a configuração `SeedDemoData` está ativa no ficheiro `appsettings.json`, a aplicação também insere dados de demonstração se a base de dados estiver vazia.

A geração de PDFs está implementada para os fluxos de inscrições e relatórios.

Quando ocorre cancelamento de evento, a aplicação regista notificações em ficheiros de texto na pasta `Notificacoes/` junto ao executável, incluindo referência ao comprovativo PDF de cancelamento.

Funcionalidades ainda previstas:

- aplicar validações completas às regras de negócio;
- concluir os fluxos de alteração e cancelamento de inscrições.

## Requisitos

Para compilar e executar a aplicação é necessário ter instalado:

- **.NET 10 SDK**;
- um editor de código ou IDE, como Visual Studio, Visual Studio Code ou JetBrains Rider.

## Como executar

Na pasta principal do projeto, executar:

```bash
dotnet restore
dotnet build
dotnet run
```

Também é possível abrir a solução `CQES-23-1.sln` ou o projeto `GestorEventos.csproj` num editor compatível e executar a aplicação a partir daí.

## Configuração

O ficheiro `appsettings.json` guarda valores de configuração usados pela aplicação:

```json
{
  "ConnectionStrings": {
    "GestorEventosDb": "Data Source=gestoreventos.db"
  },
  "PastaPdfs": "Pdfs",
  "SeedDemoData": true
}
```

Estes valores indicam:

- o nome e localização previstos para a base de dados;
- a pasta onde serão guardados os PDFs gerados.
- se devem ser inseridos dados de demonstração quando a base de dados está vazia.

## Base de dados SQLite

A estrutura da base de dados está separada do código C#:

- `Dados/Sql/schema.sql` - cria as tabelas, relações, constraints e índices.
- `Dados/Sql/seed-demo.sql` - insere dados de demonstração para a primeira execução.

O schema inclui:

- `eventos` - dados principais dos eventos, estado, capacidade e timestamps.
- `inscricoes` - inscrições associadas a eventos através de foreign key.

A aplicação usa soft-delete através do campo `estado`, por exemplo `ativo`, `cancelado`, `terminado`, `ativa`, `cancelada`, `cancelada_por_evento` e `terminada`. As operações principais usam SQLite através dos Models, mantendo a separação MVC.

## Organização dos ficheiros

O projeto está organizado por áreas:

- `Aplicacao/` - arranque da aplicação e menu principal.
- `Eventos/` - gestão de eventos e cancelamentos.
- `Inscricoes/` - gestão de inscrições e bilhetes.
- `Relatorios/` - consulta e apresentação de relatórios.
- `Dados/` - inicialização da base de dados e scripts SQL.
- `Partilhado/` - modelos e configuração comuns às várias áreas.

Ficheiros principais:

- `Program.cs` - ponto de entrada da aplicação.
- `GestorEventos.csproj` - ficheiro do projeto .NET.
- `appsettings.json` - ficheiro de configuração.
- `CQES-23-1.sln` - solução que pode ser aberta no Visual Studio ou noutra IDE compatível.
