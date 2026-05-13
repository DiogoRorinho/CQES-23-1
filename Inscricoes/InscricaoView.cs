using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Inscricoes
{
 
    class InscricaoView
    {
        // Exibe o menu de inscricoes e solicita a opcao do utilizador
        public void MostrarMenuInscricoes()
        {
            Console.WriteLine();
            Console.WriteLine("===== Menu Inscricoes =====");
            Console.WriteLine("1 - Criar inscricao");
            Console.WriteLine("2 - Alterar inscricao");
            Console.WriteLine("3 - Cancelar inscricao");
            Console.WriteLine("4 - Listar inscricoes");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Opcao: ");
        }

        // Exibe a lista de eventos com vagas disponiveis
        public void MostrarListaEventos(List<Evento> listaEventosComVagas)
        {
            Console.WriteLine();
            Console.WriteLine("Eventos com vagas disponiveis:");

            if (listaEventosComVagas == null || listaEventosComVagas.Count == 0)
            {
                Console.WriteLine("Nao existem eventos com vagas disponiveis.");
                return;
            }

            foreach (Evento evento in listaEventosComVagas)
            {
                Console.WriteLine(string.Format(
                    "{0} - {1} | {2:dd/MM/yyyy} | {3} | capacidade: {4}",
                    evento.Id,
                    evento.Nome,
                    evento.Data,
                    evento.Local,
                    evento.Capacidade));
            }
        }

        // Solicita o ID do evento para criar ou alterar uma inscricao
        public void SolicitarIdEvento()
        {
            Console.Write("Indique o ID do evento ou [0 para cancelar]: ");
        }

        // Solicita um campo de texto (nome, email, etc.) com base no pedido fornecido
        public void SolicitarCampoTexto(string pedido)
        {
            Console.Write(pedido);
        }

        public void SolicitarDadosCriacao()
        {
            Console.WriteLine();
            Console.WriteLine("Criacao de inscricao.");
        }

        // Solicita os dados para edicao de uma inscricao, indicando que o utilizador pode manter o valor atual
        public void SolicitarDadosEdicao()
        {
            Console.WriteLine();
            Console.WriteLine("Alteracao de inscricao.");
            Console.WriteLine("Prima Enter para manter o valor atual.");
        }

        // Solicita o ID da inscricao para alterar ou cancelar
        public void SolicitarIdInscricaoAlteracao()
        {
            Console.Write("Indique o ID da inscricao a alterar: ");
        }

        // Solicita o ID da inscricao para cancelar
        public void SolicitarIdInscricaoCancelamento()
        {
            Console.Write("Indique o ID da inscricao a cancelar: ");
        }

        // Exibe uma mensagem genérica
        public void MostrarMensagem(string mensagem)
        {
            Console.WriteLine(mensagem);
        }

        // Exibe o resultado de uma operacao que gera um bilhete PDF, mostrando a mensagem e os detalhes do documento gerado
        public void MostrarResultadoOperacaoEBilhete(string mensagem, DocumentoPdf bilhetePdf)
        {
            Console.WriteLine(mensagem);
            Console.WriteLine(string.Format("Documento gerado: {0}", bilhetePdf.NomeFicheiro));
            Console.WriteLine(string.Format("Caminho: {0}", bilhetePdf.CaminhoFicheiro));
        }

        // Exibe a lista de inscricoes, mostrando os detalhes de cada inscricao
        public void MostrarListaInscricoes(List<Inscricao> listaInscricoes)
        {
            Console.WriteLine();
            Console.WriteLine("Lista de inscricoes:");

            if (listaInscricoes == null || listaInscricoes.Count == 0)
            {
                Console.WriteLine("Nao existem inscricoes registadas.");
                return;
            }

            foreach (Inscricao inscricao in listaInscricoes)
            {
                Console.WriteLine(string.Format(
                    "{0} - Evento {1} | {2} | {3} | idade: {4} | qtd: {5} | estado: {6}",
                    inscricao.Id,
                    inscricao.IdEvento,
                    inscricao.NomeParticipante,
                    inscricao.EmailParticipante,
                    inscricao.IdadeParticipante,
                    inscricao.Quantidade,
                    inscricao.Estado));
            }
        }

        // Exibe os detalhes de uma inscricao para edicao, mostrando os dados atuais da inscricao
        public void MostrarDadosParaEdicao(Inscricao dadosInscricao)
        {
            Console.WriteLine();
            Console.WriteLine(string.Format("Inscricao selecionada: {0}", dadosInscricao.Id));
            Console.WriteLine(string.Format("Evento: {0}", dadosInscricao.IdEvento));
            Console.WriteLine(string.Format("Nome: {0}", dadosInscricao.NomeParticipante));
            Console.WriteLine(string.Format("Email: {0}", dadosInscricao.EmailParticipante));
            Console.WriteLine(string.Format("Idade: {0}", dadosInscricao.IdadeParticipante));
            Console.WriteLine(string.Format("Quantidade: {0}", dadosInscricao.Quantidade));
            Console.WriteLine(string.Format("Estado: {0}", dadosInscricao.Estado));
        }

        // Solicita a confirmacao do cancelamento de uma inscricao
        public void PedirConfirmacaoCancelamento()
        {
            Console.Write("Confirma o cancelamento da inscricao? (s/n): ");
        }

        // Exibe o resultado de uma operacao generica, mostrando a mensagem fornecida
        public void MostrarResultadoOperacao(string mensagem)
        {
            Console.WriteLine(mensagem);
        }

        // Exibe uma mensagem de erro indicando que nao existem vagas suficientes ou que os dados introduzidos sao invalidos
        public void MostrarErroSemVagas()
        {
            Console.WriteLine("Nao existem vagas suficientes ou os dados introduzidos sao invalidos.");
        }

        // Exibe uma mensagem de erro indicando que a inscricao selecionada nao existe ou que a operacao nao pode ser realizada
        public void MostrarErroMenu(string mensagem)
        {
            Console.WriteLine(string.Format("Erro no menu de inscricoes: {0}", mensagem));
        }

        // Exibe uma mensagem de encerramento da operacao, indicando que o utilizador sera redirecionado para o menu principal
        public void FinalizarOperacaoMenu()
        {
            Console.WriteLine();
        }
    }
}
