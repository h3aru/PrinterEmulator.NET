using System.Configuration;
using System.Data;
using System.Windows;
using EscPosEmulator.Emulator;
using EscPosEmulator.Networking;
using System.Text;

namespace EscPosEmulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ReceiptPrinter? Printer = null;
        public static NetServer? Server = null;
        
        /// <summary>
        /// 애플리케이션 시작 시 호출됩니다
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // CodePages 인코딩 등록 (EUC-KR, CP949 등 사용 가능)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            // 프린터 초기화
            Printer = new ReceiptPrinter(PaperConfiguration.Default);

            // 네트워크 서버 초기화 (포트 1234)
            Server = new NetServer(1234);
            _ = Server.Run();
        }

        /// <summary>
        /// 애플리케이션 종료 시 호출됩니다
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            // 서버 중지
            Server?.Stop();
        }
    }
}
