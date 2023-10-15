using System;
using System.IO;  
using MaxRev.Gdal.Core;
using OSGeo.GDAL;
using OSGeo.OSR;

namespace DTM_DSM_Minus
{
    internal class Program
    {
        static void Main(string[] args)
        {

            GdalBase.ConfigureAll();
            Gdal.AllRegister();

            Console.Write("Enter folder path: ");
            string folderPathInput = Console.ReadLine();
            string folderPath = Path.GetFullPath(folderPathInput); 

            Console.Write("Enter destination path: ");
            string destinationInput = Console.ReadLine();
            string destination = Path.GetFullPath(destinationInput);  

            subtract.subtractFolder(folderPath, destination);
        }
    }
}
