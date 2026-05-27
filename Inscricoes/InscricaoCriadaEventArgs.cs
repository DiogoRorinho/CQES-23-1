using System;

namespace GestorEventos.Inscricoes {
    // Transporta os dados relevantes do domínio quando uma inscrição é criada.
    class InscricaoCriadaEventArgs : EventArgs {
        public int IdInscricao { get; }
        public int IdEvento { get; }
        public string NomeParticipante { get; }
        public string EmailParticipante { get; }
        public int Quantidade { get; }
        public DateTime DataCriacao { get; }

        public InscricaoCriadaEventArgs(
            int idInscricao,
            int idEvento,
            string nomeParticipante,
            string emailParticipante,
            int quantidade,
            DateTime dataCriacao) {
            IdInscricao = idInscricao;
            IdEvento = idEvento;
            NomeParticipante = nomeParticipante;
            EmailParticipante = emailParticipante;
            Quantidade = quantidade;
            DataCriacao = dataCriacao;
        }
    }
}