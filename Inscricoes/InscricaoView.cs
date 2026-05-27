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

        // Exibe a lista de eventos com a disponibilidade calculada pelo Model
        public void MostrarListaEventos(List<EventoDisponivel> listaEventosComDisponibilidade)
        {
            Console.WriteLine();
            Console.WriteLine("Eventos:");

            if (listaEventosComDisponibilidade == null || listaEventosComDisponibilidade.Count == 0)
            {
                Console.WriteLine("Nao existem eventos registados.");
                return;
            }

            const string formatoTabela = "{0,-5} {1,-30} {2,-12} {3,-20} {4,10} {5,15} {6,-12}";
            Console.WriteLine(string.Format(
                formatoTabela,
                "ID",
                "Nome",
                "Data",
                "Local",
                "Capacidade",
                "Disponibilidade",
                "Estado"));
            Console.WriteLine(new string('-', 112));

            foreach (EventoDisponivel evento in listaEventosComDisponibilidade)
            {
                string linha = string.Format(
                    formatoTabela,
                    evento.Id,
                    LimitarTexto(evento.Nome, 30),
                    FormatarData(evento.Data),
                    LimitarTexto(evento.Local, 20),
                    evento.Capacidade,
                    evento.Disponibilidade,
                    FormatarEstado(evento.Estado));

                EscreverLinhaComCor(linha, !EstadoAtivo(evento.Estado));
            }
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

        // Exibe uma mensagem generica
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

        // Exibe a lista de inscricoes em formato de tabela
        public void MostrarListaInscricoes(List<Inscricao> listaInscricoes)
        {
            Console.WriteLine();
            Console.WriteLine("Lista de inscricoes:");

            if (listaInscricoes == null || listaInscricoes.Count == 0)
            {
                Console.WriteLine("Nao existem inscricoes registadas.");
                return;
            }

            const string formatoTabela = "{0,-5} {1,-8} {2,-24} {3,-30} {4,6} {5,6} {6,-18}";
            Console.WriteLine(string.Format(
                formatoTabela,
                "ID",
                "Evento",
                "Nome",
                "Email",
                "Idade",
                "Qtd",
                "Estado"));
            Console.WriteLine(new string('-', 105));

            foreach (Inscricao inscricao in listaInscricoes)
            {
                string prefixo = string.Format(
                    "{0,-5} {1,-8} {2,-24} {3,-30} {4,6} {5,6} ",
                    inscricao.Id,
                    inscricao.IdEvento,
                    LimitarTexto(inscricao.NomeParticipante, 24),
                    LimitarTexto(inscricao.EmailParticipante, 30),
                    inscricao.IdadeParticipante,
                    inscricao.Quantidade);

                Console.Write(prefixo);
                EscreverEstado(FormatarEstado(inscricao.Estado), EstadoAtivo(inscricao.Estado));
                Console.WriteLine();
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
            Console.WriteLine(string.Format("Estado: {0}", FormatarEstado(dadosInscricao.Estado)));
        }

        // Solicita a confirmacao do cancelamento de uma inscricao
        public void PedirConfirmacaoCancelamento()
        {
            Console.Write("Confirma o cancelamento da inscricao? (s/n): ");
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

        private void EscreverLinhaComCor(string linha, bool vermelho)
        {
            ConsoleColor corOriginal = Console.ForegroundColor;

            if (vermelho)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine(linha);
            Console.ForegroundColor = corOriginal;
        }

        private void EscreverEstado(string estado, bool ativo)
        {
            ConsoleColor corOriginal = Console.ForegroundColor;

            if (!ativo)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.Write(estado);
            Console.ForegroundColor = corOriginal;
        }

        private bool EstadoAtivo(string estado)
        {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            return estadoNormalizado == "ativo" || estadoNormalizado == "ativa";
        }

        private string FormatarEstado(string estado)
        {
            string estadoNormalizado = (estado ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(estadoNormalizado))
            {
                return string.Empty;
            }

            estadoNormalizado = estadoNormalizado.Replace('_', ' ').ToLowerInvariant();
            return char.ToUpperInvariant(estadoNormalizado[0]) + estadoNormalizado.Substring(1);
        }

        private string FormatarData(DateTime data)
        {
            if (data == DateTime.MinValue)
            {
                return string.Empty;
            }

            return data.ToString("dd/MM/yyyy");
        }

        private string LimitarTexto(string texto, int tamanhoMaximo)
        {
            string textoNormalizado = texto ?? string.Empty;

            if (textoNormalizado.Length <= tamanhoMaximo)
            {
                return textoNormalizado;
            }

            if (tamanhoMaximo <= 3)
            {
                return textoNormalizado.Substring(0, tamanhoMaximo);
            }

            return textoNormalizado.Substring(0, tamanhoMaximo - 3) + "...";
        }
    }
}
