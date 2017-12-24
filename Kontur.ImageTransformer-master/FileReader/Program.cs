using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] imageArray = File.ReadAllBytes(@"C:\Users\Mega\Pictures\zebra.png");
            string imageSmallInBase64 = Convert.ToBase64String(imageArray);
            //var content = File.ReadAllLines(@"C:\Users\Mega\Documents\K.S.2018\capture.txt");
        }
    }
}
