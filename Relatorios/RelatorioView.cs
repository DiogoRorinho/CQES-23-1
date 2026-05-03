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

        public void SolicitarIdEvento() {
            Console.Write("Indique o ID do evento: ");
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
