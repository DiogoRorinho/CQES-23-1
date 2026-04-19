using GestorEventosEsqueleto.Eventos;
using GestorEventosEsqueleto.Inscricoes;
using GestorEventosEsqueleto.Relatorios;

namespace GestorEventosEsqueleto.Aplicacao {
    class AplicacaoController {
        private readonly MenuPrincipalView menuPrincipalView;
        private readonly EventoController eventoController;
        private readonly InscricaoController inscricaoController;
        private readonly RelatorioController relatorioController;

        public AplicacaoController() {
            menuPrincipalView = new MenuPrincipalView();

            EventoView eventoView = new EventoView();
            EventoModel eventoModel = new EventoModel();

            InscricaoView inscricaoView = new InscricaoView();
            InscricaoModel inscricaoModel = new InscricaoModel();

            RelatorioView relatorioView = new RelatorioView();
            RelatorioModel relatorioModel = new RelatorioModel();

            NotificacaoAnulacaoHandler notificacaoAnulacaoHandler = new NotificacaoAnulacaoHandler(inscricaoModel);
            eventoModel.EventoCancelado += notificacaoAnulacaoHandler.OnEventoCancelado;

            eventoController = new EventoController(this, eventoView, eventoModel);
            inscricaoController = new InscricaoController(this, inscricaoView, inscricaoModel);
            relatorioController = new RelatorioController(this, relatorioView, relatorioModel);
        }

        public void IniciarPrograma() {
            menuPrincipalView.ApresentarBoasVindas();
            menuPrincipalView.MostrarMenuPrincipal();
        }

        public void SelecionarOpcao(string opcao) {
            switch (opcao) {
                case "Eventos":
                    eventoController.MostrarMenuModulo();
                    break;
                case "Inscricoes":
                    inscricaoController.MostrarMenuModulo();
                    break;
                case "Relatorios":
                    relatorioController.MostrarMenuModulo();
                    break;
                case "Terminar":
                    TerminarPrograma();
                    break;
                default:
                    menuPrincipalView.MostrarOpcaoInvalida();
                    menuPrincipalView.MostrarMenuPrincipal();
                    break;
            }
        }

        public void RegressarMenuPrincipal() {
            menuPrincipalView.MostrarMenuPrincipal();
        }

        public void TerminarPrograma() {
            menuPrincipalView.ApresentarMensagemEncerramento();
        }
    }
}
