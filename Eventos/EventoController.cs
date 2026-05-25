using System;
using System.Globalization;
using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;

namespace GestorEventos.Eventos {
    class EventoController {
        private const string MensagemIdInvalidoOuSaida = "Introduza um ID valido ou 0 para sair.";
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
                    SelecionarOpcao(LerEntrada());
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

            DadosEvento dados = RecolherDadosCriacaoEvento();

            ResultadoOperacaoEvento resultado = model.CriarEvento(dados);
            view.MostrarResultadoOperacao(resultado.Mensagem);
        }

        private void AlterarEvento() {
            List<Evento> eventosAtivos = model.ListarEventosAtivos();
            if (eventosAtivos.Count == 0) {
                view.MostrarMensagem("Nao existem eventos ativos para alterar.");
                return;
            }

            view.MostrarListaEventos(eventosAtivos);
            
            Evento? evento = LerEventoAtivoValidoOuSair(eventosAtivos, view.SolicitarIdEventoAlteracao);
            if (evento == null) {
                view.MostrarMensagem("Alteracao de evento cancelada.");
                return;
            }

            view.MostrarDadosParaEdicao(evento);

            DadosEvento dados = RecolherDadosAlteracaoEvento(evento);

            ResultadoOperacaoEvento resultado = model.AlterarEvento(evento.Id, dados);
            view.MostrarResultadoOperacao(resultado.Mensagem);
        }

        private void CancelarEvento() {
            List<Evento> eventosAtivos = model.ListarEventosAtivos();
            if (eventosAtivos.Count == 0) {
                view.MostrarMensagem("Nao existem eventos ativos para cancelar.");
                return;
            }

            view.MostrarListaEventos(eventosAtivos);

            Evento? evento = LerEventoAtivoValidoOuSair(eventosAtivos, view.SolicitarIdEventoAlteracao);
            if (evento == null) {
                view.MostrarMensagem("Alteracao de evento cancelada.");
                return;
            }

            view.MostrarDadosParaCancelamento(evento);      // Não imprime "Prima Enter para manter o valor atual."
            view.PedirConfirmacaoCancelamento();

            string confirmacao = NormalizarOpcao(LerEntrada());
            if (confirmacao != "s" && confirmacao != "sim") {
                view.MostrarMensagem("Cancelamento interrompido.");
                return;
            }

            ResultadoOperacaoEvento resultado = model.CancelarEvento(evento.Id);
            view.MostrarResultadoOperacao(resultado.Mensagem);
        }

        private Evento? LerEventoAtivoValidoOuSair(List<Evento> eventosAtivos, Action solicitarId)
        {
            while (true)
            {
                solicitarId();
                string entrada = LerEntrada();

                if (!int.TryParse(entrada, out int idEvento) || idEvento < 0)
                {
                    view.MostrarMensagem("Opcao invalida.");
                    continue;
                }

                if (idEvento == 0)
                {
                    return null;
                }

                Evento? evento = EncontrarEventoPorId(eventosAtivos, idEvento);
                if (evento == null)
                {
                    view.MostrarMensagem("ID invalido.");
                    continue;
                }

                return evento;
            }
        }

        private static Evento? EncontrarEventoPorId(List<Evento> eventos, int idEvento) {
            foreach (Evento evento in eventos) {
                if (evento.Id == idEvento) {
                    return evento;
                }
            }

            return null;
        }

        private void ListarEventos() {
            view.MostrarListaEventos(model.ListarEventos());
        }

        private DadosEvento RecolherDadosCriacaoEvento() {
            return new DadosEvento {
                Nome = LerTextoObrigatorio(view.SolicitarNome),
                Local = LerTextoObrigatorio(view.SolicitarLocal),
                Data = LerDataFuturaObrigatoria(view.SolicitarData),
                Capacidade = LerInteiroPositivoObrigatorio(view.SolicitarCapacidade)
            };
        }

        private DadosEvento RecolherDadosAlteracaoEvento(Evento evento) {
            return new DadosEvento {
                Nome = LerTextoAlteravel(view.SolicitarNome, evento.Nome),
                Local = LerTextoAlteravel(view.SolicitarLocal, evento.Local),
                Data = LerDataFuturaAlteravel(view.SolicitarData, evento.Data),
                Capacidade = LerInteiroPositivoAlteravel(view.SolicitarCapacidade, evento.Capacidade)
            };
        }

        private string LerTextoObrigatorio(Action solicitarCampo) {
            while (true) {
                solicitarCampo();
                string valor = LerEntrada();

                if (!string.IsNullOrWhiteSpace(valor)) {
                    return valor.Trim();
                }

                view.MostrarMensagem("O valor introduzido nao pode estar vazio.");
            }
        }

        private string LerTextoAlteravel(Action solicitarCampo, string valorAtual) {
            solicitarCampo();
            string valor = LerEntrada();

            if (string.IsNullOrWhiteSpace(valor)) {
                return valorAtual;
            }

            return valor.Trim();
        }

        private DateTime LerDataFuturaObrigatoria(Action solicitarCampo) {
            while (true) {
                solicitarCampo();

                if (TentarLerDataFutura(LerEntrada(), out DateTime data)) {
                    return data;
                }

                view.MostrarMensagem("Introduza uma data futura no formato dd/MM/yyyy.");
            }
        }

        private DateTime LerDataFuturaAlteravel(Action solicitarCampo, DateTime valorAtual) {
            while (true) {
                solicitarCampo();
                string entrada = LerEntrada();

                if (string.IsNullOrWhiteSpace(entrada)) {
                    return valorAtual;
                }

                if (TentarLerDataFutura(entrada, out DateTime data)) {
                    return data;
                }

                view.MostrarMensagem("Introduza uma data futura no formato dd/MM/yyyy.");
            }
        }

        private int LerInteiroPositivoObrigatorio(Action solicitarCampo) {
            while (true) {
                solicitarCampo();

                if (int.TryParse(LerEntrada(), out int valor) && valor > 0) {
                    return valor;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        private int LerInteiroPositivoAlteravel(Action solicitarCampo, int valorAtual) {
            while (true) {
                solicitarCampo();
                string entrada = LerEntrada();

                if (string.IsNullOrWhiteSpace(entrada)) {
                    return valorAtual;
                }

                if (int.TryParse(entrada, out int valor) && valor > 0) {
                    return valor;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        private bool TentarLerDataFutura(string entrada, out DateTime data) {
            bool dataValida = DateTime.TryParseExact(
                entrada.Trim(),
                "dd/MM/yyyy",
                CultureInfo.GetCultureInfo("pt-PT"),
                DateTimeStyles.None,
                out data);

            if (!dataValida) {
                return false;
            }

            data = data.Date;
            return data > DateTime.Today;
        }

        private string LerEntrada() {
            return Console.ReadLine() ?? string.Empty;
        }

        private string NormalizarOpcao(string? opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }

            return opcao.Trim().ToLowerInvariant();
        }
    }
}
