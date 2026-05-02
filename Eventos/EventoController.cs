using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;
using System;

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

        // O método MostrarMenuModulo é responsável por exibir o menu de eventos e processar as opções selecionadas pelo usuário. Ele utiliza um loop para permitir que o usuário continue interagindo com o menu até que decida regressar ao menu principal.
        public void MostrarMenuModulo()
        {
            bool regressar = false;

            while (!regressar)
            {
                view.MostrarMenuEventos();
                string opcao = Console.ReadLine() ?? string.Empty;
                regressar = SelecionarOpcao(opcao);
            }
        }


        // O método SelecionarOpcao é responsável por interpretar a opção selecionada pelo usuário no menu de eventos e chamar o método correspondente para cada operação (criação, alteração, cancelamento ou regressar ao menu principal).
        public bool SelecionarOpcao(string opcao)
        {
            switch (opcao.Trim())
            {
                case "1":
                    view.MostrarMensagem("Opcao a desenvolver.");   // retirar quando for implementada
                    //CriarEvento();                                // descomentar quando for implementada    
                    return false;

                case "2":
                    view.MostrarMensagem("Opcao a desenvolver.");   // retirar quando for implementada
                    //AlterarEvento();                              // descomentar quando for implementada
                    return false;

                case "3":
                    view.MostrarMensagem("Opcao a desenvolver.");   // retirar quando for implementada
                    //CancelarEvento();                             // descomentar quando for implementada
                    return false;

                case "0":
                    RegressarMenuPrincipal();
                    return true;

                default:
                    view.MostrarMensagem("Opcao de inscricoes invalida. Escolha 1, 2, 3 ou 0 para regressar ao menu principal.");
                    return false;
            }
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }



        // ----------------------------   A IMPLEMENTAR POSTERIORMENTE   ---------------------------
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

    }
}
