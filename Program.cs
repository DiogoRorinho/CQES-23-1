using GestorEventos.Aplicacao;

namespace GestorEventos {
    class Program {
        static void Main(string[] args) {
            AplicacaoController aplicacaoController = new AplicacaoController();
            aplicacaoController.IniciarPrograma();
        }
    }
}
