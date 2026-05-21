using System;

namespace GestorEventos.Eventos {
    class EventoCriadoEventArgs : EventArgs {
        public int IdEvento { get; }
        public string NomeEvento { get; }
        public string LocalEvento { get; }
        public DateTime DataEvento { get; }
        public int Capacidade { get; }
        public DateTime DataCriacao { get; }

        public EventoCriadoEventArgs(
            int idEvento,
            string nomeEvento,
            string localEvento,
            DateTime dataEvento,
            int capacidade,
            DateTime dataCriacao) {
            IdEvento = idEvento;
            NomeEvento = nomeEvento;
            LocalEvento = localEvento;
            DataEvento = dataEvento;
            Capacidade = capacidade;
            DataCriacao = dataCriacao;
        }
    }
}