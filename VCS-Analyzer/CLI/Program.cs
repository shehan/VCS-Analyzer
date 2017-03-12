using Fclp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCS_Analyzer;
using VCS_Analyzer.Services;

namespace VCSAnalyzer.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<ApplicationArguments>();

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.Process)
             .As('p', "process") // define the short and long option name
             .Required(); // using the standard fluent Api to declare this Option as required.

            p.Setup(arg => arg.Options)
             .As('o', "option");


            var result = p.Parse(args);

            if (result.HasErrors == false)
            {
                ProcessFactory factory = new ProcessFactory();
                IProcess process = factory.GetProcess(p.Object.Process.ToLower(), p.Object.Options);

                process.NotificationIssued += Process_NotificationIssued;
                process.StartProcess();
            }
            else
            {
                Console.WriteLine("Invalid aruments provided");
                Console.ReadLine();
            }
        }

        private static object locker = new object();
        private static void Process_NotificationIssued(object sender, NotificationArgs e)
        {           
            lock (locker)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    var text = string.Format("{0};{1};{2}", e.Type, e.Message, e.Timestamp);
                    w.WriteLine(text);
                    Console.WriteLine(text);
                }
            }
        }
    }
    public class ApplicationArguments
    {
        public string Process { get; set; }
        public string Options { get; set; }
    }
}
