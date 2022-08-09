using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;
namespace RunAsCSharp
{
    internal class Program
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(String userName, String domain, String password, UInt32 logonFlags, String applicationName, String commandLine, uint creationFlags, UInt32 environment, String currentDirectory, ref STARTUPINFO startupInfo, out ProcessInformation processInformation);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private struct ProcessInformation
        {
            public IntPtr process;
            public IntPtr thread;
            public int processId;
            public int threadId;
        }
        static void Main(string[] args)
        {
            String cmd = null;
            String username = null;
            String password = null;
            String domain = null;
            String fileName = null;
            String logonMethod = "Standard";
            String[] allowedLogonMethods = new String[] { "CreateProcessWithLogonW", "Standard" };
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-u":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing Username \n Exiting");
                            return;
                        }
                        username = args[i + 1];
                        i++;
                        break;
                    case "-p":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing Password\n Exiting");
                            return;
                        }
                        password = args[i + 1];
            
                        i++;

                        break;
                    case "-d":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing Domain (. for Local) \n Exiting");
                            return;
                        }
                        domain = args[i + 1];
                        i++;
                        break;
                    case "-c":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing command  \n Exiting");
                            return;
                        }
                        cmd = args[i + 1];
                        i++;
                        break;
                    case "-h":
                        printHelp();
                        return;
                    case "-f":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing Filename\n Exiting");
                            return;
                        }
                        fileName = args[i + 1];

                        i++;
                        break;
                     
                    case "-m":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("[-] Missing LogonMethod\n Exiting");
                            return;
                        } else if  (!allowedLogonMethods.Contains(args[i + 1])){
                            Console.WriteLine("[-] Unknown Logon Method:  " + args[i+1] + ", allowed methods are: CreateProcessWithTokenW, Standard\n Exiting");
                            return;
                        }
                        logonMethod = args[i + 1];
                        i++;
                        break;
                    default:
                        Console.WriteLine("[-] Unknown argument '" + args[i] + "' \n Exiting");
                        return;
                }
            }
            if (!validateArgs(cmd, username, password, domain)){
                return;
            }
            
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            if (fileName == null){
                fileName = @"C:\Windows\System32\cmd.exe";
                cmd = "/C " + cmd;
            }
            runAs(username, password, domain, fileName, cmd, logonMethod);

            
        }
        static void runAs(String username, String password, String domain, String fileName, String cmd, String logonMethod)
        {
            Console.WriteLine("GOING TO RUN: " + fileName + " " + cmd + " | AS " + domain + "\\" + username + "| PWD:" + password + " via " + logonMethod);

            if (logonMethod == "Standard")
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = fileName;
                startInfo.Arguments = cmd;
                startInfo.UserName = username;
                SecureString sspw = new SecureString();
                foreach (var c in password)
                {
                    sspw.AppendChar(c);
                }
                startInfo.Domain = domain;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;


                startInfo.Password = sspw;
                startInfo.WorkingDirectory = ".";
                process.StartInfo = startInfo;
                process.OutputDataReceived += new DataReceivedEventHandler(outputHappened);
                process.ErrorDataReceived += new DataReceivedEventHandler(outputHappened);
                process.Start();
                process.StandardInput.Flush();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.Start();
                process.WaitForExit();

            }
            else
            {
                STARTUPINFO startupInfo = new STARTUPINFO();
                startupInfo.cb = Marshal.SizeOf(startupInfo);
                startupInfo.lpReserved = null;
                int logonFlags = 0;
                ProcessInformation processInfo = new ProcessInformation();
                bool success = CreateProcessWithLogonW(username, domain, password, (UInt32)logonFlags, fileName, cmd, 0x08000000, (UInt32)0, null, ref startupInfo, out processInfo);
            }

        }
        private static void outputHappened(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
        private static Boolean validateArgs(String cmd, String username, String password, String domain)
        {
            if (cmd == null)
            {
                Console.WriteLine("[-] Missing command (-c )  \n Exiting");
                return false;
            }
            else if (username == null)
            {
                Console.WriteLine("[-] Missing username (-u )  \n Exiting");
                return false;
            }
            else if (password == null)
            {
                Console.WriteLine("[-] Missing password (-p )  \n Exiting");
                return false;
            }
            else if (password == null)
            {
                Console.WriteLine("[-] Missing password (-p )  \n Exiting");
                return false;
            }
            
            return true;
        }
     
        static void printHelp()
        {
            Console.WriteLine("RunAsSharp.exe: Executes command as another user ");
            Console.WriteLine("Arguments");
            Console.WriteLine("\t -u <USERNAME>: Specifies the username to run as");
            Console.WriteLine("\t -p <PASSWORD>: Specifies the user's password");
            Console.WriteLine("\t -d <DOMAN>: Specifies the user's domain. Use '.' for local account");
            Console.WriteLine("\t -c <CMD>: Specify the command to run, run's as cmd.exe /C 'CMD'. Escape double quotes by doing two \"\" in a row. ");
            Console.WriteLine("\t -f <FILENAME>: Specify the executeable to run, if specified the command will be run as: <FILENAME> CMD");
            Console.WriteLine("\t -m <LOGON_METHOD>: Specify method to logon and run a process as a user, options are CreateProcessWithLogonW and Standard (to use .NET method of starting a process). ");


        }
    }
}
