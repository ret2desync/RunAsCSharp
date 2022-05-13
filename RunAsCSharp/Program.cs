using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;
namespace RunAsCSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String cmd = null;
            String username = null;
            String password = null;
            String domain = null;
            String fileName = null;
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
            if (fileName != null){
                startInfo.FileName = fileName;
                startInfo.Arguments = cmd;

            }else{
                startInfo.FileName = @"C:\Windows\System32\cmd.exe";
                startInfo.Arguments = "/C " + cmd;
            }
            startInfo.UserName = username;
            SecureString sspw = new SecureString();
            foreach (var c in password){
                sspw.AppendChar(c);
            }
            startInfo.Domain = domain;
            Console.WriteLine("GOING TO RUN: " + startInfo.FileName  + " "+ startInfo.Arguments+ " | AS " + startInfo.Domain + "\\" + startInfo.UserName + "| PWD:" + password);
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

        }
    }
}
