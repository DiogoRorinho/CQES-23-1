using System;
using GestorEventos.Eventos;
using GestorEventos.Inscricoes;
using GestorEventos.Relatorios;

namespace GestorEventos.Aplicacao {
    class AplicacaoController {
        private readonly MenuPrincipalView menuPrincipalView;
        private readonly EventoController eventoController;
        private readonly InscricaoController inscricaoController;
        private readonly RelatorioController relatorioController;
        private bool programaAtivo;

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
            programaAtivo = true;
            menuPrincipalView.ApresentarBoasVindas();

            while (programaAtivo) {
                try {
                    menuPrincipalView.MostrarMenuPrincipal();
                    SelecionarOpcao(Console.ReadLine());
                }
                catch (Exception ex) {
                    menuPrincipalView.MostrarErroMenu(ex.Message);
                }
                finally {
                    menuPrincipalView.FinalizarOperacaoMenu();
                }
            }
        }

        public void SelecionarOpcao(string opcao) {
            switch (NormalizarOpcao(opcao)) {
                case "1":                // case "Eventos"
                    eventoController.MostrarMenuModulo();
                    break;
                case "2":                // case "Inscricoes"
                    inscricaoController.MostrarMenuModulo();
                    break;
                case "3":                // case "Relatorios"
                    relatorioController.MostrarMenuModulo();
                    break;
                case "0":                // case "Terminar"
                    TerminarPrograma();
                    break;
                default:
                    menuPrincipalView.MostrarOpcaoInvalida();
                    break;
            }
        }

        public void RegressarMenuPrincipal() {
            menuPrincipalView.MostrarMensagemRegresso();
        }

        public void TerminarPrograma() {
            programaAtivo = false;
            menuPrincipalView.ApresentarMensagemEncerramento();
        }

        private string NormalizarOpcao(string opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim();
        }
    }
}
