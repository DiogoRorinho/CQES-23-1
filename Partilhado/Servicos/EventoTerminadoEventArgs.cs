using System;

namespace GestorEventos.Partilhado.Servicos {
    // Transporta os dados relevantes do domínio quando um evento passa automaticamente a terminado.
    public class EventoTerminadoEventArgs : EventArgs {
        public int IdEvento { get; }
        public string NomeEvento { get; }
        public DateTime DataEvento { get; }
        public DateTime DataAtualizacao { get; }

        public EventoTerminadoEventArgs(int idEvento, string nomeEvento, DateTime dataEvento, DateTime dataAtualizacao) {
            IdEvento = idEvento;
            NomeEvento = nomeEvento;
            DataEvento = dataEvento;
            DataAtualizacao = dataAtualizacao;
        }
    }
}