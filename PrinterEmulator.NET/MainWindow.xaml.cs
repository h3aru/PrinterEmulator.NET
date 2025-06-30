using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EscPosEmulator.Emulator;
using EscPosEmulator.Logging;
using EscPosEmulator.Utils;
using Image = System.Windows.Controls.Image;

namespace EscPosEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 윈도우가 로드될 때 호출됩니다
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // 프린터 활동 이벤트 구독
            App.Printer!.OnActivityEvent += (o, args) =>
            {
                RefreshUI();
                WindowsUtils.FlashWindow(this);
                WindowsUtils.ExclaimSoft();
            }; 
            
            RefreshUI();
        }

        /// <summary>
        /// 드래그가 윈도우에 들어올 때 호출됩니다
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            // 파일 드래그인지 확인
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 파일이 드롭될 때 호출됩니다
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    
                    // 텍스트 파일인지 확인
                    if (System.IO.Path.GetExtension(filePath).ToLower() == ".txt")
                    {
                        try
                        {
                            LoadReceiptFile(filePath);
                            Logger.Info($"드래그 앤 드롭으로 파일 로드: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"파일 로드 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            Logger.Exception(ex, "드래그 앤 드롭 파일 로드 오류");
                        }
                    }
                    else
                    {
                        MessageBox.Show("텍스트 파일(.txt)만 지원됩니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// 영수증 파일을 로드합니다
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        private void LoadReceiptFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");
            }

            // 파일을 byte[]로 읽어서 FeedEscPosBytes로 넘김
            byte[] rawBytes = File.ReadAllBytes(filePath);
            App.Printer?.FeedEscPosBytes(rawBytes);
        }

        /// <summary>
        /// 리셋 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Resetting");
            
            // 프린터 리셋
            App.Printer!.ReceiptStack.Clear();
            App.Printer.Initialize();
            App.Printer.StartNewReceipt();

            // UI에서 기존 영수증 이미지 제거
            var toRemove = new List<Image>();
            foreach (var childControl in ReceiptImageRoot.Children)
                if (childControl is Image imgControl)
                    toRemove.Add(imgControl);
            toRemove.ForEach(img => ReceiptImageRoot.Children.Remove(img));

            RefreshUI();
        }

        /// <summary>
        /// 테스트 인쇄 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">이벤트 인수</param>
        private void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 테스트 영수증 파일이 있으면 로드하여 인쇄
            if (!File.Exists("test.txt"))
            {
                // 테스트 파일이 없으면 기본 테스트 영수증 생성
                CreateTestReceipt();
            }
            else
            {
                // test.txt 파일을 byte[]로 읽어서 FeedEscPosBytes로 넘김
                byte[] rawBytes = File.ReadAllBytes("test.txt");
                App.Printer?.FeedEscPosBytes(rawBytes);
            }
        }

        /// <summary>
        /// 기본 테스트 영수증을 생성합니다
        /// </summary>
        private void CreateTestReceipt()
        {
            var testCommands = new StringBuilder();
            
            // 프린터 초기화
            testCommands.Append(PrinterHelper.InitializePrinter);
            
            // 제목 (가운데 정렬, 볼드)
            testCommands.Append(PrinterHelper.AlignCenter);
            testCommands.Append(PrinterHelper.BoldOn);
            testCommands.Append("=== 테스트 영수증 ===");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append(PrinterHelper.BoldOff);
            testCommands.Append(PrinterHelper.LineFeed);
            
            // 좌측 정렬로 변경
            testCommands.Append(PrinterHelper.AlignLeft);
            
            // 상품 정보
            testCommands.Append("상품명: 테스트 상품");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append("수량: 1개");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append("단가: 1,000원");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append("금액: 1,000원");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append(PrinterHelper.LineFeed);
            
            // 구분선
            testCommands.Append("------------------------");
            testCommands.Append(PrinterHelper.LineFeed);
            
            // 총액 (볼드)
            testCommands.Append(PrinterHelper.BoldOn);
            testCommands.Append("총액: 1,000원");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append(PrinterHelper.BoldOff);
            testCommands.Append(PrinterHelper.LineFeed);
            
            // 가운데 정렬로 변경
            testCommands.Append(PrinterHelper.AlignCenter);
            testCommands.Append("감사합니다!");
            testCommands.Append(PrinterHelper.LineFeed);
            testCommands.Append(PrinterHelper.LineFeed);
            
            // 커트
            testCommands.Append(PrinterHelper.Cut);
            
            App.Printer?.FeedEscPos(testCommands.ToString());
        }

        /// <summary>
        /// UI를 새로고침합니다
        /// </summary>
        private void RefreshUI()
        {
            // 상태 라벨 업데이트
            Address.Text = $"{App.Server!.EndPoint}";
            Address.Foreground = new SolidColorBrush(App.Server!.IsRunning ? Colors.SpringGreen : Colors.Crimson);

            // 영수증 이미지 업데이트
            foreach (var receipt in App.Printer!.ReceiptStack)
                CreateOrUpdateReceiptControl(ReceiptImageRoot, receipt);
            
            // 스크롤을 맨 아래로 이동
            MainScrollView.ScrollToBottom();
        }

        /// <summary>
        /// 영수증 컨트롤을 생성하거나 업데이트합니다
        /// </summary>
        /// <param name="parentControl">부모 컨트롤</param>
        /// <param name="receipt">영수증 객체</param>
        private void CreateOrUpdateReceiptControl(Panel parentControl, Receipt receipt)
        {
            if (receipt.IsEmpty)
                return;
            
            var guidName = "R" + receipt.Guid.Replace("-", "");
            
            Image? ourControl = null;
            
            // 기존 컨트롤 찾기
            foreach (var childControl in parentControl.Children)
            {
                if (childControl is Image imgControl)
                {
                    if (imgControl.Name == guidName)
                    {
                        ourControl = imgControl;
                        break;
                    }
                }
            }

            // 새 컨트롤 생성
            if (ourControl == null)
            {
                ourControl = new Image();
                ourControl.Name = guidName;
                ourControl.Stretch = Stretch.None;
                ourControl.Margin = new Thickness(0, 0, 0, 10);
                
                parentControl.Children.Add(ourControl);
            }

            // 영수증을 비트맵으로 렌더링하여 이미지 소스 설정
            ourControl.Source = ConvertBitmap(receipt.Render());
        }
        
        /// <summary>
        /// 비트맵을 WPF ImageBrush에서 처리할 수 있는 이미지로 변환합니다
        /// </summary>
        /// <param name="src">비트맵 이미지</param>
        /// <returns>WPF용 BitmapImage</returns>
        public BitmapImage ConvertBitmap(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}