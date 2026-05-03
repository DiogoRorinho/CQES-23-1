using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class EventoView {
        public void MostrarMenuEventos() {
            Console.WriteLine();
            Console.WriteLine("=== Menu eventos ===");
            Console.WriteLine("1 - Criar evento (Em desenvolvimento)");
            Console.WriteLine("2 - Alterar evento (Em desenvolvimento)");
            Console.WriteLine("3 - Cancelar evento (Em desenvolvimento)");
            Console.WriteLine("4 - Listar eventos (Em desenvolvimento)");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Escolha uma opcao: ");
        }

        public string LerEntrada() {                        // Este método deve passar ao Controller (input)
            return Console.ReadLine() ?? string.Empty;
        }

        public void SolicitarDadosCriacao() {
            Console.WriteLine();
            Console.WriteLine("Formulario de criacao de evento.");
        }

        public void SolicitarNome() {
            Console.Write("Nome: ");
        }

        public void SolicitarLocal() {
            Console.Write("Local: ");
        }

        public void SolicitarData() {
            Console.Write("Data (dd/MM/yyyy): ");
        }

        public void SolicitarCapacidade() {
            Console.Write("Capacidade: ");
        }

        public void SolicitarIdEventoAlteracao() {
            Console.Write("Indique o ID do evento a alterar: ");
        }

        public void SolicitarIdEventoCancelamento() {
            Console.Write("Indique o ID do evento a cancelar: ");
        }

        public void PedirConfirmacaoCancelamento() {
            Console.Write("Confirma o cancelamento do evento? (s/n): ");
        }

        public void MostrarListaEventos(List<Evento> listaEventos) {
            Console.WriteLine();
            Console.WriteLine("Lista de eventos:");

            if (listaEventos == null || listaEventos.Count == 0) {
                Console.WriteLine("Nao existem eventos registados.");
                return;
            }

            foreach (Evento evento in listaEventos) {
                Console.WriteLine(string.Format(
                    "{0} - {1} | {2:dd/MM/yyyy} | {3} | capacidade: {4}",
                    evento.Id,
                    evento.Nome,
                    evento.Data,
                    evento.Local,
                    evento.Capacidade));
            }
        }

        public void MostrarDadosParaEdicao(Evento dadosEvento) {
            Console.WriteLine();
            Console.WriteLine(string.Format("Evento selecionado: {0}", dadosEvento.Nome));
        }

        public void MostrarResultadoOperacao(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarErroMenu(string mensagem) {
            Console.WriteLine(string.Format("Erro no menu de eventos: {0}", mensagem));
        }

        public void FinalizarOperacaoMenu() {
            Console.WriteLine();
        }
    }
}
