using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
                    //Virtual_Disk.write_Cluster(new byte[1024], clusterIndex);

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
        public static void Copy_CreateFile(File_Entry file_path, string name)
        {
            if (name.Contains("\\") && name.Contains("."))
            {
                int last_index_for_name = name.LastIndexOf("\\");
                string name_of_Directory = name.Substring(0, last_index_for_name); // get fullpath of parent of this file 
                Directory targetDirectory = ExecutionClass.MoveToDir(name_of_Directory, Program.currentDirectory); // move to this directory
                string name_Of_File = name.Substring(last_index_for_name + 1); // get name of File 
                if (targetDirectory == null) // this is mean directory is null and path is error
                {
                    Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                    return;
                }
                else // we found this directory 
                {
                    file_path.Read_File_Content(); // read content of this file to ensure we get the content and convert it from byte to string .
                    int first_Cluster = Mini_FAT.get_Availabel_Cluster(); // get free cluster to this file we want to copy it.
                    int counter = 0; // counter to count file are copied .
                    File_Entry File_are_copied = new File_Entry(name_Of_File.ToCharArray(), file_path.dir_Attr, first_Cluster, file_path.dir_FileSize, targetDirectory, file_path.content); // this is File are Copied .
                    if(targetDirectory.Can_Add_Entry(File_are_copied.GetDirectory_Entry()))
                    {
                        File_are_copied.Write_File_Content(); // write this file and ensure FAT table save this data .
                        targetDirectory.DirectoryTable.Add(File_are_copied); // add this file in Directoy Table .
                        counter++;
                        targetDirectory.Write_Directory();
                        Console.WriteLine($"{new string(targetDirectory.Dir_Namee) + "\\" + name_Of_File}");
                        Console.WriteLine($"{counter} file(s) copied.");
                    }
                    else
                    {
                        Console.WriteLine("No space on disk to copy this file!");
                        return;
                    }
                }
            }
            else if(!name.Contains("."))
            {
                 // get fullpath of parent of this file 
                Directory targetDirectory = ExecutionClass.MoveToDir(name, Program.currentDirectory); // move to this directory
                if (targetDirectory == null) // this is mean directory is null and path is error
                {
                    Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                    return;
                }
                else // we found this directory 
                {
                    file_path.Read_File_Content(); // read content of this file to ensure we get the content and convert it from byte to string .
                    int first_Cluster = Mini_FAT.get_Availabel_Cluster(); // get free cluster to this file we want to copy it.
                    int counter = 0; // counter to count file are copied .
                    File_Entry File_are_copied = new File_Entry(file_path.Dir_Namee, file_path.dir_Attr, first_Cluster, file_path.dir_FileSize, targetDirectory, file_path.content); // this is File are Copied .
                    if (targetDirectory.Can_Add_Entry(File_are_copied.GetDirectory_Entry()))
                    {
                        File_are_copied.Write_File_Content(); // write this file and ensure FAT table save this data .
                        targetDirectory.DirectoryTable.Add(File_are_copied); // add this file in Directoy Table .
                        counter++;
                        targetDirectory.Write_Directory();
                        Console.WriteLine($"{counter} file(s) copied.");
                    }
                    else
                    {
                        Console.WriteLine("No space on disk to copy this file!");
                        return;
                    }                   
                }
            }
            else if(name.Contains("."))
            {
                file_path.Read_File_Content();
                int first_Cluster = Mini_FAT.get_Availabel_Cluster(); // get free cluster to this file we want to copy it.
                int counter = 0; // counter to count file are copied .
                File_Entry File_are_copied = new File_Entry(name.ToCharArray(), file_path.dir_Attr, first_Cluster, file_path.dir_FileSize, Program.currentDirectory, file_path.content); // this is File are Copied .
                if(Program.currentDirectory.Can_Add_Entry(File_are_copied.GetDirectory_Entry()))
                {
                    File_are_copied.Write_File_Content(); // write this file and ensure FAT table save this data .
                    Program.currentDirectory.DirectoryTable.Add(File_are_copied); // add this file in Directoy Table .
                    counter++;
                    Program.currentDirectory.Write_Directory();
                    Console.WriteLine($"{counter} file(s) copied.");
                }
                else
                {
                    Console.WriteLine("No space on disk to copy this file!");
                    return;
                }             
            }          
        }
       
        public static File_Entry OverWrite(File_Entry file, string destinationFileName, string destinationPath)
        {
            Directory targetDirectory;
            File_Entry file_OverWriteed;
            if (destinationPath.Contains(".") && destinationPath.Contains("\\")) // if we go to fullpath to file 
            {
                int last_index_for_name_Dir = destinationPath.LastIndexOf("\\");

                string name_Of_Dir = destinationPath.Substring(0, last_index_for_name_Dir);
                targetDirectory = ExecutionClass.MoveToDir(name_Of_Dir, Program.currentDirectory);
                file.Read_File_Content();
                int index = targetDirectory.search_Directory(destinationFileName);

                Console.WriteLine($"this file \"{destinationFileName}\" is already exist on your disk!");
                Console.WriteLine($"NOTE: Do you want to overwrite this file \"{destinationFileName}\"? Please enter y for Yes or n for No!");
                int first_Cluster = targetDirectory.DirectoryTable[index].dir_First_Cluster;
                int file_Size = targetDirectory.DirectoryTable[index].dir_FileSize;
                file_OverWriteed = new File_Entry(destinationFileName.ToCharArray(), 0x0, first_Cluster, file_Size, Program.currentDirectory, "");
                int counter = 0;
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        file_OverWriteed.content = file.content;
                        file_OverWriteed.dir_FileSize = file.dir_FileSize;
                        file_OverWriteed.Write_File_Content();
                        counter++;
                        Console.WriteLine($"{counter} file(s) copied.");
                        break;
                    }
                    else if (answer == "n")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{destinationFileName}\" , please enter y for Yes n for No!");
                        answer = Console.ReadLine();
                    }
                }
                return file_OverWriteed;
            }
            else if (destinationPath.Contains("\\") && !destinationPath.Contains("."))
            {
                targetDirectory = ExecutionClass.MoveToDir(destinationPath, Program.currentDirectory);
                file.Read_File_Content();
                int index = targetDirectory.search_Directory(destinationFileName);

                Console.WriteLine($"this file \"{destinationFileName}\" is already exist on your disk!");
                Console.WriteLine($"NOTE: Do you want to overwrite this file \"{destinationFileName}\"? Please enter y for Yes or n for No!");
                int first_Cluster = targetDirectory.DirectoryTable[index].dir_First_Cluster;
                int file_Size = targetDirectory.DirectoryTable[index].dir_FileSize;
                file_OverWriteed = new File_Entry(destinationFileName.ToCharArray(), 0x0, first_Cluster, file_Size, Program.currentDirectory, "");
                int counter = 0;
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        file_OverWriteed.content = file.content;
                        file_OverWriteed.dir_FileSize = file.dir_FileSize;
                        file_OverWriteed.Write_File_Content();
                        counter++;
                        Console.WriteLine($"{counter} file(s) copied.");
                        break;
                    }
                    else if (answer == "n")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{destinationFileName}\" , please enter y for Yes n for No!");
                        answer = Console.ReadLine();
                    }
                }
                return file_OverWriteed;
            }
            else if (destinationFileName.Contains("."))
            {
                file.Read_File_Content();
                int index = Program.currentDirectory.search_Directory(destinationFileName);
                Console.WriteLine($"this file \"{destinationFileName}\" is already exist on your disk!");
                Console.WriteLine($"NOTE: Do you want to overwrite this file \"{destinationFileName}\"? Please enter y for Yes or n for No!");
                int first_Cluster = Program.currentDirectory.DirectoryTable[index].dir_First_Cluster;
                int file_Size = Program.currentDirectory.DirectoryTable[index].dir_FileSize;
                file_OverWriteed = new File_Entry(destinationFileName.ToCharArray(), 0x0, first_Cluster, file_Size, Program.currentDirectory, "");
                int counter = 0;
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        file_OverWriteed.content = file.content;
                        file_OverWriteed.dir_FileSize = file.dir_FileSize;
                        file_OverWriteed.Write_File_Content();
                        counter++;
                        Console.WriteLine($"{counter} file(s) copied.");
                        break;
                    }
                    else if (answer == "n")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{destinationFileName}\" , please enter y for Yes n for No!");
                        answer = Console.ReadLine();
                    }
                }
                return file_OverWriteed;
            }
            else // for only directory 
            {
                targetDirectory = ExecutionClass.MoveToDir(destinationPath, Program.currentDirectory);
                file.Read_File_Content();
                int index = targetDirectory.search_Directory(destinationFileName);
                Console.WriteLine($"this file \"{destinationFileName}\" is already exist on your disk!");
                Console.WriteLine($"NOTE: Do you want to overwrite this file \"{destinationFileName}\"? Please enter y for Yes or n for No!");
                int first_Cluster = Program.currentDirectory.DirectoryTable[index].dir_First_Cluster;
                int file_Size = Program.currentDirectory.DirectoryTable[index].dir_FileSize;
                file_OverWriteed = new File_Entry(destinationFileName.ToCharArray(), 0x0, first_Cluster, file_Size, Program.currentDirectory, "");
                int counter = 0;
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        file_OverWriteed.content = file.content;
                        file_OverWriteed.dir_FileSize = file.dir_FileSize;
                        file_OverWriteed.Write_File_Content();
                        counter++;
                        Console.WriteLine($"{counter} file(s) copied.");
                        break;
                    }
                    else if (answer == "n")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{destinationFileName}\" , please enter y for Yes n for No!");
                        answer = Console.ReadLine();
                    }
                }
                return file_OverWriteed;

            }           
        }
         // for export file
        public static bool CheckerMethod(string physical_DISK)
        {
            if (physical_DISK.Contains("\\") && physical_DISK.Contains("."))
            {               
                int last_Index_For_Directory_In_Physical = physical_DISK.LastIndexOf("\\");
                string name_Of_Directory_In_Physical = physical_DISK.Substring(0, last_Index_For_Directory_In_Physical);
                if (System.IO.Directory.Exists(name_Of_Directory_In_Physical))
                {                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(physical_DISK.Contains("\\") && !physical_DISK.Contains("."))
            {
                int last_Index_For_Directory_In_Physical = physical_DISK.LastIndexOf("\\");
                string name_Of_Directory_In_Physical = physical_DISK.Substring(0, last_Index_For_Directory_In_Physical);
                if (System.IO.Directory.Exists(name_Of_Directory_In_Physical))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false ;
            }
        }
        public static void helpermethod_Exported_In_Your_Disk(File_Entry file, string physical_DISK)
        {
            if (physical_DISK.Contains("\\") && physical_DISK.Contains("."))
            {
                file.Read_File_Content();
                string[] pathParts = physical_DISK.Split('\\'); // Split the path (extract name & size)
                int last_Index_For_Directory_In_Physical = physical_DISK.LastIndexOf("\\");
                string name_Of_Directory_In_Physical = physical_DISK.Substring(0, last_Index_For_Directory_In_Physical);
                string name_of_File = pathParts[pathParts.Length - 1]; // get name of File 
                file.Dir_Namee = name_of_File.ToCharArray();
                string content = file.content.Replace("\0", " ").Trim();
                if (System.IO.Directory.Exists(name_Of_Directory_In_Physical))
                {
                    using (StreamWriter sw = new StreamWriter(name_Of_Directory_In_Physical + "\\" + name_of_File))
                    {
                        sw.WriteLine(content);
                    }
                    return;
                }
                else
                {
                    return ;
                }
            }
            else if (physical_DISK.Contains("\\") && !physical_DISK.Contains("."))
            {
                string file_Name = new string(file.Dir_Namee).Replace("\0", " ").Trim();
                file.Read_File_Content();
                string content = (file.content).Replace("\0", " ").Trim();                
                if (System.IO.Directory.Exists(physical_DISK))
                {
                    using (StreamWriter sw = new StreamWriter(physical_DISK + "\\" + file_Name))
                    {
                        sw.WriteLine(content);
                    }
                    return;
                }
                else
                {
                    return ;
                }
            }
            return ;
        }
        public static void helpermethod_Exported_In_EXE_File(File_Entry file, string name)
        {
            int counter = 0;
            file.Read_File_Content();
            string name_File = new string(file.Dir_Namee).Trim('\0');
            string exportedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name_File);
            string content = file.content;
            if (content.Contains("\0"))
            {
                content = content.Replace("\0", " ");
            }
            content = content.Trim();
            File.WriteAllText(exportedPath, content);
            counter++;
            string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
            string name_of_File;
            string name_Parent_Of_File;
            string fullpath_File_Exported;
            if (pathParts.Length > 2)
            {
                name_of_File = pathParts[pathParts.Length - 1]; // get name of File 
                name_Parent_Of_File = pathParts[pathParts.Length - 2]; // get parent directory of this File 
                fullpath_File_Exported = name_Parent_Of_File + "\\" + name_of_File;
                Console.WriteLine($"{fullpath_File_Exported}");
                Console.WriteLine($"\t{counter} file(s) exported.");
                return;
            }
            else
            {
                name_of_File = name;
                name_Parent_Of_File = new string(Program.currentDirectory.Dir_Namee).Trim('\0');
                fullpath_File_Exported = name_Parent_Of_File + "\\" + name_of_File;
                Console.WriteLine($"{fullpath_File_Exported}");
                Console.WriteLine($"\t{counter} file(s) exported.");
                return;
            }
        }
        public void Print_Content()
        {
            Console.Write($"\n{new string(Dir_Namee)}\n\n{content} \n \n");

        }
    }
}
