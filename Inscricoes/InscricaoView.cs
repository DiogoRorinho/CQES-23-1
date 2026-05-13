using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Inscricoes
{
    class InscricaoView
    {
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

        public void SolicitarIdEvento()
        {
            Console.Write("Indique o ID do evento: ");
        }

        public void SolicitarCampoTexto(string pedido)
        {
            Console.Write(pedido);
        }

        public void SolicitarDadosCriacao()
        {
            Console.WriteLine();
            Console.WriteLine("Criacao de inscricao.");
        }

        public void SolicitarDadosEdicao()
        {
            Console.WriteLine();
            Console.WriteLine("Alteracao de inscricao.");
            Console.WriteLine("Prima Enter para manter o valor atual.");
        }

        public void SolicitarIdInscricaoAlteracao()
        {
            Console.Write("Indique o ID da inscricao a alterar: ");
        }

        public void SolicitarIdInscricaoCancelamento()
        {
            Console.Write("Indique o ID da inscricao a cancelar: ");
        }

        public void MostrarMensagem(string mensagem)
        {
            Console.WriteLine(mensagem);
        }

        public void MostrarResultadoOperacaoEBilhete(string mensagem, DocumentoPdf bilhetePdf)
        {
            Console.WriteLine(mensagem);
            Console.WriteLine(string.Format("Documento gerado: {0}", bilhetePdf.NomeFicheiro));
            Console.WriteLine(string.Format("Caminho: {0}", bilhetePdf.CaminhoFicheiro));
        }

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

        public void PedirConfirmacaoCancelamento()
        {
            Console.Write("Confirma o cancelamento da inscricao? (s/n): ");
        }

        public void MostrarResultadoOperacao(string mensagem)
        {
            Console.WriteLine(mensagem);
        }

        public void MostrarErroSemVagas()
        {
            Console.WriteLine("Nao existem vagas suficientes ou os dados introduzidos sao invalidos.");
        }

        public void MostrarErroMenu(string mensagem)
        {
            Console.WriteLine(string.Format("Erro no menu de inscricoes: {0}", mensagem));
        }

        public void FinalizarOperacaoMenu()
        {
            Console.WriteLine();
        }
    }
}
