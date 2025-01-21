using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace OS_Simple_Shell
{
    internal class ExecutionClass
    {
        public static Directory MoveToDir(string fullPath, Directory currentDirectory)
        {
            string[] parts = fullPath.Split('\\');
            if (parts.Length == 0)
            {
                return null;
            }
            Directory targetDirectory = currentDirectory;
            string name_OF_Target = new string(currentDirectory.Dir_Namee).Trim('\0');           
            if (parts[0].Equals(name_OF_Target, StringComparison.OrdinalIgnoreCase)) 
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
            if (lastSlashIndex == -1) // if no path 
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
            File_Entry file = new File_Entry(fileEntry, Program.currentDirectory);
            return file;
        }
        // helper method for import Files to Virtual_Disk_Directory
        private static bool ImportFileToDirectory(string fileName, string fileContent, Directory targetDirectory)
        {
            // Clean up content if necessary
            if (fileContent.Contains("\0"))
            {
                fileContent = fileContent.Replace("\0", " ").Trim();
            }
            // Calculate file size and find an available cluster
            int fileSize = fileContent.Length;
            int firstCluster = Mini_FAT.get_Availabel_Cluster();
            // Create File_Entry object
            File_Entry importedFile = new File_Entry(fileName.ToCharArray(), 0x0, firstCluster, fileSize, targetDirectory, fileContent);
            if(targetDirectory.Can_Add_Entry(importedFile.GetDirectory_Entry()))
            {
                importedFile.Write_File_Content();
                // Create a Directory_Entry and add to the directory table
                Directory_Entry entry = new Directory_Entry(importedFile.Dir_Namee, importedFile.dir_Attr, importedFile.dir_First_Cluster, importedFile.dir_FileSize);
                targetDirectory.DirectoryTable.Add(entry);
                targetDirectory.Write_Directory();
                // Update the parent directory content
                if (targetDirectory.Parent != null)
                {
                    targetDirectory.Update_Content(targetDirectory.Get_Directory_Entry(), targetDirectory.Parent.Get_Directory_Entry());
                }
                return true; // File successfully imported
            }
            else
            {
                return false;
            }
           
        }
        // helper method for overWrite imported command 
        private static void OverWrite_ImportedFiles(File_Entry file ,string fullpath, string filename)
        {          
            Console.WriteLine($"Error: A file is \"{filename}\" already exists on your disk!");
            Console.WriteLine($"NOTE : do you want to overwrite this file: \"{filename}\" , please enter y for Yes n for No!");
            string fileContent = System.IO.File.ReadAllText(fullpath);
            if (fileContent.Contains("\0"))
            {
                fileContent = fileContent.Replace("\0", " ").Trim();
            }
            int size = fileContent.Length;
            string answer = Console.ReadLine();
            while (answer != "y" || answer != "n")
            {
                if (answer == "y")
                {
                    file.Read_File_Content();
                    file.content = fileContent;
                    file.dir_FileSize = size;
                    file.Write_File_Content();
                    return;
                }
                if (answer == "n")
                {
                    return;
                }
                else
                {
                    Console.WriteLine($"NOTE : do you want to overwrite this file: \"{fileContent}\" , please enter y for Yes n for No!");
                    answer = Console.ReadLine();
                }
            }
        }
        // helper method for import single files to Virtual_Disk
        private static bool ImportSingleFile(string filePath)
        {
            string[] pathParts = filePath.Split('\\');
            string fileName = pathParts[pathParts.Length - 1]; // get name 
            string fileContent = System.IO.File.ReadAllText(filePath);
            if (fileContent.Contains("\0"))
            {
                fileContent = fileContent.Replace("\0", " ").Trim();
            }
            int size = fileContent.Length;
            int index = Program.currentDirectory.search_Directory(fileName);
            if (index == -1) // file not found on my disk
            {
                int first_Cluster = Mini_FAT.get_Availabel_Cluster();
                File_Entry file_Imported = new File_Entry(fileName.ToCharArray(), 0, first_Cluster, size, Program.currentDirectory, fileContent);
                if (Program.currentDirectory.Can_Add_Entry(file_Imported.GetDirectory_Entry())) //  هو بيقولك بتاخد اوبجكت من نوع ديركتوري انتري مش  
                {
                    file_Imported.Write_File_Content();
                    Directory_Entry entry = new Directory_Entry(fileName.ToCharArray(), 0x0, file_Imported.dir_First_Cluster, size);
                    Program.currentDirectory.DirectoryTable.Add(entry);
                    Program.currentDirectory.Write_Directory();

                    if (Program.currentDirectory.Parent != null)
                    {
                        Program.currentDirectory.Update_Content(Program.currentDirectory.Get_Directory_Entry(), Program.currentDirectory.Parent.Get_Directory_Entry());
                    }
                    return true; // Successfully imported   
                }
                else
                {
                    return false;
                }                                
            }
            else // file found so ask user if want overwrite 
            {
                Console.WriteLine($"Error: A file is \"{fileName}\" already exists on your disk!");
                Console.WriteLine($"NOTE : do you want to overwrite this file: \"{fileName}\" , please enter y for Yes n for No!");
                Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                int first_Cluster = entry.dir_First_Cluster;
                int file_Size = entry.dir_FileSize;
                File_Entry file_OverWriteed = new File_Entry(entry.Dir_Namee, 0x0, first_Cluster, file_Size, Program.currentDirectory, "");
                string answer = Console.ReadLine();
                int counter = 0;
                while (answer != "y" || answer != "n")
                {
                    if (answer == "y")
                    {
                        file_OverWriteed.content = fileContent;
                        file_OverWriteed.dir_FileSize = size;
                        file_OverWriteed.Write_File_Content();
                        counter++;
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
        public static void ImportMethod(string source , string destinition)
        {
            string fullpath;
            int imported_Files = 0;
            if (System.IO.Path.IsPathRooted(source))
            {
                fullpath = source; // It's an absolute path
            }
            else
            {
                fullpath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), source); // from .exe app 
            }

            if(!System.IO.File.Exists(fullpath)) // check if source file is exist or not 
            {
                Console.WriteLine($"Error this path :{source} does not exist on your Computer!");
                return;
            }
            else // this file is found 
            {
                if (destinition.Contains("\\") && destinition.Contains(".")) // this is fullpath to file 
                {
                    File_Entry file = MoveToFile(destinition);
                    if (file == null) // this mean file not found so import it 
                    {
                        string[] pathParts = destinition.Split('\\'); // Split the path (extract name & size)
                        string file_Name = pathParts[pathParts.Length - 1]; // get file name 
                        string parent_Directory_Of_File = pathParts[pathParts.Length - 2];
                        string file_Content = System.IO.File.ReadAllText(fullpath); // get content of this file 

                        Directory targetDirectory = MoveToDir(parent_Directory_Of_File, Program.currentDirectory);
                        if (ImportFileToDirectory(file_Name, file_Content, targetDirectory))
                        {
                            imported_Files++;
                            Console.WriteLine($"{source}");
                            Console.WriteLine($"\t{imported_Files} file(s) imported.");
                        }
                        else
                        {
                            Console.WriteLine("No space on disk to imported this file !");
                            return;
                        }
                    }
                    else // file is found so ask user if want to overwrite or not 
                    {
                        string[] pathParts = destinition.Split('\\'); // Split the path (extract name & size)
                        string file_Name = pathParts[pathParts.Length - 1]; // get file name 
                        OverWrite_ImportedFiles(file, fullpath, file_Name);
                        return;
                        // ............
                    }
                }
                else if (!destinition.Contains("\\") && destinition.Contains(".")) // this is filename without fullpath
                {
                    File_Entry file = MoveToFile(destinition);
                    if(file == null)
                    {
                        string file_Name = destinition; // get file name 
                        string parent_Directory_Of_File = new string(Program.currentDirectory.Dir_Namee).Trim('\0');
                        string file_Content = System.IO.File.ReadAllText(fullpath); // get content of this file 
                        Directory targetDirectory = MoveToDir(parent_Directory_Of_File, Program.currentDirectory);
                        if (ImportFileToDirectory(file_Name, file_Content, targetDirectory))
                        {
                            imported_Files++;
                            Console.WriteLine($"{source}");
                            Console.WriteLine($"\t{imported_Files} file(s) imported.");
                        }
                        else
                        {
                            Console.WriteLine("No space on disk to imported this file !");                            
                            return;
                        }
                    }
                    else // this mean this file is found so ask user if want to overwrite
                    {
                        string file_Name = destinition; // get file name 

                        OverWrite_ImportedFiles(file, fullpath, file_Name);
                        return;
                         // ...............
                    }
                }
                else if(destinition.Contains("\\") && !destinition.Contains(".")) // this si full path to directory
                {
                    Directory targetDirectory = MoveToDir(destinition,Program.currentDirectory);
                    if (targetDirectory == null) // this is mean directory is not found 
                    {
                        Console.WriteLine($"this path : \"{destinition}\" does not exist on your disk!");
                        return;
                    }
                    else // this directory is found 
                    {
                        // in this directory wee need to check if this file is found in thid directory or not 
                        string fileName = fullpath.Substring(fullpath.LastIndexOf("\\") + 1);
                        int index = targetDirectory.search_Directory(fileName);
                        if(index == -1) // this mean this file does not exist 
                        {
                            string file_Content = File.ReadAllText(fullpath);
                            if (ImportFileToDirectory(fileName, file_Content, targetDirectory))
                            {
                                imported_Files++;
                                Console.WriteLine($"{source}");
                                Console.WriteLine($"\t{imported_Files} file(s) imported.");
                            }
                            else
                            {
                                Console.WriteLine("No space on disk to imported this file !");                         
                                return;
                            }
                        }
                        else // this file is already exist so ask use if want to overwrite or not 
                        {
                            Directory_Entry entry = targetDirectory.DirectoryTable[index];
                            File_Entry file = new File_Entry(entry, targetDirectory);
                            OverWrite_ImportedFiles(file, fullpath, fileName);
                            return;
                        }                        
                    }
                }
                else // this mean this is directory without fullpath
                {
                    Directory targetDirectory = MoveToDir (destinition,Program.currentDirectory);
                    if (targetDirectory == null) // this directory not found 
                    {
                        Console.WriteLine($"this path : \"{destinition}\" does not exist on your disk!");
                        return;
                    }
                    else
                    {
                        string fileName = fullpath.Substring(fullpath.LastIndexOf("\\") + 1);
                        int index = targetDirectory.search_Directory(fileName);
                        if (index == -1) // this mean this file does not exist 
                        {
                            string file_Content = File.ReadAllText(fullpath);
                            if (ImportFileToDirectory(fileName, file_Content, targetDirectory))
                            {
                                imported_Files++;
                                Console.WriteLine($"{source}");
                                Console.WriteLine($"\t{imported_Files} file(s) imported.");
                            }
                            else
                            {
                                Console.WriteLine("No space on disk to imported this file !");
                                return;
                            }
                        }
                        else // this file is already exist so ask use if want to overwrite or not 
                        {
                            Directory_Entry entry = targetDirectory.DirectoryTable[index];
                            File_Entry file = new File_Entry (entry, targetDirectory);
                            OverWrite_ImportedFiles(file, fullpath, fileName);
                            return;
                        }
                    }
                }                
            }
        }
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
            if (System.IO.File.Exists(fullPath)) // if path is only file 
            {
                // Import a single file
                if (ImportSingleFile(fullPath))
                {                  
                    Console.WriteLine(path);
                    importedFileCount++;
                    Console.WriteLine($"\t {importedFileCount} file(s) imported");
                    return;
                }
                else
                {
                    Console.WriteLine("No space on disk to imported this file !");
                    return;
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
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Could not find file '{fullPath}'");
                Console.WriteLine($"This file: \"{path}\" does not exist on your computer!");
            }
        }
        public static void Dir()
        {
            int file_Counter = 0;
            int folder_Counter = 0;
            int file_Sizes = 0;
            int total_File_Size = 0;
            string name = new string(Program.currentDirectory.Dir_Namee);
            Console.WriteLine($"Directory of {name} is  \n");
            for (int i = 0; i < Program.currentDirectory.DirectoryTable.Count; i++) // iterator for counter of DirectoryTable 
            {
                file_Sizes = 0;
                if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x0) // if this entry is size 
                {
                    file_Counter++;
                    file_Sizes += Program.currentDirectory.DirectoryTable[i].dir_FileSize; // get size 
                    total_File_Size += file_Sizes;
                    string name_File = string.Empty;
                    name_File += new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee);
                    Console.WriteLine($"\t\t{file_Sizes}\t\t" + name_File); // get name of this file and his size 
                }
                else if (Program.currentDirectory.DirectoryTable[i].dir_Attr == 0x10) // if this entry is Directory 
                {
                    folder_Counter++; // increase counter of folder 
                    string name_Directory = new string(Program.currentDirectory.DirectoryTable[i].Dir_Namee); 
                    Console.WriteLine("\t\t<DIR>\t\t" + name_Directory.Trim());
                }
            }
            Console.Write($"\t\t\t{file_Counter} File(s)\t ");
            if (file_Counter > 0)
            {
                Console.Write(total_File_Size+" bytes");
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
            Directory cc = ExecutionClass.MoveToDir(name, Program.currentDirectory);
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
                    Console.Write(total_File_Size + " bytes");
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
                Console.Write(total_File_Size + " bytes");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }
            Console.WriteLine($"\t\t\t{folder_Counter} Dir(s)\t {Mini_FAT.get_Free_Size()} bytes free");
        }
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
        public static void ChangeDirectory_v2(string name)
        {
            if (name == ".") // test case(6) 
            {
                return; // do nothing because “.” Means the current directory. 
            }
            else if (name.Contains("\\") && !name.Contains(".")) // this is fullpath to directory 
            {
                Directory targetDirectory = MoveToDir(name, Program.currentDirectory);
                if (targetDirectory != null) // this mean this directory is found 
                {
                    Program.currentDirectory = targetDirectory; // make current_Directory seek or point to this directory 
                    Program.path = name; // update path 
                    return;
                }
                else // this directory not found 
                {
                    Console.WriteLine($"Error: This path \"{name}\" is not exists!");
                    return;
                }
            }
            else if (name.StartsWith("..")) // this mean go back to parent  
            {
                string[] pathParts = name.Split('\\'); // Split the path 
                int length = pathParts.Length; // get length of {..}
                for (int i = 0; i < length; i++)
                {
                    int last_INDEX = Program.path.LastIndexOf("\\");
                    if (Program.currentDirectory.Parent != null)
                    {
                        Program.currentDirectory = Program.currentDirectory.Parent;
                        Program.path = Program.path.Substring(0, last_INDEX);
                        Program.currentDirectory.Read_Directory();                        
                    }
                    else
                    {
                        return;
                    }
                }
                return;
            }
            else // this is name without fullpath 
            {
                int index = Program.currentDirectory.search_Directory(name); // search for this name 
                if(index == -1) // don't found this directory 
                {
                    Console.WriteLine($"Error: this path '{name}' is not exists!");
                    return;
                }
                else // found this dire but need to ensure this is directory by att = 0x10 
                {
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    if (entry.dir_Attr == 0x0) // this mean this is file 
                    {
                        Console.WriteLine($"Error: '{name}' is not a directory.");
                        return;
                    }
                    else // this mean this is directory so move to this directory 
                    {
                        Directory tagretDirectory = new Directory(entry.Dir_Namee, entry.dir_Attr, entry.dir_First_Cluster, Program.currentDirectory);
                        tagretDirectory.Read_Directory();
                        Program.currentDirectory = tagretDirectory;
                        Program.path += "\\" + name;
                        return;
                    }
                }
            }
        }
        public static void deleteFile(string name)
        {
            if (name.Contains("\\") && !name.Contains("."))
            {
                Console.WriteLine($"This file : {name} is not file name or ACCESS DENIE!");
                return;
            }
            else if (name.Contains("\\") && name.Contains("."))
            {
                File_Entry file_deleted = MoveToFile(name);
                if (file_deleted == null) // this file not found 
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk");
                    return;
                }
                else // file found so ask user if want delete this file 
                {
                    string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
                    string fileName = pathParts[pathParts.Length - 1]; // here we get the file name 
                    string name_of_Directory = pathParts[pathParts.Length - 2]; // get directory parent of this file 
                    Console.Write($"Are you sure that you want to delete \"{fileName}\", please enter Y for yes or N for no: ");
                    Directory targetDirectory = MoveToDir(name_of_Directory, Program.currentDirectory);
                    int index = targetDirectory.search_Directory(fileName);
                    string answer;
                    answer = Console.ReadLine();
                    while (answer != "y" || answer != "n")
                    {
                        if (answer == "y")
                        {
                            file_deleted.Delete_File(fileName);
                            targetDirectory.DirectoryTable.RemoveAt(index);
                            targetDirectory.Write_Directory();
                            targetDirectory.Read_Directory();
                            if (Program.currentDirectory.Parent != null)
                            {
                                Program.currentDirectory.Update_Content(targetDirectory, Program.currentDirectory.Parent);
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
                int index = Program.currentDirectory.search_Directory(name);
                if (index == -1) // file not found 
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk");
                    return;
                }
                else // file is found so ensure this file you search is file not a directory 
                {
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    if(entry.dir_Attr == 0x10) // this is not file 
                    {
                        Console.WriteLine($"This file : {name} is not file name or ACCESS DENIE!");
                        return;
                    }
                    else // this is file so delte it 
                    {
                        File_Entry file = new File_Entry(entry, Program.currentDirectory);
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
                }
            }
        }
        public static void RemoveDirectory(string name)
        {
            if (name.Contains(".")) // this is a file not Directory 
            {
                Console.WriteLine($"This Directory : \"{name}\" is not Directory name or ACCESS DENIE!");
                return;
            }
            if (name.Contains("\\") && !name.Contains("."))
            {
                int last_Index = name.LastIndexOf("\\");
                string directory_Name = name.Substring(0, last_Index); // this is parent of this directory 
                Directory target_Deleted_Directory = MoveToDir(name, Program.currentDirectory); // directory we want to delete it 
                Directory parent_Of_Target = MoveToDir(directory_Name, Program.currentDirectory);
                if (target_Deleted_Directory == null) // this directory is not found 
                {
                    Console.WriteLine($"Error : this directory \"{name}\" is not exist!");
                    return;
                }
                else
                {
                    string name_Target_Directory = new string(target_Deleted_Directory.Dir_Namee); // get name of target we want to delete it 
                    int index = parent_Of_Target.search_Directory(name_Target_Directory); // search in his parent to get index of target 
                    if (target_Deleted_Directory.DirectoryTable.Count > 0) // check if tagret have any files or sup_directory 
                    {
                        Console.WriteLine($"Error: Directory '{name_Target_Directory}' is not empty!");
                        return;
                    }
                    else // this mean this directory is Empty so delete this directory 
                    {
                        Console.Write($"Are you sure you want to delete '{name_Target_Directory}' ,please enter y for Yes or n for No: "); // ask user if want to delete it 
                        string answer = Console.ReadLine();
                        while (answer != "y" || answer != "n")
                        {
                            if (answer == "y")
                            {
                                target_Deleted_Directory.delete_Directory();
                                parent_Of_Target.DirectoryTable.RemoveAt(index);
                                parent_Of_Target.Write_Directory();
                                if (parent_Of_Target != null)
                                {
                                    Program.currentDirectory.Update_Content(target_Deleted_Directory, parent_Of_Target);
                                }
                                return;
                            }
                            else if (answer == "n")
                            {
                                return;
                            }
                            else
                            {
                                Console.Write($"Are you sure you want to delete '{name_Target_Directory}' ,please enter y for Yes or n for No: ");
                                answer = Console.ReadLine();
                            }
                        }
                        Program.currentDirectory.Read_Directory();
                    }
                }
            }

            int index_of_DirectoryDeleted = Program.currentDirectory.search_Directory(name);
            if (index_of_DirectoryDeleted == -1) // if this directory not found 
            {
                Console.WriteLine($"Error: this directory '{name}' does not exist on your disk!");
                return;
            }
            Directory_Entry entry = Program.currentDirectory.DirectoryTable[index_of_DirectoryDeleted]; // get this directory
            if (entry.dir_Attr == 0x0) // checl if this is file or directory 
            {
                Console.WriteLine($"This Directory : \"{name}\" is not Directory name or ACCESS DENIE!");
                return;
            }
            else
            {
                Directory targetDirectory = new Directory(entry.Dir_Namee, entry.dir_Attr, entry.dir_First_Cluster, Program.currentDirectory);
                targetDirectory.Read_Directory(); // ensure read this directory to get content of this directory 
                if (targetDirectory.DirectoryTable.Count > 0) // check if this directory have any files or sup-Directories
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
                            targetDirectory.delete_Directory();
                            Program.currentDirectory.DirectoryTable.RemoveAt(index_of_DirectoryDeleted);
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
            }
        }
        public static void Rename(string _oldName, string _newName)
        {
            if (_newName.Contains("\\")) // ensure the new name should not have any fullpath 
            {
                Console.WriteLine("Error: the new file name should be a file name only you cannot provide a full path!");
                return;
            }
            if (_oldName.Contains("\\"))
            {
                if (_oldName.Contains(".")) // to ensure this is file 
                {
                    File_Entry file = MoveToFile(_oldName);
                    if (file == null) // file not found
                    {
                        Console.WriteLine($"this file: \"{_oldName}\" does not exist on your disk!");
                        return;
                    }
                    else // file found 
                    {
                        // we need to get directory of this file
                        string[] pathParts = _oldName.Split('\\'); // Split the path (extract name & size)
                        string fileName = pathParts[pathParts.Length - 1]; // here we get the file name 
                        string name_of_Directory = pathParts[pathParts.Length - 2]; // get directory parent of this file 
                        Directory targetDirectory = MoveToDir(name_of_Directory, Program.currentDirectory);
                        int index_new_file = targetDirectory.search_Directory(_newName);
                        int index_old_file = targetDirectory.search_Directory(fileName); // we need index of old file 
                        if (index_new_file != -1) // this file found
                        {
                            Console.WriteLine("Error: A duplicate file name exists!");
                            return;
                        }
                        else // file not found 
                        {
                            if (_newName.Contains("."))
                            {
                                Directory_Entry entry = targetDirectory.DirectoryTable[index_old_file];
                                entry.Dir_Namee = _newName.ToCharArray();
                                targetDirectory.Write_Directory();
                                File_Entry file_renamed = new File_Entry(entry, targetDirectory);
                                file_renamed.Read_File_Content();
                                file_renamed.Write_File_Content();
                                targetDirectory.Write_Directory();
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"Error : new file name \"{_newName}\" should have \".txt\" in last name!");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error : this path \"{_oldName}\" is a Directory");
                    return;
                }
            }
            int index_old = Program.currentDirectory.search_Directory(_oldName);
            int index_new = Program.currentDirectory.search_Directory(_newName);
            if (index_old == -1) // file not found
            {
                Console.WriteLine($"this file: \"{_oldName}\" does not exist on your disk!");
                return;
            }
            else // file found 
            {
                Directory_Entry entry = Program.currentDirectory.DirectoryTable[index_old];
                if (index_new != -1) // new file is found 
                {
                    Console.WriteLine("Error: A duplicate file name exists!");
                    return;
                }
                if (entry.dir_Attr == 0x0) // ensure this is file 
                {
                    if (_newName.Contains(".")) // ensure new name should have extension 
                    {
                        entry.Dir_Namee = _newName.ToCharArray(); // change oldName with newName 
                        Program.currentDirectory.Write_Directory(); // ensure save this change with write directory 
                        File_Entry file = new File_Entry(entry, Program.currentDirectory); // get object of this file 
                        file.Read_File_Content(); // read this content 
                        file.Write_File_Content(); // and write it to ensure save his content and size of this file 
                        Program.currentDirectory.Write_Directory(); // save this changes is his directory with writeDirectory
                    }
                    else
                    {
                        Console.WriteLine($"Error : new file name \"{_newName}\" should have \".txt\" in last name!");
                        return;
                    }
                }
            }
        }
        public static void Make_Directory(string name)
        {
            if (name.Contains("\\"))
            {
                // we need to chack if this directory is found or nor 
                Directory found = MoveToDir(name, Program.currentDirectory);
                if (found == null) // so this directory not found so check if can add entry or not 
                {
                    string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
                    string name_of_Directory = pathParts[pathParts.Length - 1]; // get name of directory 
                    string name_Parent_Of_new_Directory = pathParts[pathParts.Length - 2]; // get parent directory of this Directory 
                    Directory parent_New_Directory = MoveToDir(name_Parent_Of_new_Directory, Program.currentDirectory); // get object of parent directory 
                    Directory new_Directory = new Directory(name_of_Directory.ToCharArray(), 0x10, 0, parent_New_Directory);
                    if (parent_New_Directory.Can_Add_Entry(new_Directory)) // check if can add any entry or not 
                    {
                        parent_New_Directory.add_Entry(new_Directory); // add new entry
                        parent_New_Directory.Write_Directory();
                        if (parent_New_Directory.Parent != null)
                        {
                            parent_New_Directory.Parent.Write_Directory();
                            parent_New_Directory.Update_Content(parent_New_Directory.Get_Directory_Entry(), parent_New_Directory.Parent.Get_Directory_Entry());
                        }
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Error: could not create the directory");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Error : this directory \"{new string(found.Dir_Namee)}\" is already exits!");
                    return;
                }
            }
            else // if this without fullpath
            {
                int index = Program.currentDirectory.search_Directory(name); // get index of search this directory 
                if (index == -1) // this directory is not found so create this directory
                {
                    Directory newDirectory = new Directory(name.ToCharArray(), 0x10, 0, Program.currentDirectory);
                    if (Program.currentDirectory.Can_Add_Entry(newDirectory)) // check if can add this entry or not
                    {
                        Program.currentDirectory.add_Entry(newDirectory); // add this entry 
                        Program.currentDirectory.Write_Directory(); // and ensure writeDirectory 
                        if (Program.currentDirectory.Parent != null) // check if parent is not null
                        {
                            Program.currentDirectory.Parent.Write_Directory(); // update parent to save new data
                            Program.currentDirectory.Update_Content(Program.currentDirectory.Get_Directory_Entry(), Program.currentDirectory.Parent.Get_Directory_Entry());
                        }
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Error: Could not create the directory.");
                        return;
                    }
                }
                else // this directory is found so print massage this directory is found 
                {
                    Console.WriteLine($"Error : this directory {name} is already exists!");
                    return;
                }
            }
        }
        public static void TypeFiles(string name)
        {
            if (name.Contains("\\") && !name.Contains("."))
            {
                Console.WriteLine($"Error: may be this \"{name}\" is not a file name or ACCESS IS DENIED!");
                return;
            }
            if (name.Contains("\\") && name.Contains("."))
            {
                File_Entry file = MoveToFile(name); // split to get file name 
                if (file == null) // mean if file not found 
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk!");
                    return;
                }
                else // file found so read contend of this file 
                {
                    file.Read_File_Content(); // read this content 
                    file.Print_Content(); // print conetent of this file 
                }
            }
            else
            {
                int index = Program.currentDirectory.search_Directory(name);
                if (index == -1) // file not found 
                {
                    Console.WriteLine($"this file : \"{name}\" does not exist on your disk!");
                    return;
                }
                else // file is found 
                {
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    if (entry.dir_Attr == 0x10) // this mean this is directory it's not file 
                    {
                        Console.WriteLine($"Error: may be this \"{name}\" is not a file name or ACCESS IS DENIED!");
                        return;
                    }
                    else // this is file so get content of this file 
                    {
                        File_Entry file = new File_Entry(entry, Program.currentDirectory);
                        file.Read_File_Content();
                        file.Print_Content();
                        return;
                    }
                }
            }
        }
        public static void Export_Files(string name)
        {
            if (name.Contains("\\") && name.Contains(".")) // to ensure this is file 
            {
                File_Entry file_exported = MoveToFile(name);
                if (file_exported == null) // this is mean file not found 
                {
                    Console.WriteLine($"this path: \"{name}\" does not exist on your disk!");
                    return;
                }
                else // this is mean file is found 
                {
                    File_Entry.helpermethod_Exported_In_EXE_File(file_exported, name);
                    return;
                }
            }
            else if (name.Contains("\\") && !name.Contains(".")) // this is full path to directory 
            {
                Directory targetDirectory = MoveToDir(name, Program.currentDirectory);
                if (targetDirectory == null) // this directory does not exist so errro 
                {
                    Console.WriteLine($"this path :\"{name}\" does not exist on your disk!");
                    return;
                }
                else // we found this directory 
                {
                    List<string> list_Fullpath_Files = new List<string>();
                    int counter = 0;
                    foreach (var entry in targetDirectory.DirectoryTable)
                    {
                        if (entry.dir_Attr == 0x10) // this is directory so don't do anythiing 
                        {
                            continue;
                        }
                        else // this is file 
                        {
                            File_Entry file_Exported = new File_Entry(entry, targetDirectory); // get object of File_Entry to this file 
                            file_Exported.Read_File_Content();
                            string name_File = new string(file_Exported.Dir_Namee).Trim('\0');
                            string exportedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name_File);
                            string content = file_Exported.content.Replace("\0", " ").Trim();
                            File.WriteAllText(exportedPath, content);
                            counter++;
                            string[] pathParts = name.Split('\\'); // Split the path (extract name & size)
                            string name_of_File = name_File; // get name of parent file 
                            string name_Parent_Of_File = pathParts[pathParts.Length - 1]; // get parent directory of this File 
                            string fullpath_File_Exported = name_Parent_Of_File + "\\" + name_of_File;
                            list_Fullpath_Files.Add(fullpath_File_Exported);
                        }
                    }
                    foreach (var files in list_Fullpath_Files)
                    {
                        Console.WriteLine(files);
                    }
                    Console.WriteLine($"\t{counter} file(s) exported.");
                    return;
                }
            }
            else // here only files without full path or directory without fullpath
            {
                int index = Program.currentDirectory.search_Directory(name);
                if (index == -1) // not found this file or directory
                {
                    Console.WriteLine($"This path :\"{name}\" does not exist on your disk!");
                    return;
                }
                else // if found this file or directory 
                {
                    // need to check this is directory or file
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    if (entry.dir_Attr == 0x0) // if this is file 
                    {
                        File_Entry file_Exported = new File_Entry(entry, Program.currentDirectory); // get object of this file 
                        // use helper method to export this file 
                        File_Entry.helpermethod_Exported_In_EXE_File(file_Exported, name);
                        return;
                    }
                    else // this is directory 
                    {
                        Directory targetDirectory = MoveToDir(name, Program.currentDirectory);
                        if (targetDirectory == null)
                        {
                            Console.WriteLine($"this path : \"{name}\"does not exists on your disk!");
                            return;
                        }
                        else // directory found 
                        {
                            int counter = 0;
                            List<string> list = new List<string>();
                            foreach (var entryFiles in targetDirectory.DirectoryTable)
                            {
                                if (entryFiles.dir_Attr == 0x10) // this is directory to don't do any thing and continue 
                                {
                                    continue;
                                }
                                else // this is file so export it 
                                {
                                    File_Entry file_Exported = new File_Entry(entryFiles, targetDirectory);
                                    file_Exported.Read_File_Content();
                                    string name_File = new string(file_Exported.Dir_Namee).Trim('\0');
                                    string exportedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name_File);
                                    string content = file_Exported.content;
                                    if (content.Contains("\0"))
                                    {
                                        content = content.Replace("\0", " ");
                                    }
                                    content = content.Trim();
                                    File.WriteAllText(exportedPath, content);
                                    counter++;
                                    string name_of_File = name_File;
                                    string name_Parent_Of_File = new string(targetDirectory.Dir_Namee).Trim('\0');
                                    string fullpath_File_Exported = name_Parent_Of_File + "\\" + name_of_File;
                                    list.Add(fullpath_File_Exported);
                                }
                            }
                            foreach (var files in list)
                            {
                                Console.WriteLine(files);
                            }
                            Console.WriteLine($"\t{counter} file(s) exported.");
                            return;
                        }
                    }
                }
            }
        }
        public static void Export_Files(string source, string destinition)
        {
            if (source.Contains("\\") && source.Contains(".")) // to ensure this is fullpath to file
            {
                File_Entry file = MoveToFile(source);
                if (file == null) // this file not found 
                {
                    Console.WriteLine($"this path: \"{source}\" does not exist on your disk!");
                    return;
                }
                else // this file is found 
                {
                    file.Read_File_Content();
                    if (destinition.Contains("\\") && destinition.Contains("."))
                    {
                        if(File_Entry.CheckerMethod(destinition))
                        {
                            int counter = 0;
                            File_Entry.helpermethod_Exported_In_Your_Disk(file, destinition);
                            counter++;
                            int lastSlashIndex = source.LastIndexOf('\\');
                            int secondLastSlashIndex = source.LastIndexOf('\\', lastSlashIndex - 1);
                            string relativePath = source.Substring(secondLastSlashIndex + 1);
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"\t{counter} file(s) exported.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"Error : this path {destinition} does not exist on your Copmuter!");
                            return;
                        }
                        
                    }
                    else if (destinition.Contains("\\") && !destinition.Contains(".")) // this is fullpath to directory 
                    {

                        if (System.IO.Directory.Exists(destinition))
                        {
                            int counter = 0;
                            if(File_Entry.CheckerMethod(destinition))
                            {
                                File_Entry.helpermethod_Exported_In_Your_Disk(file, destinition);
                                counter++;
                                int lastSlashIndex = source.LastIndexOf('\\');
                                int secondLastSlashIndex = source.LastIndexOf('\\', lastSlashIndex - 1);
                                string relativePath = source.Substring(secondLastSlashIndex + 1);
                                Console.WriteLine($"{relativePath}");
                                Console.WriteLine($"\t{counter} file(s) exported.");
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"Error : this path {destinition} does not exist on your Copmuter!");
                                return;
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine($"this path :\"{destinition}\" does not exist on your computer !");
                            return;
                        }
                    }
                }
            }
            else // if file without fullpath 
            {
                int index = Program.currentDirectory.search_Directory(source); // search this file 
                if (index == -1)
                {
                    Console.WriteLine($"this path : \"{source}\" does not found on your disk");
                    return;
                }
                else // if source file is found 
                {
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    File_Entry file_Exported = new File_Entry(entry, Program.currentDirectory); // get this file 
                    if (destinition.Contains("\\") && destinition.Contains("."))
                    {
                        int counter = 0;
                        File_Entry.helpermethod_Exported_In_Your_Disk(file_Exported, destinition);
                        counter++;
                        string relativePath = new string(Program.currentDirectory.Dir_Namee).Trim('\0') + "\\" + source;
                        Console.WriteLine($"{relativePath}");
                        Console.WriteLine($"\t{counter} file(s) exported.");
                        return;
                    }
                    else if (destinition.Contains("\\") && !destinition.Contains("."))
                    {
                        if (System.IO.Directory.Exists(destinition))
                        {
                            int counter = 0;
                            File_Entry.helpermethod_Exported_In_Your_Disk(file_Exported, destinition);
                            counter++;
                            string relativePath = new string(Program.currentDirectory.Dir_Namee).Trim('\0') + "\\" + source;
                            Console.WriteLine($"{relativePath}");
                            Console.WriteLine($"\t{counter} file(s) exported.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"this path :\"{destinition}\" does not exist on your computer !");
                            return;
                        }
                    }

                }
            }
        }
        public static void Copy_Source(string source_name)
        {
            if(source_name.Contains("\\") && source_name.Contains(".")) // this is mean this is fullpath to file name 
            {
                File_Entry file = MoveToFile(source_name); // get this file 
                if(file == null) // this is mean file not found so copy this file 
                {
                    Console.WriteLine($"this path \"{source_name}\" does not exist on your disk!");
                    return;
                }
                else // this file is found so ask user if he want to overwrite this file or not
                {
                    // we need to get file name 
                    string file_Name = new string(file.Dir_Namee);
                    // ok we need to check in current directory if this fils is found or nor 
                    int index = Program.currentDirectory.search_Directory(file_Name);
                    if(index == -1) // mean file not found so copy it without overwrite
                    {
                        File_Entry.Copy_CreateFile(file, file_Name);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("can't copy this file onto itself");
                        return;
                        //File_Entry.OverWrite(file, file_Name, source_name);
                        //return;
                    }                    
                }
            }
            else if(source_name.Contains("\\") && !source_name.Contains("."))  // this is mean fullpath to directory
            {
                Directory targetDirectory = MoveToDir(source_name, Program.currentDirectory); // get this directory 
                if(targetDirectory == null) // this directory not found 
                {
                    Console.WriteLine($"this path \"{source_name}\" does not exist on your disk!");
                    return;
                }
                else // this directory is found 
                {
                    int copied_files = 0;
                    List<string> files_copied_list = new List<string>();

                    foreach (var entry in targetDirectory.DirectoryTable) // foreach to check all file 
                    {
                        if (entry.dir_Attr == 0x0)
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
                                        File_Entry sourceFile = new File_Entry(entry, targetDirectory);
                                        sourceFile.Read_File_Content(); // Load content from source
                                        existingFile.content = sourceFile.content;
                                        existingFile.dir_FileSize = sourceFile.dir_FileSize;
                                        existingFile.Write_File_Content(); // Overwrite the content
                                        copied_files++;
                                        string name_Directory = new string(targetDirectory.Dir_Namee) + "\\" + file_Name;
                                        files_copied_list.Add(name_Directory);
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
                                File_Entry file = new File_Entry(entry, targetDirectory);
                                File_Entry.Copy_CreateFile(file, file_Name);                               
                                copied_files++;
                                string name_Directory = new string(targetDirectory.Dir_Namee) + "\\" + file_Name;
                                files_copied_list.Add(name_Directory);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    foreach (var items in files_copied_list)
                    {
                        Console.WriteLine(items);
                    }
                    Console.WriteLine($"\t{copied_files} file(s) copied.");
                    return;
                }
            }
            else
            {
                int index = Program.currentDirectory.search_Directory(source_name);
                if(index == -1) // this mean file not found 
                {
                    Console.WriteLine($"this file \"{source_name}\" does not exist on your disk!");
                    return;
                }
                else // this mean file is found 
                {
                    Directory_Entry entry = Program.currentDirectory.DirectoryTable[index];
                    int copy_files = 0;
                    List<string> files_copied_list = new List<string>();

                    if (entry.dir_Attr == 0x10) // this is directory 
                    {
                        Directory targetDirectory = new Directory(entry.Dir_Namee, entry.dir_Attr, entry.dir_First_Cluster, Program.currentDirectory);
                        targetDirectory.Read_Directory();
                        foreach (var entryFiles in targetDirectory.DirectoryTable)
                        {
                            if (entryFiles.dir_Attr == 0x0)
                            {                                
                                string file_Name = new string(entryFiles.Dir_Namee).Trim();
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
                                            File_Entry sourceFile = new File_Entry(entryFiles, targetDirectory);

                                            sourceFile.Read_File_Content(); // Load content from source
                                            existingFile.content = sourceFile.content;
                                            existingFile.dir_FileSize = sourceFile.dir_FileSize;
                                            existingFile.Write_File_Content(); // Overwrite the content
                                            copy_files++;
                                            string name_Directory = new string(targetDirectory.Dir_Namee) + "\\" + file_Name;
                                            files_copied_list.Add(name_Directory);
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
                                    File_Entry file = new File_Entry(entryFiles, targetDirectory);
                                    File_Entry.Copy_CreateFile(file, new string(file.Dir_Namee).Trim('\0'));                                   
                                    copy_files++;                                   
                                    string name_Directory = new string(targetDirectory.Dir_Namee) + "\\" + file_Name;
                                    files_copied_list.Add(name_Directory);
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        foreach (var items in files_copied_list)
                        {
                            Console.WriteLine(items);
                        }
                        Console.WriteLine($"{copy_files} file(s) copied.");
                    }
                    else // this is file and file is found so ask user if want to overwrite this file or not 
                    {
                        Console.WriteLine("Can't copy this file onto itself.");
                        return;
                        //File_Entry file_Copied = new File_Entry(entry, Program.currentDirectory);
                        //string name_Of_Directoy = new string(Program.currentDirectory.Dir_Namee);
                        //File_Entry.OverWrite(file_Copied, source_name, name_Of_Directoy);
                    }
                }
            }
        }
        public static void CopyMethod(string source_Name, string destinition_Name)
        {
            // first if source is fullpath 
            if(source_Name.Contains("\\") && !source_Name.Contains(".") || !source_Name.Contains("\\") && !source_Name.Contains("."))
            {
                if(destinition_Name.Contains("\\") && !destinition_Name.Contains(".") || !destinition_Name.Contains("\\") && !destinition_Name.Contains("."))
                Console.WriteLine("This is Bonus ... !");
                return;
            }
            // first if Source is full path to file 
            if(source_Name.Contains("\\") && source_Name.Contains(".")) // to ensure this is file
            {
                File_Entry File = MoveToFile(source_Name); // get this file 
                if(File == null) // if this File not Found 
                {
                    Console.WriteLine($"this path \"{source_Name}\" does not exist on your disk!");
                    return;
                }
                if(destinition_Name.Contains("\\") && !destinition_Name.Contains (".")) // here is destinition is fullpath to directory 
                {
                    Directory directory_Distinition = MoveToDir(destinition_Name, Program.currentDirectory);
                    if (directory_Distinition != null) // this is mean we copy file to directory 
                    {
                        // we need to ensure if this file is found in this directory or not 
                        // so get this file name 
                        string name_of_File = new string(File.Dir_Namee);
                        int index = directory_Distinition.search_Directory(name_of_File); // and search for it 
                        if (index != -1) // this mean file is found so ask user if want to overwrite it or not with help method OverWrite in Class File_Entry 
                        {
                            File_Entry.OverWrite(File, name_of_File, destinition_Name); // this method help us to overwrite or not 
                            return;
                        }
                        else
                        {
                            File_Entry.Copy_CreateFile(File,destinition_Name);
                            return;
                        }
                    }
                }
                else if(destinition_Name.Contains("\\") && destinition_Name.Contains(".")) // here is destinition is fullpath to file  
                {
                    // first we need to check if file is exist here or not 
                    File_Entry File_Destiniton_Copy = MoveToFile(destinition_Name);
                    if(File_Destiniton_Copy != null ) // here this is mean this file are Found
                    {
                        // so ask user if he want to overwrite this file or not with help function overwrite in class File_Entry
                        string name_File_will_copyied = new string(File_Destiniton_Copy.Dir_Namee); // get name of file 
                        File_Entry.OverWrite(File_Destiniton_Copy, name_File_will_copyied, destinition_Name); // method help us to overwrite
                    }
                    else // here this is mean file not found 
                    {
                        // First get name of Destinition file 
                        int last_index_for_name = destinition_Name.LastIndexOf("\\"); 
                        string name_of_Directory = destinition_Name.Substring(0, last_index_for_name); // get fullpath of parent of this file 
                        Directory targetDirectory = MoveToDir(name_of_Directory, Program.currentDirectory); // move to this directory

                        if (targetDirectory == null) // this is mean directory is null and path is error
                        {
                            Console.WriteLine($"this path \"{destinition_Name}\" does not exist on your disk!");
                            return;
                        }
                        else // we found this directory 
                        {
                            File_Entry.Copy_CreateFile(File, destinition_Name);
                            return;
                        }
                    }
                }
                else // if destinition is file without fullpath 
                {
                    
                    int index = Program.currentDirectory.search_Directory(destinition_Name);
                    if (index != -1) // this is mean File are found 
                    {
                        // so ask user if he want to overwrite this file or not 
                        string name_Of_Directory = new string(Program.currentDirectory.Dir_Namee); // get name of Directory 
                        File_Entry.OverWrite(File, destinition_Name, name_Of_Directory);
                    }
                    else // here if file not found so copy it 
                    {
                        int lastSlashIndex = source_Name.LastIndexOf('\\');
                        int secondLastSlashIndex = source_Name.LastIndexOf('\\', lastSlashIndex - 1);
                        string relativePath = source_Name.Substring(secondLastSlashIndex + 1); 
                        Console.WriteLine($"{relativePath}");
                        File_Entry.Copy_CreateFile(File, destinition_Name);
                        return;
                    }
                }                            
            }
            else // else for source without fullpath if not contain \ and . 
            {
                int index = Program.currentDirectory.search_Directory(source_Name); // search for index if this file is found or not 
                if(index == -1 ) // source file not found on disk 
                {
                    Console.WriteLine($"this path \"{source_Name}\" does not exist on your disk!");
                    return;
                }
                else // source file is found 
                {
                    // first if destinition is fullpath to file 
                    if(destinition_Name.Contains(".") && destinition_Name.Contains("\\")) 
                    {
                        File_Entry file_Of_Destinition = MoveToFile(destinition_Name);
                        if(file_Of_Destinition != null ) // this file is exist with this same name 
                        {
                            // so ask user if want to overwrite or not 
                            // we need to get name of directory of this file 
                            File_Entry.OverWrite(file_Of_Destinition, source_Name, destinition_Name);
                        }
                        else // here if file not found 
                        {
                            // we need to get directory name by split this fullpath 
                            int last_index_for_name = destinition_Name.LastIndexOf("\\");
                            string name_of_Directory = destinition_Name.Substring(0, last_index_for_name); // get fullpath of parent of this file 
                            Directory targetDirectory = MoveToDir(name_of_Directory, Program.currentDirectory);
                            Directory_Entry Dire_OR_Files = Program.currentDirectory.DirectoryTable[index];                                                       
                            File_Entry file_Copied = new File_Entry(Dire_OR_Files, targetDirectory);
                            File_Entry.Copy_CreateFile(file_Copied, destinition_Name);                            
                        }
                    }
                    else if(destinition_Name.Contains("\\") && !destinition_Name.Contains("."))// if destinition is fullpath to directory 
                    {
                        // so we need to search this directory if found 
                        Directory targetDirectory = MoveToDir(destinition_Name, Program.currentDirectory);
                        if(targetDirectory == null ) // this directory is not found 
                        {
                            Console.WriteLine($"this path \"{destinition_Name}\" does not exist on your disk!");
                            return;
                        }
                        else // if we found directory 
                        {
                            // first we need to search this file if found or not in this directory or not 

                            int index_File_Destinition = targetDirectory.search_Directory(source_Name);
                            if(index_File_Destinition == -1) // this mean this file not found in destinition 
                            {
                                // so we now will copy this file here 
                                Directory_Entry Dire_Or_Files = Program.currentDirectory.DirectoryTable[index]; // get index of source file we need to copy it 
                                File_Entry file_Copied = new File_Entry(Dire_Or_Files, Program.currentDirectory);
                                File_Entry.Copy_CreateFile(file_Copied, destinition_Name);
                                return;
                            }
                            else // we found this file in this destinition directory 
                            {
                                // so ask user if want to overwrite this file or not 
                                Directory_Entry Dire_Or_Files = Program.currentDirectory.DirectoryTable[index]; // get index of source file we need to copy it 
                                File_Entry file_Copied = new File_Entry(Dire_Or_Files, Program.currentDirectory);
                                File_Entry.OverWrite(file_Copied, source_Name, destinition_Name);
                                return;
                            }
                        }

                    }
                    else if(destinition_Name.Contains(".")) // here else for destinition without fullpath but to file  
                    {
                        int index_Destinition = Program.currentDirectory.search_Directory(destinition_Name);
                        Directory_Entry Dire_OR_Files = Program.currentDirectory.DirectoryTable[index];
                        File_Entry file_Copied = new File_Entry(Dire_OR_Files, Program.currentDirectory);
                        if (index_Destinition == -1) // here we don't found File so copy it 
                        {
                            File_Entry.Copy_CreateFile(file_Copied, destinition_Name);
                            return;
                        }
                        else // here is file is found 
                        {
                            // so ask user for if he want to overwrite this file or not 
                            // get name of Dirctory of this file
                            string name_Of_Directory_Destinition = new string(Program.currentDirectory.Dir_Namee);
                            File_Entry.OverWrite(file_Copied, destinition_Name, name_Of_Directory_Destinition);
                        }

                    }
                    else // this mean destinition is directory 
                    {
                        Directory targetDirectory = MoveToDir(destinition_Name, Program.currentDirectory);
                        if(targetDirectory != null) // directory is found so check if this file with same name is found or nor 
                        {
                            int index_Destinition_File = targetDirectory.search_Directory(source_Name);
                            if(index_Destinition_File == -1) // this is mean file does not exist so copy it 
                            {
                                Directory_Entry Dire_Or_Files = Program.currentDirectory.DirectoryTable[index];
                                File_Entry file_Copied = new File_Entry(Dire_Or_Files, Program.currentDirectory);
                                File_Entry.Copy_CreateFile(file_Copied, destinition_Name);
                                return;
                            }
                            else // this mean file is found so ask user if want overwrite it 
                            {
                                Directory_Entry Dire_Or_Files = targetDirectory.DirectoryTable[index_Destinition_File];
                                File_Entry file_Copied = new File_Entry(Dire_Or_Files, targetDirectory);
                                File_Entry.OverWrite(file_Copied, source_Name, destinition_Name);
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"this path \"{destinition_Name}\" does not exist on your disk!");
                            return;
                        }
                    }

                }
            }
        }
    }
}