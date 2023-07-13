using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownload.Console
{
    public class ConsoleUtility
    {
        const char _block = '■';
        const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        const string _twirl = "-\\|/";

        public static void WriteProgressBar(int percent, int total, bool update = false)
        {
            if (update)
                System.Console.Write(_back);
            
            System.Console.Write("{0}/{1}", percent, total);
        }

        public static void WriteProgress(int progress, bool update = false)
        {
            if (update)
                System.Console.Write("\b");
            System.Console.Write(_twirl[progress % _twirl.Length]);
        }
    }
}
