using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hadooper
{
    class Program
    {

        private static string username;
        private static string password;
        private static int host;
        private static SshClient client;
        private static SftpClient sftp;

        //Usage hadooper.exe <java folder> <inputfolder> <usrname> <password> [host#] [-e extra args]
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");


            if (args.Length < 4)
            {
                Console.WriteLine("Wrong number of arguments");
                return;
            }

            username = args[2];
            password = args[3];

            if (args.Length > 4 && args[4] != "-e")
            {
                host = int.Parse(args[4]);
            }
            else
            {
                Random rand = new Random();

                int machine = rand.Next(1, 7);
                if (machine == 2)
                    machine = 7;

                host = machine;
            }

            if (args.Length < 4)
                return;

            string extraArgs = "";
            if (args.Length < 5)
            {

            }
            else if (args[4] == "-e")
            {
                for (int i = 5; i < args.Length; i++)
                {
                    extraArgs += " " + args[i];
                }
            }
            else if (args.Length >= 6 && args[5] == "-e")
            {
                for (int i = 6; i < args.Length; i++)
                {
                    extraArgs += " " + args[i];
                }
            }

            extraArgs = extraArgs.Length == 0 ? "" : extraArgs.Substring(1);

            DirectoryInfo input;

            if (Path.IsPathRooted(args[1]))
            {
                input = new DirectoryInfo(args[1]);
            }
            else
            {
                input = new DirectoryInfo(Path.Combine(args[0], args[1]));
            }

            DirectoryInfo java = new DirectoryInfo(args[0]);

            // find main method
            string mainClass = "";
            foreach (FileInfo f in java.GetFiles("*.java"))
            {
                string text = File.ReadAllText(f.FullName);
                if (text.Contains("public static void main(String[] args)"))
                {
                    mainClass = f.Name.Substring(0, f.Name.IndexOf('.'));
                    break;
                }
            }

            string name;
            if (mainClass.Contains("Driver"))
                name = mainClass.Substring(0, mainClass.IndexOf("Driver"));
            else
                name = mainClass;

            client = new SshClient("unix2.csc.calpoly.edu", username, password);
            sftp = new SftpClient(client.ConnectionInfo);
            sftp.Connect();
            client.Connect();

            Console.WriteLine("Making nessecary directories");

            Run("mkdir hadooper");
            Run("mkdir hadooper/.scripts");
            Run("mkdir hadooper/" + name);
            Run("rm -r hadooper/" + name + "/*");
            Run("mkdir hadooper/" + name + "/java");
            Run("mkdir hadooper/" + name + "/input");
            Run("touch hadooper/.settings");

            string[] settings = Run("cat hadooper/.settings", false).Split('\n');

            string version = settings.ElementAtOrDefault(0);
            string ssh = settings.ElementAtOrDefault(1);
            string currentVersion = Run("cat ~rnett/.hadooper/version", false).Trim('\n', '\r', ' ');

            if(ssh != "ssh")
            {
                Console.WriteLine("Updating SSH for passwordless logon to 127x## machines");
                Run("cat /dev/zero | ssh-keygen -f .ssh/hadooper_key -q -N \"\"");
                Run("cat .ssh/hadooper_key.pub >> .ssh/authorized_keys");
            }

            if(currentVersion != version)
            {
                Console.WriteLine("Updating scripts");
                Run("rm hadooper/.scripts/*");
                Run("cp ~rnett/.hadooper/scripts/* hadooper/.scripts");
                var t = Run("printf \"" + currentVersion + "\\nssh\" > hadooper/.settings");
            }

            Console.WriteLine("Uploading input files");
            foreach (FileInfo f in input.GetFiles())
            {
                //Run("touch hadooper/" + name + "/input/" + f.Name);
                UploadFile(f, "//home/" + username + "/hadooper/" + name + "/input");
            }

            Console.WriteLine("Uploading java files");
            foreach (FileInfo f in java.GetFiles())
            {
                //Run("touch hadooper/" + name + "/java/" + f.Name);
                UploadFile(f, "//home/" + username + "/hadooper/" + name + "/java");
            }

            sftp.Disconnect();

            Console.WriteLine("Uploading to hadoop servers, compiling, and running hadoop job");
            string start = "hadooper/.scripts/start.sh " + host + " " + name + " " + mainClass + " " + username + " \"" + extraArgs + "\"";
            var outS = Run(start);

            Console.WriteLine("Downloading log files");

            Run("sed 's/$/\\r\\n/' hadooper/" + name + "/" + name + "_hadoop.out > hadooper/" + name + "/" + name + "_hadoop_out.txt");
            Run("sed 's/$/\\r\\n/' hadooper/" + name + "/" + name + "_compile.out > hadooper/" + name + "/" + name + "_compile_out.txt");


            // possibility of accessing hdfs directiy via hdfs://127x01.csc.calpoly.edu:8020/user/rnett/hadooper/input ?

            //TODO ftp download output and out files
            string output = Path.Combine(input.FullName + "/../output");
            string remoteBase = "//home/" + username + "/hadooper/" + name + "/";
            sftp.Connect();

            Directory.CreateDirectory(output);

            foreach (string f in Directory.GetFiles(output))
                File.Delete(f);

            DownloadFile(output + "/" + name + "_compile_out.txt", remoteBase + name + "_compile_out.txt");
            DownloadFile(output + "/" + name + "_hadoop_out.txt", remoteBase + name + "_hadoop_out.txt");

            Run("rm " + remoteBase + "*.txt");

            Console.WriteLine("Downloading output files");

            var list = Run("ls " + remoteBase + "output").Split('\n');

            foreach (string file in list)
            {
                if (file == "" || file == "." || file == "..")
                    continue;

                if(!file.Contains(".") && file != "_SUCCESS")
                {
                    Run("sed 's/$/\\r\\n/' " + remoteBase + "output/" + file + " > " + remoteBase + "output/" + file + ".txt");

                    DownloadFile(output + "/" + file + ".txt", remoteBase + "output/" + file + ".txt");
                }

                DownloadFile(output + "/" + file, remoteBase + "output/" + file);
                
            }

            Run("rm " + remoteBase + "output/*.txt");

            client.Disconnect();
        }


        public static void UploadFile(FileInfo file, string ftpDir)
        {
            Run("touch " + ftpDir + "/" + file.Name);
            var inputStream = file.OpenRead();

            var up = sftp.BeginUploadFile(inputStream, ftpDir + "/" + file.Name);
            while (!up.IsCompleted)
                ;
            sftp.EndUploadFile(up);
            inputStream.Close();
        }

        public static void DownloadFile(string file, string ftpFile)
        {

            if (File.Exists(file))
                File.Delete(file);

            var outputStream = File.OpenWrite(file);

            var down = sftp.BeginDownloadFile(ftpFile, outputStream);
            while (!down.IsCompleted)
                ;

            sftp.EndDownloadFile(down);

            outputStream.Close();
        }

        public static string Run(string command, bool write = true)
        {
            string r = client.RunCommand(command).Result;

            if(write)
                Console.WriteLine(r);

            return r;
        }

    }
}
