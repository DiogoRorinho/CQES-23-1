using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Inscricoes {
    class InscricaoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly InscricaoView view;
        private readonly InscricaoModel model;

        public InscricaoController(AplicacaoController aplicacaoController, InscricaoView view, InscricaoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            view.MostrarMenuInscricoes();
        }

        public void SelecionarOpcao(string opcao) {
            switch (opcao) {
                case "Criar inscricao":
                    view.MostrarListaEventos(model.ListarEventosDisponiveis());
                    view.SolicitarDadosCriacao();
                    break;
                case "Alterar inscricao":
                    view.MostrarListaInscricoes(model.ListarInscricoes());
                    break;
                case "Cancelar inscricao":
                    view.MostrarListaInscricoes(model.ListarInscricoes());
                    break;
                case "Regressar ao menu principal":
                    RegressarMenuPrincipal();
                    break;
                default:
                    view.MostrarMensagem("Opcao de inscricoes invalida.");
                    break;
            }
        }

        public void IntroduzirDadosInscricao(DadosInscricao dados) {
            if (model.VerificarDisponibilidade(dados.IdEvento, dados.Quantidade)) {
                DocumentoPdf bilhetePdf = model.CriarInscricao(dados);
                view.MostrarResultadoOperacaoEBilhete("Inscricao criada com sucesso.", bilhetePdf);
                view.MostrarMenuInscricoes();
                return;
            }

            view.MostrarErroSemVagas();
        }

        public void SelecionarInscricao(int idInscricao) {
            Inscricao inscricao = model.ObterInscricao(idInscricao);
            view.MostrarDadosParaEdicao(inscricao);
        }

        public void IntroduzirDadosAlterados(int idInscricao, DadosInscricao dados) {
            if (model.ValidarAlteracaoInscricao(idInscricao, dados)) {
                DocumentoPdf bilhetePdf = model.AlterarInscricao(idInscricao, dados);
                view.MostrarResultadoOperacaoEBilhete("Inscricao alterada com sucesso.", bilhetePdf);
                view.MostrarMenuInscricoes();
                return;
            }

            view.MostrarErroSemVagas();
        }

        public void PedirConfirmacaoCancelamento() {
            view.PedirConfirmacaoCancelamento();
        }

        public void ConfirmarCancelamento(int idInscricao) {
            model.CancelarInscricao(idInscricao);
            view.MostrarResultadoOperacao("Inscricao cancelada com sucesso.");
            view.MostrarMenuInscricoes();
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }
    }
}
