using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Eventos {
    class EventoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly EventoView view;
        private readonly EventoModel model;

        public EventoController(AplicacaoController aplicacaoController, EventoView view, EventoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            view.MostrarMenuEventos();
        }

        public void SelecionarOpcao(string opcao) {
            switch (opcao) {
                case "Criar evento":
                    view.SolicitarDadosCriacao();
                    break;
                case "Alterar evento":
                    view.MostrarListaEventos(model.ListarEventos());
                    break;
                case "Cancelar evento":
                    view.MostrarListaEventos(model.ListarEventos());
                    break;
                case "Regressar ao menu principal":
                    RegressarMenuPrincipal();
                    break;
                default:
                    view.MostrarMensagem("Opcao de eventos invalida.");
                    break;
            }
        }

        public void IntroduzirDadosEvento(DadosEvento dados) {
            model.CriarEvento(dados);
            view.MostrarResultadoOperacao("Evento criado com sucesso.");
            view.MostrarMenuEventos();
        }

        public void SelecionarEvento(int idEvento) {
            Evento evento = model.ObterEvento(idEvento);
            view.MostrarDadosParaEdicao(evento);
        }

        public void IntroduzirDadosAlterados(int idEvento, DadosEvento dados) {
            model.AlterarEvento(idEvento, dados);
            view.MostrarResultadoOperacao("Evento alterado com sucesso.");
            view.MostrarMenuEventos();
        }

        public void PedirConfirmacaoCancelamento() {
            view.PedirConfirmacaoCancelamento();
        }

        public void ConfirmarCancelamento(int idEvento) {
            model.CancelarEvento(idEvento);
            view.MostrarResultadoOperacao("Evento cancelado com sucesso.");
            view.MostrarMenuEventos();
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }
    }
}
