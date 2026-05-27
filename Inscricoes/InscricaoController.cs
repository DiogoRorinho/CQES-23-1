using GestorEventos.Aplicacao;
using GestorEventos.Partilhado;
using System;
using System.Collections.Generic;

namespace GestorEventos.Inscricoes {
    /* Controller responsável por coordenar os fluxos do módulo de Inscrições.
     * Gere o submenu, valida inputs previsíveis e articula a View com o Model. */
    class InscricaoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly InscricaoView view;
        private readonly InscricaoModel model;
        private bool regressarMenuPrincipal;


        public InscricaoController(AplicacaoController aplicacaoController, InscricaoView view, InscricaoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        // Mantém o submenu de Inscrições ativo até o utilizador regressar ao menu principal.
        public void MostrarMenuModulo() {
            regressarMenuPrincipal = false;

            while (!regressarMenuPrincipal) {
                try {
                    view.MostrarMenuInscricoes();
                    regressarMenuPrincipal = SelecionarOpcao(LerEntrada());
                }
                catch (Exception ex) {
                    view.MostrarErroMenu(ex.Message);
                }
                finally {
                    view.FinalizarOperacaoMenu();
                }
            }
        }

        public bool SelecionarOpcao(string opcao) {
            switch (NormalizarOpcao(opcao)) {
                case "1":
                    CriarInscricao();
                    return false;

                case "2":
                    AlterarInscricao();
                    return false;

                case "3":
                    CancelarInscricao();
                    return false;

                case "4":
                    ListarInscricoes();
                    return false;

                case "0":
                    RegressarMenuPrincipal();
                    return true;

                default:
                    view.MostrarMensagem("Opcao de inscricoes invalida. Escolha 1, 2, 3, 4 ou 0 para regressar ao menu principal.");
                    return false;
            }
        }

        // Coordena o fluxo de criação de inscrição, desde a escolha do evento até ao resultado final.
        private void CriarInscricao() {
            List<EventoDisponivel> eventosDisponiveis = model.ListarEventosDisponiveis();

            if (eventosDisponiveis == null || eventosDisponiveis.Count == 0) {
                view.MostrarMensagem("Nao existem eventos registados.");
                return;
            }

            view.MostrarListaEventos(eventosDisponiveis);

            if (!ExisteEventoDisponivelParaInscricao(eventosDisponiveis)) {
                view.MostrarMensagem("Nao existem eventos ativos com disponibilidade para novas inscricoes.");
                return;
            }

            view.SolicitarDadosCriacao();

            int idEvento = LerIdEventoValido(eventosDisponiveis);

            if (idEvento == 0) {
                return;
            }

            EventoDisponivel? eventoSelecionado = ObterEventoDaLista(eventosDisponiveis, idEvento);
            if (eventoSelecionado == null) {
                view.MostrarMensagem("Evento nao encontrado.");
                return;
            }

            DadosInscricao dados = new DadosInscricao {
                IdEvento = idEvento,
                NomeParticipante = LerTextoNaoVazio("Nome do inscrito: "),
                EmailParticipante = LerTextoNaoVazio("Email do inscrito: "),
                IdadeParticipante = LerInteiroPositivo("Idade do inscrito: ")
            };

            while (true) {
                int vagasDisponiveis = ObterVagasLivresEvento(idEvento);

                if (vagasDisponiveis <= 0) {
                    view.MostrarMensagem("Nao existem vagas disponiveis para este evento.");
                    view.MostrarMensagem("Criacao de inscricao cancelada.");
                    return;
                }

                view.MostrarMensagem("Vagas disponiveis para este evento: " + vagasDisponiveis);
                int quantidade = LerQuantidadeComLimite(
                    "Numero de inscricoes pretendido (0 para cancelar): ",
                    vagasDisponiveis);

                if (quantidade == 0) {
                    view.MostrarMensagem("Criacao de inscricao cancelada.");
                    return;
                }

                dados.Quantidade = quantidade;
                ResultadoCriacaoInscricao resultado = model.CriarInscricao(dados);

                if (resultado.Sucesso && resultado.BilhetePdf != null) {
                    view.MostrarResultadoOperacaoEBilhete(resultado.Mensagem, resultado.BilhetePdf);
                    return;
                }

                view.MostrarMensagem(resultado.Mensagem);

                if (!MensagemIndicaFaltaVagas(resultado.Mensagem)) {
                    return;
                }

                view.MostrarMensagem("Introduza uma nova quantidade ou 0 para cancelar.");
            }
        }

        // Lista inscrições ativas, valida a escolha do utilizador e coordena a alteração da inscrição selecionada.
        private void AlterarInscricao() {
            List<Inscricao> inscricoesAtivas = model.ListarInscricoesAtivas();
            if (inscricoesAtivas.Count == 0) {
                view.MostrarMensagem("Nao existem inscricoes ativas para alterar.");
                return;
            }

            view.MostrarListaInscricoes(inscricoesAtivas);

            Inscricao? inscricao = LerInscricaoAtivaValidaOuSair(
                inscricoesAtivas,
                view.SolicitarIdInscricaoAlteracao);

            if (inscricao == null) {
                view.MostrarMensagem("Alteracao de inscricao cancelada.");
                return;
            }

            view.MostrarDadosParaEdicao(inscricao);

            int vagasLivresEvento = ObterVagasLivresEvento(inscricao.IdEvento);
            int limiteQuantidade = vagasLivresEvento + inscricao.Quantidade;

            view.MostrarMensagem("Vagas livres no evento da inscricao: " + vagasLivresEvento);
            view.MostrarMensagem("Quantidade atual desta inscricao: " + inscricao.Quantidade);
            view.MostrarMensagem("Quantidade maxima permitida para esta inscricao: " + limiteQuantidade);

            DadosInscricao dados = RecolherDadosAlteracaoInscricao(inscricao, limiteQuantidade);

            if (!model.ValidarAlteracaoInscricao(inscricao.Id, dados)) {
                view.MostrarMensagem("Nao foi possivel alterar a inscricao com os dados indicados.");
                return;
            }

            DocumentoPdf bilhetePdf = model.AlterarInscricao(inscricao.Id, dados);
            view.MostrarResultadoOperacaoEBilhete("Inscricao alterada com sucesso.", bilhetePdf);
        }

        private int ObterVagasLivresEvento(int idEvento) {
            List<EventoDisponivel> eventosDisponiveis = model.ListarEventosDisponiveis();
            EventoDisponivel? evento = ObterEventoDaLista(eventosDisponiveis, idEvento);

            if (evento == null) {
                return 0;
            }
            return evento.Disponibilidade;
        }

        // Garante que a quantidade pedida respeita o limite de vagas disponíveis para o evento selecionado.
        private int LerQuantidadeComLimite(string pedido, int vagasDisponiveis) {
            while (true) {
                view.SolicitarCampoTexto(pedido);
                string entrada = LerEntrada();

                if (int.TryParse(entrada, out int quantidade) && quantidade >= 0) {
                    if (quantidade == 0) {
                        return 0;
                    }

                    if (quantidade <= vagasDisponiveis) {
                        return quantidade;
                    }

                    view.MostrarMensagem("Nao existem vagas suficientes.");
                    view.MostrarMensagem("Vagas disponiveis para este evento: " + vagasDisponiveis);
                    view.MostrarMensagem("Introduza uma quantidade entre 1 e " + vagasDisponiveis + ".");
                    continue;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo ou 0 para cancelar.");
            }
        }

        private bool MensagemIndicaFaltaVagas(string mensagem) {
            return !string.IsNullOrWhiteSpace(mensagem) &&
                   mensagem.IndexOf("vagas", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int LerQuantidadeAlteravelComLimite(string pedido, int valorAtual, int limiteQuantidade) {
            while (true) {
                view.SolicitarCampoTexto(pedido);
                string entrada = LerEntrada();

                if (string.IsNullOrWhiteSpace(entrada)) {
                    return valorAtual;
                }

                if (int.TryParse(entrada, out int quantidade) && quantidade > 0) {
                    if (quantidade <= limiteQuantidade) {
                        return quantidade;
                    }

                    view.MostrarMensagem("Nao existem vagas suficientes.");
                    view.MostrarMensagem("Quantidade maxima permitida para esta inscricao: " + limiteQuantidade);
                    view.MostrarMensagem("Introduza uma quantidade entre 1 e " + limiteQuantidade + ".");
                    continue;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        // Lista inscrições ativas, valida a escolha do utilizador e coordena o cancelamento da inscrição selecionada.
        private void CancelarInscricao() {
            List<Inscricao> inscricoesAtivas = model.ListarInscricoesAtivas();
            if (inscricoesAtivas.Count == 0) {
                view.MostrarMensagem("Nao existem inscricoes ativas para cancelar.");
                return;
            }

            view.MostrarListaInscricoes(inscricoesAtivas);

            Inscricao? inscricao = LerInscricaoAtivaValidaOuSair(
                inscricoesAtivas,
                view.SolicitarIdInscricaoCancelamento);

            if (inscricao == null) {
                view.MostrarMensagem("Cancelamento de inscricao cancelado.");
                return;
            }

            MostrarDadosCancelamentoInscricao(inscricao);
            view.PedirConfirmacaoCancelamento();

            string confirmacao = NormalizarOpcao(LerEntrada());
            if (confirmacao != "s" && confirmacao != "sim") {
                view.MostrarMensagem("Cancelamento interrompido.");
                return;
            }

            model.CancelarInscricao(inscricao.Id);
            DocumentoPdf comprovativo = model.GerarComprovativoCancelamento(inscricao.Id);
            view.MostrarResultadoOperacaoEBilhete("Inscricao cancelada com sucesso.", comprovativo);
        }

        // Mantém o utilizador no fluxo até escolher uma inscrição válida ou introduzir 0 para sair.    
        private Inscricao? LerInscricaoAtivaValidaOuSair(List<Inscricao> inscricoesAtivas, Action solicitarId) {
            while (true) {
                solicitarId();
                string entrada = LerEntrada();

                if (!int.TryParse(entrada, out int idInscricao) || idInscricao < 0) {
                    view.MostrarMensagem("Introduza um ID valido ou 0 para sair.");
                    continue;
                }

                if (idInscricao == 0) {
                    return null;
                }

                Inscricao? inscricao = EncontrarInscricaoPorId(inscricoesAtivas, idInscricao);
                if (inscricao == null) {
                    view.MostrarMensagem("Introduza um ID valido ou 0 para sair.");
                    continue;
                }

                return inscricao;
            }
        }

        private static Inscricao? EncontrarInscricaoPorId(List<Inscricao> inscricoes, int idInscricao) {
            foreach (Inscricao inscricao in inscricoes) {
                if (inscricao.Id == idInscricao) {
                    return inscricao;
                }
            }
            return null;
        }

        // Solicita ao Model a lista de inscrições e envia-a à View para apresentação tabular.
        private void ListarInscricoes() {
            view.MostrarListaInscricoes(model.ListarInscricoes());
        }

        // Metodo auxiliar para recolher os dados de alteracao de uma inscricao, permitindo ao utilizador manter os valores atuais ou introduzir novos valores
        private DadosInscricao RecolherDadosAlteracaoInscricao(Inscricao inscricao, int limiteQuantidade) {
            return new DadosInscricao {
                IdEvento = inscricao.IdEvento,

                NomeParticipante = LerTextoAlteravel(
                    "Nome do inscrito (Enter para manter o atual): ",
                    inscricao.NomeParticipante),

                EmailParticipante = LerTextoAlteravel(
                    "Email do inscrito (Enter para manter o atual): ",
                    inscricao.EmailParticipante),

                IdadeParticipante = LerInteiroPositivoAlteravel(
                    "Idade do inscrito (Enter para manter o atual): ",
                    inscricao.IdadeParticipante),

                Quantidade = LerQuantidadeAlteravelComLimite(
                    "Numero de inscricoes pretendido (Enter para manter o atual): ",
                    inscricao.Quantidade,
                    limiteQuantidade)
            };
        }

        /* Mantém o utilizador num ciclo de validação até introduzir um ID de evento válido
         * ou escolher 0 para cancelar a operação. */
        private int LerIdEventoValido(List<EventoDisponivel> eventosDisponiveis) {
            while (true) {
                view.SolicitarCampoTexto("ID do evento (0 para sair): ");
                string entrada = LerEntrada();

                if (!int.TryParse(entrada, out int idEvento) || idEvento < 0) {
                    view.MostrarMensagem("Introduza um ID valido ou 0 para sair.");
                    continue;
                }

                if (idEvento == 0) {
                    return 0;
                }

                EventoDisponivel? eventoEncontrado = ObterEventoDaLista(eventosDisponiveis, idEvento);

                if (eventoEncontrado == null) {
                    view.MostrarMensagem("O ID indicado nao corresponde a um evento disponivel.");
                    continue;
                }

                if (!EventoPodeReceberInscricao(eventoEncontrado)) {
                    view.MostrarMensagem("O evento indicado nao esta ativo ou nao tem disponibilidade.");
                    continue;
                }

                return idEvento;
            }
        }

        // Reaproveita os dados já recebidos do Model, evitando uma nova consulta desnecessária.
        private EventoDisponivel? ObterEventoDaLista(List<EventoDisponivel> eventosDisponiveis, int idEvento) {
            foreach (EventoDisponivel evento in eventosDisponiveis) {
                if (evento.Id == idEvento) {
                    return evento;
                }
            }
            return null;
        }

        private bool ExisteEventoDisponivelParaInscricao(List<EventoDisponivel> eventosDisponiveis) {
            foreach (EventoDisponivel evento in eventosDisponiveis) {
                if (EventoPodeReceberInscricao(evento)) {
                    return true;
                }
            }
            return false;
        }

        private bool EventoPodeReceberInscricao(EventoDisponivel evento) {
            return evento != null &&
                   evento.Disponibilidade > 0 &&
                   EstadoAtivo(evento.Estado);
        }

        private bool EstadoAtivo(string estado) {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            return estadoNormalizado == "ativo" || estadoNormalizado == "ativa";
        }

        private string LerTextoNaoVazio(string pedido) {
            while (true) {
                view.SolicitarCampoTexto(pedido);
                string valor = LerEntrada();

                if (!string.IsNullOrWhiteSpace(valor)) {
                    return valor.Trim();
                }

                view.MostrarMensagem("O valor introduzido nao pode estar vazio.");
            }
        }

        /* Metodo auxiliar para ler um texto alteravel, permitindo ao utilizador manter o valor atual ou
         * introduzir um novo valor, validando que o valor introduzido nao esta vazio ou composto apenas por espacos. */
        private string LerTextoAlteravel(string pedido, string valorAtual) {
            view.SolicitarCampoTexto(pedido);
            string valor = LerEntrada();

            if (string.IsNullOrWhiteSpace(valor)) {
                return valorAtual;
            }
            return valor.Trim();
        }

        /* Metodo auxiliar para ler um numero inteiro positivo, solicitando ao utilizador que introduza 
         * um valor e validando que o valor e um numero inteiro positivo. */
        private int LerInteiroPositivo(string pedido) {
            while (true) {
                view.SolicitarCampoTexto(pedido);

                if (int.TryParse(LerEntrada(), out int valor) && valor > 0) {
                    return valor;
                }

                view.MostrarMensagem("Introduza um numero inteiro positivo.");
            }
        }

        /* Metodo auxiliar para ler um numero inteiro positivo alteravel, permitindo ao utilizador manter o valor
         * atual ou introduzir um novo valor, validando que o valor introduzido e um numero inteiro positivo. */
        private int LerInteiroPositivoAlteravel(string pedido, int valorAtual) {
            while (true) {
                view.SolicitarCampoTexto(pedido);
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

        private string LerEntrada() {
            return Console.ReadLine() ?? string.Empty;
        }

        private string NormalizarOpcao(string? opcao) {
            if (string.IsNullOrWhiteSpace(opcao)) {
                return string.Empty;
            }
            return opcao.Trim().ToLowerInvariant();
        }

        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        private void MostrarDadosCancelamentoInscricao(Inscricao inscricao) {
            view.MostrarMensagem("Dados da inscricao selecionada:");
            view.MostrarMensagem("ID: " + inscricao.Id);
            view.MostrarMensagem("Evento: " + inscricao.IdEvento);
            view.MostrarMensagem("Nome: " + inscricao.NomeParticipante);
            view.MostrarMensagem("Email: " + inscricao.EmailParticipante);
            view.MostrarMensagem("Idade: " + inscricao.IdadeParticipante);
            view.MostrarMensagem("Quantidade: " + inscricao.Quantidade);
            view.MostrarMensagem("Estado: " + inscricao.Estado);
        }
    }
}
