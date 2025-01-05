using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simple_Shell
{
    internal class Directory_Entry
    {
        public char[] Dir_Namee = new char[11];
        public byte dir_Attr;
        public byte[] Dir_Empty = new byte[12];
        public int dir_First_Cluster;
        public int dir_FileSize;
        // Directory 
        public Directory_Entry(char[] Dir_Name, byte dir_Attribute, int f_Cluster)
        {
            string DIR_NAME = new string(Dir_Name);
            assign_DirName(DIR_NAME);
            this.dir_Attr = dir_Attribute;
            this.dir_First_Cluster = f_Cluster;
            Array.Clear(Dir_Empty, 0, Dir_Empty.Length);

        }
        // File 
        public Directory_Entry(char[] Dir_Name, byte dir_Attribute, int f_Cluster, int f_size)
        {
            string DIR_NAME = new string(Dir_Name);
            AssignFileName(DIR_NAME);
            this.dir_Attr = dir_Attribute;
            this.dir_FileSize = f_size;
            this.dir_First_Cluster = f_Cluster;
            Array.Clear(Dir_Empty, 0, Dir_Empty.Length);

        }
        public void AssignFileName(string fullName)
        {
            if (fullName.Contains('.')) // It's a file
            {
                string[] parts = fullName.Split('.');
                string name = parts[0];
                string extension = parts[1];

                if (name.Length > 7)
                {
                    name = name.Substring(0, 7);
                }

                if (extension.Length > 3)
                {
                    extension = extension.Substring(0, 3);
                }

                string finalName = name + "." + extension;
                Array.Copy(finalName.PadRight(11, '\0').ToCharArray(), Dir_Namee, 11);


                dir_Attr = 0x0;  // File attribute
            }
            else  // It's a directory
            {
                if (fullName.Length > 11)
                    fullName = fullName.Substring(0, 11); // Truncate

                Array.Copy(fullName.PadRight(11, '\0').ToCharArray(), Dir_Namee, 11);


                dir_Attr = 0x10; // Directory attribute
            }
        }
        public void assign_DirName(string name)
        {
            string cleaned_Name = name;
            if (cleaned_Name.Length > 11)
            {
                cleaned_Name = cleaned_Name.Substring(0, 11);
            }
            if (cleaned_Name.Length < 11)
            {
                for (int i = cleaned_Name.Length; i < 11; i++)
                {
                    cleaned_Name += " ";
                }
            }
            cleaned_Name = cleaned_Name.Trim();
            Array.Clear(Dir_Namee, 0, Dir_Namee.Length); // Clear the array
            Array.Copy(cleaned_Name.ToCharArray(), Dir_Namee, cleaned_Name.Length);
        }
    }
}
