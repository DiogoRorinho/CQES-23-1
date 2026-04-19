using GestorEventosEsqueleto.Aplicacao;

namespace GestorEventosEsqueleto {
    class Program {
        static void Main(string[] args) {
            AplicacaoController aplicacaoController = new AplicacaoController();
            aplicacaoController.IniciarPrograma();
        }
    }
}
