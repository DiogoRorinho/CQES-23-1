/*
ALTERACOES REALIZADAS NOS MODULOS DE RELATORIOS E MODELOS

1. A classe Inscricao passou a incluir os campos NomeParticipante, IdadeParticipante e Quantidade.
   Motivo: O modelo Inscricao nao refletia a estrutura real da tabela "inscricoes" na base de dados, o que causava erros ao tentar usar o campo Quantidade nos relatorios.
   Solucao: Foi alinhada a classe Inscricao com os dados existentes em SQLite, mantendo Quantidade na entidade usada pelos relatorios.

2. O RelatorioModel passou a importar o namespace GestorEventos.Dados.
   Motivo: O codigo usava BaseDados.CriarLigacaoAberta(), mas o compilador nao encontrava a classe BaseDados no contexto atual.
   Solucao: Foi adicionado o using GestorEventos.Dados para permitir o acesso direto a BaseDados.

3. O RelatorioModel passou a ler os campos nome_participante, idade_participante e quantidade na query das inscricoes.
   Motivo: O relatorio estava a ler apenas parte da informacao da inscricao, ignorando dados que ja existiam na base de dados e que eram necessarios para os calculos e para o detalhe apresentado.
   Solucao: A query SQL e o mapeamento para a classe Inscricao foram atualizados para incluir todos os campos relevantes.

4. O total de inscritos no relatorio por evento deixou de usar apenas inscricoes.Count e passou a somar Quantidade.
   Motivo: Uma inscricao pode representar varias vagas/lugares, pelo que contar apenas o numero de registos produzia valores incorretos.
   Solucao: Foi criado um metodo para somar a Quantidade de todas as inscricoes do evento.

5. O relatorio de ocupacao passou a somar Quantidade apenas nas inscricoes com estado "ativa".
   Motivo: A ocupacao estava a ser calculada pelo numero de registos, em vez do numero real de vagas ocupadas.
   Solucao: O calculo passou a acumular inscricao.Quantidade para refletir corretamente a ocupacao do evento.

6. O relatorio de ocupacao passou a gerar efetivamente o ficheiro PDF antes de o apresentar na view.
   Motivo: O sistema mostrava o nome e caminho do PDF, mas o metodo nao chamava a rotina de gravacao do ficheiro, deixando a pasta vazia.
   Solucao: Foi gerado o conteudo do relatorio, criado o DocumentoPdf e chamada a funcao GerarFicheiroPdf antes de devolver o resultado.

7. Foi ativado o suporte a fontes do Windows no arranque da aplicacao.
   Motivo: O PDFsharp 6.2.4 em build Core nao resolve "Arial" automaticamente, gerando a excecao "No appropriate font found for family name 'Arial'".
   Solucao: Foi definido GlobalFontSettings.UseWindowsFontsUnderWindows = true no arranque da aplicacao para permitir o uso de fontes Windows conhecidas.

8. Resultado final:
   O projeto voltou a compilar e os dois relatorios passaram a gerar PDFs corretamente.
   Motivo: Era necessario garantir consistencia entre modelos, base de dados, calculos de relatorio e geracao de PDF.
   Solucao: Foram corrigidos os modelos, queries, calculos, geracao de ficheiros PDF e configuracao de fontes.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GestorEventos.Dados;
using GestorEventos.Partilhado;
using Microsoft.Data.Sqlite;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GestorEventos.Relatorios {
    class RelatorioModel {
        private readonly string connectionString;
        private readonly string pastaPdfs;
        private DocumentoPdf? ultimoRelatorioGerado;

        public RelatorioModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ObterListaEventos() {
            List<Evento> eventos = new List<Evento>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = new SqliteCommand(
                @"SELECT id, nome, local, data, estado, capacidade
                  FROM eventos
                  ORDER BY data, nome;",
                ligacao);
            using SqliteDataReader leitor = comando.ExecuteReader();

            while (leitor.Read()) {
                eventos.Add(LerEvento(leitor));
            }

            return eventos;
        }

        public DadosRelatorio ListarInscritosPorEvento(int idEvento) {
            return ObterDadosRelatorioEGerarPdf(idEvento);
        }

        public bool EventoExiste(int idEvento) {
            return idEvento > 0 && ObterEventoPorId(idEvento) != null;
        }

        public DadosRelatorio ObterDadosRelatorioEGerarPdf(int idEvento) {
            const string titulo = "Listagem de inscritos por evento";
            Evento? evento = ObterEventoPorId(idEvento);
            string conteudo;

            if (evento == null) {
                ultimoRelatorioGerado = null;
                conteudo = "Evento nao encontrado.";
            }
            else {
                List<Inscricao> inscricoes = ObterInscricoesPorEvento(idEvento);
                conteudo = ConstruirConteudoInscritos(evento, inscricoes);

                ultimoRelatorioGerado = CriarDocumentoPdf(
                    titulo,
                    "relatorio-inscritos-evento-" + idEvento + ".pdf");

                GerarFicheiroPdf(ultimoRelatorioGerado, conteudo);
            }

            return new DadosRelatorio {
                Titulo = titulo,
                Conteudo = conteudo
            };
        }

        public DadosRelatorio ListarEventosComOcupacao() {
            return ObterDadosRelatorioOcupacaoEGerarPdf();
        }

        public DadosRelatorio ObterDadosRelatorioOcupacaoEGerarPdf() {
            string conteudo = ConstruirConteudoOcupacao();
            ultimoRelatorioGerado = CriarDocumentoPdf("Eventos com ocupacao", "relatorio-ocupacao.pdf");
            GerarFicheiroPdf(ultimoRelatorioGerado, conteudo);

            return new DadosRelatorio {
                Titulo = "Eventos com ocupacao",
                Conteudo = conteudo
            };
        }

        public DocumentoPdf ObterUltimoRelatorioGerado() {
            return ultimoRelatorioGerado ?? CriarDocumentoPdf("Relatorio", "relatorio.pdf");
        }

        public string ObterConnectionString() {
            return connectionString;
        }

        public string ObterPastaPdfs() {
            return pastaPdfs;
        }

        private DocumentoPdf CriarDocumentoPdf(string titulo, string nomeFicheiro) {
            return new DocumentoPdf {
                Titulo = titulo,
                NomeFicheiro = nomeFicheiro,
                CaminhoFicheiro = ConfiguracaoAplicacao.CombinarCaminhoPdf(nomeFicheiro)
            };
        }

        private Evento? ObterEventoPorId(int idEvento) {
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = new SqliteCommand(
                @"SELECT id, nome, local, data, estado, capacidade
                  FROM eventos
                  WHERE id = @idEvento;",
                ligacao);

            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read()) {
                return LerEvento(leitor);
            }

            return null;
        }

        private List<Inscricao> ObterInscricoesPorEvento(int idEvento) {
            List<Inscricao> inscricoes = new List<Inscricao>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = new SqliteCommand(
                @"SELECT id, id_evento, nome_participante, email_participante, idade_participante, quantidade, estado
                  FROM inscricoes
                  WHERE id_evento = @idEvento
                  ORDER BY id;",
                ligacao);

            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read()) {
                inscricoes.Add(new Inscricao {
                    Id = LerInteiro(leitor, "id"),
                    IdEvento = LerInteiro(leitor, "id_evento"),
                    NomeParticipante = LerTexto(leitor, "nome_participante"),
                    EmailParticipante = LerTexto(leitor, "email_participante"),
                    IdadeParticipante = LerInteiro(leitor, "idade_participante"),
                    Quantidade = LerInteiro(leitor, "quantidade"),
                    Estado = LerTexto(leitor, "estado")
                });
            }

            return inscricoes;
        }

        private string ConstruirConteudoInscritos(Evento? evento, List<Inscricao> inscricoes) {
            if (evento == null) {
                return "Evento nao encontrado.";
            }

            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
            conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
            conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            conteudo.AppendLine(string.Format("Total de lugares inscritos: {0}", SomarQuantidadeInscricoes(inscricoes)));

            if (inscricoes.Count == 0) {
                conteudo.AppendLine("Nao existem inscricoes registadas para este evento.");
                return conteudo.ToString();
            }

            conteudo.AppendLine("Inscritos:");

            foreach (Inscricao inscricao in inscricoes) {
                conteudo.AppendLine(string.Format(
                    "- #{0} | {1} | {2} | {3} | {4}",
                    inscricao.Id,
                    inscricao.NomeParticipante,
                    inscricao.EmailParticipante,
                    inscricao.Estado,
                    inscricao.Quantidade));
            }

            return conteudo.ToString();
        }

        private string ConstruirConteudoOcupacao() {
            StringBuilder conteudo = new StringBuilder();

            foreach (Evento evento in ObterListaEventos()) {
                int totalInscricoesAtivas = 0;

                foreach (Inscricao inscricao in ObterInscricoesPorEvento(evento.Id)) {
                    if (inscricao.Estado == "ativa") {
                        totalInscricoesAtivas += inscricao.Quantidade;
                    }
                }

                decimal percentagem = evento.Capacidade == 0
                    ? 0
                    : (decimal)totalInscricoesAtivas / evento.Capacidade * 100;

                conteudo.AppendLine(string.Format(
                    "{0}: {1}/{2} vagas ocupadas ({3:0.##}%)",
                    evento.Nome,
                    totalInscricoesAtivas,
                    evento.Capacidade,
                    percentagem));
            }

            return conteudo.ToString();
        }

        private int SomarQuantidadeInscricoes(List<Inscricao> inscricoes) {
            int total = 0;

            foreach (Inscricao inscricao in inscricoes) {
                total += inscricao.Quantidade;
            }

            return total;
        }

        private SqliteConnection CriarLigacao() {
            if (string.IsNullOrWhiteSpace(connectionString)) {
                throw new InvalidOperationException("Connection string da base de dados nao configurada.");
            }

            string connectionStringResolvida = ResolverDataDirectory(connectionString);

            return new SqliteConnection(connectionStringResolvida);
        }

        private string ResolverDataDirectory(string textoConnectionString) {
            string? dataDirectory = Convert.ToString(AppDomain.CurrentDomain.GetData("DataDirectory"));

            if (string.IsNullOrWhiteSpace(dataDirectory)) {
                dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            return textoConnectionString.Replace("|DataDirectory|", dataDirectory);
        }

        private Evento LerEvento(SqliteDataReader leitor) {
            return new Evento {
                Id = LerInteiro(leitor, "id"),
                Nome = LerTexto(leitor, "nome"),
                Local = LerTexto(leitor, "local"),
                Data = LerData(leitor, "data"),
                Estado = LerTexto(leitor, "estado"),
                Capacidade = LerInteiro(leitor, "capacidade")
            };
        }

        private int LerInteiro(SqliteDataReader leitor, string coluna) {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal)) {
                return 0;
            }

            return Convert.ToInt32(leitor.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private string LerTexto(SqliteDataReader leitor, string coluna) {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal)) {
                return string.Empty;
            }

            return Convert.ToString(leitor.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private DateTime LerData(SqliteDataReader leitor, string coluna) {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal)) {
                return DateTime.MinValue;
            }

            object valor = leitor.GetValue(ordinal);

            if (valor is DateTime dataDireta) {
                return dataDireta;
            }

            string texto = Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;

            if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data) ||
                DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.None, out data)) {
                return data;
            }

            return DateTime.MinValue;
        }

        private void GerarFicheiroPdf(DocumentoPdf documentoPdf, string conteudo) {
            string? pastaDestino = Path.GetDirectoryName(documentoPdf.CaminhoFicheiro);

            if (!string.IsNullOrWhiteSpace(pastaDestino)) {
                Directory.CreateDirectory(pastaDestino);
            }

            using PdfDocument documento = new PdfDocument();
            documento.Info.Title = documentoPdf.Titulo;

            PdfPage pagina = documento.AddPage();
            pagina.Size = PageSize.A4;

            XGraphics grafico = XGraphics.FromPdfPage(pagina);
            XFont fonteTitulo = new XFont("Arial", 16, XFontStyleEx.Bold);
            XFont fonteCorpo = new XFont("Arial", 10, XFontStyleEx.Regular);

            const double margem = 40;
            const double alturaLinha = 14;
            double y = margem;

            grafico.DrawString(
                documentoPdf.Titulo,
                fonteTitulo,
                XBrushes.Black,
                new XRect(margem, y, pagina.Width.Point - margem * 2, 24),
                XStringFormats.TopLeft);

            y += 34;

            foreach (string linha in SepararLinhasPdf(conteudo, grafico, fonteCorpo, pagina.Width.Point - margem * 2)) {
                if (y + alturaLinha > pagina.Height.Point - margem) {
                    grafico.Dispose();

                    pagina = documento.AddPage();
                    pagina.Size = PageSize.A4;

                    grafico = XGraphics.FromPdfPage(pagina);
                    y = margem;
                }

                grafico.DrawString(
                    linha,
                    fonteCorpo,
                    XBrushes.Black,
                    new XRect(margem, y, pagina.Width.Point - margem * 2, alturaLinha),
                    XStringFormats.TopLeft);

                y += alturaLinha;
            }

            grafico.Dispose();
            documento.Save(documentoPdf.CaminhoFicheiro);
        }

        private List<string> SepararLinhasPdf(string texto, XGraphics grafico, XFont fonte, double larguraMaxima) {
            List<string> linhas = new List<string>();
            string textoNormalizado = (texto ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            foreach (string linhaOriginal in textoNormalizado.Split('\n')) {
                linhas.AddRange(QuebrarLinhaPdf(linhaOriginal, grafico, fonte, larguraMaxima));
            }

            return linhas;
        }

        private List<string> QuebrarLinhaPdf(string linhaOriginal, XGraphics grafico, XFont fonte, double larguraMaxima) {
            List<string> linhas = new List<string>();

            if (string.IsNullOrWhiteSpace(linhaOriginal)) {
                linhas.Add(string.Empty);
                return linhas;
            }

            string linhaAtual = string.Empty;

            foreach (string palavra in linhaOriginal.Split(' ')) {
                string candidata = string.IsNullOrEmpty(linhaAtual)
                    ? palavra
                    : linhaAtual + " " + palavra;

                if (grafico.MeasureString(candidata, fonte).Width <= larguraMaxima) {
                    linhaAtual = candidata;
                    continue;
                }

                if (!string.IsNullOrEmpty(linhaAtual)) {
                    linhas.Add(linhaAtual);
                }

                linhaAtual = palavra;
            }

            if (!string.IsNullOrEmpty(linhaAtual)) {
                linhas.Add(linhaAtual);
            }

            return linhas;
        }
    }
}
