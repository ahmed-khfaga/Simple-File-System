using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Command_Line
    {
        public string[] comm_Arg;
        public Command_Line(string command)
        {
            comm_Arg = command.Split(' ');
            if (comm_Arg.Length == 1)
            {
                Command(comm_Arg);
            }
            else if (comm_Arg.Length > 1)
            {
                Commmand2Arg(comm_Arg);
            }
        }
        static void Command(string[] command_Array)
        {
            if (command_Array[0].ToLower().Trim() == "quit")
            {
                Program.currentDirectory.Write_Directory();
                Environment.Exit(0);
            }
            else if (command_Array[0].ToLower() == "cls")
            {
                Console.Clear();
            }
            else if (command_Array[0].ToLower() == "cd") // this is test case (1) we print the current directory
            {
                Console.WriteLine(Program.currentDirectory.Dir_Namee); 
            }
            else if (string.IsNullOrWhiteSpace(command_Array[0].ToLower()))
            {
                return;
            }
            else if (command_Array[0].ToLower() == "help")
            {
                Console.WriteLine("cd\t\t- Change the current default directory to .\n\t\tIf the argument is not present, report the current directory.\n\t\tIf the directory does not exist an appropriate error should be reported.");
                Console.WriteLine("cls\t\t- Clear the screen.");
                Console.WriteLine("dir\t\t- List the contents of directory.");
                Console.WriteLine("quit\t\t- Quit the shell.");
                Console.WriteLine("copy\t\t- Copies one or more files to another location.");
                Console.WriteLine("del\t\t- Deletes one or more files.");
                Console.WriteLine("help\t\t- Provides Help information for commands.");
                Console.WriteLine("md\t\t- Creates a directory.");
                Console.WriteLine("rd\t\t- Removes a directory.");
                Console.WriteLine("rename\t\t- Renames a file.");
                Console.WriteLine("type\t\t- Displays the contents of a text file.");
                Console.WriteLine("import\t\t- import text file(s) from your computer.");
                Console.WriteLine("export\t\t- export text file(s) to your computer.");
            }
            else if (command_Array[0].ToLower() == "md")
            {
                Console.WriteLine("Error : md command syntax is \n md [directory] \n[directory] can be a new directory name or fullpath of a new directory\nCreates a directory.");
            }
            else if (command_Array[0].ToLower() == "rename")
            {
                Console.WriteLine("ERROR:\nRenames a file. \r\nrename command syntax is \r\nrename [fileName] [new fileName] \r\n[fileName] can be a file name or fullpath of a filename \r\n[new fileName] can be a new file name not fullpath");
            }
            else if (command_Array[0].ToLower() == "type")
            {
                Console.WriteLine("ERROR:\nDisplays the contents of a text file. \r\ntype command syntax is \r\ntype [file]+ \r\nNOTE: it displays the filename before its content for every \r\nfile \r\n[file] can be file Name (or fullpath of file) of text file \r\n+ after [file] represent that you can pass more than file \r\nName (or fullpath of file).");
            }
            else if (command_Array[0].ToLower() == "rd")
            {
                Console.WriteLine("Error : rd command syntax is \r\nrd [directory]+ \r\n[directory] can be a directory name or fullpath of a directory \r\n+ after [directory] represent that you can pass more than \r\ndirectory name (or fullpath of directory)");
            }
            else if (command_Array[0].ToLower() == "copy")
            {
                Console.WriteLine("ERROR:\nCopies one or more files to another location. \r\ncopy command syntax is \r\n copy [source] \r\nor \r\n copy [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory ");
            }
            else if (command_Array[0].ToLower() == "import")
            {
                Console.WriteLine("ERROR:\n- import text file(s) from your computer \r\nimport command syntax is \r\nimport [source] \r\nor \r\nimport [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) from your physical disk \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory ");
            }
            else if (command_Array[0].ToLower() == "export")
            {
                Console.WriteLine("ERROR:\n- export text file(s) to your computer \r\nexport command syntax is \r\nexport [source] \r\nor \r\nexport [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) from your virtual disk \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory. ");
            }
            else if (command_Array[0].ToLower() == "del")
            {
                Console.WriteLine("ERROR:\nDeletes one or more files. \r\nNOTE: it confirms the user choice to delete the file before \r\ndeleting \r\ndel command syntax is \r\ndel [dirFile]+ \r\n+ after [dirfile] represent that you can pass more than file \r\nName (or fullpath of file) or directory name (or fullpath of \r\ndirectory) \r\n[dirfile] can be file Name (or fullpath of file) or \r\ndirectory name (or fullpath of directory).");
            }
            else if (command_Array[0].ToLower() == "dir")
            {
                ExecutionClass.Dir(); // for current Directory
            }
            else
            {
                Console.WriteLine($"Error: {command_Array[0].ToLower()} => This command is not supported by the help utility.");
                Console.WriteLine("Enter help to show our support command");
            }
        }
        static void Commmand2Arg(string[] commandArray_2Agr)
        {
            if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "cd")
            {
                Console.WriteLine("cd\t\t - Change the current default directory to .\n\t\tIf the argument is not present, report the current directory.\n\t\tIf the directory does not exist an appropriate error should be reported.");
            }           
            else if (commandArray_2Agr[0].ToLower() == "")
            {
                return;
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "")
            {
                Console.WriteLine("cd\t\t- Change the current default directory to .\n\t\tIf the argument is not present, report the current directory.\n\t\tIf the directory does not exist an appropriate error should be reported.");
                Console.WriteLine("cls\t\t- Clear the screen.");
                Console.WriteLine("dir\t\t- List the contents of directory.");
                Console.WriteLine("quit\t\t- Quit the shell.");
                Console.WriteLine("copy\t\t- Copies one or more files to another location.");
                Console.WriteLine("del\t\t- Deletes one or more files.");
                Console.WriteLine("help\t\t- Provides Help information for commands.");
                Console.WriteLine("md\t\t- Creates a directory.");
                Console.WriteLine("rd\t\t- Removes a directory.");
                Console.WriteLine("rename\t\t- Renames a file.");
                Console.WriteLine("type\t\t- Displays the contents of a text file.");
                Console.WriteLine("import\t\t- import text file(s) from your computer.");
                Console.WriteLine("export\t\t- export text file(s) to your computer.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "cls")
            {
                Console.WriteLine("cls\t\t- Clear the screen.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "dir")
            {
                Console.WriteLine("dir\t\t- List the contents of directory.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "quit")
            {
                Console.WriteLine("quit\t\t- Quit the shell.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "copy")
            {
                Console.WriteLine("Copies one or more files to another location. \r\ncopy command syntax is \r\n copy [source] \r\nor \r\n copy [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory ");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "del")
            {
                Console.WriteLine("del\t\t- Deletes one or more files.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "help")
            {
                Console.WriteLine("Provides Help information for commands. \r\n help command syntax is \r\n help \r\nor \r\n For more information on a specific command, type help \r\n[command] \r\ncommand - displays help information on that command.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "md")
            {
                Console.WriteLine("md\t\t- Creates a directory.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "rd")
            {
                Console.WriteLine("Removes a directory. \r\nNOTE: it confirms the user choice to delete the directory \r\nbefore deleting \r\nrd command syntax is \r\nrd [directory]+ \r\n[directory] can be a directory name or fullpath of a \r\ndirectory \r\n+ after [directory] represent that you can pass more than \r\ndirectory name (or fullpath of directory).");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "rename")
            {
                Console.WriteLine("rename\t\t- Renames a file.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "type")
            {
                Console.WriteLine("type\t\t- Displays the contents of a text file.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "import")
            {
                Console.WriteLine("import\t\t- import text file(s) from your computer.");
            }
            else if (commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() == "export")
            {
                Console.WriteLine("export\t\t- export text file(s) to your computer.");

            }
            else if (commandArray_2Agr[0].ToLower() == "cls")
            {
                if (commandArray_2Agr[1].ToLower() == string.Empty)
                {
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Error : cls command syntax is \n cls \n function: Clear the screen");
                }
            }
            else if (commandArray_2Agr[0].ToLower() == "quit")
            {
                if (commandArray_2Agr[1].ToLower() == string.Empty)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Error : quit command syntax is \n quit \n function: Quit the shell");
                }
            }
            // dir fullpath done with all test case            
            else if (commandArray_2Agr[0].ToLower() == "dir")
            {

                string dname = commandArray_2Agr[1];
                if (dname == ".")
                {
                    // Current directory
                    ExecutionClass.Dir_Sup_Dir(true);
                    return;
                }
                else if (dname == "..")
                {
                    Directory d = Program.currentDirectory;

                    if (d.Dir_Namee == Mini_FAT.Root.Dir_Namee)
                    {
                        Console.WriteLine("Error : This is Root Directory!");
                        return;
                    }
                    // Get the parent directory and list its contents
                    if (d.Parent != null)
                    {
                        int file_Counter1 = 0;
                        int folder_Counter1 = 0;
                        int file_Sizes1 = 0;
                        int total_File_Size1 = 0;
                        Directory cc = d.Parent;
                        Console.WriteLine($"Directory of {Program.path}\\.. : \n");
                        for (int i = 0; i < cc.DirectoryTable.Count; i++)
                        {
                            if (cc.DirectoryTable[i].dir_Attr == 0x0)
                            {
                                file_Counter1++;
                                file_Sizes1 += cc.DirectoryTable[i].dir_FileSize;
                                total_File_Size1 += file_Sizes1;
                                string m = string.Empty;
                                m += new string(cc.DirectoryTable[i].Dir_Namee);
                                Console.WriteLine($"\t\t{file_Sizes1}\t\t" + m);
                                // Console.WriteLine();
                            }
                            else if (cc.DirectoryTable[i].dir_Attr == 0x10) // لو في فولدر 
                            {
                                folder_Counter1++;
                                string S = new string(cc.DirectoryTable[i].Dir_Namee);
                                Console.WriteLine("\t\t<DIR>\t\t" + S);
                            }
                            file_Sizes1 = 0;
                        }
                        Console.Write($"\t\t\t{file_Counter1} File(s)\t ");
                        if (file_Counter1 > 0)
                        {
                            Console.Write(total_File_Size1);
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine($"\t\t\t{folder_Counter1} Dir(s)\t {Mini_FAT.get_Free_Size()} bytes free");
                    }
                    else
                    {
                        Console.WriteLine("Error: Cannot move above the root directory.");
                    }
                    return;
                }
            
                else
                {                   
                    ExecutionClass.list_OF_Directory(dname);
                    return;
                }
            }
            // work
            else if (commandArray_2Agr[0].ToLower() == "md")
            {
                if (commandArray_2Agr.Length > 2)
                {
                    Console.WriteLine("Error : md command syntax is \n md [directory] \n[directory] can be a new directory name or fullpath of a new directory\nCreates a directory.");
                    return;
                }
                char[] prohibitedChars = new char[]
                {
                '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '=', '+', '[', ']', '{', '}',
                ';', ':', ',', '.', '<', '>', '/', '?', '\\', '|', '~', '`','"',
                };
                string dirName = commandArray_2Agr[1];
                if (dirName.StartsWith("\"") && dirName.EndsWith("\""))
                {
                    dirName = dirName.Trim('"');
                }
                if (commandArray_2Agr.Length < 2 || string.IsNullOrWhiteSpace(dirName) || dirName == "." || dirName.ToLower().Contains("."))
                {
                    Console.WriteLine("Error : md command syntax is \n md [directory] \n[directory] can be a new directory name or fullpath of a new directory\nCreates a directory.");
                    return;
                }
                if (prohibitedChars.Contains(dirName[0]))
                {
                    Console.WriteLine($"Error: Directory name cannot start with \"{dirName[0]}\". Please enter a valid name.");
                    return;
                }              
                ExecutionClass.Make_Directory(dirName);
                return;
            }
            // work
            else if (commandArray_2Agr[0].ToLower() == "rd")
            {
                if (commandArray_2Agr[1] == "")
                {
                    Console.WriteLine("Error : rd command syntax is \r\nrd [directory]+ \r\n[directory] can be a directory name or fullpath of a directory \r\n+ after [directory] represent that you can pass more than \r\ndirectory name (or fullpath of directory)");
                }
                for (int i = 0; i < commandArray_2Agr.Length; i++)
                {
                    if (commandArray_2Agr[i] == "rd")
                    {
                        continue;
                    }
                    else if (commandArray_2Agr[i] == string.Empty)
                    {
                        continue;
                    }
                    ExecutionClass.RemoveDirectory(commandArray_2Agr[i]);
                }
            }
            //work xd done with all test case  
            else if (commandArray_2Agr[0].ToLower() == "cd")
            {
                
                ExecutionClass.ChangeDirectory_v2(commandArray_2Agr[1]);
            }
            // work
            else if (commandArray_2Agr[0].ToLower() == "rename")
            {
                if(commandArray_2Agr.Length == 3)
                {
                    ExecutionClass.Rename(commandArray_2Agr[1], commandArray_2Agr[2]);
                }
                else
                {
                    Console.WriteLine("Error :\n-Renames a file. \r\nrename command syntax is \r\nrename [fileName] [new fileName] \r\n[fileName] can be a file name or fullpath of a filename \r\n[new fileName] can be a new file name not fullpath");
                    return;
                }                                     
            }
            // work          
            else if (commandArray_2Agr[0].ToLower() == "import")
            {
                if (commandArray_2Agr[1].ToLower() == string.Empty)
                {
                    Console.WriteLine("ERROR:\n- import text file(s) from your computer \r\nimport command syntax is \r\nimport [source] \r\nor \r\nimport [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) from your physical disk \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory ");
                    return;
                }
                if (commandArray_2Agr.Length == 2)
                {
                    ExecutionClass.Importv2(commandArray_2Agr[1]);
                }
                else
                {
                    ExecutionClass.ImportMethod(commandArray_2Agr[1], commandArray_2Agr[2]);
                }
            }
            // work
            else if (commandArray_2Agr[0].ToLower() == "export")
            {
                if (commandArray_2Agr[1] == string.Empty)
                {
                    Console.WriteLine("ERROR:\n- export text file(s) to your computer \r\nexport command syntax is \r\nexport [source] \r\nor \r\nexport [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) from your virtual disk \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory. ");
                    return;
                }
                if (commandArray_2Agr.Length == 2)
                {
                    ExecutionClass.Export_Files(commandArray_2Agr[1]);
                }
                else
                {
                    ExecutionClass.Export_Files(commandArray_2Agr[1], commandArray_2Agr[2]);
                }
            }
            //del work done all test case
            else if (commandArray_2Agr[0].ToLower() == "del") // for only files without any bonus
            {
                for (int i = 0; i < commandArray_2Agr.Length; i++)
                {
                    if (commandArray_2Agr[i] == "")
                    {
                        continue;
                    }
                    if (commandArray_2Agr[i] == "del")
                    {
                        continue;
                    }
                    ExecutionClass.deleteFile(commandArray_2Agr[i]);
                }
            }
            // work done all test case
            else if (commandArray_2Agr[0].ToLower() == "type")//display the file content
            {
                if ((commandArray_2Agr.Length == 2))
                {
                    if (commandArray_2Agr[1].ToLower() ==  string.Empty)
                    {
                        Console.WriteLine("ERROR:\nDisplays the contents of a text file. \r\ntype command syntax is \r\ntype [file]+ \r\nNOTE: it displays the filename before its content for every \r\nfile \r\n[file] can be file Name (or fullpath of file) of text file \r\n+ after [file] represent that you can pass more than file \r\nName (or fullpath of file).");
                        return;
                    }
                    else
                    {
                        ExecutionClass.TypeFiles(commandArray_2Agr[1]);
                    }
                }
                else
                {
                    for (int i = 1; i < commandArray_2Agr.Length; i++)
                    {                        
                        if (commandArray_2Agr[i] == string.Empty)
                        {
                            continue;
                        }
                        ExecutionClass.TypeFiles(commandArray_2Agr[i]);
                    }
                }               
            }
            // work with all test case 
            else 
            if (commandArray_2Agr[0].ToLower() == "copy")
            {                
                if (commandArray_2Agr[1].ToLower() == "")
                {
                    Console.WriteLine("ERROR:\nCopies one or more files to another location. \r\ncopy command syntax is \r\n copy [source] \r\nor \r\n copy [source] [destination] \r\n[source] can be file Name (or fullpath of file) or directory \r\nName (or fullpath of directory) \r\n[destination] can be file Name (or fullpath of file) or \r\ndirectory name or fullpath of a directory ");
                    return;
                }
                if (commandArray_2Agr.Length == 2)
                {
                    ExecutionClass.Copy_Source(commandArray_2Agr[1]);
                }
                else
                {
                    ExecutionClass.CopyMethod(commandArray_2Agr[1], commandArray_2Agr[2]);
                }
            }

            else if (
                commandArray_2Agr[0].ToLower() == "help" && commandArray_2Agr[1].ToLower() != "cd" && commandArray_2Agr[1].ToLower() != "cls" && commandArray_2Agr[1].ToLower() != "quit" && commandArray_2Agr[1].ToLower() != "copy" && commandArray_2Agr[1].ToLower() != "del"
                && commandArray_2Agr[1].ToLower() != "help" && commandArray_2Agr[1].ToLower() != "md" && commandArray_2Agr[1].ToLower() != "rd" && commandArray_2Agr[1].ToLower() != "rename" && commandArray_2Agr[1].ToLower() != "type"
                && commandArray_2Agr[1].ToLower() != "import" && commandArray_2Agr[1].ToLower() != "export")
                
            {
                Console.WriteLine($"Error: {commandArray_2Agr[1].ToLower()} => This command is not supported by the help utility.");
            }
            else
            {
                Console.WriteLine($"Error: \"{commandArray_2Agr[0].ToLower()}\" => This command is not supported by the help utility.");
                Console.WriteLine("Enter help to show our support command");
            }
        }
    }
}
