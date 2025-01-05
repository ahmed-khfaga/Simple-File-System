using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Virtual_Disk
    {
        public static FileStream disk;
        public const int clusterSize = 1024;
        public const int clusters = 1024;
        public const int diskSize = clusterSize * clusters;
        private static bool isnew = false;
        public static void create_Or_Open_Disk(string path)
        {
            if (!File.Exists(path))
            {
                disk = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                isnew = true;
            }
            else
            {
                disk = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            }
        }
        public static void write_Cluster(byte[] clusterdata, int clusterIndex)
        {
            disk.Seek(clusterIndex * clusterSize, SeekOrigin.Begin);
            disk.Write(clusterdata, 0, clusterdata.Length);
            disk.Flush();
        }
        public static byte[] read_Cluster(int clusterIndex)
        {
            disk.Seek(clusterIndex * 1024, SeekOrigin.Begin);
            byte[] bytes = new byte[1024];
            disk.Read(bytes, 0, 1024);
            return bytes;
        }
        public static int get_Free_Space()
        {
            disk.Seek(0, SeekOrigin.End);
            return diskSize - (int)disk.Position;
        }
        public static bool is_New()
        {
            return isnew == true ? true : false;
        }
    }
}
