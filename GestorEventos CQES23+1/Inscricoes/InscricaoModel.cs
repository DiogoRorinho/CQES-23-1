using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Inscricoes {
    class InscricaoModel {
        private readonly string connectionString;
        private readonly string pastaPdfs;

        public InscricaoModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
        }

        public List<Evento> ListarEventosDisponiveis() {
            return ObterEventosComDisponibilidade();
        }

        public List<Evento> ObterEventosComDisponibilidade() {
            // Aqui ficará a query SQLite para listar eventos com vagas.
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

        public bool VerificarDisponibilidade(int idEvento, int quantidade) {
            return ValidarDisponibilidade(idEvento, quantidade);
        }

        public bool ValidarDisponibilidade(int idEvento, int quantidade) {
            // Aqui ficará a validação apoiada em queries SQLite.
            return true;
        }

        public DocumentoPdf CriarInscricao(DadosInscricao dados) {
            return ValidarRegistarInscricaoEGerarBilhete(dados);
        }

        public DocumentoPdf ValidarRegistarInscricaoEGerarBilhete(DadosInscricao dados) {
            // Aqui ficarão o INSERT SQLite e a geração do bilhete em PDFsharp.
            return CriarDocumentoPdf("Bilhete de inscricao", "bilhete-inscricao.pdf");
        }

        public List<Inscricao> ListarInscricoes() {
            return ObterListaInscricoes();
        }

        public List<Inscricao> ObterListaInscricoes() {
            // Aqui ficará a query SQLite para listar inscrições.
            return new List<Inscricao> {
                new Inscricao { Id = 1, IdEvento = 1, Estado = "ativa", EmailParticipante = "participante1@exemplo.pt" },
                new Inscricao { Id = 2, IdEvento = 2, Estado = "ativa", EmailParticipante = "participante2@exemplo.pt" }
            };
        }

        public Inscricao ObterInscricao(int idInscricao) {
            return ObterDadosInscricao(idInscricao);
        }

        public Inscricao ObterDadosInscricao(int idInscricao) {
            // Aqui ficará a query SQLite para obter os dados de uma inscrição.
            return new Inscricao {
                Id = idInscricao,
                IdEvento = 1,
                Estado = "ativa",
                EmailParticipante = "participante@exemplo.pt"
            };
        }

        public bool ValidarAlteracaoInscricao(int idInscricao, DadosInscricao dados) {
            return VerificarSeAlteracaoEhPossivel(idInscricao, dados);
        }

        public bool VerificarSeAlteracaoEhPossivel(int idInscricao, DadosInscricao dados) {
            // Aqui ficará a lógica de validação apoiada em SQLite.
            return true;
        }

        public DocumentoPdf AlterarInscricao(int idInscricao, DadosInscricao dados) {
            return ValidarAtualizarInscricaoEGerarBilhete(idInscricao, dados);
        }

        public DocumentoPdf ValidarAtualizarInscricaoEGerarBilhete(int idInscricao, DadosInscricao dados) {
            // Aqui ficarão o UPDATE SQLite e a regeneração do bilhete em PDFsharp.
            return CriarDocumentoPdf("Bilhete atualizado", "bilhete-atualizado.pdf");
        }

        public void CancelarInscricao(int idInscricao) {
            AtualizarEstadoInscricao(idInscricao, "cancelada");
        }

        public void AtualizarEstadoInscricao(int idInscricao, string estado) {
            // Aqui ficará o UPDATE SQLite do estado da inscrição.
        }

        public List<Inscricao> ObterInscritosAfetados(int idEvento) {
            List<Inscricao> resultados = new List<Inscricao>();

            foreach (Inscricao inscricao in ObterListaInscricoes()) {
                if (inscricao.IdEvento == idEvento) {
                    resultados.Add(inscricao);
                }
            }

            return resultados;
        }

        public void CancelarOuInvalidarInscricao(int idInscricao) {
            AtualizarEstadoInscricao(idInscricao, "cancelada_por_evento");
        }

        public DocumentoPdf GerarComprovativoCancelamento(int idInscricao) {
            // Aqui ficará a geração do comprovativo de cancelamento em PDFsharp.
            return CriarDocumentoPdf(
                "Comprovativo de cancelamento",
                "comprovativo-cancelamento-" + idInscricao + ".pdf");
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
