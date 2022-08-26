using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Net.CLI.Utils
{
    internal class FileUtils
    {
        public static bool WaitForHotFile(string path, int timeout = 5000)
        {
            var start = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    return true;
                }
                catch
                {
                    if ((DateTime.UtcNow - start).TotalMilliseconds >= timeout)
                        return false;

                    Thread.Sleep(200);
                }
            }
        }
    }
}
