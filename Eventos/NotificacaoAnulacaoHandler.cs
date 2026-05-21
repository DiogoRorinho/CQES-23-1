using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GestorEventos.Inscricoes;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class NotificacaoAnulacaoHandler {
        private readonly InscricaoModel inscricaoModel;
        private readonly string pastaNotificacoes;

        public NotificacaoAnulacaoHandler(InscricaoModel inscricaoModel) {
            this.inscricaoModel = inscricaoModel;
            pastaNotificacoes = Path.Combine(AppContext.BaseDirectory, "Notificacoes");
            Directory.CreateDirectory(pastaNotificacoes);
        }

        public void OnEventoCancelado(object sender, EventoCanceladoEventArgs e) {
            List<Inscricao> inscritosAfetados = ObterInscritosAfetados(e.IdEvento);

            foreach (Inscricao inscricao in inscritosAfetados) {
                CancelarOuInvalidarInscricao(inscricao.Id);
                DocumentoPdf comprovativo = GerarComprovativoCancelamento(inscricao.Id);
                EnviarNotificacaoAoParticipante(inscricao.EmailParticipante, comprovativo, e);
            }
        }

        public List<Inscricao> ObterInscritosAfetados(int idEvento) {
            return inscricaoModel.ObterInscritosAfetados(idEvento);
        }

        public void CancelarOuInvalidarInscricao(int idInscricao) {
            inscricaoModel.CancelarOuInvalidarInscricao(idInscricao);
        }

        public DocumentoPdf GerarComprovativoCancelamento(int idInscricao) {
            return inscricaoModel.GerarComprovativoCancelamento(idInscricao);
        }

        public void EnviarNotificacaoAoParticipante(
            string destinatario,
            DocumentoPdf comprovativo,
            EventoCanceladoEventArgs dadosCancelamento) {
            string emailNormalizado = NormalizarParaNomeFicheiro(destinatario);
            string nomeFicheiro = string.Format(
                "notificacao-cancelamento-evento-{0}-{1:yyyyMMdd-HHmmss}-{2}.txt",
                dadosCancelamento.IdEvento,
                DateTime.Now,
                emailNormalizado);

            string caminhoFicheiro = Path.Combine(pastaNotificacoes, nomeFicheiro);
            string conteudo = ConstruirConteudoNotificacao(destinatario, comprovativo, dadosCancelamento);

            File.WriteAllText(caminhoFicheiro, conteudo, Encoding.UTF8);
            Console.WriteLine(string.Format(
                "Notificacao de cancelamento registada para {0}: {1}",
                destinatario,
                caminhoFicheiro));
        }

        private static string NormalizarParaNomeFicheiro(string valor) {
            if (string.IsNullOrWhiteSpace(valor)) {
                return "destinatario";
            }

            StringBuilder resultado = new StringBuilder(valor.Length);
            foreach (char caractere in valor.Trim().ToLowerInvariant()) {
                if (char.IsLetterOrDigit(caractere) || caractere == '-' || caractere == '_') {
                    resultado.Append(caractere);
                }
                else if (caractere == '@' || caractere == '.') {
                    resultado.Append('-');
                }
            }

            return resultado.Length == 0 ? "destinatario" : resultado.ToString();
        }

        private static string ConstruirConteudoNotificacao(
            string destinatario,
            DocumentoPdf comprovativo,
            EventoCanceladoEventArgs dadosCancelamento) {
            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine("Notificacao de cancelamento de evento");
            conteudo.AppendLine(string.Format("Gerado em: {0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
            conteudo.AppendLine(string.Format("Destinatario: {0}", destinatario));
            conteudo.AppendLine(string.Format("Evento: {0} - {1}", dadosCancelamento.IdEvento, dadosCancelamento.NomeEvento));
            conteudo.AppendLine(string.Format("Data cancelamento: {0:dd/MM/yyyy HH:mm:ss}", dadosCancelamento.DataCancelamento));
            conteudo.AppendLine(string.Format("Estado do evento: {0}", dadosCancelamento.Estado));
            conteudo.AppendLine(string.Format("Comprovativo: {0}", comprovativo.CaminhoFicheiro));

            return conteudo.ToString();
        }
    }
}
