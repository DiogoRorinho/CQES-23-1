using System;
using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;

namespace GestorEventos.Relatorios {
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

        public void SelecionarOpcao(string? opcao) {
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
            List<Evento> listaEventos = model.ListarEventos();
            view.MostrarListaEventos(listaEventos);

            int? idEvento = LerIdEventoValidoOuSair(listaEventos);
            if (idEvento == null)
            {
                view.MostrarMensagem("Operacao cancelada.");
                return;
            }

            SelecionarEvento(idEvento.Value);
        }

        public void SelecionarEvento(int idEvento) {
            DadosRelatorio dadosRelatorio = model.ListarInscritosPorEvento(idEvento);
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
        }

        private int? LerIdEventoValidoOuSair(List<Evento> listaEventos)
        {
            while (true)
            {
                view.SolicitarIdEvento();
                string entrada = Console.ReadLine() ?? string.Empty;

                if (!int.TryParse(entrada, out int idEvento) || idEvento < 0)
                {
                    view.MostrarMensagem("Opcao invalida.");
                    continue;
                }

                if (idEvento == 0)
                {
                    return null;
                }

                if (!EventoExisteNaLista(listaEventos, idEvento))
                {
                    view.MostrarMensagem("ID invalido.");
                    continue;
                }

                return idEvento;
            }
        }

        private bool EventoExisteNaLista(List<Evento> listaEventos, int idEvento)
        {
            foreach (Evento evento in listaEventos)
            {
                if (evento.Id == idEvento)
                {
                    return true;
                }
            }

            return false;
        }

        public void ApresentarRelatorioEventosComOcupacao() {
            DadosRelatorio dadosRelatorio = model.ListarEventosComOcupacao();
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        private string NormalizarOpcao(string? opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim();
        }
    }
}
