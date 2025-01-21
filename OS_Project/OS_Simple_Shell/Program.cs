using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Program
    {
        public static Directory currentDirectory;
        public static string path = "DD";
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to OS_Project_Virtual_DISK_shell ^_^ ");
            Console.WriteLine("developed by AHMED KHFAGA Under Supervision: DR – KHALED GAMAL ELTURKY \n");
            Console.WriteLine();
            Console.WriteLine();
            Mini_FAT.InitializeOrOpenFileSystem(path);
            currentDirectory = Mini_FAT.Root;
            path = new string(currentDirectory.Dir_Namee).Trim('\0');
            while (true)
            {
                Console.Write(path + ">>");
                string Command = Console.ReadLine();
                Command_Line command = new Command_Line(Command.Trim());
            }
        }
    }
}
