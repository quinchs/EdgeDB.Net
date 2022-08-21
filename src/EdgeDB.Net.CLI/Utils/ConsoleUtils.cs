using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class ConsoleUtils
    {
        public static string ReadSecretInput()
        {
            string input = "";
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace)
                    input = input.Length > 0 ? input[..^1] : "";
                else if(!char.IsControl(keyInfo.KeyChar))
                    input += keyInfo.KeyChar;
            }
            while (key != ConsoleKey.Enter);

            return input;
        }
    }
}
