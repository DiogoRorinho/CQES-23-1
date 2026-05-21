using System;

namespace GestorEventos.Eventos {
    class EventoAlteradoEventArgs : EventArgs {
        public int IdEvento { get; }
        public string NomeEvento { get; }
        public string LocalEvento { get; }
        public DateTime DataEvento { get; }
        public int Capacidade { get; }
        public DateTime DataAlteracao { get; }

        public EventoAlteradoEventArgs(
            int idEvento,
            string nomeEvento,
            string localEvento,
            DateTime dataEvento,
            int capacidade,
            DateTime dataAlteracao) {
            IdEvento = idEvento;
            NomeEvento = nomeEvento;
            LocalEvento = localEvento;
            DataEvento = dataEvento;
            Capacidade = capacidade;
            DataAlteracao = dataAlteracao;
        }
    }
}