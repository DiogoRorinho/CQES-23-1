using System;
using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Eventos {
    class EventoView {
        // O método MostrarMenuEventos apresenta as opções disponíveis para o usuário no módulo de eventos.
        public void MostrarMenuEventos() {
            Console.WriteLine();
            Console.WriteLine("===== Menu Eventos =====");
            Console.WriteLine("1 - Criar evento (A desenvolver)");
            Console.WriteLine("2 - Alterar evento (A desenvolver)");
            Console.WriteLine("3 - Cancelar evento (A desenvolver)");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Opcao: ");
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
