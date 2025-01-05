using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Mini_FAT
    {
        public static int[] FAT = new int[1024];

        public static Directory Root;
        public static void Initialization()
        {
            FAT[0] = -1;
            FAT[1] = 2;
            FAT[2] = 3;
            FAT[3] = 4;
            FAT[4] = -1;
            for (int i = 5; i < FAT.Length; i++)
            {
                FAT[i] = 0;
            }
        }
        public static byte[] create_Super_Block()
        {

            byte[] bytes = new byte[1024];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }
            return bytes;
        }
        public static void write_FAT()
        {
            byte[] Fatbytes = Converter.IntArrayToByteArray(FAT);
            List<byte[]> bytes = Converter.SplitBytes(Fatbytes);
            for (int i = 0; i < bytes.Count; i++)
            {
                Virtual_Disk.write_Cluster(bytes[i], i + 1);
            }
            Virtual_Disk.disk.Flush();
        }
        public static void readFAT()
        {
            List<byte> bytes = new List<byte>();
            // i start from 1 cause 0 for superBlock 
            for (int i = 1; i <= 4; i++)
            {
                bytes.AddRange(Virtual_Disk.read_Cluster(i));
            }
            FAT = Converter.ByteArrayToIntArray(bytes.ToArray());
        }
        public static void print_Fat()
        {
            Console.WriteLine("FAT has the following");
            for (int i = 0; i < 1024; i++)
            {
                Console.WriteLine($" FAT[{i}] =  {FAT[i]} ");
            }
        }
        public static void set_FAT(int[] fatArray)
        {
            if (fatArray.Length == FAT.Length)
            {
                Array.Copy(fatArray, FAT, fatArray.Length);

            }
            else
                Console.WriteLine("Out Of Range , Your size must be 1024");
        }
        public static int get_Availabel_Clusters()
        {
            int counter = 0;
            for (int i = 5; i < 1024; i++)
            {
                if (FAT[i] == 0)
                {
                    counter++;
                }
            }
            return counter;
        }
        public static int get_Availabel_Cluster()
        {
            for (int i = 5; i < 1024; i++)
            {
                if (FAT[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }
        public static int get_Free_Size()
        {
            return get_Availabel_Clusters() * 1024;
        }
        public static void InitializeOrOpenFileSystem(string name)
        {
            Virtual_Disk.create_Or_Open_Disk(name);

            if (Virtual_Disk.is_New())
            {
                byte[] superBlock = create_Super_Block();

                Virtual_Disk.write_Cluster(superBlock, 0);

                Initialization();

                write_FAT();
                Root = new Directory("N:".ToCharArray(), 0x10, 5, null);
                Root.Write_Directory();
            }
            else
            {
                readFAT();
                Root = new Directory("N:".ToCharArray(), 0x10, 5, null);
                Root.Read_Directory();
            }
        }
        //setClusterPointer
        public static void setNext(int clusterIndex, int status)
        {
            if (clusterIndex >= 0 && clusterIndex < 1024)
            {
                FAT[clusterIndex] = status;
            }
        }
        //getClusterStatus
        public static int getNext(int clusterindex)
        {
            return (clusterindex >= 0 && clusterindex < 1024) ? FAT[clusterindex] : -1;
        }
        public static void closeTheSystem()
        {
            write_FAT();
        }
    }
}
