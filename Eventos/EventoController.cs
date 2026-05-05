// NOTA DE VERIFICACAO:
// Os fluxos de criacao, alteracao e cancelamento de eventos encontram-se
// genericamente encaminhados, mas ficam sinalizados alguns pontos a alinhar
// em iteracao futura:
// - o input continua encapsulado na View, afastando-se da interpretacao mais
//   rigorosa de Krasner & Pope seguida noutros ajustamentos do projeto;
// - a validacao de input no Controller pode ser reforcada, de forma semelhante
//   ao modulo de Inscricoes, nomeadamente nome/local nao vazios, data futura
//   valida;;
// - o Controller apresenta atualmente mensagens de sucesso apos chamar o Model,
//   sem que este devolva ainda confirmacao explicita de sucesso/falha.
// Estes pontos nao invalidam a estrutura geral implementada, mas deverao ser
// alinhados quando a validacao completa e a integracao com SQLite estiverem
// consolidadas.

using System;
using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class EventoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly EventoView view;
        private readonly EventoModel model;
        private bool regressarMenuPrincipal;

        public EventoController(AplicacaoController aplicacaoController, EventoView view, EventoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        public void MostrarMenuModulo() {
            regressarMenuPrincipal = false;

            while (!regressarMenuPrincipal) {
                try {
                    view.MostrarMenuEventos();
                    SelecionarOpcao(view.LerEntrada());
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
                case "1":
                    CriarEvento();
                    break;

                case "2":
                    AlterarEvento();
                    break;

                case "3":
                    CancelarEvento();
                    break;

                case "4":
                    ListarEventos();
                    break;

                case "0":
                    RegressarMenuPrincipal();
                    regressarMenuPrincipal = true;
                    break;

                default:
                    view.MostrarMensagem("Opcao de eventos invalida. Escolha 1, 2, 3, 4 ou 0 para regressar ao menu principal.");
                    break;
            }
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        private void CriarEvento() {
            view.SolicitarDadosCriacao();

            DadosEvento? dados = RecolherDadosEvento();
            if (dados == null) {
                view.MostrarMensagem("Dados do evento invalidos.");
                return;
            }

            model.CriarEvento(dados);
            view.MostrarResultadoOperacao("Evento criado com sucesso.");
        }

        // Deve permitir que o user altere só o campo pretendido, Enter para manter (e não precisa validar)
        // e valida os campos alterados (Se for diminuicao de capacidade o Model deve verificar se existem
        // inscricoes que ultrapassem a nova capacidade, e impedir a alteracao se for o caso, por exemplo).
        private void AlterarEvento() {
            view.MostrarListaEventos(model.ListarEventos());
            view.SolicitarIdEventoAlteracao();

            int idEvento;
            if (!int.TryParse(view.LerEntrada(), out idEvento) || idEvento <= 0) {
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            Evento? evento = model.ObterEvento(idEvento);
            if (evento == null) {
                view.MostrarMensagem("Evento nao encontrado.");
                return;
            }

            view.MostrarDadosParaEdicao(evento);

            DadosEvento? dados = RecolherDadosEvento();
            if (dados == null) {
                view.MostrarMensagem("Dados do evento invalidos.");
                return;
            }

            model.AlterarEvento(idEvento, dados);                               // Nota: o Model ainda nao devolve confirmacao de sucesso/falha, mas o Controller assume sucesso se nao for lancada excecao.
            view.MostrarResultadoOperacao("Evento alterado com sucesso.");      // Nota: a mensagem de sucesso e a sua apresentacao pelo Controller deverao ser alinhadas com o resultado real da operacao, quando a validacao completa e a integracao com SQLite estiverem consolidadas.
        }

        private void CancelarEvento() {
            view.MostrarListaEventos(model.ListarEventos());
            view.SolicitarIdEventoCancelamento();

            int idEvento;
            if (!int.TryParse(view.LerEntrada(), out idEvento) || idEvento <= 0) {      // A leitura do input deve ser feita pelo Controller
                view.MostrarMensagem("ID de evento invalido.");
                return;
            }

            Evento? evento = model.ObterEvento(idEvento);
            if (evento == null) {
                view.MostrarMensagem("Evento nao encontrado.");
                return;
            }

            view.MostrarDadosParaEdicao(evento);
            view.PedirConfirmacaoCancelamento();

            string confirmacao = NormalizarOpcao(view.LerEntrada());            // o INPUT deve ser lido pelo Controller.
            if (confirmacao != "s" && confirmacao != "sim") {
                view.MostrarMensagem("Cancelamento interrompido.");
                return;
            }

            model.CancelarEvento(idEvento);                                     // Nota: o Model ainda nao devolve confirmacao de sucesso/falha, mas o Controller assume sucesso se nao for lancada excecao.
            view.MostrarResultadoOperacao("Evento cancelado com sucesso.");     // Nota: a mensagem de sucesso e a sua apresentacao pelo Controller deverao ser alinhadas com o resultado real da operacao, quando a validacao completa e a integracao com SQLite estiverem consolidadas.
        }

        private void ListarEventos() {
            view.MostrarListaEventos(model.ListarEventos());
        }

        private DadosEvento? RecolherDadosEvento() {
            view.SolicitarNome();
            string nome = view.LerEntrada();            // Acrescentar validacao de nome como texto nao vazio. e a leitura do input deve ser feita pelo Controller

            view.SolicitarLocal();
            string local = view.LerEntrada();           // Acrescentar validacao de local como texto nao vazio. e a leitura do input deve ser feita pelo Controller

            view.SolicitarData();
            DateTime data;
            if (!DateTime.TryParse(view.LerEntrada(), out data)) {  // Acrescentar condição de data futura, e a leitura do input deve ser feita pelo Controller
                return null;
            }

            view.SolicitarCapacidade();
            int capacidade;
            if (!int.TryParse(view.LerEntrada(), out capacidade) || capacidade <= 0) {  // A leitura do input deve ser feita pelo Controller//
                return null;
            }

            return new DadosEvento {
                Nome = nome,
                Local = local,
                Data = data,
                Capacidade = capacidade
            };
        }

        private string NormalizarOpcao(string? opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim().ToLowerInvariant();
        }
    }
}
