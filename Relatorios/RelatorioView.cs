using System;
using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Relatorios {
    class RelatorioView {
        public void MostrarMenuRelatorios() {
            Console.WriteLine("Menu relatorios: Listagem de inscritos por evento | Eventos com ocupacao | Regressar ao menu principal");
        }

        public void MostrarListaEventos(List<Evento> listaEventos) {
            Console.WriteLine("Lista de eventos:");
            foreach (Evento evento in listaEventos) {
                Console.WriteLine(string.Format("{0} - {1}", evento.Id, evento.Nome));
            }
        }

        public void ApresentarRelatorioEPdf(DadosRelatorio dadosRelatorio, DocumentoPdf relatorioPdf) {
            Console.WriteLine(string.Format("Relatorio: {0}", dadosRelatorio.Titulo));
            Console.WriteLine(string.Format("PDF gerado: {0}", relatorioPdf.NomeFicheiro));
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }
    }
}
