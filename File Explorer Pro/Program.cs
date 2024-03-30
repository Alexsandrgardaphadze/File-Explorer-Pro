using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management;
using System.Runtime;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Globalization;
using System.Linq;
using System.Threading;
using IWshRuntimeLibrary;
using File = IWshRuntimeLibrary.File;
using System.Security.Principal;
using ShellProgressBar;
using System.Text.RegularExpressions;


public enum ColorTheme
{
    Default,
    Dark,
    Light,
    Blue,
    Green,
    Red,
    Yellow,
    Cyan
}


class FileExplorerPro
{
    //TODO: "Undo" and "Redo" commands logic needs to be rewritten.
    //TODO: "Switch Language" command logic needs to be rewritten.
    private const string AppVersion = "4.1";
    private const string LastUpdateDate = "30.03.2024";
    static CultureInfo currentCulture = CultureInfo.CurrentCulture;
    static bool isFilterEnabled = false;
    static Stack<string> directoryStack = new Stack<string>();
    static List<string> lastOperationFiles = new List<string>();
    static ColorTheme currentTheme = ColorTheme.Default;
    static Stack<Action> undoStack = new Stack<Action>();
    static Stack<Action> redoStack = new Stack<Action>();



    static void DisplayGrittyTextWithLoading()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("   Welcome to the File Explorer Pro!");
        Thread.Sleep(100);
        Console.WriteLine();
        Console.ResetColor();
        // Dynamic percentage loading
        for (int i = 0; i <= 37; i++)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"\r   Loading: {((i + 1) * 100 / 38):D2}% [");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(new string('-', i + 1));
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("]");
            Thread.Sleep(100);
        }
        Console.WriteLine();
        Console.ResetColor();
        Thread.Sleep(1);
        Console.WriteLine();
    }


    static void DisplayShellProgressBarLoading()
    {
        const int totalSteps = 20; // Total number of steps in the progress bar
        using (var progressBar = new ProgressBar(totalSteps, " Loading... \n", ConsoleColor.Cyan))
        {
            for (int i = 0; i <= totalSteps; i++)
            {
                progressBar.Tick($" Progress: {i * 5}% ");
                Thread.Sleep(1);
            }
        }
        Console.WriteLine("");
        Console.WriteLine(" Loading Complete!");
    }



    static string Colorize(string number, ConsoleColor numberColor, string text, ConsoleColor textColor)
    {
        Console.ForegroundColor = numberColor;
        Console.Write(number);
        Console.ResetColor();
        Console.ForegroundColor = textColor;
        Console.WriteLine(text);
        Console.ResetColor();
        return $"{number} {text}";
    }

    static void Main()
    {
        try
        {
            DisplayGrittyTextWithLoading();
            string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string currentDirectory = userProfileDirectory ?? "C:\\DefaultDirectory"; // Provide a default directory if userProfileDirectory is null
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Do you want to enable the filter? ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("(");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("y");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("/");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("): ");
            Console.ResetColor();
            string filterChoice = Console.ReadLine()?.ToLower() ?? "";
            isFilterEnabled = filterChoice == "y";
            while (true)
            {
                Console.Clear();
                ApplyColorTheme();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("File Explorer Pro - Main Menu\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Current Directory: {currentDirectory ?? "Unknown"}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                if (currentDirectory != null)
                {
                    DisplayDirectoryContents(currentDirectory);
                }
                else
                {
                    Console.WriteLine("Current directory is unknown.");
                }
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Current Directory: {currentDirectory ?? "Unknown"}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n Options:");
                Console.ResetColor();
                Colorize("1.", ConsoleColor.Cyan, "  Navigate to a directory", ConsoleColor.Green);
                Colorize("2.", ConsoleColor.Cyan, "  View/Edit file content", ConsoleColor.Green);
                Colorize("3.", ConsoleColor.Cyan, "  Run a file", ConsoleColor.Green);
                Colorize("4.", ConsoleColor.Cyan, "  Create a new directory", ConsoleColor.Green);
                Colorize("5.", ConsoleColor.Cyan, "  Create a new file", ConsoleColor.Green);
                Colorize("6.", ConsoleColor.Cyan, "  Delete a file or directory", ConsoleColor.Green);
                Colorize("7.", ConsoleColor.Cyan, "  Search for a file or directory", ConsoleColor.Green);
                Colorize("8.", ConsoleColor.Cyan, "  Sort files and directories", ConsoleColor.Green);
                Colorize("9.", ConsoleColor.Cyan, "  Copy/Move files", ConsoleColor.Green);
                Colorize("10.", ConsoleColor.Cyan, " Rename files/directories", ConsoleColor.Green);
                Colorize("11.", ConsoleColor.Cyan, " File details", ConsoleColor.Green);
                Colorize("12.", ConsoleColor.Cyan, " Preview file content", ConsoleColor.Green);
                Colorize("13.", ConsoleColor.Cyan, " Toggle Filter On/Off", ConsoleColor.Green);
                Colorize("14.", ConsoleColor.Cyan, " Zip files", ConsoleColor.Green);
                Colorize("15.", ConsoleColor.Cyan, " Unzip file", ConsoleColor.Green);
                Colorize("16.", ConsoleColor.Cyan, " Associate file type with program", ConsoleColor.Green);
                Colorize("17.", ConsoleColor.Cyan, " Select multiple files/folders", ConsoleColor.Green);
                Colorize("18.", ConsoleColor.Cyan, " Show detailed help", ConsoleColor.Green);
                Colorize("19.", ConsoleColor.Cyan, " Encrypt/Decrypt files", ConsoleColor.Green);
                Colorize("20.", ConsoleColor.Cyan, " Go back to the parent directory", ConsoleColor.Green);
                Colorize("21.", ConsoleColor.Cyan, " Clone From Git Repository", ConsoleColor.Green);
                Colorize("  Exit", ConsoleColor.Red, "", ConsoleColor.White);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("    Enter your choice: ");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Blue;
                string choice = Console.ReadLine()?.ToLower() ?? "";
                Console.ResetColor();
                switch (choice)
                {
                    case "--v":
                    case "--version":
                        DisplayVersion();
                        break;
                    case "--f":
                    case "--features":
                        DisplayFeatures();
                        break;
                    case "--i":
                    case "--info":
                        DisplayUserInfo();
                        break;
                    case "--h":
                    case "--help":
                        DisplayHelp();
                        break;
                    case "--license":
                        DisplayLicense();
                        break;
                    case "--updateinfo":
                        DisplayUpdateInfo();
                        break;
                    case "--tm":
                    case "--taskmanager":
                        TaskManager();
                        break;
                    /* 
                     * TODO: I have no idea how to make this work. Please help
                     * case "--r":
                    case "--restart":
                        Console.WriteLine(" Restarting program... ");
                        Thread.Sleep(2000);
                        Process.Start(Environment.GetCommandLineArgs()[0]);
                        Environment.Exit(0);
                        break; */
                    case "--zenqterminalascii":
                        DisplayAsciiArt();
                        break;
                    case "1":
                    case "-navtodir":
                        NavigateDirectory(ref currentDirectory);
                        break;
                    case "2":
                    case "-viewedit":
                        ViewEditFileContent(currentDirectory);
                        break;
                    case "3":
                    case "-runfile":
                        RunFile(currentDirectory);
                        break;
                    case "4":
                    case "-createdir":
                        CreateNewDirectory(currentDirectory);
                        break;
                    case "5":
                    case "-createfile":
                        CreateNewTextFile(currentDirectory);
                        break;
                    case "6":
                    case "-delete":
                        DeleteFileOrDirectory(currentDirectory);
                        break;
                    case "7":
                    case "-search":
                        SearchFilesAndDirectories(currentDirectory);
                        break;
                    case "8":
                    case "-sort":
                        SortFilesAndDirectories(currentDirectory);
                        break;
                    case "9":
                    case "-copymove":
                        CopyOrMoveFiles(currentDirectory);
                        break;
                    case "10":
                    case "-rename":
                        RenameFileOrDirectory(currentDirectory);
                        break;
                    case "11":
                    case "-filedetails":
                        DisplayFileDetails(currentDirectory);
                        break;
                    /*
                     * Fix this thing please
                     * case "12":
                    case "-switchlang":
                        SetLanguage();
                        break;
                    */
                    case "12":
                    case "-preview":
                        PreviewFileContent(currentDirectory);
                        break;
                    case "13":
                    case "-togglefilter":
                        ToggleFilter();
                        break;
                    case "14":
                    case "-zip":
                        ZipFiles(currentDirectory);
                        break;
                    case "15":
                    case "-unzip":
                        UnzipFile(currentDirectory);
                        break;
                    case "16":
                    case "-associate":
                        AssociateFileTypeWithProgram();
                        break;
                    case "17":
                    case "-select":
                        SelectMultipleFilesOrFolders();
                        break;
                    /* 
                     * case "19":
                    case "-undo":
                        Undo();
                        break;
                    case "20":
                    case "-redo":
                        Redo();
                        break; 
                    */
                    case "18":
                    case "-detailedhelp":
                        ShowDetailedHelp();
                        break;
                    case "19":
                    case "-encryptdecrypt":
                        EncryptDecryptFiles(currentDirectory);
                        break;
                    case "20":
                    case "-goback":
                        GoBackToParentDirectory(ref currentDirectory);
                        break;
                    /* 
                     * Please fix this too
                     * case "24":
                    case "-changetheme":
                        ChangeColorTheme();
                        break;
                    */
                    case "21":
                    case "-gitclone":
                        GitClone(currentDirectory);
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.ReadLine();
        }
        Console.WriteLine();
    }


    static void DisplayUserInfo()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n    User Information:");

        // Get the current user's identity
        var userIdentity = WindowsIdentity.GetCurrent();

        if (userIdentity != null)
        {
            // Display user name
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  User Name: {userIdentity.Name}");
            Console.ResetColor();

            // Display domain
            string[] nameParts = userIdentity.Name.Split('\\');
            if (nameParts.Length == 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  Domain: {nameParts[0]}");
                Console.ResetColor();
            }

            // Display user profile path
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  User Profile Path: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
            Console.ResetColor();

            // Additional user information
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"  Authentication Type: {userIdentity.AuthenticationType}");
            Console.WriteLine($"  Is System User: {userIdentity.IsSystem}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  Unable to retrieve user information.");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine();
        Console.Write("    Do you want to see additional information? ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("(");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("y");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("/");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("n");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("): ");
        Console.ResetColor();
        string additionalInfoChoice = Console.ReadLine()?.ToLower() ?? "";
        if (additionalInfoChoice != null && additionalInfoChoice.ToLower() == "y")
        {
            SystemInfo();
        }
    } // 100% Complited

    static void SystemInfo()
    {
        Console.WriteLine("\n   System Information:");
        //System Information
        Console.WriteLine($" Operating System: {RuntimeInformation.OSDescription}");
        Console.WriteLine($" Processor Architecture: {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($" Processor Count: {Environment.ProcessorCount}");
        Console.WriteLine();
        // Processor Information
        Console.WriteLine("\n Processor Information:");
        ObjectQuery processorQuery = new ObjectQuery("SELECT * FROM Win32_Processor");
        ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher(processorQuery);
        ManagementObjectCollection processorCollection = processorSearcher.Get();
        foreach (ManagementObject processor in processorCollection)
        {
            Console.WriteLine($"Processor: {processor["Name"]}");
        }
        // Memory Information
        Console.WriteLine("\n   Memory Information:");
        try
        {
            ObjectQuery memoryQuery = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            ManagementObjectSearcher memorySearcher = new ManagementObjectSearcher(memoryQuery);
            ManagementObjectCollection memoryCollection = memorySearcher.Get();

            if (memoryCollection.Count > 0)
            {
                foreach (ManagementObject mo in memoryCollection)
                {
                    ulong totalPhysicalMemory = Convert.ToUInt64(mo["TotalPhysicalMemory"]);
                    Console.WriteLine($" Total Physical Memory: {totalPhysicalMemory / (1024 * 1024)} MB");

                    if (mo["FreePhysicalMemory"] != null)
                    {
                        Console.WriteLine($"Available Physical Memory: {Convert.ToUInt64(mo["FreePhysicalMemory"]) / (1024 * 1024)} MB");
                    }
                    else
                    {
                        Console.WriteLine("Available Physical Memory: Not available");
                    }

                    if (mo["TotalVirtualMemorySize"] != null && mo["FreeVirtualMemory"] != null)
                    {
                        Console.WriteLine($"Total Virtual Memory: {Convert.ToUInt64(mo["TotalVirtualMemorySize"]) / (1024 * 1024)} MB");
                        Console.WriteLine($"Available Virtual Memory: {Convert.ToUInt64(mo["FreeVirtualMemory"]) / (1024 * 1024)} MB");
                    }
                    else
                    {
                        Console.WriteLine("Virtual Memory Information: Not available");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No information available for memory.");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving memory information: {ex.Message}");
            Console.WriteLine();
        }
        Console.WriteLine();
        // Video Card Information
        Console.WriteLine("\n   Video Card Information:");
        ObjectQuery videoCardQuery = new ObjectQuery("SELECT * FROM Win32_VideoController");
        ManagementObjectSearcher videoCardSearcher = new ManagementObjectSearcher(videoCardQuery);
        ManagementObjectCollection videoCardCollection = videoCardSearcher.Get();
        if (videoCardCollection.Count == 0)
        {
            Console.WriteLine("No Video Card detected.");
        }
        else
        {
            foreach (ManagementObject videoCard in videoCardCollection)
            {
                Console.WriteLine($" Video Card: {videoCard["Caption"]}");
            }
        }
        Console.WriteLine();
        // Motherboard Information
        Console.WriteLine("\n   Motherboard Information:");
        ObjectQuery motherboardQuery = new ObjectQuery("SELECT * FROM Win32_BaseBoard");
        ManagementObjectSearcher motherboardSearcher = new ManagementObjectSearcher(motherboardQuery);
        ManagementObjectCollection motherboardCollection = motherboardSearcher.Get();
        foreach (ManagementObject motherboard in motherboardCollection)
        {
            Console.WriteLine($" Motherboard: {motherboard["Product"]} - {motherboard["Manufacturer"]}");
        }
        Console.WriteLine();
        // Hard Drive Information
        Console.WriteLine("\n   Hard Drive Information:");
        ObjectQuery hardDriveQuery = new ObjectQuery("SELECT * FROM Win32_DiskDrive");
        ManagementObjectSearcher hardDriveSearcher = new ManagementObjectSearcher(hardDriveQuery);
        ManagementObjectCollection hardDriveCollection = hardDriveSearcher.Get();
        foreach (ManagementObject hardDrive in hardDriveCollection)
        {
            Console.WriteLine($" Hard Drive: {hardDrive["Caption"]} - {hardDrive["MediaType"]}");
        }
        Console.WriteLine();
        Console.ReadLine();
    } // 100% Complited

    static void DisplayVersion()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        string asciiArt =
            @"                                                                                                                                           
    ______           _____   _____                   _             _ 
   |___  /          |  _  | |_   _|                 (_)           | |
      / /  ___ _ __ | | | |   | | ___ _ __ _ __ ___  _ _ __   __ _| |
     / /  / _ \ '_ \| | | |   | |/ _ \ '__| '_ ` _ \| | '_ \ / _` | |
   ./ /__|  __/ | | \ \/' /   | |  __/ |  | | | | | | | | | | (_| | |
   \_____/\___|_| |_|\_/\_\   \_/\___|_|  |_| |_| |_|_|_| |_|\__,_|_|
                                                          ";

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(asciiArt);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine($"   File Explorer Pro - Version {AppVersion} ");
        Console.WriteLine($"   Release Date: March 7, 2024");
        Console.WriteLine($"   Last UpDate: {LastUpdateDate} ");
        Console.WriteLine($"   Author: ZenQuant (Alex Gardaphadze)");
        Console.WriteLine($"   Description: File Explorer Pro is a command-line file explorer.");
        Console.WriteLine($"   License: MIT License");
        Console.ReadLine();
        Console.ResetColor();
    }

    static void DisplayFeatures()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("");
        Console.WriteLine("   File Explorer Pro - Features");
        Console.WriteLine("");
        Console.ResetColor();

        DisplayFeature("- Navigate, create, delete, and manage files and directories",
            "Effortlessly explore your file system and perform various file operations.");

        DisplayFeature("- View and edit file content",
            "Inspect and modify the content of text files directly from the command line.");

        DisplayFeature("- Run executable files",
            "Execute applications and scripts directly from File Explorer Pro.");

        DisplayFeature("- Search, sort, copy, move, and perform various file operations",
            "Efficiently manage your files with powerful search, sorting, copying, and moving capabilities.");

        DisplayFeature("- Zip and unzip files",
            "Compress files into ZIP archives or extract files from existing ZIP files.");

        DisplayFeature("- Associate file types with programs",
            "Define default programs to open specific file types for seamless file handling.");

        DisplayFeature("- Undo and redo file operations",
            "Revert or repeat your file operations to maintain a clean and organized file system.");

        DisplayFeature("- Preview file content and more",
            "Get a quick preview of file content and access additional features for enhanced file exploration.");

        Console.ResetColor();
        Console.ReadLine();
    }

    static void DisplayFeature(string feature, string description)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(feature);
        Console.ResetColor();
        Console.WriteLine($"  {description}\n");
        Console.ReadLine();
    }

    static void DisplayHelp()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        string helpText = @"
                    File Explorer Pro - Help

              Usage:
            --version               Display version information
            --features              Display features of the application
            --info                  Display user information
            --license               Display the project's license information
            --help                  Display this help message
            --taskmanager           Display the build in Task Manager


              Commands:
            1 or -navtodir          Navigate to a directory
            2 or -viewedit          View/Edit file content
            3 or -runfile           Run a file
            4 or -createdir         Create a new directory
            5 or -createtextfile    Create a new text file
            6 or -delete            Delete a file or directory
            7 or -search            Search for a file or directory
            8 or -sort              Sort files and directories
            9 or -copymove          Copy/Move files
            10 or -rename           Rename files/directories
            11 or -filedetails      File details
            12 or -switchlang       Switch Language
            13 or -preview          Preview file content
            14 or -togglefilter     Toggle Filter On/Off
            15 or -zip              Zip files
            16 or -unzip            Unzip file
            17 or -associate        Associate file type with program
            18 or -select           Select multiple files/folders
            19 or -undo             Undo last operation
            20 or -redo             Redo last undone operation
            21 or -showhelp         Show detailed help
            22 or -encryptdecrypt   Encrypt/Decrypt files
            23 or -goback           Go back to the parent directory
            24 or -changetheme      Change Color Theme
            25 or -gitclone         Clone From Git Repository

            ";
        Console.WriteLine(helpText);
        Console.ResetColor();
        Console.ReadLine();
    }

    static void DisplayLicense()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n     File Explorer Pro - License\n");
        Console.ResetColor();
        string licenseText = @"

            MIT License

        Copyright (c) 07.03.2024 ZenQuant (Alex Gardaphadze)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the ""Software""), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        1. Any modification to the software's script must be notified to the creator, Alex Gardaphadze, and should include a clear indication of the changes made.

        2. The user who modifies the software's script is responsible for ensuring that the modified version remains functional and does not introduce non-functional behavior.

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.

            ";
        Console.WriteLine(licenseText);
        Console.ReadLine();
    }

    static void DisplayAsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;

        string asciiArt = @"
                                                                    
                   aaaaaaaaaaaaaaaaaaaaaaaaaahhhhhhkhkkkk           
               haaaaaaaaaaaaaaaaaaaaaaaaaahhhhhhhkkkkkkbbbbbo       
             aaaaaaaaaaaaaaaaaaaaaaaaaaahahhhhhkkkkkkbbkbbbbddb     
            aaaaaaaaaoaaaoaaaoaaaoaaaahahhhhhkkkkkkbkbbbbddddddd    
           aaaaaoaaoaaaaaaaaaaaaaaahahhhhhkkkkkkkbbbbbbddbddddppp   
          aaaaoaaaaaaaaaaaaaaaaaahhhhhhkkkkkkkbbbbbbdbdddddpdppppp  
          aaaaaaaaaaaaaaaaoaaaahahhhhkhkkkkkbkbbbbbddddddppppppqpq  
          aaaaaaaaaaaaaoaaaahahhhhhkhkkkkkbbbbbbdbddddpdpppppqpqqq  
          aaaoaaaaaaaoaaaahhhhhhkhkkkkkbbbbbbdbdddddpdppppqpqqqqqw  
          aaaaaaaaoaa    hhhhhkhkkkkkbbbbbbbdddddpdpppppqpqqqqwwww  
          aaaaaaoaaaah     hhkkkkkbbbbbbbbdddddpdpppppqqqqqqwqwwww  
          aaaaoaaaahhhhh     hkkbbbkbbbddddddpdppppqpqqqqqwqwwwwwm  
          aaoaaahhhhhhhhkk      bbbbdddddddpdppppqpqqqqqwwwwwwmwmm  
          aaaahhahhhhhkkkkkkk     bddddddpppppqpqqqqqwqwwwwwmmmmmm  
          aahhhhhhhkkkkkkbbbb     ddddpdppppqpqqqqqwwwwwwwmmmmmmZZ  
          hahhhhkkkkkkbkbb      dddpdpppppqpqqqqwqwwwwwmwmmmmmZmZZ  
          hhhhkhkkkkbkbb     kdddpdppppqpqqqqqwqwwwwwmwmmmmmZZZZZZ  
          hkkkkkkkbkbb     ddddpppppp                 mmZmZZZZZZOO  
          kkkkkbkbbbb    dddpdpppppq                   ZZZZZZZOOOO  
          kkkkbbbbbbdbddddpdpppppqpqqqqwqwwwwmwmmmmmZmZZZZZOOOOOOO  
          kkbbbbbbddddddpdppppqpqqqqqwqwwwwmwmmmmmZZZZZZZOOOOOO000  
          bbbbbdbddddpppppppqpqqqqwqwwwwwmwmmmmmZZZZZZOZOOOOO00O00  
          bbbdbddddpdpppppqpqqqqwqwwwwmwmmmmmmZZZZZZOZOOOOO00O00Q0  
           bdddddpppppppqqqqqqwwwwwwmwmmmmmZZZZZZZOZOOO0O00000Q0Q   
            ddpdppppppqqqqqwqwwwwwmwmmmmZmZZZZZZOOOOO0O000000QQQ    
             bpppppqqqqqqqwwwwwwmwmmmmZmZZZZZOOOOOO0O00000QQQQQ     
               aqpqqqqqwqwwwwwmmmmmZmZZZZZZOOZOOO00O0000QQ0QC       
                   qqwwwwwwwmmmmmZmZZZZZOZOOOO0O0000Q0QQQ           
";
        Console.WriteLine(asciiArt);
        Console.ResetColor();
        Console.ReadLine();
    }

    static void DisplayUpdateInfo()
    {
        // Add more update information as needed.
        Colorize("\n   Update Information:  \n", ConsoleColor.DarkRed, "", ConsoleColor.Magenta);

        Colorize("\n   --- Version 4.1 ---  ", ConsoleColor.DarkYellow, "30.03.2024 \n", ConsoleColor.DarkYellow);
        Console.WriteLine(" - Command 'Undo' is no longer be available. This command logic will be rewritten in future. ");
        Console.WriteLine(" - Command 'Redo' is no longer be available. This command logic will be rewritten in future. ");
        Console.WriteLine(" - Command 'Switch Language' is no longer be available. This command logic will be rewritten in future. ");
        Console.WriteLine(" - Some parts for script were changed. (Nothing changed) ");
        Console.WriteLine(" - License file was changed from 'README.md' to 'LICENSE.md' file. ");

        Colorize("\n   --- Version 4.0 ---  ", ConsoleColor.DarkYellow, "24.03.2024 \n", ConsoleColor.DarkYellow);
        Console.WriteLine(" - Added support for the '--taskmanager' command. ");
        Console.WriteLine(" - Updated information in '--features' command. ");
        Console.WriteLine(" - Package 'System.IO.Abstractions' was installed. ");
        Console.WriteLine(" - Added new '-gitclone' command. ");
        Console.WriteLine(" - Some non-critical warnings were fixed. ");

        Colorize("\n   --- Version 3.0 ---  ", ConsoleColor.DarkYellow, "11.03.2024 \n", ConsoleColor.DarkYellow);
        Console.WriteLine(" - Added support for the '--updateinfo' command. ");
        Console.WriteLine(" - Added '-command' type commands. ");
        Console.WriteLine(" - The '-runfile' command's logic was rewritten and optimized. ");
        Console.WriteLine(" - NuGet packages were installed for the future improvisations. ");
        Console.WriteLine(" - Package 'ShellProgressBar' was installed. "); 
        Console.WriteLine(" - Package 'Colorful.Console' was installed. ");
        Console.WriteLine(" - Package 'System.Diagnostics.PerformanceCounter' was installed. ");
        Colorize("\n   --- Version 2.0 ---  ", ConsoleColor.DarkYellow, "09.03.2024 \n", ConsoleColor.DarkYellow);

        Console.WriteLine(" - Fixed several bugs. ");
        Console.WriteLine(" - Updated support for the '--info' command. ");
        Console.WriteLine(" - Added support for the '--help' command. ");
        Console.WriteLine(" - Added support for the '--license' command. ");
        Console.WriteLine(" - Some console colors were changed. ");
        Console.WriteLine(" - Added the 'README.md' file for license. ");
        Colorize("\n   --- Version 1.0 ---  ", ConsoleColor.DarkYellow, "07.03.2024 \n", ConsoleColor.DarkYellow);

        Console.WriteLine(" - Fixed several bugs. ");
        Console.WriteLine(" - Added support for the '--<command>' type commands. ");
        Console.WriteLine(" - Added support for the '--features' command. ");
        Console.WriteLine(" - Added '--version' command. ");

        Console.ReadLine();
    }
  
    static void ApplyColorTheme()    //TODO: Needs to be fixed in future.
    {
        switch (currentTheme)
        {
            case ColorTheme.Default:
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case ColorTheme.Dark:
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case ColorTheme.Light:
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                break;
            case ColorTheme.Blue:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case ColorTheme.Green:
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case ColorTheme.Red:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case ColorTheme.Yellow:
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case ColorTheme.Cyan:
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            default:
                // Handle the case when an unsupported color is chosen
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
        }
    }


    static void ChangeColorTheme()
    {
        // Method to allow users to change the color theme
        Console.Clear();
        Console.WriteLine("Available Color Themes:");

        int optionNumber = 1;
        foreach (var color in Enum.GetValues(typeof(ColorTheme)))
        {
            Console.WriteLine($"{optionNumber}. {color}");
            optionNumber++;
        }

        Console.Write("Enter the number for the desired color theme: ");

        if (int.TryParse(Console.ReadLine(), out int themeChoice) && themeChoice >= 1 && themeChoice <= Enum.GetValues(typeof(ColorTheme)).Length)
        {
            currentTheme = (ColorTheme)(themeChoice - 1);
            ApplyColorTheme();
            Console.WriteLine($"Color theme changed to {currentTheme}.");
        }
        else
        {
            Console.WriteLine("Invalid choice. Color theme not changed.");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void DisplayDirectoryContents(string path)
    {
        string[] directories = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        Console.WriteLine("\nDirectories:");
        foreach (string directory in directories)
        {
            Console.WriteLine($"[D] {Path.GetFileName(directory)}");
        }

        Console.WriteLine("\nFiles:");

        if (isFilterEnabled)
        {
            var fileExtensions = GetUniqueFileExtensions(files); // Get unique file extensions in the current directory
            Console.WriteLine("Filter Options:"); // Display file type filter options
            Console.WriteLine("0. All Files");
            for (int i = 0; i < fileExtensions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {fileExtensions[i].ToUpper()} Files");
            }

            Console.Write("Enter filter choice (0 for all files, 'off' to disable filter): ");
            string filterChoice = Console.ReadLine();

            if (filterChoice != null && filterChoice.ToLower() == "off")
            {
                isFilterEnabled = false;
            }
            else
            {
                isFilterEnabled = true;

                IEnumerable<string> filteredFiles = null;

                if (filterChoice == "0")
                {
                    filteredFiles = files;
                }
                else if (int.TryParse(filterChoice, out int filterIndex) && filterIndex >= 1 && filterIndex <= fileExtensions.Count)
                {
                    string selectedExtension = fileExtensions[filterIndex - 1];
                    filteredFiles = files?.Where(file => file.EndsWith(selectedExtension, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    Console.WriteLine("Invalid filter choice. Displaying all files.");
                    filteredFiles = files;
                }

                foreach (string file in filteredFiles ?? Array.Empty<string>()) // Null check added here
                {
                    Console.WriteLine($"[F] {Path.GetFileName(file)}");
                }
            }
        }
        else
        {
            // Display all files without filtering
            foreach (string file in files ?? Array.Empty<string>()) // Null check added here
            {
                Console.WriteLine($"[F] {Path.GetFileName(file)}");
            }
        }
    } // 100% Fixed

    static List<string> GetUniqueFileExtensions(string[] files)
    {
        // Extract file extensions from file names and return unique extensions
        return files
            .Select(file => Path.GetExtension(file))
            .Where(extension => !string.IsNullOrEmpty(extension))
            .Select(extension => extension.ToLower())
            .Distinct()
            .ToList();
    }


    static void NavigateDirectory(ref string currentDirectory)
    {
        Console.Write("Enter the name of the directory to navigate to: ");
        string directoryName = Console.ReadLine();

        string newPath = Path.Combine(currentDirectory, directoryName);

        if (Directory.Exists(newPath))
        {
            currentDirectory = newPath;
        }
        else
        {
            Console.WriteLine($"Directory '{directoryName}' not found.");
            Console.ReadLine(); // Pause to let the user read the message
        }
    }


    static void RunFile(string currentDirectory)
    {
        Console.Write("Enter the name of the file to run (including extension): ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            try
            {
                string fileExtension = Path.GetExtension(filePath)?.ToLower();

                switch (fileExtension)
                {
                    case ".py":
                        // Use the Python launcher to run Python scripts
                        Process.Start("py", $"\"{filePath}\"");
                        break;
                    case ".rb":
                        // Assuming you have Ruby installed, run Ruby scripts
                        Process.Start("ruby", $"\"{filePath}\"");
                        break;
                    case ".lua":
                        // Assuming you have Lua installed, run Lua scripts
                        Process.Start("lua", $"\"{filePath}\"");
                        break;
                    case ".sh":
                        // Assuming you have a Bash interpreter, run Bash scripts
                        Process.Start("bash", $"\"{filePath}\"");
                        break;
                    case ".cpp":
                    case ".c":
                        // Replace this comment with your C compilation and execution logic
                        Process.Start("notepad", filePath); // Placeholder for C
                        break;
                    case ".java":
                        // Replace this comment with your Java compilation and execution logic
                        Process.Start("notepad", filePath); // Placeholder for Java
                        break;
                    case ".js":
                        // Assuming you have a Node.js installed, run JavaScript files
                        Process.Start("node", $"\"{filePath}\"");
                        break;
                    case ".html":
                        // Open HTML files in the default web browser
                        Process.Start(filePath);
                        break;
                    case ".css":
                        // Open CSS files in the default text editor
                        Process.Start("notepad", filePath);
                        break;
                    case ".ps1":
                        // Assuming you have PowerShell installed, run PowerShell scripts
                        Process.Start("powershell", $"-File \"{filePath}\"");
                        break;
                    case ".bat":
                        // Run Batch files
                        Process.Start(filePath);
                        break;
                    case ".swift":
                        // Replace this comment with your Swift compilation and execution logic
                        Process.Start("notepad", filePath); // Placeholder for Swift
                        break;
                    case ".php":
                        // Assuming you have PHP installed, run PHP files
                        Process.Start("php", $"\"{filePath}\"");
                        break;
                    // Add more cases for other file types as needed

                    default:
                        // For other file types, open in default notepad
                        Process.Start("notepad", filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to run the file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
        }

        Console.ReadLine();
    }

    static string ResolveShortcut(string shortcutPath)
    {
        try
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            // Returns the target path of the shortcut
            return shortcut.TargetPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    static void ViewEditFileContent(string currentDirectory)
    {
        Console.Write("Enter the name of the file to view/edit: ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);
        if (System.IO.File.Exists(filePath))
        {
            string fileContent = System.IO.File.ReadAllText(filePath);
            Console.WriteLine("\nFile Content:");
            Console.WriteLine(fileContent);
            Console.WriteLine("\nDo you want to edit the content? (y/n): ");
            string editChoice = Console.ReadLine();
            if (editChoice != null && editChoice.ToLower() == "y")
            {
                Console.WriteLine("Enter the new content (press Ctrl + Z then Enter to finish):");
                string newContent = Console.In.ReadToEnd(); // Read until Ctrl + Z is pressed
                System.IO.File.WriteAllText(filePath, newContent);
                Console.WriteLine("File content updated successfully.");
            }
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
        }
        Console.ReadLine();
    }

    static void CreateNewDirectory(string currentDirectory)
    {
        Console.Write("Enter the name of the new directory: ");
        string newDirectoryName = Console.ReadLine();
        string newDirectoryPath = Path.Combine(currentDirectory, newDirectoryName);

        Directory.CreateDirectory(newDirectoryPath);
        Console.WriteLine($"Directory '{newDirectoryName}' created successfully.");
        Console.ReadLine(); // Pause to let the user read the message
    }

    static void CreateNewTextFile(string currentDirectory)
    {
        Console.Write("Enter the name of the new file: ");
        string newFileName = Console.ReadLine();
        string newFilePath = Path.Combine(currentDirectory, newFileName);
        Console.WriteLine("Enter the content for the text file (press Ctrl + Z then Enter to finish):");
        string fileContent = Console.In.ReadToEnd();
        System.IO.File.WriteAllText(newFilePath, fileContent);
        Console.WriteLine($"Text file '{newFileName}' created successfully with the specified content.");
        Console.ReadLine();
    }

    static void DeleteFileOrDirectory(string currentDirectory)
    {   
        try
        {
        Console.Write("Enter the name of the file/directory to delete: ");
        string nameToDelete = Console.ReadLine();
        string pathToDelete = Path.Combine(currentDirectory, nameToDelete);

        if (System.IO.File.Exists(pathToDelete) || Directory.Exists(pathToDelete))
        {
            Console.Write($"Are you sure you want to delete '{nameToDelete}'? (y/n): ");
            string confirmation = Console.ReadLine();

            if (confirmation != null && confirmation.ToLower() == "y")
            {
                if (System.IO.File.Exists(pathToDelete))
                {
                    System.IO.File.Delete(pathToDelete);
                    Console.WriteLine($"File '{nameToDelete}' deleted successfully.");
                }
                else
                {
                    Directory.Delete(pathToDelete, true); // Recursive delete for directories
                    Console.WriteLine($"Directory '{nameToDelete}' deleted successfully.");
                }
            }
            else
            {
                Console.WriteLine("Deletion canceled.");
            }
        }
        else
        {
            Console.WriteLine($"File/directory '{nameToDelete}' not found.");
        }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to perform the operation: {ex.Message}");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void SearchFilesAndDirectories(string currentDirectory)
    {
        Console.Write("Enter the name to search for: ");
        string searchName = Console.ReadLine();

        IEnumerable<string> results = Directory.EnumerateFileSystemEntries(currentDirectory, searchName, SearchOption.AllDirectories);

        if (results.Any())
        {
            Console.WriteLine("\nSearch Results:");
            foreach (string result in results)
            {
                Console.WriteLine(result);
            }
        }
        else
        {
            Console.WriteLine($"No matches found for '{searchName}'.");
        }

        Console.ReadLine(); // Pause to let the user read the results
    }

    static void SortFilesAndDirectories(string currentDirectory)
    {
        Console.WriteLine("\nSort Options:");
        Console.WriteLine("1. Alphabetically");
        Console.WriteLine("2. By Size");
        Console.WriteLine("3. By Date");
        Console.Write("Enter your choice: ");
        string sortChoice = Console.ReadLine();

        IEnumerable<string> items;

        switch (sortChoice)
        {
            case "1":
                items = Directory.EnumerateFileSystemEntries(currentDirectory).OrderBy(item => item);
                break;

            case "2":
                items = Directory.EnumerateFiles(currentDirectory).OrderBy(file => new FileInfo(file).Length);
                break;

            case "3":
                items = Directory.EnumerateFileSystemEntries(currentDirectory).OrderBy(item => new FileInfo(item).LastWriteTime);
                break;

            default:
                Console.WriteLine("Invalid sort option.");
                return;
        }

        Console.WriteLine("\nSorted Items:");
        foreach (string item in items)
        {
            Console.WriteLine(item);
        }

        Console.ReadLine(); // Pause to let the user read the sorted items
    }
    static void CopyOrMoveFiles(string currentDirectory)
    {
        Console.Write("Enter the name of the file to copy/move: ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            Console.Write("Enter the destination directory: ");
            string destinationDirectory = Console.ReadLine();
            string destinationPath = Path.Combine(destinationDirectory, fileName);

            Console.Write("Do you want to copy or move? (copy/move): ");
            string operation = Console.ReadLine().ToLower();

            try
            {
                if (operation == "copy")
                {
                    System.IO.File.Copy(filePath, destinationPath);
                    Console.WriteLine($"File '{fileName}' copied successfully to {destinationDirectory}.");
                }
                else if (operation == "move")
                {
                    System.IO.File.Move(filePath, destinationPath);
                    Console.WriteLine($"File '{fileName}' moved successfully to {destinationDirectory}.");
                }
                else
                {
                    Console.WriteLine("Invalid operation. Please enter 'copy' or 'move'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to perform the operation: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void RenameFileOrDirectory(string currentDirectory)
    {
        Console.Write("Enter the name of the file/directory to rename: ");
        string oldName = Console.ReadLine();
        string oldPath = Path.Combine(currentDirectory, oldName);

        if (System.IO.File.Exists(oldPath) || Directory.Exists(oldPath))
        {
            Console.Write("Enter the new name: ");
            string newName = Console.ReadLine();
            string newPath = Path.Combine(currentDirectory, newName);

            try
            {
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Move(oldPath, newPath);
                    Console.WriteLine($"File '{oldName}' renamed to '{newName}' successfully.");
                }
                else
                {
                    Directory.Move(oldPath, newPath);
                    Console.WriteLine($"Directory '{oldName}' renamed to '{newName}' successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to perform the operation: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"File/directory '{oldName}' not found.");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void DisplayFileDetails(string currentDirectory)
    {
        Console.Write("Enter the name of the file to display details: ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            FileInfo fileInfo = new FileInfo(filePath);

            Console.WriteLine($"File Details for '{fileName}':");
            Console.WriteLine($"Size: {fileInfo.Length} bytes");
            Console.WriteLine($"Last Modified: {fileInfo.LastWriteTime}");
            Console.WriteLine($"Read Only: {fileInfo.IsReadOnly}");
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
        }

        Console.ReadLine(); // Pause to let the user read the details
    }

    static void NavigateBack(Stack<string> directoryStack, ref string currentDirectory)
    {
        if (directoryStack.Count > 1)
        {
            directoryStack.Pop(); // Remove the current directory
            currentDirectory = directoryStack.Peek(); // Set current directory to the previous one
        }
        else
        {
            Console.WriteLine("Cannot navigate back. Already at the root directory.");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void NavigateToRoot(Stack<string> directoryStack, ref string currentDirectory)
    {
        directoryStack.Clear(); // Clear the stack
        currentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"); // Set current directory to the root
        directoryStack.Push(currentDirectory); // Push root directory to the stack

        Console.WriteLine("Navigated to the root directory.");

        Console.ReadLine(); // Pause to let the user read the message
    }
    static void PreviewFileContent(string currentDirectory)
    {
        Console.Write("Enter the name of the file to preview: ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            try
            {
                Console.Clear();
                Console.WriteLine($"File Preview - {fileName}\n");

                // Read and display a portion of the file content
                const int previewLines = 10;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    for (int i = 0; i < previewLines; i++)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;

                        Console.WriteLine(line);
                    }
                }

                Console.WriteLine("\n--- Press Enter to return to the main menu ---");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to preview the file: {ex.Message}");
                Console.ReadLine(); // Pause to let the user read the message
            }
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
            Console.ReadLine(); // Pause to let the user read the message
        }
    }

    /* static void SetLanguage()   TODO: This is not working properly, please fix it in future (30.03.2024).
    {
        Console.WriteLine("Select Language:");
        Console.WriteLine("1. English");
        Console.WriteLine("2. Español");
        Console.WriteLine("3. Français");
        Console.WriteLine("4. Deutsch");
        Console.WriteLine("5. Italiano");
        Console.WriteLine("6. Português");
        Console.WriteLine("7. Русский");
        Console.WriteLine("8. 中文 (简体)");
        Console.WriteLine("9. 日本語");
        Console.WriteLine("10. 한국어");
        Console.WriteLine("11. العربية");
        Console.WriteLine("12. हिन्दी");
        // Add more languages as needed

        Console.Write("Enter your choice: ");
        string languageChoice = Console.ReadLine();

        switch (languageChoice)
        {
            case "1":
                currentCulture = CultureInfo.GetCultureInfo("en-US");
                break;
            case "2":
                currentCulture = CultureInfo.GetCultureInfo("es-ES");
                break;
            case "3":
                currentCulture = CultureInfo.GetCultureInfo("fr-FR");
                break;
            case "4":
                currentCulture = CultureInfo.GetCultureInfo("de-DE");
                break;
            case "5":
                currentCulture = CultureInfo.GetCultureInfo("it-IT");
                break;
            case "6":
                currentCulture = CultureInfo.GetCultureInfo("pt-PT");
                break;
            case "7":
                currentCulture = CultureInfo.GetCultureInfo("ru-RU");
                break;
            case "8":
                currentCulture = CultureInfo.GetCultureInfo("zh-CN");
                break;
            case "9":
                currentCulture = CultureInfo.GetCultureInfo("ja-JP");
                break;
            case "10":
                currentCulture = CultureInfo.GetCultureInfo("ko-KR");
                break;
            case "11":
                currentCulture = CultureInfo.GetCultureInfo("ar-SA");
                break;
            case "12":
                currentCulture = CultureInfo.GetCultureInfo("hi-IN");
                break;
            // Add more cases for additional languages
            default:
                Console.WriteLine("Invalid choice. Using default language (English).");
                return;
        }

        Thread.CurrentThread.CurrentCulture = currentCulture;
        Thread.CurrentThread.CurrentUICulture = currentCulture;

        Console.WriteLine($"Language switched to {currentCulture.DisplayName}.");
        Console.WriteLine("Press Enter to refresh the display.");
        Console.ReadLine();
    }
    */

    static void ToggleFilter()
    {
        isFilterEnabled = !isFilterEnabled;
        Console.WriteLine($"Filter is now {(isFilterEnabled ? "enabled" : "disabled")}");
        Console.ReadLine(); // Pause to let the user read the message
    }

    static void ZipFiles(string currentDirectory)
    {
        Console.Write("Enter the name of the zip file to create: ");
        string zipFileName = Console.ReadLine();
        string zipFilePath = Path.Combine(currentDirectory, zipFileName);

        Console.Write("Enter the file names (comma-separated ',') to include in the zip: ");
        string fileNamesInput = Console.ReadLine();
        string[] fileNames = fileNamesInput.Split(',');

        try
        {
            using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (string fileName in fileNames)
                {
                    string filePath = Path.Combine(currentDirectory, fileName.Trim());
                    if (System.IO.File.Exists(filePath))
                    {
                        archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                    }
                    else
                    {
                        Console.WriteLine($"File '{fileName}' not found.");
                    }
                }
            }

            Console.WriteLine($"Files zipped successfully to '{zipFileName}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to zip files: {ex.Message}");
        }
        Console.WriteLine($"Files zipped successfully to '{zipFileName}'.");
        Console.WriteLine("Press Enter to refresh the display.");
        Console.ReadLine(); // Pause to let the user read the message
    }

    static void UnzipFile(string currentDirectory)
    {
        Console.Write("Enter the name of the zip file to unzip: ");
        string zipFileName = Console.ReadLine();
        string zipFilePath = Path.Combine(currentDirectory, zipFileName);

        string destinationFolder = Path.Combine(currentDirectory, Path.GetFileNameWithoutExtension(zipFileName));

        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryPath = Path.Combine(destinationFolder, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    entry.ExtractToFile(entryPath, true);
                }
            }

            Console.WriteLine($"Files in '{zipFileName}' unzipped successfully to '{destinationFolder}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to unzip files: {ex.Message}");
        }

        Console.WriteLine("Press Enter to refresh the display.");
        Console.ReadLine();
    }

    static void AssociateFileTypeWithProgram()
    {
        Console.Write("Enter the file extension to associate (e.g., 'txt'): ");
        string fileExtension = Console.ReadLine().ToLower();

        Console.Write("Enter the path of the program to open this file type: ");
        string programPath = Console.ReadLine();

        // Save association information to a configuration file or registry
        // For simplicity, let's assume a dictionary is used
        // In a real application, a configuration file or registry should be used
        Dictionary<string, string> fileAssociations = new Dictionary<string, string>();
        fileAssociations[fileExtension] = programPath;

        Console.WriteLine($"File type '{fileExtension}' associated with program '{programPath}'.");
        Console.ReadLine(); // Pause to let the user read the message
    }

    static void SelectMultipleFilesOrFolders()
    {
        Console.Write("Enter the names of files/folders to select (comma-separated): ");
        string selectionInput = Console.ReadLine();
        string[] selections = selectionInput.Split(',');

        lastOperationFiles.Clear();
        lastOperationFiles.AddRange(selections);

        Console.WriteLine("Files/Folders selected for the next operation:");
        foreach (string selection in selections)
        {
            Console.WriteLine(selection.Trim());
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void ShowDetailedHelp()  // Needs to be done properly. 
    {
        Console.WriteLine("\nFile Explorer Pro - Detailed Help\n");
        Console.WriteLine("1. Navigation:");
        Console.WriteLine("   - Use option 1 to navigate to a directory by entering its name.");
        Console.WriteLine("   - Use option 11 to switch the language.");
        Console.WriteLine("   - Use option 14 to toggle the filter on/off.");
        Console.WriteLine("   - Use option 16 to unzip a file.");
        Console.WriteLine("   - Use option 17 to associate a file type with a program.");
        Console.WriteLine("   - Use option 18 to select multiple files or folders for operations.");

        // Include detailed help for other options...

        Console.ReadLine(); // Pause to let the user read the detailed help
    }  

    static void EncryptDecryptFiles(string currentDirectory)
    {
        Console.Write("Enter the name of the file to encrypt/decrypt: ");
        string fileName = Console.ReadLine();
        string filePath = Path.Combine(currentDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            Console.Write("Do you want to encrypt or decrypt? (encrypt/decrypt): ");
            string operation = Console.ReadLine().ToLower();

            try
            {
                if (operation == "encrypt")
                {
                    EncryptFile(filePath);
                    Console.WriteLine($"File '{fileName}' encrypted successfully.");
                }
                else if (operation == "decrypt")
                {
                    DecryptFile(filePath);
                    Console.WriteLine($"File '{fileName}' decrypted successfully.");
                }
                else
                {
                    Console.WriteLine("Invalid operation. Please enter 'encrypt' or 'decrypt'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to perform the operation: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"File '{fileName}' not found.");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void EncryptFile(string filePath)
    {
        try
        {
            // Read the file bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Encrypt the file bytes using a simple XOR operation
            byte encryptionKey = 0xAA; // You can use any value for the encryption key
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] ^= encryptionKey;
            }

            // Write the encrypted bytes back to the file
            System.IO.File.WriteAllBytes(filePath, fileBytes);

            Console.WriteLine($"File '{Path.GetFileName(filePath)}' encrypted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to encrypt the file: {ex.Message}");
        }
    }

    static void DecryptFile(string filePath)
    {
        try
        {
            // Read the file bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Decrypt the file bytes using the same XOR operation
            byte decryptionKey = 0xAA; // Use the same key as in encryption
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] ^= decryptionKey;
            }

            // Write the decrypted bytes back to the file
            System.IO.File.WriteAllBytes(filePath, fileBytes);

            Console.WriteLine($"File '{Path.GetFileName(filePath)}' decrypted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decrypt the file: {ex.Message}");
        }
    }

    static void GoBackToParentDirectory(ref string currentDirectory)
    {
        try
        {
            DirectoryInfo currentDirInfo = new DirectoryInfo(currentDirectory);
            if (currentDirInfo.Parent != null)
            {
                currentDirectory = currentDirInfo.Parent.FullName;
                Console.WriteLine($"Moved to the parent directory: {currentDirectory}");
            }
            else
            {
                Console.WriteLine("Already in the root directory, cannot go back.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to go back to the parent directory: {ex.Message}");
        }

        Console.ReadLine(); // Pause to let the user read the message
    }

    static void GitClone(string currentDirectory)
    {
        try
        {
            Console.Write("Enter the repository URL to clone: ");
            string repositoryUrl = Console.ReadLine();
            Console.Write("Enter the name of the new folder: ");
            string newFolderName = Console.ReadLine();
            string destinationDirectory = Path.Combine(currentDirectory, newFolderName);

            Console.WriteLine($"Cloning repository to {destinationDirectory}...");

            using (Process process = new Process())
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone {repositoryUrl} \"{destinationDirectory}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = currentDirectory
                };

                process.StartInfo = psi;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Repository cloned successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to clone repository. Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        Console.ReadLine();
    }

    static void CloneRepository(string repositoryUrl, string destinationDirectory)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = destinationDirectory
            };
            using (Process process = new Process { StartInfo = psi })
            {
                process.StartInfo.Arguments = $"clone {repositoryUrl}";
                process.Start();
                // Capture the output for review or error handling
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Repository cloned successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to clone repository. Error: {error}");
                }
            }
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void TaskManager()
    {
        Console.Clear();
        ApplyColorTheme();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Task Manager\n");

        Console.WriteLine("{0,-30} {1,-15} {2,-18} {3,-20} {4,-20} {5,-10} {6,-25}\n", "Process Name", "CPU (%)", "Memory (MB)", "Disk Usage (%)", "Network Usage (%)", "Threads", "Start Time");

        Process[] processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
                using (PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true))
                using (PerformanceCounter memCounter = new PerformanceCounter("Process", "Working Set", process.ProcessName, true))
                using (PerformanceCounter diskCounter = new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true))
                {
                    cpuCounter.NextValue();
                    memCounter.NextValue();
                    diskCounter.NextValue();
                    System.Threading.Thread.Sleep(5);
                    float cpuUsage = cpuCounter.NextValue();
                    long memoryUsage = Convert.ToInt64(memCounter.NextValue());
                    float diskUsage = diskCounter.NextValue();

                    Console.WriteLine("{0,-30} {1,-15} {2,-18} {3,-20} {4,-20} {5,-10} {6,-25}",
                        process.ProcessName,
                        cpuUsage,
                        BytesToMegabytes(memoryUsage),
                        diskUsage,
                        "N/A", // Placeholder for Network Usage (%)
                        process.Threads.Count,
                        process.StartTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving information for process {process.ProcessName}: {ex.Message}");
            }
        }

        Console.WriteLine("\nPress any key to go back to the main menu.");
        Console.ReadKey();
    }

    static double BytesToMegabytes(float bytes)
    {
        return (bytes / 1024f) / 1024f;
    }

}