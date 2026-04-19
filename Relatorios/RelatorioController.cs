using System.Collections.Generic;
using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Relatorios {
    class RelatorioController {
        private readonly AplicacaoController aplicacaoController;
        private readonly RelatorioView view;
        private readonly RelatorioModel model;

        public RelatorioController(AplicacaoController aplicacaoController, RelatorioView view, RelatorioModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            view.MostrarMenuRelatorios();
        }

        public void SelecionarOpcao(string opcao) {
            switch (opcao) {
                case "Listagem de inscritos por evento":
                    view.MostrarListaEventos(model.ListarEventos());
                    break;
                case "Eventos com ocupacao":
                    ApresentarRelatorioEventosComOcupacao();
                    break;
                case "Regressar ao menu principal":
                    RegressarMenuPrincipal();
                    break;
                default:
                    view.MostrarMensagem("Opcao de relatorios invalida.");
                    break;
            }
        }

        public void SelecionarEvento(int idEvento) {
            DadosRelatorio dadosRelatorio = model.ListarInscritosPorEvento(idEvento);
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
            view.MostrarMenuRelatorios();
        }

        public void ApresentarRelatorioEventosComOcupacao() {
            DadosRelatorio dadosRelatorio = model.ListarEventosComOcupacao();
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
            view.MostrarMenuRelatorios();
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }
    }
}
