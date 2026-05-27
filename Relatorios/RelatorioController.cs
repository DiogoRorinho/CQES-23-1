using System;
using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;

namespace GestorEventos.Relatorios {
    /* Recolhe previamente as inscrições que irão transitar para terminada,
     * para permitir emitir depois eventos de domínio com os respetivos dados. */
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

        // Mantém o submenu de Relatórios ativo até o utilizador regressar ao menu principal.
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

        // Coordena a geração do relatório de inscritos para um evento escolhido pelo utilizador.
        public void ApresentarRelatorioInscritosPorEvento() {
            List<Evento> listaEventos = model.ListarEventos();
            view.MostrarListaEventos(listaEventos);

            int? idEvento = LerIdEventoValidoOuSair(listaEventos);
            if (idEvento == null) {
                view.MostrarMensagem("Operacao cancelada.");
                return;
            }

            SelecionarEvento(idEvento.Value);
        }

        // Solicita ao Model os dados do relatório e envia-os à View para apresentação e referência ao PDF gerado.
        public void SelecionarEvento(int idEvento) {
            DadosRelatorio dadosRelatorio = model.ListarInscritosPorEvento(idEvento);
            DocumentoPdf relatorioPdf = model.ObterUltimoRelatorioGerado();
            view.ApresentarRelatorioEPdf(dadosRelatorio, relatorioPdf);
        }

        /* Mantém o utilizador num ciclo local de validação até escolher um evento válido
         * ou introduzir 0 para cancelar a operação. */
        private int? LerIdEventoValidoOuSair(List<Evento> listaEventos) {
            while (true) {
                view.SolicitarIdEvento();
                string entrada = Console.ReadLine() ?? string.Empty;

                if (!int.TryParse(entrada, out int idEvento) || idEvento < 0) {
                    view.MostrarMensagem("Opcao invalida.");
                    continue;
                }

                if (idEvento == 0) {
                    return null;
                }

                if (!EventoExisteNaLista(listaEventos, idEvento)) {
                    view.MostrarMensagem("ID invalido.");
                    continue;
                }
                return idEvento;
            }
        }

        // Valida o ID do evento com base na lista já recebida do Model, evitando nova consulta desnecessária.
        private bool EventoExisteNaLista(List<Evento> listaEventos, int idEvento) {
            foreach (Evento evento in listaEventos) {
                if (evento.Id == idEvento) {
                    return true;
                }
            }
            return false;
        }

        // Gera o relatório de ocupação dos eventos e apresenta o respetivo PDF ao utilizador.
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
