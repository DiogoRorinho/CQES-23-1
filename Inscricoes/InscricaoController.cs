using GestorEventosEsqueleto.Aplicacao;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Inscricoes {
    class InscricaoController {
        private readonly AplicacaoController aplicacaoController;
        private readonly InscricaoView view;
        private readonly InscricaoModel model;

        public InscricaoController(AplicacaoController aplicacaoController, InscricaoView view, InscricaoModel model) {
            this.aplicacaoController = aplicacaoController;
            this.view = view;
            this.model = model;
        }

        // O método MostrarMenuModulo é responsável por exibir o menu de inscrições e processar as opções selecionadas pelo usuário. Ele utiliza um loop para permitir que o usuário continue interagindo com o menu até que decida regressar ao menu principal.
        public void MostrarMenuModulo()
        {
            bool regressar = false;

            while (!regressar)
            {
                view.MostrarMenuInscricoes();
                string opcao = Console.ReadLine() ?? string.Empty;
                regressar = SelecionarOpcao(opcao);
            }
        }

        // O método SelecionarOpcao é responsável por interpretar a opção selecionada pelo usuário no menu de inscrições e chamar o método correspondente para cada operação (criação, alteração, cancelamento ou regressar ao menu principal).
        public bool SelecionarOpcao(string opcao)
        {
            switch (opcao.Trim())
            {
                case "1":
                    CriarInscricao();
                    return false;

                case "2":
                    view.MostrarMensagem("Opcao a desenvolver.");   // retirar quando for implementada
                    //AlterarInscricao();                           // descomentar quando for implementada
                    return false;

                case "3":
                    view.MostrarMensagem("Opcao a desenvolver.");   // retirar quando for implementada
                    //CancelarInscricao();                          // descomentar quando for implementada
                    return false;

                case "0":
                    RegressarMenuPrincipal();
                    return true;

                default:
                    view.MostrarMensagem("Opcao de inscricoes invalida. Escolha 1, 2, 3 ou 0 para regressar ao menu principal.");
                    return false;
            }
        }

        // Os métodos CriarInscricao, AlterarInscricao e CancelarInscricao são responsáveis por iniciar os fluxos correspondentes às operações de criação, alteração e cancelamento de inscrições.
        private void CriarInscricao(){
            List<Evento> eventosDisponiveis = model.ListarEventosDisponiveis();

            if (eventosDisponiveis == null || eventosDisponiveis.Count == 0)
            {
                view.MostrarMensagem("Nao existem eventos com vagas disponiveis.");
                return;
            }

            view.MostrarListaEventos(eventosDisponiveis);

            int idEvento = LerIdEventoValido(eventosDisponiveis);
            string nome = LerTextoNaoVazio("Nome do inscrito: ");
            string email = LerTextoNaoVazio("Email do inscrito: ");
            int idade = LerInteiroPositivo("Idade do inscrito: ");
            int quantidade = LerInteiroPositivo("Numero de inscricoes pretendido: ");

            DadosInscricao dados = new DadosInscricao
            {
                IdEvento = idEvento,
                NomeParticipante = nome,
                EmailParticipante = email,
                IdadeParticipante = idade,
                Quantidade = quantidade
            };

            try
            {
                ResultadoCriacaoInscricao resultado = model.CriarInscricao(dados);

                if (resultado.Sucesso)
                {
                    view.MostrarResultadoOperacaoEBilhete(
                        "Inscricao criada com sucesso.",
                        resultado.BilhetePdf);
                }
                else
                {
                    view.MostrarMensagem(resultado.Mensagem);
                }
            }
            catch (Exception ex)
            {
                view.MostrarMensagem("Erro ao criar inscricao: " + ex.Message);
            }
        }

        // O método LerIdEventoValido é responsável por solicitar ao usuário o ID de um evento e validar se o ID corresponde a um evento disponível na lista fornecida. Ele continua solicitando até que um ID válido seja inserido.
        private int LerIdEventoValido(List<Evento> eventosDisponiveis)
        {
            while (true)
            {
                view.SolicitarIdEvento();
                string entrada = Console.ReadLine() ?? string.Empty;

                if (!int.TryParse(entrada, out int idEvento) || idEvento <= 0)
                {
                    view.MostrarMensagem("ID de evento invalido.");
                    continue;
                }

                bool existe = false;
                foreach (Evento evento in eventosDisponiveis)
                {
                    if (evento.Id == idEvento)
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    view.MostrarMensagem("O ID indicado nao corresponde a um evento disponivel.");
                    continue;
                }

                return idEvento;
            }
        }

        // O método LerTextoNaoVazio é responsável por solicitar ao usuário a entrada de um texto e garantir que o valor inserido não seja vazio ou composto apenas por espaços em branco. Ele continua solicitando até que um valor válido seja fornecido.
        private string LerTextoNaoVazio(string pedido)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);
                string valor = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(valor))
                {
                    view.MostrarMensagem("O valor introduzido nao pode estar vazio.");
                    continue;
                }

                return valor.Trim();
            }
        }
        // O método LerInteiroPositivo é responsável por solicitar ao usuário a entrada de um número inteiro positivo. Ele valida a entrada para garantir que seja um número inteiro e que seja maior que zero, continuando a solicitar até que um valor válido seja inserido.
        private int LerInteiroPositivo(string pedido)
        {
            while (true)
            {
                view.SolicitarCampoTexto(pedido);
                string entrada = Console.ReadLine() ?? string.Empty;

                if (!int.TryParse(entrada, out int valor) || valor <= 0)
                {
                    view.MostrarMensagem("Introduza um numero inteiro positivo.");
                    continue;
                }

                return valor;
            }
        }

        // O método RegressarMenuPrincipal é responsável por chamar o método correspondente no AplicacaoController para retornar ao menu principal do programa.
        public void RegressarMenuPrincipal() {
            aplicacaoController.RegressarMenuPrincipal();
        }

        
        // ---------------------------   A IMPLEMENTAR POSTERIORMENTE   ---------------------------
        private void AlterarInscricao()
        {
            view.MostrarListaInscricoes(model.ListarInscricoes());
            // depois pede ID, lê, obtém dados, etc.
        }

        // O método CancelarInscricao é responsável por iniciar o processo de cancelamento de uma inscrição. Ele exibe a lista de inscrições existentes para que o usuário possa selecionar qual deseja cancelar
        // Após a seleção, o Controller pode solicitar a confirmação do cancelamento e, se confirmado, proceder com a operação de cancelamento.
        private void CancelarInscricao()
        {
            view.MostrarListaInscricoes(model.ListarInscricoes());
            // depois pede ID, confirmação, etc.
        }

        // O método SelecionarInscricao é responsável por receber o ID de uma inscrição selecionada pelo usuário, obter os dados correspondentes a essa inscrição e exibi-los na view para que o usuário possa editá-los.
        public void SelecionarInscricao(int idInscricao) {
            Inscricao inscricao = model.ObterInscricao(idInscricao);
            view.MostrarDadosParaEdicao(inscricao);
        }

        // O método IntroduzirDadosAlterados é responsável por receber os dados editados para uma inscrição específica, validar as alterações e, se forem válidas, proceder com a atualização da inscrição. Ele também exibe o resultado da operação e o bilhete atualizado, se aplicável.
        public void IntroduzirDadosAlterados(int idInscricao, DadosInscricao dados) {
            if (model.ValidarAlteracaoInscricao(idInscricao, dados)) {
                DocumentoPdf bilhetePdf = model.AlterarInscricao(idInscricao, dados);
                view.MostrarResultadoOperacaoEBilhete("Inscricao alterada com sucesso.", bilhetePdf);
                view.MostrarMenuInscricoes();
                return;
            }

            view.MostrarErroSemVagas();
        }

        // O método PedirConfirmacaoCancelamento é responsável por solicitar ao usuário a confirmação para cancelar uma inscrição específica. Ele pode exibir uma mensagem de confirmação na view e aguardar a resposta do usuário antes de proceder com o cancelamento.
        public void PedirConfirmacaoCancelamento() {
            view.PedirConfirmacaoCancelamento();
        }

        // O método ConfirmarCancelamento é responsável por realizar a operação de cancelamento de uma inscrição específica após a confirmação do usuário. Ele interage com o model para cancelar a inscrição e, em seguida, exibe o resultado da operação na view.
        public void ConfirmarCancelamento(int idInscricao) {
            model.CancelarInscricao(idInscricao);
            view.MostrarResultadoOperacao("Inscricao cancelada com sucesso.");
            view.MostrarMenuInscricoes();
        }

    }
}
