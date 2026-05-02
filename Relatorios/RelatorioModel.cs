using System;
using System.Collections.Generic;
using System.Text;
using GestorEventos.Partilhado;

namespace GestorEventos.Relatorios {
    class RelatorioModel {
        private readonly string connectionString;
        private readonly string pastaPdfs;
        private DocumentoPdf ultimoRelatorioGerado;

        public RelatorioModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ObterListaEventos() {
            // Aqui ficara a query SQLite para listar eventos - Simulacao de dados temporaria.
            return new List<Evento> {
                new Evento {
                    Id = 1,
                    Nome = "Workshop de Arquitetura",
                    Local = "Lisboa",
                    Data = new DateTime(2026, 5, 15),
                    Estado = "ativo",
                    Capacidade = 30
                },
                new Evento {
                    Id = 2,
                    Nome = "Seminario MVC",
                    Local = "Porto",
                    Data = new DateTime(2026, 6, 10),
                    Estado = "ativo",
                    Capacidade = 50
                }
            };
        }

        public DadosRelatorio ListarInscritosPorEvento(int idEvento) {
            return ObterDadosRelatorioEGerarPdf(idEvento);
        }

        public DadosRelatorio ObterDadosRelatorioEGerarPdf(int idEvento) {
            // Aqui ficarao a query SQLite e a geracao do PDF em PDFsharp.
            Evento evento = ObterEventoPorId(idEvento);
            List<Inscricao> inscricoes = ObterInscricoesPorEvento(idEvento);

            ultimoRelatorioGerado = CriarDocumentoPdf(
                "Listagem de inscritos por evento",
                "relatorio-inscritos-evento-" + idEvento + ".pdf");

            return new DadosRelatorio {
                Titulo = "Listagem de inscritos por evento",
                Conteudo = ConstruirConteudoInscritos(evento, inscricoes)
            };
        }

        public DadosRelatorio ListarEventosComOcupacao() {
            return ObterDadosRelatorioOcupacaoEGerarPdf();
        }

        public DadosRelatorio ObterDadosRelatorioOcupacaoEGerarPdf() {
            // Aqui ficarao a query SQLite agregada e a geracao do PDF em PDFsharp.
            ultimoRelatorioGerado = CriarDocumentoPdf("Eventos com ocupacao", "relatorio-ocupacao.pdf");

            return new DadosRelatorio {
                Titulo = "Eventos com ocupacao",
                Conteudo = ConstruirConteudoOcupacao()
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

        private Evento ObterEventoPorId(int idEvento) {
            foreach (Evento evento in ObterListaEventos()) {
                if (evento.Id == idEvento) {
                    return evento;
                }
            }

            return null;
        }

        private List<Inscricao> ObterInscricoesPorEvento(int idEvento) {
            List<Inscricao> inscricoesEvento = new List<Inscricao>();

            foreach (Inscricao inscricao in ObterListaInscricoes()) {
                if (inscricao.IdEvento == idEvento) {
                    inscricoesEvento.Add(inscricao);
                }
            }

            return inscricoesEvento;
        }

        // Simulacao de dados temporaria para inscricoes.
        private List<Inscricao> ObterListaInscricoes() {
            return new List<Inscricao> {
                new Inscricao { Id = 1, IdEvento = 1, Estado = "ativa", EmailParticipante = "ana@exemplo.pt" },
                new Inscricao { Id = 2, IdEvento = 1, Estado = "ativa", EmailParticipante = "bruno@exemplo.pt" },
                new Inscricao { Id = 3, IdEvento = 2, Estado = "ativa", EmailParticipante = "carla@exemplo.pt" },
                new Inscricao { Id = 4, IdEvento = 2, Estado = "cancelada", EmailParticipante = "diogo@exemplo.pt" }
            };
        }

        private string ConstruirConteudoInscritos(Evento evento, List<Inscricao> inscricoes) {
            if (evento == null) {
                return "Evento nao encontrado.";
            }

            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
            conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
            conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            conteudo.AppendLine(string.Format("Total de inscricoes: {0}", inscricoes.Count));

            if (inscricoes.Count == 0) {
                conteudo.AppendLine("Nao existem inscricoes registadas para este evento.");
                return conteudo.ToString();
            }

            conteudo.AppendLine("Inscritos:");
            foreach (Inscricao inscricao in inscricoes) {
                conteudo.AppendLine(string.Format(
                    "- #{0} | {1} | {2}",
                    inscricao.Id,
                    inscricao.EmailParticipante,
                    inscricao.Estado));
            }

            return conteudo.ToString();
        }

        private string ConstruirConteudoOcupacao() {
            StringBuilder conteudo = new StringBuilder();

            foreach (Evento evento in ObterListaEventos()) {
                int totalInscricoesAtivas = 0;

                foreach (Inscricao inscricao in ObterInscricoesPorEvento(evento.Id)) {
                    if (inscricao.Estado == "ativa") {
                        totalInscricoesAtivas++;
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
    }
}
