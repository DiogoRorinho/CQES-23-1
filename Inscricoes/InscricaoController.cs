using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;
using System;
using System.Collections.Generic;

namespace GestorEventos.Inscricoes
{
    class InscricaoController
    {
        private readonly AplicacaoController aplicacaoController;
        private readonly InscricaoView view;
        private readonly InscricaoModel model;
        private bool regressarMenuPrincipal;

        
        public InscricaoController(AplicacaoController aplicacaoController, InscricaoView view, InscricaoModel model)
        {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        // Metodo principal para mostrar o menu de inscricoes e processar as opcoes selecionadas
        public void MostrarMenuModulo()
        {
            regressarMenuPrincipal = false;

            while (!regressarMenuPrincipal)
            {
                try
                {
                    view.MostrarMenuInscricoes();
                    regressarMenuPrincipal = SelecionarOpcao(LerEntrada());
                }
                catch (Exception ex)
                {
                    view.MostrarErroMenu(ex.Message);
                }
                finally
                {
                    view.FinalizarOperacaoMenu();
                }
            }
        }
        
        public bool SelecionarOpcao(string opcao)
        {
            switch (NormalizarOpcao(opcao))
            {
                case "1":
                    CriarInscricao();
                    return false;

                case "2":
                    AlterarInscricao();
                    return false;

                case "3":
                    CancelarInscricao();
                    return false;

                case "4":
                    ListarInscricoes();
                    return false;

                case "0":
                    RegressarMenuPrincipal();
                    return true;

                default:
                    view.MostrarMensagem("Opcao de inscricoes invalida. Escolha 1, 2, 3, 4 ou 0 para regressar ao menu principal.");
                    return false;
            }
        }

        // Metodo para criar uma nova inscricao, solicitando os dados necessarios e validando as entradas
        private void CriarInscricao()
        {
            List<Evento> eventosDisponiveis = model.ListarEventosDisponiveis();

            if (eventosDisponiveis == null || eventosDisponiveis.Count == 0)
            {
                view.MostrarMensagem("Nao existem eventos com vagas disponiveis.");
                return;
            }

            view.MostrarListaEventos(eventosDisponiveis);
            view.SolicitarDadosCriacao();

            int idEvento = LerIdEventoValido(eventosDisponiveis);

            if (idEvento == 0)
            {
                return;
            }

            DadosInscricao dados = new DadosInscricao
            {
                IdEvento = LerIdEventoValido(eventosDisponiveis),
                NomeParticipante = LerTextoNaoVazio("Nome do inscrito: "),
                EmailParticipante = LerTextoNaoVazio("Email do inscrito: "),
                IdadeParticipante = LerInteiroPositivo("Idade do inscrito: "),
                Quantidade = LerInteiroPositivo("Numero de inscricoes pretendido: ")
            };

            ResultadoCriacaoInscricao resultado = model.CriarInscricao(dados);

            if (resultado.Sucesso && resultado.BilhetePdf != null)
            {
                view.MostrarResultadoOperacaoEBilhete(resultado.Mensagem, resultado.BilhetePdf);
                return;
            }

            view.MostrarMensagem(resultado.Mensagem);
        }

        // Metodo para alterar uma inscricao existente, permitindo ao utilizador escolher a inscricao a alterar e os novos dados
        private void AlterarInscricao()
        {
            List<Inscricao> inscricoes = model.ListarInscricoes();
            view.MostrarListaInscricoes(inscricoes);

            if (inscricoes == null || inscricoes.Count == 0)
            {
                return;
            }

            view.SolicitarIdInscricaoAlteracao();
            int idInscricao = LerInteiroPositivoDaEntrada();

            Inscricao? inscricao = model.ObterInscricao(idInscricao);
            if (inscricao == null || inscricao.Id <= 0)
            {
                view.MostrarMensagem("Inscricao nao encontrada.");
                return;
            }

            if (!string.Equals(inscricao.Estado, "ativa", StringComparison.OrdinalIgnoreCase))
            {
                view.MostrarMensagem("Apenas inscricoes ativas podem ser alteradas.");
                return;
            }

            view.MostrarDadosParaEdicao(inscricao);
            view.MostrarListaEventos(model.ListarEventosDisponiveis());
            view.SolicitarDadosEdicao();

            DadosInscricao dados = RecolherDadosAlteracaoInscricao(inscricao);
            IntroduzirDadosAlterados(idInscricao, dados);
        }

        // Metodo para cancelar uma inscricao existente, permitindo ao utilizador escolher a inscricao a cancelar e confirmando a operacao
        private void CancelarInscricao()
        {
            List<Inscricao> inscricoes = model.ListarInscricoes();
            view.MostrarListaInscricoes(inscricoes);

            if (inscricoes == null || inscricoes.Count == 0)
            {
                return;
            }

            view.SolicitarIdInscricaoCancelamento();
            int idInscricao = LerInteiroPositivoDaEntrada();

            Inscricao? inscricao = model.ObterInscricao(idInscricao);
            if (inscricao == null || inscricao.Id <= 0)
            {
                view.MostrarMensagem("Inscricao nao encontrada.");
                return;
            }

            if (!string.Equals(inscricao.Estado, "ativa", StringComparison.OrdinalIgnoreCase))
            {
                view.MostrarMensagem("A inscricao selecionada ja nao se encontra ativa.");
                return;
            }

            view.MostrarDadosParaEdicao(inscricao);
            PedirConfirmacaoCancelamento();

            string confirmacao = NormalizarOpcao(LerEntrada());
            if (confirmacao != "s" && confirmacao != "sim")
            {
                view.MostrarMensagem("Cancelamento interrompido.");
                return;
            }

            ConfirmarCancelamento(idInscricao);
        }

        private void ListarInscricoes()
        {
            view.MostrarListaInscricoes(model.ListarInscricoes());
        }

        // Metodo auxiliar para recolher os dados de alteracao de uma inscricao, permitindo ao utilizador manter os valores atuais ou introduzir novos valores
        private DadosInscricao RecolherDadosAlteracaoInscricao(Inscricao inscricao)
        {
            return new DadosInscricao
            {
                IdEvento = LerInteiroPositivoAlteravel(
                    string.Format("ID do evento [{0}]: ", inscricao.IdEvento),
                    inscricao.IdEvento),
                NomeParticipante = LerTextoAlteravel(
                    string.Format("Nome do inscrito [{0}]: ", inscricao.NomeParticipante),
                    inscricao.NomeParticipante),
                EmailParticipante = LerTextoAlteravel(
                    string.Format("Email do inscrito [{0}]: ", inscricao.EmailParticipante),
                    inscricao.EmailParticipante),
                IdadeParticipante = LerInteiroPositivoAlteravel(
                    string.Format("Idade do inscrito [{0}]: ", inscricao.IdadeParticipante),
                    inscricao.IdadeParticipante),
                Quantidade = LerInteiroPositivoAlteravel(
                    string.Format("Numero de inscricoes pretendido [{0}]: ", inscricao.Quantidade),
                    inscricao.Quantidade)
            };
        }

        // Metodo auxiliar para ler um ID de evento valido, verificando se o ID introduzido corresponde a um evento disponivel
        private int LerIdEventoValido(List<Evento> eventosDisponiveis)
        {
            while (true)
            {
                view.SolicitarIdEvento();
                string entrada = LerEntrada();

                if (!int.TryParse(entrada, out int idEvento) || idEvento < 0)
                {
                    view.MostrarMensagem("ID de evento invalido.");
                    continue;
                }

                if (idEvento == 0)
                {
                    view.MostrarMensagem("Criacao de inscricao cancelada.");
                    return 0;
                }

                foreach (Evento evento in eventosDisponiveis)
                {
                    if (evento.Id == idEvento)
                    {
                        return idEvento;
                    }
                }

                view.MostrarMensagem("O ID indicado nao corresponde a um evento disponivel.");
            }
        }

        // Metodo auxiliar para ler um texto nao vazio, solicitando ao utilizador que introduza um valor e validando que o valor nao esta vazio ou composto apenas por espacos
        private string LerTextoNaoVazio(string pedido)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);
                string valor = LerEntrada();

                if (!string.IsNullOrWhiteSpace(valor))
                {
                    return valor.Trim();
                }

                view.MostrarMensagem("O valor introduzido nao pode estar vazio.");
            }
        }

        // Metodo auxiliar para ler um texto alteravel, permitindo ao utilizador manter o valor atual ou introduzir um novo valor, validando que o valor introduzido nao esta vazio ou composto apenas por espacos
        private string LerTextoAlteravel(string pedido, string valorAtual)
        {
            view.SolicitarCampoTexto(pedido);
            string valor = LerEntrada();

            if (string.IsNullOrWhiteSpace(valor))
            {
                return valorAtual;
            }

            return valor.Trim();
        }

        // Metodo auxiliar para ler um numero inteiro positivo, solicitando ao utilizador que introduza um valor e validando que o valor e um numero inteiro positivo
        private int LerInteiroPositivo(string pedido)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);

                if (int.TryParse(LerEntrada(), out int valor) && valor > 0)
                {
                    return valor;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        // Metodo auxiliar para ler um numero inteiro positivo alteravel, permitindo ao utilizador manter o valor atual ou introduzir um novo valor, validando que o valor introduzido e um numero inteiro positivo
        private int LerInteiroPositivoAlteravel(string pedido, int valorAtual)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);
                string entrada = LerEntrada();

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    return valorAtual;
                }

                if (int.TryParse(entrada, out int valor) && valor > 0)
                {
                    return valor;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        // Metodo auxiliar para ler um numero inteiro positivo da entrada, validando que o valor introduzido e um numero inteiro positivo, retornando 0 em caso de valor invalido
        private int LerInteiroPositivoDaEntrada()
        {
            string entrada = LerEntrada();

            if (!int.TryParse(entrada, out int valor) || valor <= 0)
            {
                return 0;
            }

            return valor;
        }

        private string LerEntrada()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        // Metodo auxiliar para normalizar a opcao introduzida pelo utilizador, removendo espacos em branco e convertendo para minusculas, facilitando a comparacao das opcoes
        private string NormalizarOpcao(string? opcao)
        {
            if (string.IsNullOrWhiteSpace(opcao))
            {
                return string.Empty;
            }

            return opcao.Trim().ToLowerInvariant();
        }

        // Metodo para regressar ao menu principal, sinalizando a intencao de regressar e permitindo que o loop principal do menu de inscricoes seja interrompido
        public void RegressarMenuPrincipal()
        {
            aplicacaoController.RegressarMenuPrincipal();
        }

        // Metodo para selecionar uma inscricao existente, verificando se a inscricao existe e esta ativa, e mostrando os dados da inscricao para edicao
        public void SelecionarInscricao(int idInscricao)
        {
            Inscricao? inscricao = model.ObterInscricao(idInscricao);

            if (inscricao == null || inscricao.Id <= 0)
            {
                view.MostrarMensagem("Inscricao nao encontrada.");
                return;
            }

            view.MostrarDadosParaEdicao(inscricao);
        }

        // Metodo para introduzir os dados alterados de uma inscricao, validando os dados e realizando a alteracao da inscricao, mostrando o resultado da operacao e o bilhete atualizado em caso de sucesso
        public void IntroduzirDadosAlterados(int idInscricao, DadosInscricao dados)
        {
            if (model.ValidarAlteracaoInscricao(idInscricao, dados))
            {
                DocumentoPdf bilhetePdf = model.AlterarInscricao(idInscricao, dados);
                view.MostrarResultadoOperacaoEBilhete("Inscricao alterada com sucesso.", bilhetePdf);
                return;
            }

            view.MostrarErroSemVagas();
        }

        // Metodo para pedir confirmacao de cancelamento de uma inscricao, solicitando ao utilizador que confirme a intencao de cancelar a inscricao selecionada
        public void PedirConfirmacaoCancelamento()
        {
            view.PedirConfirmacaoCancelamento();
        }

        // Metodo para confirmar o cancelamento de uma inscricao, realizando o cancelamento da inscricao selecionada e mostrando o resultado da operacao e o comprovativo de cancelamento em caso de sucesso
        public void ConfirmarCancelamento(int idInscricao)
        {
            model.CancelarInscricao(idInscricao);
            DocumentoPdf comprovativo = model.GerarComprovativoCancelamento(idInscricao);
            view.MostrarResultadoOperacaoEBilhete("Inscricao cancelada com sucesso.", comprovativo);
        }
    }
}
