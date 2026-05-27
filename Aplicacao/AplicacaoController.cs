using System;
using GestorEventos.Eventos;
using GestorEventos.Inscricoes;
using GestorEventos.Partilhado.Servicos;
using GestorEventos.Relatorios;

namespace GestorEventos.Aplicacao {
    /* Controller principal da aplicação.
     * Responsável por inicializar os componentes, interligar handlers/eventos,
     * apresentar o menu principal e encaminhar o utilizador para os módulos. */
    class AplicacaoController {
        private readonly MenuPrincipalView menuPrincipalView;
        private readonly EventoController eventoController;
        private readonly InscricaoController inscricaoController;
        private readonly RelatorioController relatorioController;
        private bool programaAtivo;

        /* Inicialização das dependências e subscrição dos eventos/handlers
         * necessários ao arranque e ao registo de acontecimentos do domínio. */
        public AplicacaoController() {
            menuPrincipalView = new MenuPrincipalView();

            EventoView eventoView = new EventoView();
            EventoModel eventoModel = new EventoModel();

            InscricaoView inscricaoView = new InscricaoView();
            InscricaoModel inscricaoModel = new InscricaoModel();

            RelatorioView relatorioView = new RelatorioView();
            RelatorioModel relatorioModel = new RelatorioModel();
            EventosDominioHandler eventosDominioHandler = new EventosDominioHandler();

            NotificacaoAnulacaoHandler notificacaoAnulacaoHandler = new NotificacaoAnulacaoHandler(inscricaoModel);

            eventoModel.EventoCriado += eventosDominioHandler.OnEventoCriado;
            eventoModel.EventoAlterado += eventosDominioHandler.OnEventoAlterado;
            eventoModel.EventoCancelado += notificacaoAnulacaoHandler.OnEventoCancelado;
            inscricaoModel.InscricaoCriada += eventosDominioHandler.OnInscricaoCriada;
            inscricaoModel.InscricaoAlterada += eventosDominioHandler.OnInscricaoAlterada;
            inscricaoModel.InscricaoCancelada += eventosDominioHandler.OnInscricaoCancelada;
            AtualizadorEstadosService.EventoTerminado += eventosDominioHandler.OnEventoTerminado;
            AtualizadorEstadosService.InscricaoTerminada += eventosDominioHandler.OnInscricaoTerminada;

            // Garante a coerência inicial dos estados na BD antes da utilização da aplicação.
            eventoModel.AtualizarEstados();
            
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

        public void SelecionarOpcao(string? opcao) {
            switch (NormalizarOpcao(opcao)) {
                case "1":
                    eventoController.MostrarMenuModulo();
                    break;
                case "2":
                    inscricaoController.MostrarMenuModulo();
                    break;
                case "3":
                    relatorioController.MostrarMenuModulo();
                    break;
                case "0":
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

        private string NormalizarOpcao(string? opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim();
        }
    }
}
