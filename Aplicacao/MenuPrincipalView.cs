using System;

namespace GestorEventos.Aplicacao {
    /* View responsável pela apresentação do menu principal e das mensagens
     * globais de arranque, erro, regresso e encerramento da aplicação. */
    class MenuPrincipalView {
        public void ApresentarBoasVindas() {
            Console.WriteLine("Bem-vindo ao gestor de eventos.\nDesenvolvida por CQES23+1.");
        }

        public void MostrarMenuPrincipal() {
            Console.WriteLine();
            Console.WriteLine("=== Menu principal ===");
            Console.WriteLine("1 - Eventos");
            Console.WriteLine("2 - Inscricoes");
            Console.WriteLine("3 - Relatorios");
            Console.WriteLine("0 - Terminar");
            Console.Write("Escolha uma opcao: ");
        }

        public void MostrarOpcaoInvalida() {
            Console.WriteLine("Opcao invalida. Escolha 1, 2, 3 ou 0 para Terminar.");
        }

        public void MostrarErroMenu(string mensagem) {
            Console.WriteLine(string.Format("Erro no menu principal: {0}", mensagem));
        }

        public void ApresentarMensagemEncerramento() {
            Console.WriteLine("Aplicacao terminada.");
        }

        public void MostrarMensagemRegresso() {
            Console.WriteLine("A regressar ao menu principal.");
        }

        // Garante separação visual entre iterações do menu na consola.
        public void FinalizarOperacaoMenu() {
            Console.WriteLine();
        }
    }
}
