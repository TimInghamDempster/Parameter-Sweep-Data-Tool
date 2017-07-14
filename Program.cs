using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameter_Sweep_Data_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"C:\Users\Tim\Dropbox\Phd\Publication\Field Spread\parameter sweep data";
            string[] files = System.IO.Directory.GetFiles(folder);

            foreach (string filename in files)
            {
                string name = System.IO.Path.GetFileName(filename);
                name = name.Remove(name.Length - 8);
            }
            int i = 0;
        }
    }
}
