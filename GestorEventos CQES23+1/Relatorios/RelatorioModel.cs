using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Relatorios {
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
            // Aqui ficará a query SQLite para listar eventos.
            return new List<Evento> {
                new Evento {
                    Id = 1,
                    Nome = "Workshop de Arquitetura",
                    Local = "Lisboa",
                    Data = new System.DateTime(2026, 5, 15),
                    Estado = "ativo",
                    Capacidade = 30
                },
                new Evento {
                    Id = 2,
                    Nome = "Seminario MVC",
                    Local = "Porto",
                    Data = new System.DateTime(2026, 6, 10),
                    Estado = "ativo",
                    Capacidade = 50
                }
            };
        }

        public DadosRelatorio ListarInscritosPorEvento(int idEvento) {
            return ObterDadosRelatorioEGerarPdf(idEvento);
        }

        public DadosRelatorio ObterDadosRelatorioEGerarPdf(int idEvento) {
            // Aqui ficarão a query SQLite e a geração do PDF em PDFsharp.
            ultimoRelatorioGerado = CriarDocumentoPdf(
                "Listagem de inscritos por evento",
                "relatorio-inscritos-evento-" + idEvento + ".pdf");
            return new DadosRelatorio {
                Titulo = "Listagem de inscritos por evento",
                Conteudo = "Dados do evento " + idEvento
            };
        }

        public DadosRelatorio ListarEventosComOcupacao() {
            return ObterDadosRelatorioOcupacaoEGerarPdf();
        }

        public DadosRelatorio ObterDadosRelatorioOcupacaoEGerarPdf() {
            // Aqui ficarão a query SQLite agregada e a geração do PDF em PDFsharp.
            ultimoRelatorioGerado = CriarDocumentoPdf("Eventos com ocupacao", "relatorio-ocupacao.pdf");
            return new DadosRelatorio {
                Titulo = "Eventos com ocupacao",
                Conteudo = "Dados agregados de ocupacao"
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
    }
}
