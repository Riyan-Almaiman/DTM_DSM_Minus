using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTM_DSM_Minus
{
    internal class subtract
    {

        public static void RasterSubtract(string inputFile1, string inputFile2, string outputFile)
        {
            using (Dataset ds1 = Gdal.Open(inputFile1, Access.GA_ReadOnly))
            using (Dataset ds2 = Gdal.Open(inputFile2, Access.GA_ReadOnly))
            {
                int xSize = ds1.RasterXSize;
                int ySize = ds1.RasterYSize;

                int chunkHeight = 4096;  
                int totalChunks = (int)Math.Ceiling((double)ySize / chunkHeight);

                string[] options = new string[]
                                                {
                                    "TILED=YES",
                                    "BLOCKXSIZE=256",
                                    "BLOCKYSIZE=256",
                                    //"COMPRESS=DEFLATE",
                                    //"PREDICTOR=2",     
                                    //"ZLEVEL=6"         
                                                };
                using (Dataset outDs = Gdal.GetDriverByName("GTiff").Create(outputFile, xSize, ySize, 1, DataType.GDT_Float32, options))
                {
                    outDs.SetProjection(ds1.GetProjection());
                    double[] geoTransform = new double[6];
                    ds1.GetGeoTransform(geoTransform);
                    outDs.SetGeoTransform(geoTransform);

                    for (int chunk = 0; chunk < totalChunks; chunk++)
                    {
                        int startY = chunk * chunkHeight;
                        int height = Math.Min(chunkHeight, ySize - startY);

                        double[] data1 = new double[xSize * height];
                        double[] data2 = new double[xSize * height];

                        ds1.GetRasterBand(1).ReadRaster(0, startY, xSize, height, data1, xSize, height, 0, 0);
                        ds2.GetRasterBand(1).ReadRaster(0, startY, xSize, height, data2, xSize, height, 0, 0);

                        for (int i = 0; i < data1.Length; i++)
                        {
                            data2[i] -= data1[i];
                        }

                        outDs.GetRasterBand(1).WriteRaster(0, startY, xSize, height, data1, xSize, height, 0, 0);
                    }
                }
            }
        }

        public static void CreateOverviews(string rasterPath)
        {
            using (Dataset ds = Gdal.Open(rasterPath, Access.GA_Update))
            {
                int[] overviewList = new int[] { 2, 4, 8, 16 };

     
                ds.BuildOverviews("NEAREST", overviewList);
            }
        }
        public static void subtractFolder(string folderPath, string resultPath)
        {


            Console.WriteLine($"processing {folderPath}");

            string folderName = Path.GetFileName(folderPath);
            string dtmFolderPath = Path.Combine(folderPath, $"{folderName}_DTM");
            string dsmFolderPath = Path.Combine(folderPath, $"{folderName}_DSM");

            if (Directory.Exists(dtmFolderPath))
            {
                string[] dtmFiles = Directory.GetFiles(dtmFolderPath, "*.tif");

                foreach (string dtmFile in dtmFiles)
                {
                    string imageName = Path.GetFileNameWithoutExtension(dtmFile);  
                    string imageNameBase = imageName.Replace("_dtm", "");
                    string result = folderName.Substring(4);

                    string dsmImagePath = Path.Combine(dsmFolderPath,  $"{result}.tif");

                    Console.WriteLine($"DTM: {dtmFile}");
                    Console.WriteLine($"DSM: {dsmImagePath}");

                    // Check if the DSM image exists
                    if (File.Exists(dsmImagePath))
                    {
                        string outputFilePath = Path.Combine(resultPath, imageNameBase + "_minus.tif");
                        RasterSubtract(dtmFile, dsmImagePath, outputFilePath);
                       


                    }
                    else
                    {
                        Console.WriteLine($"DSM image {dsmImagePath} does not exist.");
                    }
                }
            }
            else
            {
                Console.WriteLine("DTM folder path does not exist.");
            }

        }
    }
}