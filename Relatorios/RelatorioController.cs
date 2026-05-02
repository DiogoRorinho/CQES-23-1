using System;
using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Relatorios {
    class RelatorioController {
        private readonly AplicacaoController aplicacaoController;
        private readonly RelatorioView view;
        private readonly RelatorioModel model;
        private bool regressarMenuPrincipal;

        public RelatorioController(AplicacaoController aplicacaoController, RelatorioView view, RelatorioModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            regressarMenuPrincipal = false;

            while (!regressarMenuPrincipal) {
                try {
                    view.MostrarMenuRelatorios();
                    SelecionarOpcao(Console.ReadLine());
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
                case "1":                //case "Listagem de inscritos por evento"
                    ApresentarRelatorioInscritosPorEvento();
                    break;
                case "2":                //case "Eventos com ocupacao"
                    ApresentarRelatorioEventosComOcupacao();
                    break;
                case "0":                //case "Regressar ao menu principal"
                    RegressarMenuPrincipal();
                    regressarMenuPrincipal = true;
                    break;
                default:
                    view.MostrarMensagem("Opcao de relatorios invalida.");
                    break;
            }
        }

        public void ApresentarRelatorioInscritosPorEvento() {
            view.MostrarListaEventos(model.ListarEventos());

            view.SolicitarIdEvento();

            int idEvento;
            if (!int.TryParse(Console.ReadLine(), out idEvento)) {
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            if (idEvento <= 0) {
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            SelecionarEvento(idEvento);
        }

        public void SelecionarEvento(int idEvento) {
            DadosRelatorio dadosRelatorio = model.ListarInscritosPorEvento(idEvento);
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
        }

        public void ApresentarRelatorioEventosComOcupacao() {
            DadosRelatorio dadosRelatorio = model.ListarEventosComOcupacao();
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        private string NormalizarOpcao(string opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim();
        }
    }
}
