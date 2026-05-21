using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class EventoView {
        public void MostrarMenuEventos() {
            Console.WriteLine();
            Console.WriteLine("=== Menu eventos ===");
            Console.WriteLine("1 - Criar evento");
            Console.WriteLine("2 - Alterar evento");
            Console.WriteLine("3 - Cancelar evento");
            Console.WriteLine("4 - Listar eventos");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Escolha uma opcao: ");
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
            Console.Write("Introduza um ID valido ou 0 para sair: ");
        }

        public void SolicitarIdEventoCancelamento() {
            Console.Write("Introduza um ID valido ou 0 para sair: ");
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

            Console.WriteLine("ID  | Nome                 | Data       | Local                | Cap. | Estado");
            Console.WriteLine(new string('-', 80));

            foreach (Evento evento in listaEventos) {
                bool destacar = !string.Equals(evento.Estado, "ativo", StringComparison.OrdinalIgnoreCase);
                ConsoleColor corOriginal = Console.ForegroundColor;

                if (destacar) {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.WriteLine(string.Format(
                    "{0,-3} | {1,-20} | {2:dd/MM/yyyy} | {3,-20} | {4,4} | {5}",
                    evento.Id,
                    LimitarTexto(evento.Nome, 20),
                    evento.Data,
                    LimitarTexto(evento.Local, 20),
                    evento.Capacidade,
                    evento.Estado));

                if (destacar) {
                    Console.ForegroundColor = corOriginal;
                }
            }
        }

        public void MostrarDadosParaEdicao(Evento dadosEvento) {
            Console.WriteLine();
            Console.WriteLine(string.Format(
                "Evento selecionado: {0} - {1} | {2:dd/MM/yyyy} | {3} | capacidade: {4}",
                dadosEvento.Id,
                dadosEvento.Nome,
                dadosEvento.Data,
                dadosEvento.Local,
                dadosEvento.Capacidade));
            Console.WriteLine("Prima Enter para manter o valor atual.");
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

        private static string LimitarTexto(string texto, int limite) {
            if (string.IsNullOrWhiteSpace(texto)) {
                return string.Empty;
            }

            if (texto.Length <= limite) {
                return texto;
            }

            return texto.Substring(0, limite - 3) + "...";
        }
    }
}
