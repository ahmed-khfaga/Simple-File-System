using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Directory : Directory_Entry
    {
        public Directory Parent;
        public List<Directory_Entry> DirectoryTable;
        public int cluster_Index;
        public Directory(char[] name, byte attr, int first_Cluster, Directory Parent) : base(name, attr, first_Cluster)
        {
            DirectoryTable = new List<Directory_Entry>();

            if (Parent != null)
            {
                this.Parent = Parent;
            }
        }
        public Directory_Entry Get_Directory_Entry()
        {
            Directory_Entry me = new Directory_Entry(this.Dir_Namee, this.dir_Attr, this.dir_First_Cluster);
            for (int i = 0; i < 12; i++)
            {
                me.Dir_Empty[i] = this.Dir_Empty[i];
            }
            me.dir_FileSize = this.dir_FileSize;
            return me;
        }
        public int Get_My_Size_On_Disk()
        {
            int size = 0;
            if (this.dir_First_Cluster != 0)
            {
                int cluster = this.dir_First_Cluster;
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
        public bool Can_Add_Entry(Directory_Entry d)
        {
            bool can = false;
            int needed_Size = (DirectoryTable.Count + 1) * 32;
            int needed_Cluster = needed_Size / 1024;
            int rem = needed_Size % 1024;
            if (rem > 0)
            {
                needed_Cluster++;
            }
            needed_Cluster += d.dir_FileSize / 1024;
            int rem1 = d.dir_FileSize % 1024;
            if (rem1 > 0)
            {
                needed_Cluster++;
            }
            if (Get_My_Size_On_Disk() + Mini_FAT.get_Availabel_Clusters() >= needed_Cluster)
            {
                can = true;
            }
            return can;
        }
        public void Empty_My_Clusters()
        {
            if (this.dir_First_Cluster == 0)
                return;
            if (this.dir_First_Cluster != 0)
            {
                int cluster = this.dir_First_Cluster;
                int next = Mini_FAT.getNext(cluster);

                if (cluster == 5 && next == 0)
                    return;
                do
                {
                    Mini_FAT.setNext(cluster, 0);
                    cluster = next;
                    if (cluster != -1)
                        next = Mini_FAT.getNext(cluster);

                } while (cluster != -1);
            }
        }
        public void Read_Directory()
        {
            DirectoryTable = new List<Directory_Entry>();

            if (dir_First_Cluster != 0)
            {

                cluster_Index = dir_First_Cluster;
                int next = Mini_FAT.getNext(cluster_Index);
                List<byte> ls = new List<byte>();
                if (cluster_Index == 5 && next == 0)
                    return;
                do
                {

                    ls.AddRange(Virtual_Disk.read_Cluster(cluster_Index));
                    cluster_Index = next;
                    if (cluster_Index != -1)
                    {
                        ls.AddRange(Virtual_Disk.read_Cluster(cluster_Index));
                        next = Mini_FAT.getNext(cluster_Index);
                    }
                } while (next != -1);

                DirectoryTable = Converter.BytesToDirectory_Entries(ls);
            }
        }
        public void Write_Directory()
        {
            Directory_Entry o = Get_Directory_Entry();

            List<byte> dirs_Or_Files_Bytes = Converter.Directory_EntriesToBytes(DirectoryTable);

            List<byte[]> bytes = Converter.SplitBytes(dirs_Or_Files_Bytes.ToArray());

            if (this.dir_First_Cluster != 0)
            {
                cluster_Index = this.dir_First_Cluster;
            }
            else
            {
                cluster_Index = Mini_FAT.get_Availabel_Cluster();
                this.dir_First_Cluster = cluster_Index;
            }
            int last_Cluster = -1;
            for (int i = 0; i < bytes.Count; i++)
            {

                Virtual_Disk.write_Cluster(bytes[i], cluster_Index);

                Mini_FAT.setNext(cluster_Index, -1);

                if (last_Cluster != -1)
                {
                    Mini_FAT.setNext(last_Cluster, cluster_Index);
                }

                last_Cluster = cluster_Index;

                cluster_Index = Mini_FAT.get_Availabel_Cluster();
            }

            if (DirectoryTable.Count == 0)
            {
                if (Parent != null)
                {
                    Empty_My_Clusters();
                    dir_First_Cluster = 0;
                }
            }

            if (Parent != null)
            {
                Directory_Entry n = Get_Directory_Entry();

                this.Parent.Update_Content(o, n);
            }

            Mini_FAT.write_FAT();
        }
        public void Update_Content(Directory_Entry OLD, Directory_Entry NEW)
        {
            Read_Directory();
            char[] old_Name = OLD.Dir_Namee;
            string O_Name = new string(old_Name);
            int index = search_Directory(O_Name);
            if (index != -1)
            {
                DirectoryTable[index] = NEW;
            }
            Write_Directory();

        }
        public int search_Directory(string name)
        {

            if (DirectoryTable == null || DirectoryTable.Count == 0)
            {
                Read_Directory();
            }

            for (int i = 0; i < DirectoryTable.Count; i++)
            {
                string dirNameInTable = new string(DirectoryTable[i].Dir_Namee).Trim('\0');
                if (name.Contains("\0"))
                {
                    name = name.Replace("\0", " ");
                }
                if (dirNameInTable.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return i; // Directory found
                }
            }
            return -1; // Directory not found
        }
        public void add_Entry(Directory_Entry d)
        {

            DirectoryTable.Add(d);
            //Write_Directory();
        }
        public void remove_Entry(Directory_Entry d)
        {
            if (DirectoryTable.Count() != 0)
            {
                DirectoryTable.Remove(d);
                Mini_FAT.write_FAT();
            }
            else
            {
                Console.WriteLine($"Error Entry \"{d.Dir_Namee}\" not found");
            }
        }
        public void delete_Directory()
        {
            Empty_My_Clusters();
            if (this.Parent != null)
            {
                Directory_Entry dir_Entry = Get_Directory_Entry();
                Parent.remove_Entry(dir_Entry);
                Parent.Write_Directory();
            }
            if (this.dir_First_Cluster != 0)
                this.dir_First_Cluster = 0;

            Mini_FAT.write_FAT();
        }
    }
}
