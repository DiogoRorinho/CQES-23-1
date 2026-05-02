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

        // O método ListarEventosDisponiveis retorna uma lista de eventos que possuem vagas disponíveis para inscrição.
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

        // O método VerificarDisponibilidade é responsável por verificar se há vagas disponíveis para um evento específico, com base no ID do evento e na quantidade de vagas solicitadas.
        public bool VerificarDisponibilidade(int idEvento, int quantidade) {
            return ValidarDisponibilidade(idEvento, quantidade);
        }

        // O método ValidarDisponibilidade contém a lógica de validação para verificar se a quantidade de vagas solicitada está disponível para o evento especificado.
        // ----- provisório até SQLite estar implementado -----
        public bool ValidarDisponibilidade(int idEvento, int quantidade) {
            // Aqui ficará a validação apoiada em queries SQLite.
            return true;
        }

        // O método CriarInscricao é responsável por criar uma nova inscrição com base nos dados fornecidos.
        public ResultadoCriacaoInscricao CriarInscricao(DadosInscricao dados) {
            if (string.IsNullOrWhiteSpace(dados.NomeParticipante) ||
                string.IsNullOrWhiteSpace(dados.EmailParticipante) ||
                dados.IdadeParticipante <= 0) {
                return new ResultadoCriacaoInscricao {
                    Sucesso = false,
                    Mensagem = "Dados de inscricao invalidos."
                };
            }

            if (!ValidarDisponibilidade(dados.IdEvento, dados.Quantidade)) {
                return new ResultadoCriacaoInscricao {
                    Sucesso = false,
                    Mensagem = "Nao existem vagas suficientes para o numero de inscricoes pretendido."
                };
            }

            DocumentoPdf bilhetePdf = ValidarRegistarInscricaoEGerarBilhete(dados);

            return new ResultadoCriacaoInscricao {
                Sucesso = true,
                Mensagem = "Inscricao criada com sucesso.",
                BilhetePdf = bilhetePdf
            };
        }


        // O método ValidarRegistarInscricaoEGerarBilhete contém a lógica de validação para os dados da inscrição, bem como a lógica para registrar a inscrição no banco de dados SQLite e gerar um bilhete em formato PDF usando PDFsharp.
        public DocumentoPdf ValidarRegistarInscricaoEGerarBilhete(DadosInscricao dados) {
            // Aqui ficarão o INSERT SQLite e a geração do bilhete em PDFsharp.
            return CriarDocumentoPdf("Bilhete de inscricao", "bilhete-inscricao.pdf");
        }

        // O método ListarInscricoes retorna uma lista de todas as inscrições registradas no sistema. Ele chama o método ObterListaInscricoes, que é responsável por executar a query SQLite para obter as inscrições.
        public List<Inscricao> ListarInscricoes() {
            return ObterListaInscricoes();
        }

        // O método ObterListaInscricoes contém a lógica para executar a query SQLite e obter a lista de inscrições registradas no sistema. Ele retorna uma lista de objetos do tipo Inscricao, que representam as inscrições.
        public List<Inscricao> ObterListaInscricoes() {
            // Aqui ficará a query SQLite para listar inscrições.
            return new List<Inscricao> {
                new Inscricao { Id = 1, IdEvento = 1, Estado = "ativa", EmailParticipante = "participante1@exemplo.pt" },
                new Inscricao { Id = 2, IdEvento = 2, Estado = "ativa", EmailParticipante = "participante2@exemplo.pt" }
            };
        }

        // O método ObterInscricao é responsável por obter os dados de uma inscrição específica com base no ID da inscrição. Ele chama o método ObterDadosInscricao, que é responsável por executar a query SQLite para obter os dados da inscrição.
        public Inscricao ObterInscricao(int idInscricao) {
            return ObterDadosInscricao(idInscricao);
        }

        // O método ObterDadosInscricao contém a lógica para executar a query SQLite e obter os dados de uma inscrição específica com base no ID da inscrição. Ele retorna um objeto do tipo Inscricao, que representa os dados da inscrição.
        public Inscricao ObterDadosInscricao(int idInscricao) {
            // Aqui ficará a query SQLite para obter os dados de uma inscrição.
            return new Inscricao {
                Id = idInscricao,
                IdEvento = 1,
                Estado = "ativa",
                EmailParticipante = "participante@exemplo.pt"
            };
        }

        // O método ValidarAlteracaoInscricao é responsável por validar se a alteração de uma inscrição específica é possível com base nos dados fornecidos.
        public bool ValidarAlteracaoInscricao(int idInscricao, DadosInscricao dados) {
            return VerificarSeAlteracaoEhPossivel(idInscricao, dados);
        }

        // O método VerificarSeAlteracaoEhPossivel contém a lógica de validação para verificar se a alteração de uma inscrição específica é possível com base nos dados fornecidos.
        public bool VerificarSeAlteracaoEhPossivel(int idInscricao, DadosInscricao dados) {
            // Aqui ficará a lógica de validação apoiada em SQLite.
            return true;
        }

        // O método AlterarInscricao é responsável por alterar os dados de uma inscrição específica com base no ID da inscrição e nos dados fornecidos.
        public DocumentoPdf AlterarInscricao(int idInscricao, DadosInscricao dados) {
            return ValidarAtualizarInscricaoEGerarBilhete(idInscricao, dados);
        }

        public DocumentoPdf ValidarAtualizarInscricaoEGerarBilhete(int idInscricao, DadosInscricao dados) {
            // Aqui ficarão o UPDATE SQLite e a regeneração do bilhete em PDFsharp.
            return CriarDocumentoPdf("Bilhete atualizado", "bilhete-atualizado.pdf");
        }

        // O método CancelarInscricao é responsável por cancelar uma inscrição específica com base no ID da inscrição.
        public void CancelarInscricao(int idInscricao) {
            AtualizarEstadoInscricao(idInscricao, "cancelada");
        }

        // O método AtualizarEstadoInscricao contém a lógica para atualizar o estado de uma inscrição específica no banco de dados SQLite.
        public void AtualizarEstadoInscricao(int idInscricao, string estado) {
            // Aqui ficará o UPDATE SQLite do estado da inscrição.
        }


        // O método ObterInscritosAfetados é responsável por obter a lista de inscrições afetadas por um evento cancelado, com base no ID do evento. 
        public List<Inscricao> ObterInscritosAfetados(int idEvento) {
            List<Inscricao> resultados = new List<Inscricao>();

            foreach (Inscricao inscricao in ObterListaInscricoes()) {
                if (inscricao.IdEvento == idEvento) {
                    resultados.Add(inscricao);
                }
            }

            return resultados;
        }

        // O método CancelarOuInvalidarInscricao é responsável por cancelar ou invalidar uma inscrição específica com base no ID da inscrição. Ele chama o método AtualizarEstadoInscricao para atualizar o estado da inscrição para "cancelada_por_evento" no banco de dados SQLite.
        public void CancelarOuInvalidarInscricao(int idInscricao) {
            AtualizarEstadoInscricao(idInscricao, "cancelada_por_evento");
        }

        // O método GerarComprovativoCancelamento é responsável por gerar um comprovativo de cancelamento em formato PDF para uma inscrição específica com base no ID da inscrição. Ele chama o método CriarDocumentoPdf para criar o documento PDF correspondente.
        public DocumentoPdf GerarComprovativoCancelamento(int idInscricao) {
            // Aqui ficará a geração do comprovativo de cancelamento em PDFsharp.
            return CriarDocumentoPdf(
                "Comprovativo de cancelamento",
                "comprovativo-cancelamento-" + idInscricao + ".pdf");
        }

        // O método ObterConnectionString é responsável por obter a string de conexão para o banco de dados SQLite. Ele retorna a string de conexão armazenada na variável connectionString.
        public string ObterConnectionString() {
            return connectionString;
        }

        // O método ObterPastaPdfs é responsável por obter o caminho da pasta onde os arquivos PDF serão armazenados. Ele retorna o caminho da pasta armazenado na variável pastaPdfs.
        public string ObterPastaPdfs() {
            return pastaPdfs;
        }

        // O método CombinarCaminhoPdf é responsável por combinar o caminho da pasta de PDFs com o nome do arquivo PDF para obter o caminho completo do arquivo PDF. Ele recebe o nome do arquivo como parâmetro e retorna o caminho completo do arquivo PDF.
        private DocumentoPdf CriarDocumentoPdf(string titulo, string nomeFicheiro) {
            return new DocumentoPdf {
                Titulo = titulo,
                NomeFicheiro = nomeFicheiro,
                CaminhoFicheiro = ConfiguracaoAplicacao.CombinarCaminhoPdf(nomeFicheiro)
            };
        }
    }
}
