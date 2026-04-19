using System;
using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Eventos {
    class EventoView {
        public void MostrarMenuEventos() {
            Console.WriteLine("Menu eventos: Criar evento | Alterar evento | Cancelar evento | Regressar ao menu principal");
        }

        public void SolicitarDadosCriacao() {
            Console.WriteLine("Formulario de criacao de evento.");
        }

        public void MostrarListaEventos(List<Evento> listaEventos) {
            Console.WriteLine("Lista de eventos disponiveis:");
            foreach (Evento evento in listaEventos) {
                Console.WriteLine(string.Format("{0} - {1} ({2})", evento.Id, evento.Nome, evento.Estado));
            }
        }

        public void MostrarDadosParaEdicao(Evento dadosEvento) {
            Console.WriteLine(string.Format("Evento selecionado: {0}", dadosEvento.Nome));
        }

        public void PedirConfirmacaoCancelamento() {
            Console.WriteLine("Confirma o cancelamento do evento?");
        }

        public void MostrarResultadoOperacao(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }
    }
}
