# Gestor de Eventos CQES 23+1

Aplicação de consola para apoiar a gestão de eventos, inscrições e relatórios. O objetivo é permitir que um utilizador crie e consulte eventos, registe participantes, acompanhe inscrições e obtenha informação resumida sobre a ocupação dos eventos.

A aplicação é desenvolvida em C# e corre em **.NET 10**.

## O que a aplicação permite fazer

- Consultar um menu principal com acesso às áreas de Eventos, Inscrições e Relatórios.
- Criar, alterar, cancelar e listar eventos.
- Criar inscrições para eventos disponíveis.
- Validar dados introduzidos pelo utilizador, como campos obrigatórios e números positivos.
- Simular a criação de bilhetes em PDF para inscrições.
- Listar inscritos por evento.
- Consultar a ocupação dos eventos.
- Tratar o cancelamento de eventos, identificando inscrições afetadas.

## Como funciona

Ao iniciar a aplicação, é apresentado um menu principal na consola. A partir desse menu, o utilizador pode escolher a área onde pretende trabalhar:

- **Eventos**: gestão dos eventos disponíveis.
- **Inscrições**: registo de participantes nos eventos.
- **Relatórios**: consulta de informação sobre inscritos e ocupação.
- **Terminar**: encerra a aplicação.

Quando um evento é cancelado, a aplicação aciona o processo associado ao cancelamento e identifica as inscrições relacionadas com esse evento.

## Estado atual

A aplicação já tem os principais menus e fluxos encaminhados, mas ainda não está ligada a uma base de dados real.

Neste momento, alguns dados são simulados no próprio código para demonstrar o funcionamento dos menus, das listagens e dos relatórios. Também a geração de PDFs e o envio de notificações estão preparados como intenção de funcionamento, mas ainda não geram documentos reais nem enviam mensagens reais.

Funcionalidades ainda previstas:

- guardar e consultar eventos numa base de dados SQLite;
- guardar e consultar inscrições numa base de dados SQLite;
- aplicar validações completas às regras de negócio;
- gerar PDFs reais para bilhetes e comprovativos;
- enviar notificações reais aos participantes;
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
  "PastaPdfs": "Pdfs"
}
```

Estes valores indicam:

- o nome e localização previstos para a base de dados;
- a pasta onde serão guardados os PDFs gerados.

## Organização dos ficheiros

O projeto está organizado por áreas:

- `Aplicacao/` - arranque da aplicação e menu principal.
- `Eventos/` - gestão de eventos e cancelamentos.
- `Inscricoes/` - gestão de inscrições e bilhetes.
- `Relatorios/` - consulta e apresentação de relatórios.
- `Partilhado/` - modelos e configuração comuns às várias áreas.

Ficheiros principais:

- `Program.cs` - ponto de entrada da aplicação.
- `GestorEventos.csproj` - ficheiro do projeto .NET.
- `appsettings.json` - ficheiro de configuração.
- `CQES-23-1.sln` - solução que pode ser aberta no Visual Studio ou noutra IDE compatível.
