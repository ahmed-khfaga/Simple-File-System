using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class File_Entry : Directory_Entry
    {
        public string content;
        public Directory parent;

        public File_Entry(char[] name, byte dir_attr, int dir_First_Cluster, int fz, Directory pa, string Content = "") : base(name, dir_attr, dir_First_Cluster, fz)
        {
            this.content = Content;
            this.parent = pa;
        }
        public File_Entry(Directory_Entry d, Directory pa) : base(d.Dir_Namee, d.dir_Attr, d.dir_First_Cluster, d.dir_FileSize)
        {
            for (int i = 0; i < 12; i++)
            {
                Dir_Empty[i] = d.Dir_Empty[i];
            }
            this.content = "";
            dir_FileSize = d.dir_FileSize;
            string content = this.content;
            if (pa != null)
                this.parent = pa;
        }

        public int Get_My_Size_On_Disk()
        {
            int size = 0;
            if (dir_First_Cluster != 0)
            {
                int cluster = dir_First_Cluster;
                int next = Mini_FAT.getNext(cluster);
                do
                {
                    size++;
                    cluster = next;
                    if (cluster != -1)
                        next = Mini_FAT.getNext(cluster);
                } while (cluster != -1);
            }
            return size;
        }

        public Directory_Entry GetDirectory_Entry()
        {
            Directory_Entry m = new Directory_Entry(Dir_Namee, dir_Attr, dir_First_Cluster);
            for (int i = 0; i < 12; i++)
            {
                m.Dir_Empty[i] = Dir_Empty[i];
            }
            m.dir_FileSize = dir_FileSize;

            return m;
        }

        public void Write_File_Content()
        {
            Directory_Entry o = GetDirectory_Entry();
            if (content != string.Empty)
            {
                byte[] contentBYTES = Converter.StringToByteArray(content);
                List<byte[]> bytesls = Converter.SplitBytes(contentBYTES);
                int cluster_FAT_Index;
                if (dir_First_Cluster != 0)
                {

                    cluster_FAT_Index = dir_First_Cluster;
                }
                else
                {
                    cluster_FAT_Index = Mini_FAT.get_Availabel_Cluster();
                    if (cluster_FAT_Index != -1)
                    {
                        dir_First_Cluster = cluster_FAT_Index;
                    }
                }
                int last_Cluster = -1;
                for (int i = 0; i < bytesls.Count; i++)
                {

                    Virtual_Disk.write_Cluster(bytesls[i], cluster_FAT_Index);
                    Mini_FAT.setNext(cluster_FAT_Index, -1);
                    if (last_Cluster != -1)
                    {
                        Mini_FAT.setNext(last_Cluster, cluster_FAT_Index);
                    }
                    last_Cluster = cluster_FAT_Index;
                    cluster_FAT_Index = Mini_FAT.get_Availabel_Cluster();
                }
            }
            if (content == string.Empty)
            {
                if (dir_First_Cluster != 0)
                {
                    Empty_My_Clusters();
                }
                dir_First_Cluster = 0;
            }
            if (parent != null)
            {
                Directory_Entry n = GetDirectory_Entry();

                parent.Update_Content(o, n);

            }
            Mini_FAT.write_FAT();
        }

        public void Read_File_Content()
        {
            if (dir_First_Cluster != 0)
            {
                int clusterIndex = dir_First_Cluster;
                int next = Mini_FAT.getNext(clusterIndex);
                List<byte> ls = new List<byte>();
                do
                {
                    ls.AddRange(Virtual_Disk.read_Cluster(clusterIndex));
                    clusterIndex = next;
                    if (clusterIndex != -1)
                    {
                        next = Mini_FAT.getNext(clusterIndex);
                    }
                } while (clusterIndex != -1);
                content = Converter.ByteArrayToString(ls.ToArray());
            }
        }
        public void Empty_My_Clusters()
        {
            if (dir_First_Cluster != 0)
            {
                int clusterIndex = dir_First_Cluster;
                int next = Mini_FAT.getNext(clusterIndex);
                do
                {
                    Virtual_Disk.write_Cluster(new byte[1024], clusterIndex);

                    Mini_FAT.setNext(clusterIndex, 0);
                    clusterIndex = next;
                    if (clusterIndex != -1)
                    {
                        next = Mini_FAT.getNext(clusterIndex);
                    }
                } while (clusterIndex != -1);
            }
        }
        public void Delete_File(string fileName)
        {
            if (parent != null)
            {
                Empty_My_Clusters();
                parent.Read_Directory();
                int indexParent = parent.search_Directory(fileName);
                if (indexParent != -1)
                {
                    parent.DirectoryTable.RemoveAt(indexParent);
                    parent.Write_Directory();
                    Virtual_Disk.disk.Flush();
                }
            }
            Mini_FAT.write_FAT();
        }      
        public void Print_Content()
        {
            Console.Write($" \n {Dir_Namee} \n\n {content} \n \n");

        }
    }
}
