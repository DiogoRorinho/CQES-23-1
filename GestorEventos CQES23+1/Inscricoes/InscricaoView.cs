using System;
using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Inscricoes {
    class InscricaoView {
        public void MostrarMenuInscricoes() {
            Console.WriteLine("Menu inscricoes: Criar inscricao | Alterar inscricao | Cancelar inscricao | Regressar ao menu principal");
        }

        public void MostrarListaEventos(List<Evento> listaEventosComVagas) {
            Console.WriteLine("Eventos com vagas disponiveis:");
            foreach (Evento evento in listaEventosComVagas) {
                Console.WriteLine(string.Format("{0} - {1}", evento.Id, evento.Nome));
            }
        }

        public void SolicitarDadosCriacao() {
            Console.WriteLine("Formulario de criacao de inscricao.");
        }

        public void MostrarListaInscricoes(List<Inscricao> listaInscricoes) {
            Console.WriteLine("Lista de inscricoes:");
            foreach (Inscricao inscricao in listaInscricoes) {
                Console.WriteLine(string.Format("{0} - Evento {1} - {2}", inscricao.Id, inscricao.IdEvento, inscricao.Estado));
            }
        }

        public void MostrarDadosParaEdicao(Inscricao dadosInscricao) {
            Console.WriteLine(string.Format("Inscricao selecionada: {0}", dadosInscricao.Id));
        }

        public void PedirConfirmacaoCancelamento() {
            Console.WriteLine("Confirma o cancelamento da inscricao?");
        }

        public void MostrarResultadoOperacaoEBilhete(string mensagem, DocumentoPdf bilhetePdf) {
            Console.WriteLine(mensagem);
            Console.WriteLine(string.Format("Bilhete gerado: {0}", bilhetePdf.NomeFicheiro));
        }

        public void MostrarResultadoOperacao(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarErroSemVagas() {
            Console.WriteLine("Nao existem vagas suficientes.");
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }
    }
}
