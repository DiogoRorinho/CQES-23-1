using System.Collections.Generic;
using GestorEventosEsqueleto.Inscricoes;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Eventos {
    class NotificacaoAnulacaoHandler {
        private readonly InscricaoModel inscricaoModel;

        public NotificacaoAnulacaoHandler(InscricaoModel inscricaoModel) {
            this.inscricaoModel = inscricaoModel;
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
            // Aqui ficará o envio de notificação relativo ao evento cancelado.
        }
    }
}
