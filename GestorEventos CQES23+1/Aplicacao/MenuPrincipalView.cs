using System;

namespace GestorEventosEsqueleto.Aplicacao {
    class MenuPrincipalView {
        public void ApresentarBoasVindas() {
            Console.WriteLine("Bem-vindo ao gestor de eventos.");
        }

        public void MostrarMenuPrincipal() {
            Console.WriteLine("Menu principal: Eventos | Inscricoes | Relatorios | Terminar");
        }

        public void MostrarOpcaoInvalida() {
            Console.WriteLine("Opcao invalida.");
        }

        public void ApresentarMensagemEncerramento() {
            Console.WriteLine("Aplicacao terminada.");
        }
    }
}
