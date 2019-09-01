using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SetDigitSubstitution
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetLocaleInfo(int locale, int type, string data, int dataSize);
        private const int LOCALE_IDIGITSUBSTITUTION = 4116;
        private const int LOCALE_SNATIVEDIGITS = 19;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetLocaleInfo(int locale, int type, string data);
        private const int WM_SETTINGSCHANGE = 0x001A;
        private const int HWND_BROADCAST = 65535;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(int hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(int hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [STAThread]
        static int Main(string[] args)
        {
            if (ArgumentsAreValid(args, out int lcid, out DigitShapes? shapes, out string digits))
            {
                try { Apply(lcid, shapes, digits); }
                catch (Win32Exception e)
                {
                    Console.WriteLine(e.Message);
                    return e.HResult;
                }

                return 0;
            }

            new Application().Run(new MainWindow());
            return 0;
        }

        private static bool ArgumentsAreValid(string[] args, out int lcid, out DigitShapes? shapes, out string digits)
        {
            lcid = CultureInfo.CurrentCulture.LCID;
            shapes = null;
            digits = null;

            // [shapes] [zero or digits]

            if (args == null || args.Length != 2)
                return false;

            if (ArgumentsParseShapes(args[0], out DigitShapes parsed))
                shapes = parsed;
            else
                return false;

            if (ArgumentsParseZero(args[1], out int zero))
                digits = DigitsFrom(zero);
            else
                digits = args[1]; // pass through to the API

            return true;
        }
        private static bool ArgumentsParseShapes(string value, out DigitShapes shapes)
        {
            if (Enum.TryParse(value, true, out shapes))
                return true;

            if ("native".Equals(value, StringComparison.CurrentCultureIgnoreCase) || "national".Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                shapes = DigitShapes.NativeNational;
                return true;
            }

            return false;
        }
        private static bool ArgumentsParseZero(string value, out int zero)
        {
            if (int.TryParse(value, NumberStyles.HexNumber, null, out zero))
                return true;

            string data = Properties.Resources.Data;
            int index = data.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0)
                return false;

            int lineIndex = data.LastIndexOf('\n', index);
            zero = char.ConvertToUtf32(data, lineIndex + 1);
            return true;
        }

        private static string DigitsFrom(int zero)
        {
            return string.Concat(from digit in Enumerable.Range(zero, 10)
                                 select char.ConvertFromUtf32(digit));
        }

        public static void Apply(int lcid, DigitShapes? shapes, string digits)
        {
            Console.WriteLine("Applying...");
            bool result;

            if (digits != null)
            {
                result = SetLocaleInfo(lcid, LOCALE_SNATIVEDIGITS, digits + "\0");
                if (!result) throw new Win32Exception();
            }

            if (shapes != null)
            {
                result = SetLocaleInfo(lcid, LOCALE_IDIGITSUBSTITUTION, (int)shapes + "\0");
                if (!result) throw new Win32Exception();
            }

            Console.WriteLine("Notifying...");

            IntPtr intl = Marshal.StringToHGlobalUni("intl\0");
            SendMessage(HWND_BROADCAST, WM_SETTINGSCHANGE, IntPtr.Zero, intl);
            Marshal.FreeHGlobal(intl);
        }
        

        // requires http://www.unicode.org/Public/UCD/latest/ucd/NamesList.txt
        // and http://www.unicode.org/Public/UCD/latest/ucd/UnicodeData.txt in the current directory,
        // outputs Data.txt that needs to be put in the project directory for recompilation 
        private static void PrepareData()
        {
            Dictionary<int, int> numbers = new Dictionary<int, int>();
            List<string> preparedLines = new List<string>();

            foreach (string line in File.ReadLines("UnicodeData.txt"))
            {
                string[] tokens = line.Split(';');
                if (tokens[2] == "Nd")
                {
                    int codepoint = int.Parse(tokens[0], NumberStyles.HexNumber);
                    int number = int.Parse(tokens[6]);
                    numbers[codepoint] = number;
                }
            }

            string lastSubsection = null;
            string lastSection = null;
            string currentSubsection = null;
            string currentSection = null;
            int[] digits = null;
            foreach (string line in File.ReadLines("NamesList.txt"))
            {
                if (line.StartsWith("@\t"))
                    currentSubsection = line;

                if (line.StartsWith("@@\t"))
                    currentSection = line;

                if (char.IsLetterOrDigit(line, 0))
                {
                    int tab = line.IndexOf('\t');
                    if (int.TryParse(line.Substring(0, tab), NumberStyles.HexNumber, null, out int codepoint))
                        if (numbers.TryGetValue(codepoint, out int number))
                        {
                            if (currentSubsection != lastSubsection || currentSection != lastSection)
                            {
                                if (digits != null)
                                {
                                    string preparedLine = PrepareLine(lastSection, lastSubsection, digits);
                                    Console.WriteLine(preparedLine);
                                    preparedLines.Add(preparedLine);
                                }

                                lastSubsection = currentSubsection;
                                lastSection = currentSection;
                                digits = new int[10];
                            }

                            digits[number] = codepoint;
                        }
                }
            }

            File.WriteAllLines("Data.txt", preparedLines);
        }
        private static string PrepareLine(string sectionLine, string subsectionLine, int[] digits)
        {
            string[] sectionTokens = sectionLine.Split('\t');
            string[] subsectionTokens = subsectionLine.Split('\t');

            string sectionName = sectionTokens[2];
            string subsectionName = subsectionTokens[2];

            StringBuilder s = new StringBuilder(digits.Length * 2);
            for (int i = 0; i < digits.Length; i++)
                s.Append(char.ConvertFromUtf32(digits[i]));

            return $"{s};{sectionName};{subsectionName}";
        }
    }
}
