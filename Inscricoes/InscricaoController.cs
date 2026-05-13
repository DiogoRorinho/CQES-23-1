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

        private int LerIdEventoValido(List<Evento> eventosDisponiveis)
        {
            while (true)
            {
                view.SolicitarIdEvento();
                string entrada = LerEntrada();

                if (!int.TryParse(entrada, out int idEvento) || idEvento <= 0)
                {
                    view.MostrarMensagem("ID de evento invalido.");
                    continue;
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

        private string NormalizarOpcao(string? opcao)
        {
            if (string.IsNullOrWhiteSpace(opcao))
            {
                return string.Empty;
            }

            return opcao.Trim().ToLowerInvariant();
        }

        public void RegressarMenuPrincipal()
        {
            aplicacaoController.RegressarMenuPrincipal();
        }

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

        public void PedirConfirmacaoCancelamento()
        {
            view.PedirConfirmacaoCancelamento();
        }

        public void ConfirmarCancelamento(int idInscricao)
        {
            model.CancelarInscricao(idInscricao);
            DocumentoPdf comprovativo = model.GerarComprovativoCancelamento(idInscricao);
            view.MostrarResultadoOperacaoEBilhete("Inscricao cancelada com sucesso.", comprovativo);
        }
    }
}
