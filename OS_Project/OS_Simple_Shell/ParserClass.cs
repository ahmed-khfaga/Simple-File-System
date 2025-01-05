using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class ParserClass
    {
        // method dir to help me 
        public static void Dir()
        {
            int file_Counter = 0;
            int folder_Counter = 0;
            int file_Sizes = 0;
            int total_File_Size = 0;
            string name = new string(Program.currentDirectory.Dir_Namee);
            Console.WriteLine($"Directory of {name} is  \n");
            for (int i = 0; i < Program.currentDirectory.DirectoryTable.Count; i++)
            {
                file_Sizes = 0;
                if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x0)
                {
                    file_Counter++;
                    file_Sizes += Program.currentDirectory.DirectoryTable[i].dir_FileSize;
                    total_File_Size += file_Sizes;
                    string m = string.Empty;
                    m += new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee);
                    Console.WriteLine($"\t\t{file_Sizes}\t\t" + m);
                }
                else if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x10) // لو في فولدر 
                {
                    folder_Counter++;
                    string S = new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee);
                    Console.WriteLine("\t\t<DIR>\t\t" + S.Trim());
                }
            }
            Console.Write($"\t\t\t{file_Counter} File(s)\t ");
            if (file_Counter > 0)
            {
                Console.Write(total_File_Size);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }
            Console.WriteLine($"\t\t\t{folder_Counter} Dir(s)\t {Mini_FAT.get_Free_Size()} bytes free");
        }
        public static void Dir(string name)
        {
            int file_Counter = 0;
            int folder_Counter = 0;
            int file_Sizes = 0;
            int total_File_Size = 0;
            int fol = 2;
            Directory cc = ParserClass.MoveToDir(name, Program.currentDirectory);
            if (cc != null) // this is directory 
            {
                cc.Read_Directory();
                Console.WriteLine($"Directory of {name} : \n");
                Console.WriteLine("\t\t<DIR>\t\t .");
                Console.WriteLine("\t\t<DIR>\t\t ..");
                for (int i = 0; i < cc.DirectoryTable.Count; i++)
                {
                    file_Sizes = 0;
                    if (cc.DirectoryTable[i].dir_Attr == 0x0)
                    {
                        file_Counter++;
                        file_Sizes += cc.DirectoryTable[i].dir_FileSize;
                        total_File_Size += file_Sizes;
                        string m = string.Empty;
                        m += new string(cc.DirectoryTable[i].Dir_Namee);
                        Console.WriteLine($"\t\t{file_Sizes}\t\t" + m);
                    }
                    else if (cc.DirectoryTable[i].dir_Attr == 0x10) // لو في فولدر 
                    {
                        folder_Counter++;
                        string S = new string(cc.DirectoryTable[i].Dir_Namee);
                        Console.WriteLine("\t\t<DIR>\t\t" + S);
                    }
                }
                Console.Write($"\t\t\t{file_Counter} File(s)\t ");
                if (file_Counter > 0)
                {
                    Console.Write(total_File_Size);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }
                Console.WriteLine($"\t\t\t{folder_Counter + fol} Dir(s)\t {Mini_FAT.get_Free_Size()} bytes free");
            }
            else // this is file 
            {
                Console.WriteLine($"Error : this path \"{name}\" is not a directory ");
            }
        }
        public static void Dir_Sup_Dir(bool x)
        {
            Program.currentDirectory.Read_Directory();
            int file_Counter = 0;
            int folder_Counter = 0;
            int file_Sizes = 0;
            int total_File_Size = 0;
            Console.WriteLine($"Directory of \"{new string(Program.currentDirectory.Dir_Namee)}\"  is: ");
            Console.WriteLine();
            if (x == true)
            {
                Console.WriteLine("\t\t<DIR>\t\t . ");
                Console.WriteLine("\t\t<DIR>\t\t .. ");
                folder_Counter += 2;
            }
            for (int i = 0; i < Program.currentDirectory.DirectoryTable.Count; i++)
            {
                if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x0)
                {
                    file_Counter++;
                    file_Sizes += Program.currentDirectory.DirectoryTable[i].dir_FileSize;
                    total_File_Size += file_Sizes;
                    string m = string.Empty;
                    m += new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee);
                    Console.WriteLine($"\t\t{file_Sizes}\t\t" + m);
                }
                else if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x10) // لو في فولدر 
                {
                    folder_Counter++;
                    string S = new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee);
                    Console.WriteLine("\t\t<DIR>\t\t" + S);
                }
                file_Sizes = 0;
            }
            Console.Write($"\t\t\t{file_Counter} File(s)\t ");
            if (file_Counter > 0)
            {
                Console.Write(total_File_Size);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }
            Console.WriteLine($"\t\t\t{folder_Counter} Dir(s)\t {Mini_FAT.get_Free_Size()} bytes free");
        }
        public static Directory MoveToDir(string fullPath, Directory currentDirectory)
        {
            string[] parts = fullPath.Split('\\');
            if (parts.Length == 0)
            {
                Console.WriteLine("Error: Invalid path.");
                return null;
            }
            Directory targetDirectory = currentDirectory;
            string s = new string(currentDirectory.Dir_Namee);
            if (s.Contains("\0"))
                s = s.Replace("\0", " ");
            s = s.Trim();
            if (parts[0].Equals(s.Trim('\0'), StringComparison.OrdinalIgnoreCase))
            {
                parts = parts.Skip(1).ToArray();
            }
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }
                int index = targetDirectory.search_Directory(part);
                if (index == -1)
                {
                    return null;
                }
                Directory_Entry entry = targetDirectory.DirectoryTable[index];
                if ((entry.dir_Attr & 0x10) != 0x10)
                {
                    return null;
                }
                targetDirectory = new Directory(entry.Dir_Namee, entry.dir_Attr, entry.dir_First_Cluster, targetDirectory);
                targetDirectory.Read_Directory();
            }
            return targetDirectory;
        }
        public static File_Entry MoveToFile(string fullPath)
        {
            int lastSlashIndex = fullPath.LastIndexOf('\\');
            if (lastSlashIndex == -1)
            {
                return null;
            }
            string fileName = fullPath.Substring(lastSlashIndex + 1); // Extract file name
            string directoryPath = fullPath.Substring(0, lastSlashIndex); // Remove file name to get directory path
            Directory parentDirectory = MoveToDir(directoryPath, Program.currentDirectory);
            if (parentDirectory == null)
            {
                return null;
            }
            parentDirectory.Read_Directory();
            int index = parentDirectory.search_Directory(fileName);
            if (index == -1)
            {
                return null;
            }
            Directory_Entry fileEntry = parentDirectory.DirectoryTable[index];
            File_Entry e = new File_Entry(fileEntry, Program.currentDirectory);
            File_Entry file = new File_Entry(e.Dir_Namee, e.dir_Attr, e.dir_First_Cluster, e.dir_FileSize, parentDirectory, e.content);
            return file;
        }
        public static void ChangeDirectory(string path)
        {
            if (path == ".")
            {
                return;
            }
            if (path.StartsWith(".."))
            {
                string[] levelsUp = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                int l = levelsUp.Length;
                for (int i = 0; i < l; i++)
                {
                    int lastBackslash = Program.path.LastIndexOf("\\");
                    if (Program.currentDirectory.Parent != null)
                    {
                        Program.currentDirectory = Program.currentDirectory.Parent;
                        Program.path = Program.path.Substring(0, lastBackslash);
                        Program.currentDirectory.Read_Directory();
                    }
                    else
                    {
                        return;
                    }
                }
                return;
            }
            if (path.Contains("\\") || path.Contains("/"))
            {
                Directory targetDir = MoveToDir(path, Program.currentDirectory);
                if (targetDir != null) // folder is found 
                {
                    Program.currentDirectory = targetDir;
                    Program.path = path;
                }
                else // folder not found 
                {
                    Console.WriteLine($"Error: This path \"{path}\" is not exists!");
                    return;
                }
                return;
            }
            int index = Program.currentDirectory.search_Directory(path);
            if (index == -1) // this folder not found 
            {
                Console.WriteLine($"Error: this path '{path}' is not exists!");
                return;
            }
            Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
            if (entry.dir_Attr != 0x10)
            {
                Console.WriteLine($"Error: '{path}' is not a directory.");
                return;
            }
            else // this is folder 
            {
                string name = new string(entry.Dir_Namee).Trim();
                if (name.Contains("\0"))
                {
                    name = name.Replace("\0", " ");
                }
                name = name.Trim();
                Directory newDir = new Directory(name.ToCharArray(), entry.dir_Attr, entry.dir_First_Cluster, Program.currentDirectory);
                newDir.Read_Directory();
                Program.currentDirectory = newDir;
                Program.path = Program.path + "\\" + path;
            }

        } //cd 
        public static void RemoveDirectory(string name)
        {
            if(name.Contains("."))
            {
                Console.WriteLine($"Error : this path \"{name}\" is not Directory");
                return; 
            }
            if (name.Contains("\\") && !name.Contains("."))
            {
                int las = name.LastIndexOf('\\');
                string dire = name.Substring(0, las);

                Directory d = MoveToDir(name, Program.currentDirectory);// this is directory we want delete it 
                Directory dd = MoveToDir(dire, Program.currentDirectory); // this is parent for this directory 
                if (d != null)// directory is found 
                {
                    string dname = new string(d.Dir_Namee);// get name 
                    int index3 = dd.search_Directory(dname);// search for this name 

                    if (d.DirectoryTable.Count > 0) // if directory have sup directory or files 
                    {
                        Console.WriteLine($"Error: Directory '{dname}' is not empty!");
                        return;
                    }
                    else
                    {
                        Console.Write($"Are you sure you want to delete '{dname}' ,please enter y for Yes or n for No: "); // ask user if want to delete it 
                        string answer = Console.ReadLine();
                        while (answer != "y" || answer != "n")
                        {
                            if (answer == "y")
                            {
                                d.delete_Directory();
                                dd.DirectoryTable.RemoveAt(index3);
                                dd.Write_Directory();
                                if (Program.currentDirectory.Parent != null)
                                {
                                    Program.currentDirectory.Update_Content(d, Program.currentDirectory.Parent);
                                }
                                return;
                            }
                            else if (answer == "n")
                            {
                                return;
                            }
                            else
                            {
                                Console.Write($"Are you sure you want to delete '{dname}' ,please enter y for Yes or n for No: ");
                                answer = Console.ReadLine();
                            }
                        }
                        Program.currentDirectory.Read_Directory();
                    }
                }
                else // if directory not found 
                {
                    Console.WriteLine($"Error : this directory \"{name}\" is not exist!");
                    return;
                }
            }
            // here for non fullpath to directory 
            int index = Program.currentDirectory.search_Directory(name); // search if found or not 
            if (index == -1) // here not found 
            {
                Console.WriteLine($"Error: this directory '{name}' does not exist on your disk!");
                return;
            }
            // here if found directory 
            Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
            if (entry.dir_Attr != 0x10) // need to check if this is directory or not 
            {
                Console.WriteLine($"Error: '{name}' is not a directory!"); 
                return;
            }
            int firstCluster = entry.dir_First_Cluster;
            Directory dirToDelete = new Directory(name.ToCharArray(), entry.dir_Attr, firstCluster, Program.currentDirectory);
            dirToDelete.Read_Directory();
            if (dirToDelete.DirectoryTable.Count > 0) // if directory have sup directory or files 
            {
                Console.WriteLine($"Error: Directory '{name}' is not empty!");
                return;
            }
            else 
            {
                Console.Write($"Are you sure you want to delete '{name}' ,please enter y for Yes or n for No: ");// ask user if want to delete it 
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        dirToDelete.delete_Directory();
                        Program.currentDirectory.DirectoryTable.RemoveAt(index);
                        Program.currentDirectory.Write_Directory();
                        return;
                    }
                    else if (answer == "n")
                    {
                        return;
                    }
                    else
                    {
                        Console.Write($"Are you sure you want to delete '{name}' ,please enter y for Yes or n for No:");
                        answer = Console.ReadLine();
                    }
                }
            }
        }//rd
        public static void list_OF_Directory(string name)
        {
            if (name == "")
            {
                Console.WriteLine($"Error : dir command syntax is \r\ndir \r\nor \r\ndir [directory] \r\n[directory] can be directory name or fullpath of a directory \r\nor file name or full path of a file.");
                return;
            }
            if (name.Contains("."))
            {
                Console.WriteLine($"Error this path \"{name}\" is not a Directory");
                return;
            }
            if (name.Contains("\\") && !name.Contains("."))
            {
                Directory o;
                o = MoveToDir(name, Program.currentDirectory);
                if (o != null)
                {
                    Dir(name);
                    return;
                }
                else
                {
                    Console.WriteLine($"Error this path \"{name}\" is not exists!");
                    return;
                }
            }
            int index = Program.currentDirectory.search_Directory(name);
            if (index == -1)
            {
                Console.WriteLine($"Error : this  Directory \"{name}\" does not exists !");
                return;
            }
            else
            {
                Dir(name);
            }
        }//dir       
        public static void Rename(string oldname, string newname)
        {
            if (string.IsNullOrWhiteSpace(oldname) || string.IsNullOrWhiteSpace(newname)) // check if user don't input name old directory or name new directory
            {
                Console.WriteLine("Error: Invalid syntax. Correct usage: rename [fileName] [new fileName]");
                return;
            }
            if (newname.Contains("\\")) // if full path new name so print error new name can't be full path 
            {
                Console.WriteLine("Error: The new file name should be a file name only you can't provide a full path.");
                return;
            }
            Directory targetDirectory;
            string oldFileName = oldname;
            if (oldname.Contains("\\") && oldname.Contains(".")) // if 
            {
                string path = oldname.Substring(0, oldname.LastIndexOf('\\'));
                oldFileName = oldname.Substring(oldname.LastIndexOf('\\') + 1);
                targetDirectory = ParserClass.MoveToDir(path, Program.currentDirectory);
                if (targetDirectory == null)
                {
                    Console.WriteLine($"this path \"{oldname}\" does not exist on your disk!");
                    return;
                }
            }
            else
            {
                //targetDirectory = Program.currentDirectory;
                Console.WriteLine($"Error : this path \"{oldname}\" is a Directory!");
                return;
            }
            int index_oldName = targetDirectory.search_Directory(oldFileName);
            if (index_oldName == -1)
            {
                Console.WriteLine($"this file \"{oldname}\" does not exist on your disk!");
                return;
            }
            if (targetDirectory.DirectoryTable[index_oldName].dir_Attr == 0x10)
            {
                Console.WriteLine("Error : can't rename Directory!");
                return;
            }
            int index_newName = targetDirectory.search_Directory(newname);
            if (index_newName != -1)
            {
                Console.WriteLine("Error: A duplicate file name exists!");
                return;
            }
            Directory_Entry entry = targetDirectory.DirectoryTable[index_oldName];
            entry.Dir_Namee = newname.PadRight(11, '\0').ToCharArray();
            targetDirectory.Write_Directory();
        }
        public static void Make_Directory2(string name)
        {
            string dirName = name;
            if (dirName.Contains("\\"))
            {
                Directory cd = MoveToDir(dirName, Program.currentDirectory);
                if (cd != null) // this directory is found 
                {
                    Console.WriteLine($"Error : this directory \"{new string(cd.Dir_Namee)}\" is already exits!");
                    return;
                }
                else // directory not found so make it 
                {
                    int last = dirName.LastIndexOf('\\');
                    string namedir = dirName.Substring(last + 1); // get name of directory 
                    string ddd = dirName.Substring(0, last);
                    Directory d = MoveToDir(ddd, Program.currentDirectory); // get parent of this dir
                    Directory newDir = new Directory(namedir.ToCharArray(), 0x10, 0, d); // make new dire 
                    if (d.Can_Add_Entry(newDir))
                    {
                        d.add_Entry(newDir);
                        d.Write_Directory();
                        if (d.Parent != null)
                        {
                            d.Parent.Write_Directory();
                            d.Update_Content(d.Get_Directory_Entry(), d.Parent.Get_Directory_Entry());
                        }
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Error: could not create the directory");
                        return;
                    }
                }
            }
            else // this without fullpath 
            {
                if (Program.currentDirectory.search_Directory(dirName) != -1)
                {
                    Console.WriteLine($"Error: this directory \"{name}\" already exists!");
                    return;
                }
                Directory newDir = new Directory(dirName.ToCharArray(), 0x10, 0, Program.currentDirectory);
                if (Program.currentDirectory.Can_Add_Entry(newDir))
                {
                    Program.currentDirectory.add_Entry(newDir);
                    Program.currentDirectory.Write_Directory();
                    if (Program.currentDirectory.Parent != null)
                    {
                        Program.currentDirectory.Parent.Write_Directory();
                        Program.currentDirectory.Update_Content(Program.currentDirectory.Get_Directory_Entry(), Program.currentDirectory.Parent.Get_Directory_Entry());
                    }
                }
                else
                {
                    Console.WriteLine("Error: Could not create the directory.");
                }
            }
        } //md        
        //done with all test cases
        public static void Type(string name)
        {
          
            if (name.Contains("\\")) // to enshure this is file 
            {
                File_Entry ff = MoveToFile(name);
                if (ff == null) // file not found 
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk!");
                    return;
                }
                if (ff.dir_Attr == 0x10 && ff != null)// this is not file 
                {
                    Console.WriteLine($"Error: may be this \"{name}\" is not a file name or ACCESS IS DENIED!");
                    return;
                }
                if (ff != null)// file found 
                {
                    string fname = new string(ff.Dir_Namee);
                    int opp = name.LastIndexOf('\\');
                    string move_to_this_dir = name.Substring(0, opp);
                    Directory d = MoveToDir(move_to_this_dir, Program.currentDirectory);
                    int index2 = d.search_Directory(fname);
                    if (index2 != -1)
                    {
                        int fc = d.DirectoryTable[index2].dir_First_Cluster;
                        int sz = d.DirectoryTable[index2].dir_FileSize;
                        File_Entry ss = new File_Entry(fname.ToCharArray(), ff.dir_Attr, fc, sz, d, "");
                        ss.Read_File_Content();
                        Console.WriteLine(ss.content);
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"this file : \"{name}\" does not exist on your disk!");
                        return;
                    }
                }

            }

            int index = Program.currentDirectory.search_Directory(name);
            if (index != -1) // found 
            {
                if (Program.currentDirectory.DirectoryTable[index].dir_Attr == 0x10) // check if is filr or dir id dir error
                {
                    Console.WriteLine($"Error: may be this \"{name}\" is not a file name or ACCESS IS DENIED!");
                    return;
                }
                int fc = Program.currentDirectory.DirectoryTable[index].dir_First_Cluster;
                int sz = Program.currentDirectory.DirectoryTable[index].dir_FileSize;
                File_Entry f = new File_Entry(name.ToCharArray(), 0x0, fc, sz, Program.currentDirectory, "");
                f.Read_File_Content();
                Console.WriteLine(f.content);
                return;
            }
            else
            {
                Console.WriteLine($"this file : \"{name}\" does not exist on your disk!");
                return;
            }
        }
        #region Import
        public static void Importv2(string sorc, string dest)
        {
            string fullpath;
            int imported_File_Count = 0;
            if (System.IO.Path.IsPathRooted(sorc))
            {
                fullpath = sorc; // It's an absolute path
            }
            else
            {
                fullpath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), sorc); // from .exe app 

            }
            // First check if File exist
            if (System.IO.File.Exists(fullpath))
            {
                if (dest.Contains("\\") || dest.Contains("."))
                {
                    // if destinition is only file like import t1.txt pp.txt or t1.txt t1.txt 
                    if (dest.Contains(".") && !dest.Contains("\\"))
                    {
                        int index_des = Program.currentDirectory.search_Directory(dest);
                        if (index_des != -1) // found 
                        {
                            Console.WriteLine($"Error: A file is \"{dest}\" already exists on your disk!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file: \"{dest}\" , please enter y for Yes n for No!");
                            string fileContent = System.IO.File.ReadAllText(fullpath);
                            if (fileContent.Contains("\0"))
                            {
                                fileContent = fileContent.Replace("\0", " ");
                            }
                            fileContent = fileContent.Trim();
                            int size = fileContent.Length;
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    File_Entry f_in_disk = new File_Entry(Program.currentDirectory.DirectoryTable[index_des], Program.currentDirectory);
                                    f_in_disk.Read_File_Content();
                                    f_in_disk.content = fileContent;
                                    f_in_disk.dir_FileSize = size;
                                    f_in_disk.Write_File_Content();
                                    return;
                                }
                                if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file: \"{dest}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                        }
                        else // here not found
                        {
                            File_Entry ff = MoveToFile(dest);
                            if (ff == null) // file not found  // pp.txt 
                            {
                                string[] pathParts = dest.Split('\\'); // Split the path (extract name & size)
                                string fileName = pathParts[pathParts.Length - 1]; // ppp.txt 
                                string fileContent = System.IO.File.ReadAllText(fullpath);
                                if (fileContent.Contains("\0"))
                                {
                                    fileContent = fileContent.Replace("\0", " ");
                                }
                                fileContent = fileContent.Trim();
                                int size = fileContent.Length;
                                int fc = Mini_FAT.get_Availabel_Cluster();
                                File_Entry f = new File_Entry(fileName.ToCharArray(), 0x0, fc, size, Program.currentDirectory, fileContent);
                                f.Write_File_Content();
                                Directory_Entry d = new Directory_Entry(fileName.ToCharArray(), 0, f.dir_First_Cluster, size);
                                Program.currentDirectory.DirectoryTable.Add(d);
                                imported_File_Count++;
                                Program.currentDirectory.Write_Directory();
                                if (Program.currentDirectory.Parent != null)
                                {
                                    Program.currentDirectory.Update_Content(Program.currentDirectory.Get_Directory_Entry(), Program.currentDirectory.Parent.Get_Directory_Entry());
                                }
                                Console.WriteLine($"{sorc}");
                                Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                return;
                            }
                            else if (ff != null) // here file found 
                            {
                                Console.WriteLine($"this file \"{dest}\" is already exist on your disk!");

                                Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                return;
                            }
                            else // here dest is folder 
                            {
                                Directory d = MoveToDir(dest, Program.currentDirectory);
                                if (d != null)
                                {
                                    string fileContent = System.IO.File.ReadAllText(fullpath);
                                    if (fileContent.Contains("\0"))
                                    {
                                        fileContent = fileContent.Replace("\0", " ");
                                    }
                                    fileContent = fileContent.Trim();
                                    int size = fileContent.Length;
                                    int fc = Mini_FAT.get_Availabel_Cluster();
                                    File_Entry f = new File_Entry(sorc.ToCharArray(), 0x0, fc, size, d, fileContent);
                                    f.Write_File_Content();
                                    Directory_Entry dd = new Directory_Entry(sorc.ToCharArray(), 0, f.dir_First_Cluster, size);
                                    d.DirectoryTable.Add(dd);
                                    imported_File_Count++;
                                    d.Write_Directory();
                                    if (Program.currentDirectory.Parent != null)
                                    {
                                        Program.currentDirectory.Update_Content(d.Get_Directory_Entry(), d.Parent.Get_Directory_Entry());
                                    }
                                    Console.WriteLine($"{sorc}");
                                    Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"this directory \"{dest}\" not exist on your disk!");
                                    return;
                                }
                            }

                        }


                    }
                    else // dest full path 
                    {
                        File_Entry ffv2 = MoveToFile(dest); // N:\kk\p1.txt
                        if (dest.Contains(".")) // to ensure this is file
                        {
                            if (ffv2 == null) // file not found so import it  
                            {
                                string[] pathParts = dest.Split('\\');
                                string fileName = pathParts[pathParts.Length - 1]; // p1.txt 
                                int indexDir = dest.LastIndexOf("\\");
                                string name_Dir = dest.Substring(0, indexDir);
                                Directory dd = MoveToDir(name_Dir, Program.currentDirectory);
                                if (dd != null) // dir found and import file 
                                {

                                    string fileContent = System.IO.File.ReadAllText(fullpath);
                                    if (fileContent.Contains("\0"))
                                    {
                                        fileContent = fileContent.Replace("\0", " ");
                                    }
                                    fileContent = fileContent.Trim();
                                    int size = fileContent.Length;
                                    int fc = Mini_FAT.get_Availabel_Cluster();
                                    File_Entry f = new File_Entry(fileName.ToCharArray(), 0x0, fc, size, dd, fileContent);
                                    f.Write_File_Content();
                                    Directory_Entry dd2 = new Directory_Entry(fileName.ToCharArray(), 0, f.dir_First_Cluster, size);
                                    dd.DirectoryTable.Add(dd2);
                                    imported_File_Count++;
                                    dd.Write_Directory();

                                    if (Program.currentDirectory.Parent != null)
                                    {
                                        Program.currentDirectory.Update_Content(dd.Get_Directory_Entry(), dd.Parent.Get_Directory_Entry());
                                    }
                                    Console.WriteLine($"{sorc}");
                                    Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                    return;




                                }
                                else // dir not found so print error 
                                {
                                    Console.WriteLine($"Error this path \"{dest}\" does not exist on your disk!");
                                    return;
                                }


                            }
                            else // this file found 
                            {
                                string name_file = new string(ffv2.Dir_Namee);
                                Console.WriteLine($"Error: A file is \"{name_file}\" already exists on your disk!");
                                Console.WriteLine($"NOTE : do you want to overwrite this file: \"{name_file}\" , please enter y for Yes n for No!");
                                string fileContent = System.IO.File.ReadAllText(fullpath);
                                if (fileContent.Contains("\0"))
                                {
                                    fileContent = fileContent.Replace("\0", " ");
                                }
                                fileContent = fileContent.Trim();
                                int size = fileContent.Length;
                                int indexDir = dest.LastIndexOf("\\");
                                string name_Dir = dest.Substring(0, indexDir);
                                Directory dd = MoveToDir(name_Dir, Program.currentDirectory);
                                int index = dd.search_Directory(name_file);
                                string answer = Console.ReadLine();
                                while (answer != "y" || answer != "n")
                                {
                                    if (answer == "y")
                                    {
                                        File_Entry f_in_disk = new File_Entry(dd.DirectoryTable[index], dd);
                                        f_in_disk.Read_File_Content();
                                        f_in_disk.content = fileContent;
                                        f_in_disk.dir_FileSize = size;
                                        f_in_disk.Write_File_Content();
                                        return;

                                    }
                                    if (answer == "n")
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"NOTE : do you want to overwrite this file: \"{name_file}\" , please enter y for Yes n for No!");
                                        answer = Console.ReadLine();
                                    }
                                }
                            }
                        }
                        // here if this full path is folder
                        else
                        {
                            // we need move to dir 
                            Directory d = MoveToDir(dest, Program.currentDirectory);
                            if (d != null) // this dir found 
                            {
                                string[] pathParts = sorc.Split('\\'); // Split the path (extract name & size)
                                string fileName = pathParts[pathParts.Length - 1];
                                int index = d.search_Directory(fileName);
                                if (index == -1) // file not found in this directory
                                {
                                    string fileContent = System.IO.File.ReadAllText(fullpath);
                                    if (fileContent.Contains("\0"))
                                    {
                                        fileContent = fileContent.Replace("\0", " ");
                                    }
                                    fileContent = fileContent.Trim();
                                    int size = fileContent.Length;

                                    int fc = Mini_FAT.get_Availabel_Cluster();

                                    File_Entry f = new File_Entry(fileName.ToCharArray(), 0x0, fc, size, d, fileContent);
                                    f.Write_File_Content();
                                    Directory_Entry dd = new Directory_Entry(fileName.ToCharArray(), 0, f.dir_First_Cluster, size);
                                    d.DirectoryTable.Add(dd);
                                    imported_File_Count++;
                                    d.Write_Directory();

                                    if (Program.currentDirectory.Parent != null)
                                    {
                                        Program.currentDirectory.Update_Content(d.Get_Directory_Entry(), d.Parent.Get_Directory_Entry());
                                    }
                                    Console.WriteLine($"{sorc}");
                                    Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                    return;
                                }
                                else // file found 
                                {
                                    Console.WriteLine($"this file \"{sorc}\" is already exist on your disk!");
                                    Console.WriteLine($"NOTE : do you want to overwrite this file: \"{sorc}\" , please enter y for Yes n for No!");
                                    string fileContent = System.IO.File.ReadAllText(fullpath);
                                    if (fileContent.Contains("\0"))
                                    {
                                        fileContent = fileContent.Replace("\0", " ");
                                    }
                                    fileContent = fileContent.Trim();
                                    int size = fileContent.Length;
                                    //int index2 = d.search_Directory(sorc);
                                    string answer = Console.ReadLine();
                                    while (answer != "y" || answer != "n")
                                    {
                                        if (answer == "y")
                                        {
                                            File_Entry f_in_disk = new File_Entry(d.DirectoryTable[index], d);
                                            f_in_disk.Read_File_Content();
                                            f_in_disk.content = fileContent;
                                            f_in_disk.dir_FileSize = size;
                                            f_in_disk.Write_File_Content();
                                            imported_File_Count++;
                                            break;

                                        }
                                        if (answer == "n")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"NOTE : do you want to overwrite this file: \"{sorc}\" , please enter y for Yes n for No!");
                                            answer = Console.ReadLine();
                                        }
                                    }
                                    Console.WriteLine($"{sorc}");
                                    Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                    return;
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Error : this \"{dest}\" does not exist on your disk!");
                                return;
                            }
                        }

                    }
                }
                // import to folder 
                // import t1.txt kk => kk is destinition                
                else // without full path in dest
                {
                    // here folder without full path
                    int index_dir = Program.currentDirectory.search_Directory(dest);
                    if (index_dir != -1) // dir found 
                    {
                        Directory d = MoveToDir(dest, Program.currentDirectory);
                        if (d != null)
                        {
                            int index_File = d.search_Directory(sorc); // seacrch to file 
                            if (index_File == -1) // if file not exist import this file 
                            {
                                string fileContent = System.IO.File.ReadAllText(fullpath);
                                if (fileContent.Contains("\0"))
                                {
                                    fileContent = fileContent.Replace("\0", " ");
                                }
                                fileContent = fileContent.Trim();
                                int size = fileContent.Length;

                                int fc = Mini_FAT.get_Availabel_Cluster();

                                File_Entry f = new File_Entry(sorc.ToCharArray(), 0x0, fc, size, d, fileContent);
                                f.Write_File_Content();
                                Directory_Entry dd = new Directory_Entry(sorc.ToCharArray(), 0, f.dir_First_Cluster, size);
                                d.DirectoryTable.Add(dd);
                                imported_File_Count++;
                                d.Write_Directory();

                                if (Program.currentDirectory.Parent != null)
                                {
                                    Program.currentDirectory.Update_Content(d.Get_Directory_Entry(), d.Parent.Get_Directory_Entry());
                                }
                                Console.WriteLine($"{sorc}");
                                Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                return;
                            }
                            // here if file is exist in this folder with same name 
                            // so ask user if want overwrite or not 
                            else
                            {
                                Console.WriteLine($"this file \"{sorc}\" is already exist on your disk!");
                                Console.WriteLine($"NOTE : do you want to overwrite this file: \"{sorc}\" , please enter y for Yes n for No!");
                                string fileContent = System.IO.File.ReadAllText(fullpath);
                                if (fileContent.Contains("\0"))
                                {
                                    fileContent = fileContent.Replace("\0", " ");
                                }
                                fileContent = fileContent.Trim();
                                int size = fileContent.Length;
                                int index = d.search_Directory(sorc);
                                string answer = Console.ReadLine();
                                while (answer != "y" || answer != "n")
                                {
                                    if (answer == "y")
                                    {
                                        File_Entry f_in_disk = new File_Entry(d.DirectoryTable[index], d);
                                        f_in_disk.Read_File_Content();
                                        f_in_disk.content = fileContent;
                                        f_in_disk.dir_FileSize = size;
                                        f_in_disk.Write_File_Content();
                                        imported_File_Count++;
                                        break;

                                    }
                                    if (answer == "n")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"NOTE : do you want to overwrite this file: \"{sorc}\" , please enter y for Yes n for No!");
                                        answer = Console.ReadLine();
                                    }
                                }
                                Console.WriteLine($"{sorc}");
                                Console.WriteLine($"\t{imported_File_Count} file(s) imported.");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"this directory \"{dest}\" not exist on your disk!");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"this directory \"{dest}\" not exist on your disk!");
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error this File is not exist on your computer!");
                return;
            }
        }
        // import source 
        public static void Importv2(string path)
        {
            string fullPath;
            int importedFileCount = 0; 

            // Check if the provided path is absolute or relative file
            if (System.IO.Path.IsPathRooted(path))
            {
                fullPath = path; // It's an path
            }
            else
            {
                // Combine relative file path with the current working directory
                fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
            }

            if (System.IO.File.Exists(fullPath))
            {
                // Import a single file
                if (ImportSingleFile(fullPath))
                {
                    Console.WriteLine(fullPath);
                    importedFileCount++;
                    Console.WriteLine($"\t {importedFileCount} file(s) imported");
                }
            }
            else if (System.IO.Directory.Exists(fullPath)) // Bounce
            {
                List<string> files = new List<string>();
                // Import all .txt files in the directory
                string[] textFiles = System.IO.Directory.GetFiles(fullPath, "*.txt");
                if (textFiles.Length == 0)
                {
                    Console.WriteLine($"No text files found in the directory: \"{fullPath}\"");
                }
                else // files are found
                {
                    foreach (var file in textFiles)
                    {
                        if (ImportSingleFile(file))
                        {
                            files.Add(file);
                            importedFileCount++;
                        }
                    }
                    foreach (var f in files)
                    {
                        Console.WriteLine(f);
                    }
                    Console.WriteLine($"\t{importedFileCount} file(s) imported.");
                }
            }
            else
            {
                Console.WriteLine($"Could not find file or directory '{fullPath}'");
                Console.WriteLine($"This file: \"{path}\" does not exist on your computer!");
            }
        }

        // Helper method to import a single file
        private static bool ImportSingleFile(string filePath)
        {
            string[] pathParts = filePath.Split('\\');
            string fileName = pathParts[pathParts.Length - 1]; // get name 
            string fileContent = System.IO.File.ReadAllText(filePath);
            if (fileContent.Contains("\0"))
            {
                fileContent = fileContent.Replace("\0", " ");
            }
            fileContent = fileContent.Trim();
            int size = fileContent.Length;
            int index = Program.currentDirectory.search_Directory(fileName);
            int fc = Mini_FAT.get_Availabel_Cluster();

            if (index == -1) // file not found on my disk
            {
                File_Entry f = new File_Entry(fileName.ToCharArray(), 0, fc, size, Program.currentDirectory, fileContent);
                f.Write_File_Content();
                Directory_Entry d = new Directory_Entry(fileName.ToCharArray(), 0, f.dir_First_Cluster, size);
                Program.currentDirectory.DirectoryTable.Add(d);
                Program.currentDirectory.Write_Directory();

                if (Program.currentDirectory.Parent != null)
                {
                    Program.currentDirectory.Update_Content(Program.currentDirectory.Get_Directory_Entry(), Program.currentDirectory.Parent.Get_Directory_Entry());
                }

                return true; // Successfully imported
            }
            else // file found so ask user if want overwrite 
            {
                Console.WriteLine($"Error: A file is \"{fileName}\" already exists on your disk!");
                Console.WriteLine($"NOTE : do you want to overwrite this file: \"{fileName}\" , please enter y for Yes n for No!");
                string answer = Console.ReadLine();
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        File_Entry f_in_disk = new File_Entry(Program.currentDirectory.DirectoryTable[index], Program.currentDirectory);
                        f_in_disk.Read_File_Content();
                        f_in_disk.content = fileContent;
                        f_in_disk.dir_FileSize = size;
                        f_in_disk.Write_File_Content();
                        return true;
                    }
                    if (answer == "n")
                    {
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"NOTE : do you want to overwrite this file: \"{fileName}\" , please enter y for Yes n for No!");
                        answer = Console.ReadLine();
                    }
                }
                return false; // Import failed
            }
        }
        #endregion
        // source 
        #region Export
        public static void Export(string name)
        {
            //for directory
            if (name.Contains("\\") && !name.Contains("."))
            {
                Directory d = MoveToDir(name, Program.currentDirectory);
                if (d != null) // dir found
                {
                    int cc = 0;
                    List<string> list = new List<string>();
                    foreach (var dd in d.DirectoryTable)
                    {
                        if (dd.dir_Attr == 0x0) // to ensure it's file with att is 0x0
                        {
                            File_Entry f = new File_Entry(dd, d);
                            f.Read_File_Content();
                            string fileName = new string(dd.Dir_Namee).Trim();
                            if (fileName.Contains("\0"))
                            {
                                fileName = fileName.Replace("\0", " ");
                            }
                            fileName = fileName.Trim();
                            string exportPath3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                            string content3 = f.content;
                            if(content3.Contains("\0"))
                            {
                                content3 = content3.Replace("\0", " ");
                            }
                            content3 = content3.Trim();
                            File.WriteAllText(exportPath3, content3);
                            cc++;
                            string relativePath = new string(d.Dir_Namee) + "\\" + fileName;
                            list.Add(relativePath);
                        }
                        else
                        {
                            continue; // att is 0x10 it's dir 
                        }
                    }
                    foreach (var e in list)
                    {
                        Console.WriteLine(e);
                    }
                    Console.WriteLine($"\t{cc} file(s) exported.");
                    return;

                }
                else
                {
                    Console.WriteLine($"this directory \"{name}\" not found");
                    return;
                }
            }

            if (name.Contains("\\") && name.Contains(".")) // for files 
            {
                File_Entry ff = MoveToFile(name);
                if (ff != null)
                {
                    string namefile = new string(ff.Dir_Namee).Trim();
                    if (namefile.Contains("\0"))
                    {
                        namefile = namefile.Replace("\0", " ");
                    }
                    namefile = namefile.Trim();
                    int fcount2 = 0;
                    ff.Read_File_Content();
                    string exportPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, namefile);
                    string content2 = ff.content;
                    if(content2.Contains("\0"))
                    {
                        content2 = content2.Replace("\0", " ");
                    }
                    content2 = content2.Trim();
                    File.WriteAllText(exportPath2, content2);
                    fcount2++;
                    int lastSlashIndex = name.LastIndexOf('\\');
                    int secondLastSlashIndex = name.LastIndexOf('\\', lastSlashIndex - 1);
                    string relativePath = name.Substring(secondLastSlashIndex + 1);
                    Console.WriteLine($"{relativePath}");
                    Console.WriteLine($"\t{fcount2} file(s) exported.");
                    return;
                }
                else
                {
                    Console.WriteLine($"this file: \"{name}\" does not exist on your disk!");
                    return;
                }
            }

            // here only file without path 
            int index = Program.currentDirectory.search_Directory(name);
            if (index == -1) // File not found
            {
                Console.WriteLine($"This file \"{name}\" does not exist on your disk!");
                return;
            }
            Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
            if (entry.dir_Attr != 0x0) // 0x0 indicates a file 
            {
                Directory dir = MoveToDir(name, Program.currentDirectory);
                if(dir == null)
                {
                    Console.WriteLine($"Error this Directory \"{name}\"does not exists on your disk!");
                    return;
                }
                // here is directory without fullpath
                int cc = 0;
                List<string> list = new List<string>();
                foreach (var dd in dir.DirectoryTable)
                {
                    if (dd.dir_Attr == 0x0)
                    {
                        File_Entry f = new File_Entry(dd, Program.currentDirectory);
                        f.Read_File_Content();
                        string fileName = new string(dd.Dir_Namee).Trim();
                        if (fileName.Contains("\0"))
                        {
                            fileName = fileName.Replace("\0", " ");
                        }
                        fileName = fileName.Trim();
                        string exportPath3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                        string content4 = f.content;
                        if(content4.Contains("\0"))
                        {
                            content4 = content4.Replace("\0", " ");
                        }
                        content4 = content4.Trim();
                        File.WriteAllText(exportPath3, content4);
                        cc++;
                        string relativePath = new string(Program.currentDirectory.Dir_Namee) + "\\" + fileName;
                        list.Add(relativePath);
                    }
                    else
                    {
                        continue;
                    }
                }
                foreach (var e in list)
                {
                    Console.WriteLine(e);
                }
                Console.WriteLine($"\t{cc} file(s) exported.");
                return;
            }
             
            // if is only file
            int fcount = 0;
            File_Entry file = new File_Entry(entry, Program.currentDirectory);
            file.Read_File_Content();
            string content = file.content;
            if (content.Contains("\0"))
            {
                content = content.Replace("\0", " ");
            }
            content = content.Trim();
            string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            File.WriteAllText(exportPath, content);
            fcount++;
            string currentDirectoryPath = Program.path; // Adjust as needed to get the path like "N:\"

            Console.WriteLine($"{currentDirectoryPath}\\{name}");
            Console.WriteLine($"\t{fcount} file(s) exported.");
            return;
        }

        // done with all test cases
        public static void Export(string sorc, string dest)
        {
            // full path to full path 
            if (sorc.Contains("\\") && sorc.Contains(".")) // to ensure this is file 
            {
                File_Entry ff = MoveToFile(sorc);
                if (ff != null) // file found
                {
                    ff.Read_File_Content();
                    int cc = 0;
                    if (dest.Contains("\\") || dest.Contains(".")) // da file 
                    {
                        int lastindex_for_Dir = dest.LastIndexOf("\\");
                        string dest_in_pysical_Disk = dest.Substring(0, lastindex_for_Dir);
                        if (dest.Contains("."))
                        {
                            string dest_in_pysical_Disk_file = dest.Substring(lastindex_for_Dir + 1);
                            int fc = ff.dir_First_Cluster;
                            int fz = ff.dir_FileSize;
                            File_Entry fv2 = new File_Entry(dest_in_pysical_Disk_file.ToCharArray(), ff.dir_Attr, fc, fz, Program.currentDirectory, ff.content);
                            fv2.Read_File_Content();

                            string content9 = fv2.content;
                            if(content9.Contains("\0"))
                            {
                                content9 = content9.Replace("\0", " ");
                            }
                            content9 = content9.Trim();
                            using (StreamWriter sw = new StreamWriter(dest_in_pysical_Disk + '\\' + dest_in_pysical_Disk_file))
                            {
                                sw.WriteLine(content9);
                            }
                            cc++;
                            int lastSlashIndex = sorc.LastIndexOf('\\');
                            int secondLastSlashIndex = sorc.LastIndexOf('\\', lastSlashIndex - 1);
                            string relativePath = sorc.Substring(secondLastSlashIndex + 1);
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"\t{cc} file(s) exported.");
                            return;

                        }

                        if (System.IO.Directory.Exists(dest_in_pysical_Disk))
                        {
                            string dest_in_pysical_Disk_file = new string(ff.Dir_Namee).Trim();
                            int fc = ff.dir_First_Cluster;
                            int fz = ff.dir_FileSize;
                            File_Entry fv2 = new File_Entry(dest_in_pysical_Disk_file.ToCharArray(), ff.dir_Attr, fc, fz, Program.currentDirectory, ff.content);
                            fv2.Read_File_Content();
                            if (dest_in_pysical_Disk_file.Contains("\0"))
                            {
                                dest_in_pysical_Disk_file = dest_in_pysical_Disk_file.Replace("\0", " ");
                            }
                            dest_in_pysical_Disk_file = dest_in_pysical_Disk_file.Trim();
                            string content7 = fv2.content;
                            if(content7.Contains("\0"))
                            {
                                content7 = content7.Replace("\0", " ");
                            }
                            content7 = content7.Trim();
                            using (StreamWriter sw = new StreamWriter(dest_in_pysical_Disk + '\\' + dest_in_pysical_Disk_file))
                            {
                                sw.WriteLine(content7);
                            }
                            cc++;
                            int lastSlashIndex = sorc.LastIndexOf('\\');
                            int secondLastSlashIndex = sorc.LastIndexOf('\\', lastSlashIndex - 1);
                            string relativePath = sorc.Substring(secondLastSlashIndex + 1);
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"\t{cc} file(s) exported.");
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"this file \"{sorc}\" does not exist on your disk!");
                    return;
                }
            }
            else
            {
                int index = Program.currentDirectory.search_Directory(sorc);
                if (index != -1) // file found 
                {
                    Directory_Entry dd = Program.currentDirectory.DirectoryTable[index];
                    File_Entry ffv2 = new File_Entry(dd, Program.currentDirectory);
                    ffv2.Read_File_Content();
                    int ccv2 = 0;
                    if (dest.Contains("\\") || dest.Contains(".")) // da file 
                    {
                        int lastindex_for_Dir = dest.LastIndexOf("\\");
                        string dest_in_pysical_Disk = dest.Substring(0, lastindex_for_Dir);
                        if (dest.Contains("."))
                        {
                            string dest_in_pysical_Disk_file = dest.Substring(lastindex_for_Dir + 1);
                            int fc = ffv2.dir_First_Cluster;
                            int fz = ffv2.dir_FileSize;
                            File_Entry fv2 = new File_Entry(dest_in_pysical_Disk_file.ToCharArray(), ffv2.dir_Attr, fc, fz, Program.currentDirectory, ffv2.content);
                            fv2.Read_File_Content();
                            string contentf = ffv2.content;
                            if(contentf.Contains("\0"))
                            {
                                contentf = contentf.Replace("\0", " ");
                            }
                            contentf = contentf.Trim();
                            using (StreamWriter sw = new StreamWriter(dest_in_pysical_Disk + '\\' + dest_in_pysical_Disk_file))
                            {
                                sw.WriteLine(contentf);
                            }
                            ccv2++;
                            string relativePath = new string(Program.currentDirectory.Dir_Namee) + "\\" + sorc;
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"\t{ccv2} file(s) exported.");
                            return;
                        }
                        if (System.IO.Directory.Exists(dest_in_pysical_Disk))
                        {
                            int fc = ffv2.dir_First_Cluster;
                            int fz = ffv2.dir_FileSize;
                            File_Entry fv2 = new File_Entry(sorc.ToCharArray(), ffv2.dir_Attr, fc, fz, Program.currentDirectory, ffv2.content);
                            fv2.Read_File_Content();
                            string content5 = fv2.content;
                            if(content5.Contains("\0"))
                            {
                                content5 = content5.Replace("\0", " ");
                                
                            }
                            content5 = content5.Trim();
                            using (StreamWriter sw = new StreamWriter(dest_in_pysical_Disk + '\\' + sorc))
                            {
                                sw.WriteLine(content5);
                            }
                            ccv2++;
                            string currentDirectoryPath = Program.path; // Adjust as needed to get the path like "N:\"
                            Console.WriteLine($"{currentDirectoryPath}\\{sorc}");
                            Console.WriteLine($"\t{ccv2} file(s) exported.");
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"this file \"{sorc}\" does not found on your disk");
                    return;
                }
            }
        }
        #endregion
        #region del without any number of argument
        public static void del(string name)
        {
            //fullpath 
            if (name.Contains("\\") && name.Contains("."))
            {
                File_Entry ff = MoveToFile(name);
                if (ff != null)
                {
                    string fname = new string(ff.Dir_Namee);
                    int opp = name.LastIndexOf('\\');
                    string move_to_this_dir = name.Substring(0, opp);
                    Directory d = MoveToDir(move_to_this_dir, Program.currentDirectory);
                    int index2 = d.search_Directory(fname);
                    if (index2 != -1) // file found
                    {
                        int fc = d.DirectoryTable[index2].dir_First_Cluster;
                        int sz = d.DirectoryTable[index2].dir_FileSize;
                        File_Entry file = new File_Entry(fname.ToCharArray(), 0x0, fc, sz, d, ff.content);
                        Console.Write($"Are you sure that you want to delete \"{name}\", please enter Y for yes or N for no: ");
                        string answer;
                        answer = Console.ReadLine();
                        while (answer != "y" || answer != "n")
                        {
                            if (answer == "y")
                            {
                                file.Delete_File(fname);
                                d.Write_Directory();
                                d.Read_Directory();
                                if (Program.currentDirectory.Parent != null)
                                {
                                    Program.currentDirectory.Update_Content(d, Program.currentDirectory.Parent);
                                }
                                return;
                            }
                            else if (answer == "n")
                            {
                                return;
                            }
                            else
                                Console.Write($"Are you sure that you want to delete \"{name}\", please enter Y for yes or N for no: ");
                            answer = Console.ReadLine();
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk");
                    return;
                }
            }
            if(!name.Contains("."))
            {
                Console.WriteLine($"This file : {name} is not file name or ACCESS DENIE!");
                return;
            }

            int index = Program.currentDirectory.search_Directory(name);
            if (index != -1 && Program.currentDirectory.DirectoryTable[index].dir_Attr == 0x10)
            {
                Console.WriteLine($"This file : {name} is not file name or ACCESS DENIE!");
            }
            else
            {
                if (index != -1)
                {
                    int fc = Program.currentDirectory.DirectoryTable[index].dir_First_Cluster;
                    int sz = Program.currentDirectory.DirectoryTable[index].dir_FileSize;
                    File_Entry file = new File_Entry(name.ToCharArray(), 0x0, fc, sz, Program.currentDirectory, "");
                    Console.Write($"Are you sure that you want to delete \"{name}\", please enter Y for yes or N for no: ");
                    string answer;
                    answer = Console.ReadLine();
                    while (answer != "y" || answer != "n")
                    {
                        if (answer == "y")
                        {
                            file.Delete_File(name);
                            Program.currentDirectory.Write_Directory();
                            Program.currentDirectory.Read_Directory();
                            return;
                        }
                        else if (answer == "n")
                        {
                            return;
                        }
                        else
                            Console.Write($"Are you sure that you want to delete \"{name}\", please enter Y for yes or N for no: ");
                        answer = Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine($"This file : {name} does not exist on your Disk!");
                }
            }
        }//del 
        #endregion
        // for only source 
        public static void Copy(string name)
        {
            if (name.Contains("\\")) // this for full path N:\kk\tt.txt
            {
                if (name.Contains("\\") && name.Contains("."))
                {
                    File_Entry ff = MoveToFile(name);
                    if (ff == null)
                    {
                        Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                        return;
                    }
                    else
                    {
                        ff.Read_File_Content();
                        // we need take copy to currentDirectory
                        string fname = new string(ff.Dir_Namee).Trim();
                        int index3 = Program.currentDirectory.search_Directory(fname);
                        int count_File_copy = 0;
                        if (index3 != -1) // this file found in current directory 
                        {
                            int last_index_dir = name.LastIndexOf("\\");
                            string name_dir = name.Substring(0, last_index_dir);
                            Directory dest = MoveToDir(name_dir, Program.currentDirectory);
                            int index4 = dest.search_Directory(fname);
                            string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
                            string fileName = pathParts[pathParts.Length - 1];
                            int fc_found = dest.DirectoryTable[index4].dir_First_Cluster;
                            int fz_found = dest.DirectoryTable[index4].dir_FileSize;
                            int yy = 0;
                            File_Entry found = new File_Entry(fname.ToCharArray(), ff.dir_Attr, fc_found, fz_found, Program.currentDirectory, "");
                            Console.WriteLine($"Error : this file \"{fname}\" is already exists!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{fname}\" , please enter y for Yes n for No!");
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    found.content = ff.content;
                                    found.dir_FileSize = ff.dir_FileSize;
                                    found.Write_File_Content();
                                    dest.Write_Directory();
                                    yy++;
                                    string fullname = new string(dest.Dir_Namee) + "\\" + fileName;
                                    Console.WriteLine($"{fullname}");
                                    Console.WriteLine($"\t{yy} file(s) copied.");
                                    return;
                                }
                                if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{fname}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                            //Console.WriteLine("The file cannot be copied onto itself.");
                            Console.WriteLine($"{count_File_copy} file(s) copied.");
                            return;
                        }
                        else // file not found on current disk
                        {
                            //for file
                            int firstcluster = Mini_FAT.get_Availabel_Cluster();
                            int size = ff.dir_FileSize;
                            File_Entry c_file = new File_Entry(fname.ToCharArray(), 0x0, firstcluster, size, Program.currentDirectory, ff.content);
                            c_file.Write_File_Content();
                            Program.currentDirectory.DirectoryTable.Add(c_file);
                            count_File_copy++;
                            Program.currentDirectory.Write_Directory();
                            int lastSlashIndex = name.LastIndexOf('\\');
                            int secondLastSlashIndex = name.LastIndexOf('\\', lastSlashIndex - 1);
                            string relativePath = name.Substring(secondLastSlashIndex + 1); // Get "oo\ninja.txt"
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"{count_File_copy} file(s) copied.");
                            return;

                        }

                    }

                }
                else // this is full path to directory 
                {
                    Directory d = MoveToDir(name, Program.currentDirectory);
                    if (d != null) // this directory is found 
                    {
                        int copy_files = 0;
                        List<string> copiedFiles = new List<string>(); // To store relative paths of copied files
                        foreach (Directory_Entry entry in d.DirectoryTable)
                        {
                            if (entry.dir_Attr == 0x0)// this is file 
                            {
                                string file_Name = new string(entry.Dir_Namee).Trim();
                                int index_Search = Program.currentDirectory.search_Directory(file_Name);
                                if (index_Search != -1) // file found in current
                                {
                                    Console.WriteLine($"Error : this file \"{file_Name}\" is already exists!");
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{file_Name}\" , please enter y for Yes n for No!");
                                    string answer = Console.ReadLine();
                                    while (answer != "y" || answer != "n")
                                    {
                                        if (answer == "y")
                                        {
                                            File_Entry existingFile = new File_Entry(Program.currentDirectory.DirectoryTable[index_Search], Program.currentDirectory);
                                            File_Entry sourceFile = new File_Entry(entry, d);
                                            sourceFile.Read_File_Content(); // Load content from source
                                            existingFile.content = sourceFile.content;
                                            existingFile.dir_FileSize = sourceFile.dir_FileSize;
                                            existingFile.Write_File_Content(); // Overwrite the content
                                            copy_files++;
                                            copiedFiles.Add($"{new string(d.Dir_Namee).Trim()}\\{file_Name}"); // Add relative path
                                            break;
                                        }
                                        if (answer == "n")
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{file_Name}\" , please enter y for Yes n for No!");
                                            answer = Console.ReadLine();
                                        }
                                    }
                                }
                                else
                                {
                                    File_Entry f = new File_Entry(entry, d);
                                    f.Read_File_Content();

                                    int avc = Mini_FAT.get_Availabel_Cluster();
                                    File_Entry fv2 = new File_Entry(file_Name.ToCharArray(), f.dir_Attr, avc, f.dir_FileSize, Program.currentDirectory, f.content);
                                    fv2.Write_File_Content();
                                    copy_files++;
                                    Program.currentDirectory.DirectoryTable.Add(fv2);
                                    Program.currentDirectory.Write_Directory();
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        Program.currentDirectory.Write_Directory();
                        foreach (string file in copiedFiles)
                        {
                            Console.WriteLine(file);
                        }
                        Console.WriteLine($"\t{copy_files} file(s) copied.");
                        return;
                    }
                    else // dir not found
                    {
                        Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                        return;
                    }
                }

            }

            int index = Program.currentDirectory.search_Directory(name);
            if (index == -1)
            {
                Console.WriteLine($"this file \"{name}\" does not exist on your disk!");
                return;
            }
            else
            {
                // first check if this index is File or Directory 
                Directory_Entry dd = Program.currentDirectory.DirectoryTable[index];
                if (dd.dir_Attr == 0x10) // this is Directory 
                {
                    Directory d = new Directory(dd.Dir_Namee, dd.dir_Attr, dd.dir_First_Cluster, Program.currentDirectory);
                    d.Read_Directory();
                    int copy_files = 0;
                    foreach (Directory_Entry e in d.DirectoryTable)
                    {
                        if (e.dir_Attr == 0x0) // this is file
                        {
                            string file_Name = new string(e.Dir_Namee).Trim();
                            int index_Search = Program.currentDirectory.search_Directory(file_Name);
                            if (index_Search != -1) // file found in current
                            {
                                Console.WriteLine($"Error : this file \"{file_Name}\" is already exists!");
                                Console.WriteLine($"NOTE : do you want to overwrite this file \"{file_Name}\" , please enter y for Yes n for No!");
                                string answer = Console.ReadLine();
                                while (answer != "y" || answer != "n")
                                {
                                    if (answer == "y")
                                    {
                                        File_Entry existingFile = new File_Entry(Program.currentDirectory.DirectoryTable[index_Search], Program.currentDirectory);
                                        File_Entry sourceFile = new File_Entry(e, d);

                                        sourceFile.Read_File_Content(); // Load content from source
                                        existingFile.content = sourceFile.content;
                                        existingFile.dir_FileSize = sourceFile.dir_FileSize;
                                        existingFile.Write_File_Content(); // Overwrite the content
                                        copy_files++;
                                        break;
                                    }
                                    if (answer == "n")
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{file_Name}\" , please enter y for Yes n for No!");
                                        answer = Console.ReadLine();
                                    }

                                }

                            }
                            else // file not found so copy it 
                            {
                                File_Entry f = new File_Entry(e, d);
                                f.Read_File_Content();
                                int avc = Mini_FAT.get_Availabel_Cluster();
                                File_Entry fv2 = new File_Entry(file_Name.ToCharArray(), f.dir_Attr, avc, f.dir_FileSize, Program.currentDirectory, f.content);
                                fv2.Write_File_Content();
                                copy_files++;
                                Program.currentDirectory.DirectoryTable.Add(fv2);
                                Program.currentDirectory.Write_Directory();
                            }

                        }
                        else
                        {
                            continue;
                        }
                    }
                    Program.currentDirectory.Write_Directory();
                    string currentDirectoryPath = Program.path; // Adjust as needed to get the path like "N:\"
                    Console.WriteLine($"{currentDirectoryPath}\\{name}");
                    Console.WriteLine($"{copy_files} file(s) copied.");
                }
                else // this is File 
                {
                    int cc = 0;
                    Console.WriteLine($"Error: The file \"{name}\" already exists in the current directory.");
                    Console.WriteLine($"NOTE: Do you want to overwrite this file \"{name}\"? Please enter y for Yes or n for No!");
                    string answer = Console.ReadLine();
                    while (answer != "n" || answer != "y")
                    {
                        if (answer == "y")
                        {
                            File_Entry existingFile = new File_Entry(Program.currentDirectory.DirectoryTable[index], Program.currentDirectory);
                            File_Entry sourceFile = new File_Entry(dd, Program.currentDirectory);
                            sourceFile.Read_File_Content(); // Load content from source
                            existingFile.content = sourceFile.content;
                            existingFile.dir_FileSize = sourceFile.dir_FileSize;
                            existingFile.Write_File_Content();
                            cc++;
                            Console.WriteLine($"{cc} file(s) copied.");
                            return;
                        }
                        else if (answer == "n")
                        {
                            return;
                        }
                        else
                            Console.WriteLine($"NOTE: Do you want to overwrite this file \"{name}\"? Please enter y for Yes or n for No!");
                        answer = Console.ReadLine();
                    }
                }
            }
        }
        // for source and des
        public static void Copy(string name, string des)
        {
            // Locate the source file (name) full path done
            if (name.Contains("\\"))
            {
                File_Entry ff = MoveToFile(name);
                Directory dest = MoveToDir(des, Program.currentDirectory);
                if (ff == null)
                {
                    Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                    return;
                }
                if (dest != null) // this is Directory 
                {
                    string src_File = new string(ff.Dir_Namee).Trim();
                    int index_src = dest.search_Directory(src_File);
                    if (index_src == -1) // File not found so copy it
                    {
                        ff.Read_File_Content();
                        int fc = 0;
                        int f_clus = Mini_FAT.get_Availabel_Cluster();
                        File_Entry nn = new File_Entry(src_File.ToCharArray(), ff.dir_Attr, f_clus, ff.dir_FileSize, dest, ff.content);
                        nn.Write_File_Content();
                        dest.DirectoryTable.Add(nn);
                        fc++;
                        dest.Write_Directory();
                        Console.WriteLine($"{fc} file(s) copied.");
                        return;
                    }
                    else
                    {
                        int fc_found = dest.DirectoryTable[index_src].dir_First_Cluster;
                        int fz_found = dest.DirectoryTable[index_src].dir_FileSize;
                        int yy = 0;
                        File_Entry found = new File_Entry(src_File.ToCharArray(), ff.dir_Attr, fc_found, fz_found, dest, "");
                        Console.WriteLine($"Error : this file \"{src_File}\" is already exists!");
                        Console.WriteLine($"NOTE : do you want to overwrite this file \"{src_File}\" , please enter y for Yes n for No!");
                        string answer = Console.ReadLine();
                        while (answer != "y" || answer != "n")
                        {
                            if (answer == "y")
                            {
                                found.content = ff.content;
                                found.dir_FileSize = ff.dir_FileSize;
                                found.Write_File_Content();
                                dest.Write_Directory();
                                yy++;
                                Console.WriteLine($"\t{yy} file(s) copied.");
                                return;
                            }
                            if (answer == "n")
                            {
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"NOTE : do you want to overwrite this file \"{src_File}\" , please enter y for Yes n for No!");
                                answer = Console.ReadLine();
                            }
                        }
                        return;
                    }
                }
                else
                {
                    // here if Destinition file 
                    if (des.Contains("\\")) // fullpath to file 
                    {
                        File_Entry pp = MoveToFile(des);
                        int lastindex_forname = des.LastIndexOf("\\");
                        string ddd = des.Substring(0, lastindex_forname);
                        Directory d = MoveToDir(ddd, Program.currentDirectory);
                        string namedis = des.Substring(lastindex_forname + 1);
                        if (pp == null) // this file in destinition not found so copy it
                        {
                            if (d == null)
                            {
                                Console.WriteLine($"Error: Directory \"{des}\" not found.");
                                return;
                            }
                            int index = d.search_Directory(namedis);
                            if (index != -1)
                            {
                                Console.WriteLine("Error: File already exists at the destination.");
                                return;
                            }
                            ff.Read_File_Content();
                            int f_count = 0;
                            int f_clusert = Mini_FAT.get_Availabel_Cluster();
                            File_Entry hh = new File_Entry(namedis.ToCharArray(), ff.dir_Attr, f_clusert, ff.dir_FileSize, d, ff.content);
                            hh.Write_File_Content();
                            d.DirectoryTable.Add(hh);
                            d.Write_Directory();
                            f_count++;
                            Program.currentDirectory.Write_Directory();
                            Console.WriteLine($"{f_count} file(s) copied.");
                            return;
                        }
                        else // mean this file in destinition is found so ask user to ovwerwrite or not 
                        {
                            ff.Read_File_Content();
                            string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
                            string fileName = pathParts[pathParts.Length - 1];
                            int index_D = d.search_Directory(fileName);
                            Console.WriteLine($"this file \"{fileName}\" is already exist on your disk!");
                            Console.WriteLine($"NOTE: Do you want to overwrite this file \"{fileName}\"? Please enter y for Yes or n for No!");
                            int fc_Soruce = d.DirectoryTable[index_D].dir_First_Cluster;
                            int fz_Source = d.DirectoryTable[index_D].dir_FileSize;
                            int c = 0;
                            File_Entry ee = new File_Entry(fileName.ToCharArray(), 0x0, fc_Soruce, fz_Source, d, "");
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    ee.content = ff.content;
                                    ee.dir_FileSize = ff.dir_FileSize;
                                    ee.Write_File_Content();
                                    c++;
                                    Console.WriteLine($"{c} file(s) copied.");
                                    return;
                                }
                                else if (answer == "n")
                                {
                                    break;
                                }
                            }
                            return;
                        }
                    }
                    else
                    {
                        ff.Read_File_Content();
                        int f_count = 0;
                        int index3 = Program.currentDirectory.search_Directory(des);
                        if (index3 == -1)
                        {
                            int f_cluster = Mini_FAT.get_Availabel_Cluster();
                            File_Entry gg = new File_Entry(des.ToCharArray(), ff.dir_Attr, f_cluster, ff.dir_FileSize, Program.currentDirectory, ff.content);
                            gg.Write_File_Content();
                            Program.currentDirectory.DirectoryTable.Add(gg);
                            f_count++;
                            Program.currentDirectory.Write_Directory();
                            int lastSlashIndex = name.LastIndexOf('\\');
                            int secondLastSlashIndex = name.LastIndexOf('\\', lastSlashIndex - 1);
                            string relativePath = name.Substring(secondLastSlashIndex + 1); // Get "oo\ninja.txt"
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"{f_count} file(s) copied.");
                            return;
                        }
                        else
                        {
                            int fc_found = Program.currentDirectory.DirectoryTable[index3].dir_First_Cluster;
                            int fz_found = Program.currentDirectory.DirectoryTable[index3].dir_FileSize;
                            int yy = 0;
                            File_Entry found = new File_Entry(des.ToCharArray(), ff.dir_Attr, fc_found, fz_found, Program.currentDirectory, "");
                            found.Read_File_Content();
                            Console.WriteLine($"Error : this file \"{des}\" is already exists!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{des}\" , please enter y for Yes n for No!");
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    found.content = ff.content;
                                    found.dir_FileSize = ff.dir_FileSize;
                                    found.Write_File_Content();
                                    Program.currentDirectory.Write_Directory();
                                    yy++;
                                    Console.WriteLine($"\t{yy} file(s) copied.");
                                    return;
                                }
                                if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{des}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                            Console.WriteLine($"\t{yy} file copied.");
                            return;
                        }
                    }
                }
            }
            else
            {
                // source file without full path 
                int index1 = Program.currentDirectory.search_Directory(name);
                if (index1 != -1) // source file are found 
                {
                    Directory_Entry d = Program.currentDirectory.DirectoryTable[index1];
                    File_Entry fd = new File_Entry(d, Program.currentDirectory);
                    //file name to fullpath file name 
                    if (des.Contains(".") && des.Contains("\\")) // mean file name to fullpath file name 
                    {
                        File_Entry ef = MoveToFile(des);
                        if (ef != null) // destinition file found 
                        {
                            fd.Read_File_Content();
                            int in_for_dire = des.LastIndexOf("\\");
                            string name_to_dir = des.Substring(0, in_for_dire);
                            Directory dd = MoveToDir(name_to_dir, Program.currentDirectory);
                            string[] pathParts = des.Split('\\'); // Split the path (extract name & size)
                            string fileName = pathParts[pathParts.Length - 1];
                            int index_file_destinition = dd.search_Directory(fileName);
                            Console.WriteLine($"Error : this file \"{fileName}\" is already exists!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{fileName}\" , please enter y for Yes n for No!");
                            int fc_found = dd.DirectoryTable[index_file_destinition].dir_First_Cluster;
                            int fz_found = dd.DirectoryTable[index_file_destinition].dir_FileSize;
                            int c = 0;
                            File_Entry found = new File_Entry(fileName.ToCharArray(), 0x0, fc_found, fz_found, dd, "");
                            found.Read_File_Content();
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    found.content = fd.content;
                                    found.dir_FileSize = fd.dir_FileSize;
                                    found.Write_File_Content();
                                    dd.Write_Directory();
                                    c++;
                                    Console.WriteLine($"{c} file(s) copied");
                                    return;
                                }
                                else if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{name}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                            Console.WriteLine($"{c} file(s) copied.");
                            return;
                        }
                        else // destinition file not found 
                        {
                            // need to split to get directory 
                            int in_for_dire = des.LastIndexOf("\\");
                            string name_to_dir = des.Substring(0, in_for_dire);
                            Directory dd = MoveToDir(name_to_dir, Program.currentDirectory);
                            if (dd == null) // dire not found
                            {
                                Console.WriteLine($"Error : this path \"{des}\" does not exist on your disk");
                            }
                            else // dire found 
                            {
                                fd.Read_File_Content();
                                string[] pathParts = des.Split('\\'); // Split the path 
                                string fileName = pathParts[pathParts.Length - 1];
                                int f_c = 0;
                                int fcluster = Mini_FAT.get_Availabel_Cluster();
                                int file_size = fd.dir_FileSize;

                                File_Entry ee = new File_Entry(fileName.ToCharArray(), 0x0, fcluster, file_size, dd, fd.content);
                                ee.Write_File_Content();
                                dd.DirectoryTable.Add(ee);
                                dd.Write_Directory();
                                f_c++;
                                Program.currentDirectory.Write_Directory();
                                Console.WriteLine($"{f_c} file(s) copied.");
                                return;
                            }
                        }
                    }
                    Directory dest = MoveToDir(des, Program.currentDirectory);
                    fd.Read_File_Content();
                    // test case 13
                    if (dest != null && dest.dir_Attr == 0x10) // this is Directory 
                    {
                        string src_File = new string(fd.Dir_Namee).Trim();
                        int index_src = dest.search_Directory(src_File);
                        if (index_src == -1) // File not found so copy it
                        {
                            fd.Read_File_Content();
                            int fc = 0;
                            int f_clus = Mini_FAT.get_Availabel_Cluster();
                            File_Entry nn = new File_Entry(name.ToCharArray(), fd.dir_Attr, f_clus, fd.dir_FileSize, dest, fd.content);
                            nn.Write_File_Content();
                            dest.DirectoryTable.Add(nn);
                            dest.Write_Directory();
                            fc++;
                            Program.currentDirectory.Write_Directory();
                            string currentDirectoryPath = Program.path; // Adjust as needed to get the path like "N:\"
                            Console.WriteLine($"{currentDirectoryPath}\\{src_File}");
                            Console.WriteLine($"\t{fc} file(s) copied.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"The file \"{src_File}\" is already exist in your disk!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{src_File}\" , please enter y for Yes n for No!");
                            File_Entry found = new File_Entry(dest.DirectoryTable[index_src], dest);
                            found.Read_File_Content();
                            int ds = 0;
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "n")
                            {
                                if (answer == "y")
                                {
                                    found.content = fd.content;
                                    found.dir_FileSize = fd.dir_FileSize;
                                    found.Write_File_Content();
                                    Program.currentDirectory.Write_Directory();
                                    ds++;
                                    break;
                                }
                                if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{src_File}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                            Console.WriteLine($"\t{ds} file(s) copied.");
                            return;
                        }
                    }
                    int fil_c = 0;
                    // fullpath Destinition 
                    if (des.Contains("\\"))
                    {
                        File_Entry pp = MoveToFile(des);
                        int lastindex_forname = des.LastIndexOf("\\");
                        string ddd = des.Substring(0, lastindex_forname);
                        Directory dd = MoveToDir(ddd, Program.currentDirectory);
                        string namedis = des.Substring(lastindex_forname + 1);
                        if (pp == null)
                        {
                            if (d == null)
                            {
                                Console.WriteLine($"Error: Directory \"{des}\" not found.");
                                return;
                            }
                            int index = dd.search_Directory(namedis);
                            if (index != -1)
                            {
                                Console.WriteLine("Error: File already exists at the destination.");
                                return;
                            }
                            fd.Read_File_Content();
                            int f_count = 0;
                            int f_clusert = Mini_FAT.get_Availabel_Cluster();
                            File_Entry hh = new File_Entry(namedis.ToCharArray(), fd.dir_Attr, f_clusert, fd.dir_FileSize, dd, fd.content);
                            hh.Write_File_Content();
                            dd.DirectoryTable.Add(hh);
                            dd.Write_Directory();
                            f_count++;
                            Program.currentDirectory.Write_Directory();
                            Console.WriteLine($"{f_count} file(s) copied.");
                            return;
                        }
                        else
                        {
                            int index2 = Program.currentDirectory.search_Directory(des);
                            if (index2 == -1)
                            {
                                int fc = Mini_FAT.get_Availabel_Cluster();
                                File_Entry gg = new File_Entry(des.ToCharArray(), fd.dir_Attr, fc, fd.dir_FileSize, Program.currentDirectory, fd.content);
                                gg.Write_File_Content();
                                Program.currentDirectory.DirectoryTable.Add(gg);
                                fil_c++;
                                Program.currentDirectory.Write_Directory();
                                Console.WriteLine($"{fil_c} file(s) copied.");
                                return;
                            }
                            else
                            {
                                Console.WriteLine("des is exist !");
                                return;
                            }
                        }
                    }
                    else
                    {
                        int indexdes = Program.currentDirectory.search_Directory(des);
                        if (indexdes == -1) // file not found 
                        {
                            int fc = Mini_FAT.get_Availabel_Cluster();
                            File_Entry gg = new File_Entry(des.ToCharArray(), fd.dir_Attr, fc, fd.dir_FileSize, Program.currentDirectory, fd.content);
                            gg.Write_File_Content();
                            Program.currentDirectory.DirectoryTable.Add(gg);
                            fil_c++;
                            Program.currentDirectory.Write_Directory();
                            Console.WriteLine($"{fil_c} file(s) copied.");
                            return;
                        }
                        else // file found 
                        {
                            fd.Read_File_Content();
                            Console.WriteLine($"this file \"{des}\" is already exist on your disk!");
                            Console.WriteLine($"NOTE : do you want to overwrite this file \"{des}\" , please enter y for Yes n for No!");
                            int fc = Program.currentDirectory.DirectoryTable[indexdes].dir_First_Cluster;
                            int fz = Program.currentDirectory.DirectoryTable[indexdes].dir_FileSize;
                            int x = 0;
                            File_Entry os = new File_Entry(des.ToCharArray(), 0x0, fc, fz, Program.currentDirectory, "");
                            os.Read_File_Content();
                            string answer = Console.ReadLine();
                            while (answer != "y" || answer != "no")
                            {
                                if (answer == "y")
                                {
                                    os.content = fd.content;
                                    os.dir_FileSize = fd.dir_FileSize;
                                    os.Write_File_Content();
                                    Program.currentDirectory.Write_Directory();
                                    x++;
                                    Console.WriteLine($"{x} file(s) copied.");
                                    return;
                                }
                                else if (answer == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine($"NOTE : do you want to overwrite this file \"{des}\" , please enter y for Yes n for No!");
                                    answer = Console.ReadLine();
                                }
                            }
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"this path \"{name}\" does not exist on your disk!");
                    return;
                }

            }
        }
    }
}
