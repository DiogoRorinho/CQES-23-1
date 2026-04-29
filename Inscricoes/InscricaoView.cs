using System;
using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Inscricoes {
    class InscricaoView {
        // O método MostrarMenuInscricoes apresenta as opções disponíveis para o usuário no módulo de inscrições.
        public void MostrarMenuInscricoes() {
            Console.WriteLine();
            Console.WriteLine("===== Menu Inscricoes =====");
            Console.WriteLine("1 - Criar inscricao");
            Console.WriteLine("2 - Alterar inscricao");
            Console.WriteLine("3 - Cancelar inscricao");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Opcao: ");
        }
        
        // O método MostrarListaEventos exibe a lista de eventos com vagas disponíveis.
        public void MostrarListaEventos(List<Evento> listaEventosComVagas) {
            Console.WriteLine("Eventos com vagas disponiveis:");
            foreach (Evento evento in listaEventosComVagas) {
                Console.WriteLine(string.Format("{0} - {1}", evento.Id, evento.Nome));
            }
        }

        // O método SolicitarIdEvento solicita ao usuário que informe o ID do evento para o qual deseja criar ou alterar uma inscrição.
        public void SolicitarIdEvento()
        {
            Console.Write("Indique o ID do evento: ");
        }

        public void SolicitarCampoTexto(string pedido)
        {
            Console.Write(pedido);
        }

        // O método SolicitarDadosCriacao pode ser expandido para solicitar os dados necessários para criar uma inscrição, como o ID do evento e a quantidade de vagas.
        public void SolicitarDadosCriacao() {
            Console.WriteLine("Formulario de criacao de inscricao.");
        }

        // O método MostrarListaInscricoes exibe a lista de inscrições existentes, incluindo o ID da inscrição, o ID do evento associado e o estado da inscrição.
        public void MostrarListaInscricoes(List<Inscricao> listaInscricoes) {
            Console.WriteLine("Lista de inscricoes:");
            foreach (Inscricao inscricao in listaInscricoes) {
                Console.WriteLine(string.Format("{0} - Evento {1} - {2}", inscricao.Id, inscricao.IdEvento, inscricao.Estado));
            }
        }

        // O método MostrarDadosParaEdicao exibe os dados de uma inscrição selecionada para edição, permitindo que o usuário veja as informações atuais antes de fazer alterações.
        public void MostrarDadosParaEdicao(Inscricao dadosInscricao) {
            Console.WriteLine(string.Format("Inscricao selecionada: {0}", dadosInscricao.Id));
        }

        // O método SolicitarDadosEdicao pode ser expandido para solicitar os dados necessários para editar uma inscrição, como o novo estado ou a quantidade de vagas.
        public void PedirConfirmacaoCancelamento() {
            Console.WriteLine("Confirma o cancelamento da inscricao?");
        }

        // O método MostrarResultadoOperacaoEBilhete exibe uma mensagem de resultado da operação e o nome do arquivo do bilhete gerado em formato PDF.
        public void MostrarResultadoOperacaoEBilhete(string mensagem, DocumentoPdf bilhetePdf) {
            Console.WriteLine(mensagem);
            Console.WriteLine(string.Format("Bilhete gerado: {0}", bilhetePdf.NomeFicheiro));
        }


        // O método MostrarResultadoOperacao exibe uma mensagem de resultado da operação realizada, como sucesso ou falha.
        public void MostrarResultadoOperacao(string mensagem) {
            Console.WriteLine(mensagem);
        }

        // O método MostrarErroMenu exibe uma mensagem de erro relacionada ao menu, como uma opção inválida ou um erro de entrada.
        public void MostrarErroSemVagas() {
            Console.WriteLine("Nao existem vagas suficientes.");
        }

        // O método MostrarMensagem é um método genérico para exibir qualquer mensagem que o Controller queira comunicar ao usuário.
        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }
    }
}
