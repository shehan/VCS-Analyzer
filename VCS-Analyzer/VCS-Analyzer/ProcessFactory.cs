using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCSAnalyzer.FDroid;

namespace VCS_Analyzer
{
    public class ProcessFactory
    {
        public IProcess GetProcess(string type, string options)
        {
            IProcess process = null;

            switch (type)
            {
                case "fdroid":
                    if (string.IsNullOrEmpty(options))
                        process = new FDroid(false);
                    else if (options.Equals("u", StringComparison.InvariantCultureIgnoreCase))
                        process = new FDroid(true);
                    else
                        throw new ArgumentException();
                    break;
                default: throw new ArgumentException();
            }
            
            return process;
        }


    }
}
