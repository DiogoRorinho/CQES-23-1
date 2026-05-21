using System;

namespace GestorEventos.Inscricoes {
    class InscricaoCanceladaEventArgs : EventArgs {
        public int IdInscricao { get; }
        public string EstadoFinal { get; }
        public DateTime DataCancelamento { get; }

        public InscricaoCanceladaEventArgs(int idInscricao, string estadoFinal, DateTime dataCancelamento) {
            IdInscricao = idInscricao;
            EstadoFinal = estadoFinal;
            DataCancelamento = dataCancelamento;
        }
    }
}