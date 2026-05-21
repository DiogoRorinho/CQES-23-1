using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GestorEventos.Dados;
using GestorEventos.Partilhado;
using GestorEventos.Partilhado.Servicos;
using Microsoft.Data.Sqlite;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GestorEventos.Relatorios {
    class RelatorioModel {
        private readonly string connectionString;
        private readonly string pastaPdfs;
        private readonly IAtualizadorEstados atualizadorEstados;
        private DocumentoPdf? ultimoRelatorioGerado;

        public RelatorioModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
            atualizadorEstados = new AtualizadorEstadosService();
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ObterListaEventos() {
            AtualizarEstados();
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
            AtualizarEstados();
            return idEvento > 0 && ObterEventoPorId(idEvento) != null;
        }

        public DadosRelatorio ObterDadosRelatorioEGerarPdf(int idEvento) {
            AtualizarEstados();
            const string titulo = "Listagem de inscritos por evento";
            DateTime dataGeracao = DateTime.Now;
            Evento? evento = ObterEventoPorId(idEvento);
            string conteudo;

            if (evento == null) {
                ultimoRelatorioGerado = null;
                conteudo = "Evento nao encontrado.";
            }
            else {
                List<Inscricao> inscricoes = ObterInscricoesPorEvento(idEvento);
                conteudo = ConstruirConteudoInscritos(evento, inscricoes, dataGeracao);

                ultimoRelatorioGerado = CriarDocumentoPdf(
                    titulo,
                    string.Format(
                        "relatorio-inscritos-evento-{0}-{1:yyyyMMdd-HHmmss}.pdf",
                        idEvento,
                        dataGeracao));

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
            AtualizarEstados();
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

        private void AtualizarEstados() {
            atualizadorEstados.AtualizarEstados();
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

        private string ConstruirConteudoInscritos(Evento? evento, List<Inscricao> inscricoes, DateTime dataGeracao) {
            if (evento == null) {
                return "Evento nao encontrado.";
            }

            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine(string.Format("Gerado em {0:dd/MM/yyyy HH:mm}", dataGeracao));
            conteudo.AppendLine();
            conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
            conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
            conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            conteudo.AppendLine(string.Format("Estado: {0}", FormatarEstado(evento.Estado)));       //Acrescentado estado do evento
            conteudo.AppendLine(string.Format("Total de lugares inscritos: {0}", SomarQuantidadeInscricoes(inscricoes)));

            if (inscricoes.Count == 0) {
                conteudo.AppendLine("Nao existem inscricoes registadas para este evento.");
                return conteudo.ToString();
            }

            conteudo.AppendLine("Inscritos:");

            const string formatoTabela = "{0,-3} | {1,-18} | {2,-24} | {3,-20} | {4,3}";

            conteudo.AppendLine(string.Format(
                formatoTabela,
                "ID",
                "Nome participante",
                "Email participante",
                "Estado",
                "Qtd"));
            conteudo.AppendLine(new string('-', 80));

            foreach (Inscricao inscricao in inscricoes) {
                conteudo.AppendLine(string.Format(
                    formatoTabela,
                    inscricao.Id,
                    inscricao.NomeParticipante,
                    inscricao.EmailParticipante,
                    FormatarEstado(inscricao.Estado),
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

            PdfPage pagina = CriarPaginaPdf(documento);
            XGraphics grafico = XGraphics.FromPdfPage(pagina);
            XFont fonteTitulo = new XFont("Arial", 16, XFontStyleEx.Bold);
            XFont fonteCorpo = new XFont("Arial", 9, XFontStyleEx.Regular);

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

                    pagina = CriarPaginaPdf(documento);
                    grafico = XGraphics.FromPdfPage(pagina);
                    y = margem;
                }

                DesenharLinhaPdf(
                    grafico,
                    fonteCorpo,
                    linha,
                    margem,
                    y,
                    pagina.Width.Point - margem * 2,
                    alturaLinha);

                y += alturaLinha;
            }

            grafico.Dispose();
            documento.Save(documentoPdf.CaminhoFicheiro);
        }

        private PdfPage CriarPaginaPdf(PdfDocument documento) {
            PdfPage pagina = documento.AddPage();
            pagina.Size = PageSize.A4;
            pagina.Orientation = PageOrientation.Portrait;
            return pagina;
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

        private void DesenharLinhaPdf(XGraphics grafico, XFont fonte, string linha, double x, double y, double largura, double alturaLinha) {
            if (TentarObterSegmentosEstado(linha, out string prefixo, out string estado, out string sufixo, out bool estadoVermelho)) {
                XBrush pincelEstado = estadoVermelho ? XBrushes.Red : XBrushes.Black;

                grafico.DrawString(
                    prefixo,
                    fonte,
                    XBrushes.Black,
                    new XRect(x, y, largura, alturaLinha),
                    XStringFormats.TopLeft);

                double larguraPrefixo = grafico.MeasureString(prefixo, fonte).Width;
                grafico.DrawString(
                    estado,
                    fonte,
                    pincelEstado,
                    new XRect(x + larguraPrefixo, y, largura - larguraPrefixo, alturaLinha),
                    XStringFormats.TopLeft);

                double larguraEstado = grafico.MeasureString(estado, fonte).Width;
                grafico.DrawString(
                    sufixo,
                    fonte,
                    XBrushes.Black,
                    new XRect(x + larguraPrefixo + larguraEstado, y, largura - larguraPrefixo - larguraEstado, alturaLinha),
                    XStringFormats.TopLeft);

                return;
            }

            grafico.DrawString(
                linha,
                fonte,
                XBrushes.Black,
                new XRect(x, y, largura, alturaLinha),
                XStringFormats.TopLeft);
        }

        private bool TentarObterSegmentosEstado(string linha, out string prefixo, out string estado, out string sufixo, out bool estadoVermelho) {
            prefixo = string.Empty;
            estado = string.Empty;
            sufixo = string.Empty;
            estadoVermelho = false;

            if (linha.StartsWith("Estado: ", StringComparison.OrdinalIgnoreCase)) {
                prefixo = "Estado: ";
                estado = linha.Substring(prefixo.Length);
                sufixo = string.Empty;
                estadoVermelho = !EstadoAtivo(estado);
                return true;
            }

            if (!linha.Contains("|")) {
                return false;
            }

            string[] partes = linha.Split('|');
            if (partes.Length < 5) {
                return false;
            }

            string campoEstado = partes[3];
            string estadoLimpo = campoEstado.Trim();

            if (string.Equals(estadoLimpo, "Estado", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(estadoLimpo)) {
                return false;
            }

            int inicioCampoEstado = 0;
            for (int i = 0; i < 3; i++) {
                inicioCampoEstado += partes[i].Length + 1;
            }

            int espacosAntes = campoEstado.Length - campoEstado.TrimStart().Length;
            int inicioEstado = inicioCampoEstado + espacosAntes;

            prefixo = linha.Substring(0, inicioEstado);
            estado = estadoLimpo;
            sufixo = linha.Substring(inicioEstado + estadoLimpo.Length);
            estadoVermelho = !EstadoAtivo(estadoLimpo);

            return true;
        }

        private string FormatarEstado(string estado) {
            string estadoNormalizado = (estado ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(estadoNormalizado)) {
                return string.Empty;
            }

            estadoNormalizado = estadoNormalizado.Replace('_', ' ').ToLowerInvariant();
            return char.ToUpperInvariant(estadoNormalizado[0]) + estadoNormalizado.Substring(1);
        }

        private bool EstadoAtivo(string estado) {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            return estadoNormalizado == "ativo" || estadoNormalizado == "ativa";
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
