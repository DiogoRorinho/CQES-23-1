using System;

namespace GestorEventos.Partilhado.Servicos {
    public class InscricaoTerminadaEventArgs : EventArgs {
        public int IdInscricao { get; }
        public int IdEvento { get; }
        public string EmailParticipante { get; }
        public DateTime DataAtualizacao { get; }

        public InscricaoTerminadaEventArgs(int idInscricao, int idEvento, string emailParticipante, DateTime dataAtualizacao) {
            IdInscricao = idInscricao;
            IdEvento = idEvento;
            EmailParticipante = emailParticipante;
            DataAtualizacao = dataAtualizacao;
        }
    }
}