# Gestor de Eventos CQES 23+1

Aplicação de consola para apoiar a gestão de eventos, inscrições e relatórios. Permite criar e acompanhar eventos, gerir inscrições de participantes, gerar documentos em PDF e consultar informação resumida sobre inscritos e ocupação.

A aplicação é desenvolvida em C# e corre em **.NET 10**.

## Estado atual

A aplicação encontra-se funcional nos três módulos principais:

- **Eventos**: criação, alteração, cancelamento e listagem de eventos.
- **Inscrições**: criação, alteração, cancelamento e listagem de inscrições.
- **Relatórios**: listagem de inscritos por evento e relatório de ocupação dos eventos.

Os dados são persistidos em SQLite. A base de dados é inicializada automaticamente no arranque a partir dos scripts em `Dados/Sql/`, e os dados de demonstração são inseridos quando `SeedDemoData` está ativo e a base de dados está vazia.

A aplicação também atualiza automaticamente estados dependentes da data atual:

- eventos ativos com data passada passam para `terminado`;
- inscrições ativas associadas a eventos terminados passam para `terminada`.

As transições de domínio são registadas em ficheiro, e os estados não ativos são destacados a vermelho nas listagens de consola e nos relatórios PDF.

## Funcionalidades

- Menu principal com acesso a Eventos, Inscrições e Relatórios.
- Validação de campos obrigatórios, datas futuras, números positivos e limites de capacidade.
- Impedimento de inscrições acima da disponibilidade do evento.
- Impedimento de redução da capacidade de um evento abaixo das inscrições ativas existentes.
- Geração de bilhetes PDF na criação e alteração de inscrições.
- Geração de comprovativos PDF no cancelamento de inscrições.
- Geração de relatórios PDF para inscritos por evento e ocupação.
- Cancelamento de eventos com tratamento das inscrições ativas afetadas.
- Registo de notificações de cancelamento de evento em ficheiros de texto.
- Registo de acontecimentos de domínio em log.

## Como funciona

Ao iniciar a aplicação, é apresentado um menu principal na consola:

- **Eventos**: gere eventos ativos, cancelados e terminados.
- **Inscrições**: gere inscrições e bilhetes dos participantes.
- **Relatórios**: apresenta relatórios em consola e gera os respetivos PDFs.
- **Terminar**: encerra a aplicação.

Quando um evento é cancelado, as inscrições ativas associadas são marcadas como `cancelada_por_evento`, é gerado um comprovativo PDF de cancelamento e é criada uma notificação textual para cada participante afetado.

## Requisitos

Para compilar e executar a aplicação é necessário ter instalado:

- **.NET 10 SDK**;
- um editor de código ou IDE, como Visual Studio, Visual Studio Code ou JetBrains Rider.

## Como executar

Na pasta principal do projeto, executar:

```bash
dotnet restore
dotnet build GestorEventos.csproj
dotnet run --project GestorEventos.csproj
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

- a base de dados SQLite a usar;
- a pasta onde serão guardados os PDFs gerados;
- se devem ser inseridos dados de demonstração quando a base de dados está vazia.

Os caminhos relativos são resolvidos a partir da pasta do executável. Em execução local com `dotnet run`, os ficheiros gerados ficam normalmente em `bin/Debug/net10.0/`.

## Ficheiros gerados

Durante a execução, a aplicação pode criar:

- `gestoreventos.db` - base de dados SQLite;
- `Pdfs/` - bilhetes, comprovativos e relatórios PDF;
- `Notificacoes/eventos-dominio.log` - log de eventos de domínio;
- `Notificacoes/notificacao-cancelamento-evento-*.txt` - notificações de cancelamento de eventos.

Estes ficheiros são gerados junto ao executável e não são necessários para compilar o projeto.

## Base de dados SQLite

A estrutura da base de dados está separada do código C#:

- `Dados/Sql/schema.sql` - cria as tabelas, relações, constraints e índices.
- `Dados/Sql/seed-demo.sql` - insere dados de demonstração para a primeira execução.

O schema inclui:

- `eventos` - dados principais dos eventos, estado, capacidade e timestamps.
- `inscricoes` - inscrições associadas a eventos através de foreign key.

A aplicação usa soft-delete e transições por estado, incluindo:

- eventos: `ativo`, `cancelado`, `terminado`;
- inscrições: `ativa`, `cancelada`, `cancelada_por_evento`, `terminada`.

Na inicialização, existe ainda uma migração de compatibilidade para bases antigas que não tenham os estados `terminado` e `terminada` nas respetivas constraints.

## Organização dos ficheiros

O projeto está organizado por áreas:

- `Aplicacao/` - arranque da aplicação, menu principal e handlers de domínio.
- `Eventos/` - gestão de eventos, cancelamentos e notificações.
- `Inscricoes/` - gestão de inscrições, bilhetes e comprovativos.
- `Relatorios/` - consulta, apresentação e geração de relatórios.
- `Dados/` - inicialização da base de dados e scripts SQL.
- `Partilhado/` - modelos, configuração e serviços comuns.

Ficheiros principais:

- `Program.cs` - ponto de entrada da aplicação.
- `GestorEventos.csproj` - ficheiro do projeto .NET.
- `appsettings.json` - ficheiro de configuração.
- `CQES-23-1.sln` - solução que pode ser aberta no Visual Studio ou noutra IDE compatível.
