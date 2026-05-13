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

namespace GestorEventos.Inscricoes
{
    class InscricaoModel
    {
        private readonly string connectionString;
        private readonly string pastaPdfs;

        public InscricaoModel()
        {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
        }

        public List<Evento> ListarEventosDisponiveis()
        {
            return ObterEventosComDisponibilidade();
        }

        public List<Evento> ObterEventosComDisponibilidade()
        {
            List<Evento> eventos = new List<Evento>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT e.id, e.nome, e.local, e.data, e.estado, e.capacidade
                FROM eventos e
                LEFT JOIN inscricoes i
                    ON i.id_evento = e.id
                   AND i.estado = 'ativa'
                WHERE e.estado = 'ativo'
                GROUP BY e.id, e.nome, e.local, e.data, e.estado, e.capacidade
                HAVING e.capacidade > COALESCE(SUM(i.quantidade), 0)
                ORDER BY e.data, e.id;";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                eventos.Add(MapearEvento(leitor));
            }

            return eventos;
        }

        public bool VerificarDisponibilidade(int idEvento, int quantidade)
        {
            return ValidarDisponibilidade(idEvento, quantidade);
        }

        public bool ValidarDisponibilidade(int idEvento, int quantidade)
        {
            if (idEvento <= 0 || quantidade <= 0)
            {
                return false;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            Evento? evento = ObterEvento(ligacao, null, idEvento);

            if (evento == null || !string.Equals(evento.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, null, idEvento, null);
            return evento.Capacidade - quantidadeInscrita >= quantidade;
        }

        public ResultadoCriacaoInscricao CriarInscricao(DadosInscricao dados)
        {
            if (!DadosInscricaoValidos(dados))
            {
                return CriarResultado(false, "Dados de inscricao invalidos.", null);
            }

            if (!ValidarDisponibilidade(dados.IdEvento, dados.Quantidade))
            {
                return CriarResultado(false, "Nao existem vagas suficientes para o numero de inscricoes pretendido.", null);
            }

            DocumentoPdf bilhetePdf = ValidarRegistarInscricaoEGerarBilhete(dados);

            return CriarResultado(true, "Inscricao criada com sucesso.", bilhetePdf);
        }

        public DocumentoPdf ValidarRegistarInscricaoEGerarBilhete(DadosInscricao dados)
        {
            if (!DadosInscricaoValidos(dados))
            {
                throw new InvalidOperationException("Dados de inscricao invalidos.");
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();

            Evento? evento = ObterEvento(ligacao, transacao, dados.IdEvento);
            if (evento == null || !string.Equals(evento.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Evento nao encontrado ou indisponivel.");
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, transacao, dados.IdEvento, null);
            if (evento.Capacidade - quantidadeInscrita < dados.Quantidade)
            {
                throw new InvalidOperationException("Nao existem vagas suficientes para o numero de inscricoes pretendido.");
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                INSERT INTO inscricoes
                    (id_evento, nome_participante, email_participante, idade_participante, quantidade, estado)
                VALUES
                    (@idEvento, @nome, @email, @idade, @quantidade, 'ativa');
                SELECT last_insert_rowid();";

            AdicionarParametrosInscricao(comando, dados);
            int idInscricao = Convert.ToInt32(comando.ExecuteScalar(), CultureInfo.InvariantCulture);
            transacao.Commit();

            Inscricao inscricao = new Inscricao
            {
                Id = idInscricao,
                IdEvento = dados.IdEvento,
                NomeParticipante = dados.NomeParticipante.Trim(),
                EmailParticipante = dados.EmailParticipante.Trim(),
                IdadeParticipante = dados.IdadeParticipante,
                Quantidade = dados.Quantidade,
                Estado = "ativa"
            };

            DocumentoPdf bilhetePdf = CriarDocumentoPdf(
                "Bilhete de inscricao",
                "bilhete-inscricao-" + idInscricao + ".pdf");

            GerarFicheiroPdf(bilhetePdf, ConstruirConteudoBilhete(inscricao, evento));
            return bilhetePdf;
        }

        public List<Inscricao> ListarInscricoes()
        {
            return ObterListaInscricoes();
        }

        public List<Inscricao> ObterListaInscricoes()
        {
            List<Inscricao> inscricoes = new List<Inscricao>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                ORDER BY id;";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                inscricoes.Add(MapearInscricao(leitor));
            }

            return inscricoes;
        }

        public Inscricao? ObterInscricao(int idInscricao)
        {
            return ObterDadosInscricao(idInscricao);
        }

        public Inscricao? ObterDadosInscricao(int idInscricao)
        {
            if (idInscricao <= 0)
            {
                return null;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id = @idInscricao;";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearInscricao(leitor);
            }

            return null;
        }

        public bool ValidarAlteracaoInscricao(int idInscricao, DadosInscricao dados)
        {
            return VerificarSeAlteracaoEhPossivel(idInscricao, dados);
        }

        public bool VerificarSeAlteracaoEhPossivel(int idInscricao, DadosInscricao dados)
        {
            if (idInscricao <= 0 || !DadosInscricaoValidos(dados))
            {
                return false;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            Inscricao? inscricaoAtual = ObterInscricao(ligacao, null, idInscricao);
            Evento? eventoDestino = ObterEvento(ligacao, null, dados.IdEvento);

            if (inscricaoAtual == null ||
                !string.Equals(inscricaoAtual.Estado, "ativa", StringComparison.OrdinalIgnoreCase) ||
                eventoDestino == null ||
                !string.Equals(eventoDestino.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, null, dados.IdEvento, idInscricao);
            return eventoDestino.Capacidade - quantidadeInscrita >= dados.Quantidade;
        }

        public DocumentoPdf AlterarInscricao(int idInscricao, DadosInscricao dados)
        {
            return ValidarAtualizarInscricaoEGerarBilhete(idInscricao, dados);
        }

        public DocumentoPdf ValidarAtualizarInscricaoEGerarBilhete(int idInscricao, DadosInscricao dados)
        {
            if (!VerificarSeAlteracaoEhPossivel(idInscricao, dados))
            {
                throw new InvalidOperationException("Nao foi possivel alterar a inscricao com os dados indicados.");
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE inscricoes
                SET id_evento = @idEvento,
                    nome_participante = @nome,
                    email_participante = @email,
                    idade_participante = @idade,
                    quantidade = @quantidade,
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE id = @idInscricao
                  AND estado = 'ativa';";

            comando.Parameters.AddWithValue("@idInscricao", idInscricao);
            AdicionarParametrosInscricao(comando, dados);
            int linhasAfetadas = comando.ExecuteNonQuery();
            transacao.Commit();

            if (linhasAfetadas == 0)
            {
                throw new InvalidOperationException("Inscricao nao encontrada ou indisponivel para alteracao.");
            }

            Inscricao inscricaoAtualizada = ObterInscricao(idInscricao) ?? new Inscricao();
            Evento? evento = ObterEvento(dados.IdEvento);

            DocumentoPdf bilhetePdf = CriarDocumentoPdf(
                "Bilhete atualizado",
                "bilhete-atualizado-" + idInscricao + ".pdf");

            GerarFicheiroPdf(bilhetePdf, ConstruirConteudoBilhete(inscricaoAtualizada, evento));
            return bilhetePdf;
        }

        public void CancelarInscricao(int idInscricao)
        {
            AtualizarEstadoInscricao(idInscricao, "cancelada");
        }

        public void AtualizarEstadoInscricao(int idInscricao, string estado)
        {
            if (idInscricao <= 0 || string.IsNullOrWhiteSpace(estado))
            {
                return;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE inscricoes
                SET estado = @estado,
                    atualizado_em = CURRENT_TIMESTAMP,
                    cancelado_em = CASE
                        WHEN @estado IN ('cancelada', 'cancelada_por_evento') AND cancelado_em IS NULL THEN CURRENT_TIMESTAMP
                        ELSE cancelado_em
                    END
                WHERE id = @idInscricao
                  AND estado = 'ativa';";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);
            comando.Parameters.AddWithValue("@estado", estado.Trim());
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        public List<Inscricao> ObterInscritosAfetados(int idEvento)
        {
            List<Inscricao> resultados = new List<Inscricao>();

            if (idEvento <= 0)
            {
                return resultados;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id_evento = @idEvento
                  AND estado = 'ativa'
                ORDER BY id;";
            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                resultados.Add(MapearInscricao(leitor));
            }

            return resultados;
        }

        public void CancelarOuInvalidarInscricao(int idInscricao)
        {
            AtualizarEstadoInscricao(idInscricao, "cancelada_por_evento");
        }

        public DocumentoPdf GerarComprovativoCancelamento(int idInscricao)
        {
            Inscricao? inscricao = ObterInscricao(idInscricao);
            Evento? evento = inscricao == null ? null : ObterEvento(inscricao.IdEvento);

            DocumentoPdf comprovativo = CriarDocumentoPdf(
                "Comprovativo de cancelamento",
                "comprovativo-cancelamento-" + idInscricao + ".pdf");

            GerarFicheiroPdf(comprovativo, ConstruirConteudoCancelamento(inscricao, evento));
            return comprovativo;
        }

        public string ObterConnectionString()
        {
            return connectionString;
        }

        public string ObterPastaPdfs()
        {
            return pastaPdfs;
        }

        private ResultadoCriacaoInscricao CriarResultado(bool sucesso, string mensagem, DocumentoPdf? bilhetePdf)
        {
            return new ResultadoCriacaoInscricao
            {
                Sucesso = sucesso,
                Mensagem = mensagem,
                BilhetePdf = bilhetePdf
            };
        }

        private bool DadosInscricaoValidos(DadosInscricao dados)
        {
            return dados != null &&
                   dados.IdEvento > 0 &&
                   !string.IsNullOrWhiteSpace(dados.NomeParticipante) &&
                   !string.IsNullOrWhiteSpace(dados.EmailParticipante) &&
                   dados.IdadeParticipante > 0 &&
                   dados.Quantidade > 0;
        }

        private void AdicionarParametrosInscricao(SqliteCommand comando, DadosInscricao dados)
        {
            comando.Parameters.AddWithValue("@idEvento", dados.IdEvento);
            comando.Parameters.AddWithValue("@nome", dados.NomeParticipante.Trim());
            comando.Parameters.AddWithValue("@email", dados.EmailParticipante.Trim());
            comando.Parameters.AddWithValue("@idade", dados.IdadeParticipante);
            comando.Parameters.AddWithValue("@quantidade", dados.Quantidade);
        }

        private Evento? ObterEvento(int idEvento)
        {
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            return ObterEvento(ligacao, null, idEvento);
        }

        private Evento? ObterEvento(SqliteConnection ligacao, SqliteTransaction? transacao, int idEvento)
        {
            if (idEvento <= 0)
            {
                return null;
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT id, nome, local, data, estado, capacidade
                FROM eventos
                WHERE id = @idEvento;";
            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearEvento(leitor);
            }

            return null;
        }

        private Inscricao? ObterInscricao(SqliteConnection ligacao, SqliteTransaction? transacao, int idInscricao)
        {
            if (idInscricao <= 0)
            {
                return null;
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id = @idInscricao;";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearInscricao(leitor);
            }

            return null;
        }

        private int ObterQuantidadeInscritaAtiva(
            SqliteConnection ligacao,
            SqliteTransaction? transacao,
            int idEvento,
            int? idInscricaoAExcluir)
        {
            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT COALESCE(SUM(quantidade), 0)
                FROM inscricoes
                WHERE id_evento = @idEvento
                  AND estado = 'ativa'
                  AND (@idExcluir IS NULL OR id <> @idExcluir);";
            comando.Parameters.AddWithValue("@idEvento", idEvento);
            comando.Parameters.AddWithValue("@idExcluir", idInscricaoAExcluir.HasValue ? (object)idInscricaoAExcluir.Value : DBNull.Value);

            object? resultado = comando.ExecuteScalar();
            return Convert.ToInt32(resultado, CultureInfo.InvariantCulture);
        }

        private Evento MapearEvento(SqliteDataReader leitor)
        {
            return new Evento
            {
                Id = LerInteiro(leitor, "id"),
                Nome = LerTexto(leitor, "nome"),
                Local = LerTexto(leitor, "local"),
                Data = LerData(leitor, "data"),
                Estado = LerTexto(leitor, "estado"),
                Capacidade = LerInteiro(leitor, "capacidade")
            };
        }

        private Inscricao MapearInscricao(SqliteDataReader leitor)
        {
            return new Inscricao
            {
                Id = LerInteiro(leitor, "id"),
                IdEvento = LerInteiro(leitor, "id_evento"),
                NomeParticipante = LerTexto(leitor, "nome_participante"),
                EmailParticipante = LerTexto(leitor, "email_participante"),
                IdadeParticipante = LerInteiro(leitor, "idade_participante"),
                Quantidade = LerInteiro(leitor, "quantidade"),
                Estado = LerTexto(leitor, "estado")
            };
        }

        private int LerInteiro(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return 0;
            }

            return Convert.ToInt32(leitor.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private string LerTexto(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return Convert.ToString(leitor.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private DateTime LerData(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return DateTime.MinValue;
            }

            object valor = leitor.GetValue(ordinal);

            if (valor is DateTime dataDireta)
            {
                return dataDireta;
            }

            string texto = Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;

            if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data) ||
                DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.None, out data))
            {
                return data;
            }

            return DateTime.MinValue;
        }

        private DocumentoPdf CriarDocumentoPdf(string titulo, string nomeFicheiro)
        {
            return new DocumentoPdf
            {
                Titulo = titulo,
                NomeFicheiro = nomeFicheiro,
                CaminhoFicheiro = ConfiguracaoAplicacao.CombinarCaminhoPdf(nomeFicheiro)
            };
        }

        private string ConstruirConteudoBilhete(Inscricao inscricao, Evento? evento)
        {
            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine("Bilhete de inscricao");
            conteudo.AppendLine(string.Format("Inscricao: {0}", inscricao.Id));
            conteudo.AppendLine(string.Format("Participante: {0}", inscricao.NomeParticipante));
            conteudo.AppendLine(string.Format("Email: {0}", inscricao.EmailParticipante));
            conteudo.AppendLine(string.Format("Idade: {0}", inscricao.IdadeParticipante));
            conteudo.AppendLine(string.Format("Quantidade: {0}", inscricao.Quantidade));
            conteudo.AppendLine(string.Format("Estado: {0}", inscricao.Estado));

            if (evento != null)
            {
                conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
                conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
                conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            }
            else
            {
                conteudo.AppendLine(string.Format("Evento: {0}", inscricao.IdEvento));
            }

            return conteudo.ToString();
        }

        private string ConstruirConteudoCancelamento(Inscricao? inscricao, Evento? evento)
        {
            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine("Comprovativo de cancelamento");
            conteudo.AppendLine(string.Format("Data do cancelamento: {0:dd/MM/yyyy HH:mm}", DateTime.Now));

            if (inscricao == null)
            {
                conteudo.AppendLine("Inscricao nao encontrada.");
                return conteudo.ToString();
            }

            conteudo.AppendLine(string.Format("Inscricao: {0}", inscricao.Id));
            conteudo.AppendLine(string.Format("Participante: {0}", inscricao.NomeParticipante));
            conteudo.AppendLine(string.Format("Email: {0}", inscricao.EmailParticipante));
            conteudo.AppendLine(string.Format("Quantidade: {0}", inscricao.Quantidade));
            conteudo.AppendLine(string.Format("Estado atual: {0}", inscricao.Estado));

            if (evento != null)
            {
                conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
                conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
                conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            }
            else
            {
                conteudo.AppendLine(string.Format("Evento: {0}", inscricao.IdEvento));
            }

            return conteudo.ToString();
        }

        private void GerarFicheiroPdf(DocumentoPdf documentoPdf, string conteudo)
        {
            string? pastaDestino = Path.GetDirectoryName(documentoPdf.CaminhoFicheiro);

            if (!string.IsNullOrWhiteSpace(pastaDestino))
            {
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

            foreach (string linha in SepararLinhasPdf(conteudo, grafico, fonteCorpo, pagina.Width.Point - margem * 2))
            {
                if (y + alturaLinha > pagina.Height.Point - margem)
                {
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

        private List<string> SepararLinhasPdf(string texto, XGraphics grafico, XFont fonte, double larguraMaxima)
        {
            List<string> linhas = new List<string>();
            string textoNormalizado = (texto ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            foreach (string linhaOriginal in textoNormalizado.Split('\n'))
            {
                linhas.AddRange(QuebrarLinhaPdf(linhaOriginal, grafico, fonte, larguraMaxima));
            }

            return linhas;
        }

        private List<string> QuebrarLinhaPdf(string linhaOriginal, XGraphics grafico, XFont fonte, double larguraMaxima)
        {
            List<string> linhas = new List<string>();

            if (string.IsNullOrWhiteSpace(linhaOriginal))
            {
                linhas.Add(string.Empty);
                return linhas;
            }

            string linhaAtual = string.Empty;

            foreach (string palavra in linhaOriginal.Split(' '))
            {
                string candidata = string.IsNullOrEmpty(linhaAtual)
                    ? palavra
                    : linhaAtual + " " + palavra;

                if (grafico.MeasureString(candidata, fonte).Width <= larguraMaxima)
                {
                    linhaAtual = candidata;
                    continue;
                }

                if (!string.IsNullOrEmpty(linhaAtual))
                {
                    linhas.Add(linhaAtual);
                }

                linhaAtual = palavra;
            }

            if (!string.IsNullOrEmpty(linhaAtual))
            {
                linhas.Add(linhaAtual);
            }

            return linhas;
        }
    }
}
