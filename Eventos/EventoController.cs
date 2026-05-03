using System;
using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class EventoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly EventoView view;
        private readonly EventoModel model;
        private bool regressarMenuPrincipal;

        public EventoController(AplicacaoController aplicacaoController, EventoView view, EventoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            regressarMenuPrincipal = false;

            while (!regressarMenuPrincipal) {
                try {
                    view.MostrarMenuEventos();
                    SelecionarOpcao(view.LerEntrada());
                }
                catch (Exception ex) {
                    view.MostrarErroMenu(ex.Message);
                }
                finally {
                    view.FinalizarOperacaoMenu();
                }
            }
        }

        public void SelecionarOpcao(string opcao) {
            switch (NormalizarOpcao(opcao)) {
                case "1":
                    CriarEvento();
                    break;

                case "2":
                    AlterarEvento();
                    break;

                case "3":
                    CancelarEvento();
                    break;

                case "0":
                    RegressarMenuPrincipal();
                    regressarMenuPrincipal = true;
                    break;

                default:
                    view.MostrarMensagem("Opcao de eventos invalida.");
                    break;
            }
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        private void CriarEvento() {
            view.SolicitarDadosCriacao();

            DadosEvento dados = RecolherDadosEvento();
            if (dados == null) {
                view.MostrarMensagem("Dados do evento invalidos.");
                return;
            }

            model.CriarEvento(dados);
            view.MostrarResultadoOperacao("Evento criado com sucesso.");
        }

        private void AlterarEvento() {
            view.MostrarListaEventos(model.ListarEventos());
            view.SolicitarIdEventoAlteracao();

            int idEvento;
            if (!int.TryParse(view.LerEntrada(), out idEvento) || idEvento <= 0) {
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            Evento evento = model.ObterEvento(idEvento);
            if (evento == null) {
                view.MostrarMensagem("Evento nao encontrado.");
                return;
            }

            view.MostrarDadosParaEdicao(evento);

            DadosEvento dados = RecolherDadosEvento();
            if (dados == null) {
                view.MostrarMensagem("Dados do evento invalidos.");
                return;
            }

            model.AlterarEvento(idEvento, dados);
            view.MostrarResultadoOperacao("Evento alterado com sucesso.");
        }

        private void CancelarEvento() {
            view.MostrarListaEventos(model.ListarEventos());
            view.SolicitarIdEventoCancelamento();

            int idEvento;
            if (!int.TryParse(view.LerEntrada(), out idEvento) || idEvento <= 0) {
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            Evento evento = model.ObterEvento(idEvento);
            if (evento == null) {
                view.MostrarMensagem("Evento nao encontrado.");
                return;
            }

            view.MostrarDadosParaEdicao(evento);
            view.PedirConfirmacaoCancelamento();

            string confirmacao = NormalizarOpcao(view.LerEntrada());
            if (confirmacao != "s" && confirmacao != "sim") {
                view.MostrarMensagem("Cancelamento interrompido.");
                return;
            }

            model.CancelarEvento(idEvento);
            view.MostrarResultadoOperacao("Evento cancelado com sucesso.");
        }

        private DadosEvento RecolherDadosEvento() {
            view.SolicitarNome();
            string nome = view.LerEntrada();

            view.SolicitarLocal();
            string local = view.LerEntrada();

            view.SolicitarData();
            DateTime data;
            if (!DateTime.TryParse(view.LerEntrada(), out data)) {
                return null;
            }

            view.SolicitarCapacidade();
            int capacidade;
            if (!int.TryParse(view.LerEntrada(), out capacidade) || capacidade <= 0) {
                return null;
            }

            return new DadosEvento {
                Nome = nome,
                Local = local,
                Data = data,
                Capacidade = capacidade
            };
        }

        private string NormalizarOpcao(string opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim().ToLowerInvariant();
        }
    }
}
