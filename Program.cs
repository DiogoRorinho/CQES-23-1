using GestorEventos.Aplicacao;
using GestorEventos.Dados;

namespace GestorEventos {
    class Program {
        static void Main(string[] args) {
            BaseDados.Inicializar();

            AplicacaoController aplicacaoController = new AplicacaoController();
            aplicacaoController.IniciarPrograma();
        }
    }
}
