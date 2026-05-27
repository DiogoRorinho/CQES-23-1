using System;

namespace GestorEventos.Eventos {
    // Transporta os dados relevantes do domínio quando um evento é cancelado.
    class EventoCanceladoEventArgs : EventArgs {
        public int IdEvento { get; private set; }
        public string NomeEvento { get; private set; }
        public DateTime DataCancelamento { get; private set; }
        public string Estado { get; private set; }

        public EventoCanceladoEventArgs(int idEvento, string nomeEvento, DateTime dataCancelamento, string estado) {
            IdEvento = idEvento;
            NomeEvento = nomeEvento;
            DataCancelamento = dataCancelamento;
            Estado = estado;
        }
    }
}
