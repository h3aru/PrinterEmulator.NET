using System;

namespace EscPosEmulator
{
    public class PrinterHelper
    {
        /// <summary>NUL</summary>
        public static string NUL = Convert.ToString((char)0);
        /// <summary>SOH</summary>
        public static string SOH = Convert.ToString((char)1);
        /// <summary>STX</summary>
        public static string STX = Convert.ToString((char)2);
        /// <summary>ETX</summary>
        public static string ETX = Convert.ToString((char)3);
        /// <summary>EOT</summary>
        public static string EOT = Convert.ToString((char)4);
        /// <summary>ENQ</summary>
        public static string ENQ = Convert.ToString((char)5);
        /// <summary>ACK</summary>
        public static string ACK = Convert.ToString((char)6);
        /// <summary>BEL</summary>
        public static string BEL = Convert.ToString((char)7);
        /// <summary>BS</summary>
        public static string BS = Convert.ToString((char)8);
        /// <summary>TAB</summary>
        public static string TAB = Convert.ToString((char)9);
        /// <summary>VT</summary>
        public static string VT = Convert.ToString((char)11);
        /// <summary>FF</summary>
        public static string FF = Convert.ToString((char)12);
        /// <summary>CR</summary>
        public static string CR = Convert.ToString((char)13);
        /// <summary>SO</summary>
        public static string SO = Convert.ToString((char)14);
        /// <summary>SI</summary>
        public static string SI = Convert.ToString((char)15);
        /// <summary>DLE</summary>
        public static string DLE = Convert.ToString((char)16);
        /// <summary>DC1</summary>
        public static string DC1 = Convert.ToString((char)17);
        /// <summary>DC2</summary>
        public static string DC2 = Convert.ToString((char)18);
        /// <summary>DC3</summary>
        public static string DC3 = Convert.ToString((char)19);
        /// <summary>DC4</summary>
        public static string DC4 = Convert.ToString((char)20);
        /// <summary>NAK</summary>
        public static string NAK = Convert.ToString((char)21);
        /// <summary>SYN</summary>
        public static string SYN = Convert.ToString((char)22);
        /// <summary>ETB</summary>
        public static string ETB = Convert.ToString((char)23);
        /// <summary>CAN</summary>
        public static string CAN = Convert.ToString((char)24);
        /// <summary>EM</summary>
        public static string EM = Convert.ToString((char)25);
        /// <summary>SUB</summary>
        public static string SUB = Convert.ToString((char)26);
        /// <summary>ESC</summary>
        public static string ESC = Convert.ToString((char)27);
        /// <summary>FS</summary>
        public static string FS = Convert.ToString((char)28);
        /// <summary>GS</summary>
        public static string GS = Convert.ToString((char)29);
        /// <summary>RS</summary>
        public static string RS = Convert.ToString((char)30);
        /// <summary>US</summary>
        public static string US = Convert.ToString((char)31);
        /// <summary>Space</summary>
        public static string Space = Convert.ToString((char)32);

        public static string CutLast = Convert.ToString((char)48);

        #region 기능 커맨드 모음
        /// <summary> 프린터 초기화</summary>
        public static string InitializePrinter = ESC + "@";

        /// <summary>ASCII LF</summary>
        public static string LineFeed = Convert.ToString((char)10);

        /// <summary>
        /// 라인피드
        /// </summary>
        /// <param name="val">라인피드시킬 줄 수</param>
        /// <returns>변환된 문자열</returns>
        public static string LineFeeds(int val)
        {
            return ESC + "d" + DecimalToCharString(val);
        }

        /// <summary>볼드 On</summary>
        public static string BoldOn = ESC + "E" + DecimalToCharString(1);

        /// <summary>볼드 Off</summary>
        public static string BoldOff = ESC + "E" + DecimalToCharString(0);

        /// <summary>언더라인 On</summary>
        public static string UnderlineOn = ESC + "-" + DecimalToCharString(1);

        /// <summary>언더라인 Off</summary>
        public static string UnderlineOff = ESC + "-" + DecimalToCharString(0);

        /// <summary>흑백반전 On</summary>
        public static string ReverseOn = GS + "B" + DecimalToCharString(1);

        /// <summary>흑백반전 Off</summary>
        public static string ReverseOff = GS + "B" + DecimalToCharString(0);

        /// <summary>좌측정렬</summary>
        public static string AlignLeft = ESC + "a" + DecimalToCharString(0);

        /// <summary>가운데정렬</summary>
        public static string AlignCenter = ESC + "a" + DecimalToCharString(1);

        /// <summary>우측정렬</summary>
        public static string AlignRight = ESC + "a" + DecimalToCharString(2);

        /// <summary>커트</summary>
        //public static string Cut = GS + "V" + DecimalToCharString(1);
        //public static string Cut = GS + "V" + CutLast;
        public static string Cut = ESC + "i";

        public static string FontSelect = GS + "f" + DecimalToCharString(0);

        public static string TestPrint = GS + "(" + "A" + DecimalToCharString(1) + DecimalToCharString(0) + DecimalToCharString(1) + DecimalToCharString(2);

        public static string ConvertFontSize(int width, int height)
        {
            var result ="0";
            int _w, _h;

            //가로변환 (MPrinterHelper와 동일)
            if (width == 1)
                _w = 0;
            else if (width == 2)
                _w = 16;
            else if (width == 3)
                _w = 32;
            else if (width == 4)
                _w = 48;
            else if (width == 5)
                _w = 64;
            else if (width == 6)
                _w = 80;
            else if (width == 7)
                _w = 96;
            else if (width == 8)
                _w = 112;
            else _w = 0;

            //세로변환 (MPrinterHelper와 동일)
            if (height == 1)
                _h = 0;
            else if (height == 2)
                _h = 1;
            else if (height == 3)
                _h = 2;
            else if (height == 4)
                _h = 3;
            else if (height == 5)
                _h = 4;
            else if (height == 6)
                _h = 5;
            else if (height == 7)
                _h = 6;
            else if (height == 8)
                _h = 7;
            else _h = 0;

            //가로x세로
            int sum = _w + _h;
            result = GS + "!" + DecimalToCharString(sum);

            return result;
        }

        #endregion 기능 커맨드 모음 끝

        /// <summary>
        /// Decimal을 캐릭터 변환 후 스트링을 반환 합니다.
        /// </summary>
        /// <param name="val">커맨드 숫자</param>
        /// <returns>변환된 문자열</returns>
        private static string DecimalToCharString(decimal val)
        {
            var result ="";

            try
            {
                result = Convert.ToString((char)val);
            }
            catch { }

            return result;
        }
    }
}