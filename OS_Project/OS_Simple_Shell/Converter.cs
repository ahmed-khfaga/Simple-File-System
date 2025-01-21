using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Converter
    {
        public static byte[] IntToByte(int n)
        {
            return BitConverter.GetBytes(n);
        }
        public static int ByteToInt(byte[] bytes)
        {

            return BitConverter.ToInt32(bytes, 0);
        }
        public static byte[] IntArrayToByteArray(int[] n)
        {
            return n.SelectMany(IntToByte).ToArray();
        }
        public static int[] ByteArrayToIntArray(byte[] bytes)
        {
            int[] n = new int[bytes.Length / 4];
            for (int i = 0; i < n.Length; i++)
            {
                n[i] = BitConverter.ToInt32(bytes, i * 4);
            }
            return n;
        }
        public static List<byte[]> SplitBytes(byte[] bytes, int chunkSize = 1024)
        {
            List<byte[]> chunks = new List<byte[]>();

            // here we check if cluster is Empty 
            if (bytes.Length == 0)
            {

                chunks.Add(new byte[chunkSize]); // so we add new byte[1024] to set our data 
                return chunks; // we store it in value called chunks are return chunks[1024]
            }

            int fullChunks = bytes.Length / chunkSize;
            int remainder = bytes.Length % chunkSize;


            for (int i = 0; i < fullChunks; i++)
            {
                byte[] c = new byte[chunkSize];
                Array.Copy(bytes, i * chunkSize, c, 0, chunkSize);
                chunks.Add(c);
            }


            if (remainder > 0)
            {
                byte[] lastChunk = new byte[chunkSize];
                Array.Copy(bytes, fullChunks * chunkSize, lastChunk, 0, remainder);
                chunks.Add(lastChunk);
            }

            return chunks;
        }
        // Converts a string to a array of bytes
        public static byte[] StringToByteArray(string str) => Encoding.UTF8.GetBytes(str);
        public static string ByteArrayToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static List<byte> Directory_EntryToBytes(Directory_Entry E)
        {
            List<byte> list = new List<byte>(32);
            list.AddRange(StringToByteArray(new string(E.Dir_Namee)));

            list.Add(E.dir_Attr);

            //list.AddRange(E.Dir_Empty);
            list.AddRange(E.Dir_Empty.Take(12).Concat(Enumerable.Repeat((byte)0, 12 - E.Dir_Empty.Length)));


            list.AddRange(IntToByte(E.dir_First_Cluster));

            list.AddRange(IntToByte(E.dir_FileSize));
            return list;
        }
        public static Directory_Entry BytesToDirectory_Entry(List<byte> bytes)
        {
            if (bytes.Count != 32)
            {
                bytes = bytes.Take(32).Concat(Enumerable.Repeat((byte)0, 32 - bytes.Count)).ToList();
            }
            string rawFileName = Encoding.ASCII.GetString(bytes.ToArray(), 0, 11).Trim('\0');
            string formattedFileName = rawFileName.TrimEnd(); // إزالة المسافات
            // استخراج الحقول
            byte fileAttribute = bytes[11];
            byte[] reservedBytes = bytes.Skip(12).Take(12).ToArray();
            int firstCluster = BitConverter.ToInt32(bytes.ToArray(), 24);
            int fileSize = BitConverter.ToInt32(bytes.ToArray(), 28);

            // إنشاء الكائن
            return new Directory_Entry(formattedFileName.ToCharArray(), fileAttribute, firstCluster)
            {
                Dir_Empty = reservedBytes,
                dir_FileSize = fileSize
            };
        }
        public static List<Directory_Entry> BytesToDirectory_Entries(List<byte> bytes)
        {
            List<Directory_Entry> dirsFiles = new List<Directory_Entry>();

            for (int i = 0; i < bytes.Count; i += 32)
            {
                List<byte> chunk = bytes.GetRange(i, Math.Min(32, bytes.Count - i));
                Directory_Entry entry = BytesToDirectory_Entry(chunk);
                if (chunk[0] == 0)
                {
                    break;
                }
                string name = new string(entry.Dir_Namee).Trim();
                if (name == "")
                {
                    continue;

                }
                else
                {
                    dirsFiles.Add(entry);

                }
            }

            return dirsFiles;
        }
        public static List<byte> Directory_EntriesToBytes(List<Directory_Entry> entries)
        {
            List<byte> bytes = new List<byte>();

            foreach (Directory_Entry entry in entries)
            {
                bytes.AddRange(Directory_EntryToBytes(entry));
            }

            return bytes;
        }
    }
}
