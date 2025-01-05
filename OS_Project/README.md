# OS-Project-Simple-Shell

# Simple Shell & File system (FAT) with Sub Directory

Course Title: Operating Systems Course Code: CS321
by Prof. Khaled F. Hussain
Faculty of Computers and Information, Assiut University

#Problem Statement​

Design and implement a basic shell interface that supports the execution of a series of commands. The shell should be robust (e.g., it should not crash under any circumstance)

In this project you will implement a Mini-FAT simple file system. Your file system will be able to allow users to browse the directory structure, create and delete new files and directories, etc. You are to write a simple file system which will use a regular file as the "virtual disk". The structure of the file system is based on FAT, simplified, and called mini-FAT.

The shell must support the following internal commands:

cd - Change the current default directory to . If the argument is not present, report the current directory. If the directory does not exist an appropriate error should be reported.​

cls - Clear the screen.​

dir - List the contents of directory .​

quit - Quit the shell.

copy - Copies one or more files to another location​

del - Deletes one or more files.​

help -Provides Help information for commands.​

md - Creates a directory.​

rd - Removes a directory.​

rename - Renames a file.​

type - Displays the contents of a text file.​

import – import text file(s) from your computer​

export – export text file(s) to your computer​

The format of the directory entry is given in the following table.​
MiniFat Directory Entry
| name | offset | size | description |
|:---------------:|:------:|:----:|:------------------------------------------------:|
| dir_name | 0 | 11 |8+3 filename. Empty entry if first byte is 0x0. |
| dir_attr | 11 | 1 |0x0 for a regular file, 0x10 for a directory file.|
| dir_empty | 12 | 12 |unused |
| dir_firstcluster| 24 | 4 |cluster number of first cluster in file |
| dir_filesize | 28 | 4 |size of file in bytes if a regular file. |

# This project was implemented by:

- Ahmed Khfaga .
