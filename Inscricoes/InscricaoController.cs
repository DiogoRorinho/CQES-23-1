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
            List<EventoDisponivel> eventosDisponiveis = model.ListarEventosDisponiveis();

            if (eventosDisponiveis == null || eventosDisponiveis.Count == 0)
            {
                view.MostrarMensagem("Nao existem eventos registados.");
                return;
            }

            view.MostrarListaEventos(eventosDisponiveis);

            if (!ExisteEventoDisponivelParaInscricao(eventosDisponiveis))
            {
                view.MostrarMensagem("Nao existem eventos ativos com disponibilidade para novas inscricoes.");
                return;
            }

            view.SolicitarDadosCriacao();

            int idEvento = LerIdEventoValido(eventosDisponiveis);

            if (idEvento == 0)
            {
                return;
            }

            DadosInscricao dados = new DadosInscricao
            {
                IdEvento = idEvento,
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
            List<Inscricao> inscricoes = model.ListarInscricoesAtivas();
            view.MostrarListaInscricoes(inscricoes);

            if (inscricoes == null || inscricoes.Count == 0)
            {
                view.MostrarMensagem("Nao existem inscricoes ativas para alterar.");
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

            List<EventoDisponivel> eventosDisponiveis = model.ListarEventosDisponiveis();

            view.MostrarDadosParaEdicao(inscricao);
            view.MostrarListaEventos(eventosDisponiveis);
            view.SolicitarDadosEdicao();

            DadosInscricao dados = RecolherDadosAlteracaoInscricao(inscricao, eventosDisponiveis);
            IntroduzirDadosAlterados(idInscricao, dados);
        }

        // Metodo para cancelar uma inscricao existente, permitindo ao utilizador escolher a inscricao a cancelar e confirmando a operacao
        private void CancelarInscricao()
        {
            List<Inscricao> inscricoes = model.ListarInscricoesAtivas();
            view.MostrarListaInscricoes(inscricoes);

            if (inscricoes == null || inscricoes.Count == 0)
            {
                view.MostrarMensagem("Nao existem inscricoes ativas para cancelar.");
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
        private DadosInscricao RecolherDadosAlteracaoInscricao(Inscricao inscricao, List<EventoDisponivel> eventosDisponiveis)
        {
            return new DadosInscricao
            {
                IdEvento = LerIdEventoValidoAlteravel(
                    string.Format("ID do evento [{0}]: ", inscricao.IdEvento),
                    inscricao.IdEvento,
                    eventosDisponiveis),
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
        private int LerIdEventoValido(List<EventoDisponivel> eventosDisponiveis)
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

                EventoDisponivel? eventoEncontrado = ObterEventoDaLista(eventosDisponiveis, idEvento);

                if (eventoEncontrado == null)
                {
                    view.MostrarMensagem("O ID indicado nao corresponde a um evento existente.");
                    continue;
                }

                if (!EventoPodeReceberInscricao(eventoEncontrado))
                {
                    view.MostrarMensagem("O evento indicado nao esta ativo ou nao tem disponibilidade.");
                    continue;
                }

                return idEvento;
            }
        }

        // Metodo auxiliar para ler um ID de evento alteravel, permitindo manter o valor atual com Enter
        private int LerIdEventoValidoAlteravel(string pedido, int valorAtual, List<EventoDisponivel> eventosDisponiveis)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);
                string entrada = LerEntrada();

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    return valorAtual;
                }

                if (!int.TryParse(entrada, out int idEvento) || idEvento <= 0)
                {
                    view.MostrarMensagem("ID de evento invalido.");
                    continue;
                }

                EventoDisponivel? eventoEncontrado = ObterEventoDaLista(eventosDisponiveis, idEvento);

                if (eventoEncontrado == null)
                {
                    view.MostrarMensagem("O ID indicado nao corresponde a um evento existente.");
                    continue;
                }

                if (!EventoPodeReceberInscricao(eventoEncontrado))
                {
                    view.MostrarMensagem("O evento indicado nao esta ativo ou nao tem disponibilidade.");
                    continue;
                }

                return idEvento;
            }
        }

        private EventoDisponivel? ObterEventoDaLista(List<EventoDisponivel> eventosDisponiveis, int idEvento)
        {
            foreach (EventoDisponivel evento in eventosDisponiveis)
            {
                if (evento.Id == idEvento)
                {
                    return evento;
                }
            }

            return null;
        }

        private bool ExisteEventoDisponivelParaInscricao(List<EventoDisponivel> eventosDisponiveis)
        {
            foreach (EventoDisponivel evento in eventosDisponiveis)
            {
                if (EventoPodeReceberInscricao(evento))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EventoPodeReceberInscricao(EventoDisponivel evento)
        {
            return evento != null &&
                   evento.Disponibilidade > 0 &&
                   EstadoAtivo(evento.Estado);
        }

        private bool EstadoAtivo(string estado)
        {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            return estadoNormalizado == "ativo" || estadoNormalizado == "ativa";
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
