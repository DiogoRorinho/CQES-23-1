using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Relatorios {
    class RelatorioView {
        public void MostrarMenuRelatorios() {
            Console.WriteLine();
            Console.WriteLine("=== Menu relatorios ===");
            Console.WriteLine("1 - Listagem de inscritos por evento");
            Console.WriteLine("2 - Eventos com ocupacao");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Escolha uma opcao: ");
        }

        public void MostrarListaEventos(List<Evento> listaEventos) {
            Console.WriteLine();
            Console.WriteLine("Lista de eventos:");
            Console.WriteLine(string.Format(
                "{0,-5} {1,-30} {2,-12} {3,-20} {4,10}",
                "ID",
                "Nome",
                "Data",
                "Local",
                "Capacidade"));
            Console.WriteLine(new string('-', 83));

            foreach (Evento evento in listaEventos) {
                Console.WriteLine(string.Format(
                    "{0,-5} {1,-30} {2,-12:dd/MM/yyyy} {3,-20} {4,10}",
                    evento.Id,
                    evento.Nome,
                    evento.Data,
                    evento.Local,
                    evento.Capacidade));
            }
        }

        public void SolicitarIdEvento() {
            Console.Write("Indique o ID do evento (0 para sair): ");
        }

        public void ApresentarRelatorioEPdf(DadosRelatorio dadosRelatorio, DocumentoPdf relatorioPdf) {
            Console.WriteLine();
            Console.WriteLine(string.Format("Relatorio: {0}", dadosRelatorio.Titulo));
            Console.WriteLine(dadosRelatorio.Conteudo);
            Console.WriteLine(string.Format("PDF gerado: {0}", relatorioPdf.NomeFicheiro));
            Console.WriteLine(string.Format("Caminho: {0}", relatorioPdf.CaminhoFicheiro));
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarErroMenu(string mensagem) {
            Console.WriteLine(string.Format("Erro no menu de relatorios: {0}", mensagem));
        }

        public void FinalizarOperacaoMenu() {
            Console.WriteLine();
        }
    }
}
