# Gestor de Eventos CQES 23+1

Aplicação de consola em C# para gestão de eventos, inscrições e relatórios, organizada num estilo MVC simples. O projecto está preparado para correr em **.NET Framework 4.8** e usa uma configuração baseada em `App.config` para a ligação a base de dados e para a pasta de geração de PDFs.

## Visão geral

O programa inicia pela classe `Program` e encaminha a execução para o `AplicacaoController`, que mostra o menu principal e distribui o fluxo para os módulos do sistema:

- **Eventos**: criação, alteração, cancelamento e listagem de eventos.
- **Inscricoes**: criação, alteração e cancelamento de inscrições.
- **Relatorios**: relatórios de inscritos por evento e eventos com ocupação.

O projecto está estruturado por responsabilidades:

- `Aplicacao/` - arranque da aplicação e menu principal.
- `Eventos/` - lógica, vistas e eventos de cancelamento.
- `Inscricoes/` - gestão de inscrições e bilhetes.
- `Relatorios/` - geração e apresentação de relatórios.
- `Partilhado/` - modelos e configuração comum.

## Estrutura do projecto

- `Program.cs` - ponto de entrada da aplicação.
- `Aplicacao/AplicacaoController.cs` - controla o menu principal e a navegação entre módulos.
- `Aplicacao/MenuPrincipalView.cs` - mensagens do menu principal.
- `Eventos/EventoController.cs` - fluxo do módulo de eventos.
- `Eventos/EventoModel.cs` - operações de eventos e disparo de cancelamento.
- `Eventos/EventoView.cs` - interface de consola do módulo de eventos.
- `Eventos/EventoCanceladoEventArgs.cs` - dados do evento cancelado.
- `Eventos/NotificacaoAnulacaoHandler.cs` - trata o cancelamento e a notificação das inscrições afetadas.
- `Inscricoes/InscricaoController.cs` - fluxo do módulo de inscrições.
- `Inscricoes/InscricaoModel.cs` - operações de inscrições, bilhetes e cancelamentos associados a eventos.
- `Inscricoes/InscricaoView.cs` - interface de consola do módulo de inscrições.
- `Relatorios/RelatorioController.cs` - fluxo do módulo de relatórios.
- `Relatorios/RelatorioModel.cs` - dados e geração de relatórios.
- `Relatorios/RelatorioView.cs` - interface de consola do módulo de relatórios.
- `Partilhado/ConfiguracaoAplicacao.cs` - leitura de configuração e caminhos de ficheiros.
- `Partilhado/Modelos.cs` - modelos partilhados do domínio.

## Funcionalidades identificadas

Com base no código atual, o projecto suporta:

- menu principal para navegar entre módulos;
- gestão de eventos com criação, edição e cancelamento;
- gestão de inscrições com validação de disponibilidade;
- geração de referência para bilhetes em PDF;
- relatórios de inscritos por evento e de ocupação;
- tratamento do cancelamento de eventos com propagação para inscrições associadas.


## Requisitos

- **Visual Studio 2019 ou superior ** com suporte para **.NET Framework 4.8**.
- **.NET Framework 4.8 Developer Pack** instalado.
- Acesso a um provider SQLite compatível, caso completes a implementação de persistência real.

## Como compilar no Visual Studio

1. Abre o **Visual Studio**.
2. Escolhe **File > Open > Project/Solution**.
3. Seleciona o ficheiro `GestorEventos.csproj`.
4. Se o Visual Studio pedir para instalar componentes do .NET Framework 4.8, aceita a instalação.
5. Aguarda o carregamento do projecto.
6. No menu superior, escolhe a configuração desejada:
   - **Debug** para desenvolvimento;
   - **Release** para uma compilação final.
7. Compila com **Build > Build Solution**.
8. Executa com **F5** ou **Ctrl+F5**.

Se quiseres abrir o executável gerado, o output por omissão fica em:

- `bin\Debug\`
- `bin\Release\`

## Configuração

O ficheiro `App.config` define:

- a connection string `GestorEventosDb`;
- a pasta `Pdfs` para ficheiros gerados;
- o runtime suportado: `.NETFramework,Version=v4.8`.

### Valores relevantes

- Base de dados: `Data Source=|DataDirectory|\gestoreventos.db;Version=3;`
- Pasta de PDFs: `Pdfs`

## Observações sobre o estado atual

- A aplicação está organizada como um esqueleto funcional, com várias operações ainda em implementação.
- Os modelos e vistas usam `Console.WriteLine`, por isso a aplicação é de consola e não tem interface gráfica.

## Fluxo de execução

1. `Program.Main` cria o `AplicacaoController`.
2. O controlador inicial mostra as mensagens de boas-vindas e o menu principal.
3. O utilizador escolhe entre Eventos, Inscricoes, Relatorios ou Terminar.
4. Cada módulo apresenta o seu menu e delega operações ao respetivo model/view.