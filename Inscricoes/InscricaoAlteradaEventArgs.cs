using System;

namespace GestorEventos.Inscricoes {
    class InscricaoAlteradaEventArgs : EventArgs {
        public int IdInscricao { get; }
        public int IdEvento { get; }
        public string NomeParticipante { get; }
        public int Quantidade { get; }
        public DateTime DataAlteracao { get; }

        public InscricaoAlteradaEventArgs(
            int idInscricao,
            int idEvento,
            string nomeParticipante,
            int quantidade,
            DateTime dataAlteracao) {
            IdInscricao = idInscricao;
            IdEvento = idEvento;
            NomeParticipante = nomeParticipante;
            Quantidade = quantidade;
            DataAlteracao = dataAlteracao;
        }
    }
}