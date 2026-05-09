using GestorEventos.Aplicacao;
using GestorEventos.Dados;
using PdfSharp.Fonts;

namespace GestorEventos {
    class Program {
        static void Main(string[] args) {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            BaseDados.Inicializar();

            AplicacaoController aplicacaoController = new AplicacaoController();
            aplicacaoController.IniciarPrograma();
        }
    }
}
