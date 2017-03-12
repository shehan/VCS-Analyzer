using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCS_Analyzer.Services;

namespace VCS_Analyzer.InitDatabase
{
    class InitDatabase : IProcess
    {
        public event EventHandler<NotificationArgs> NotificationIssued;

        public void StartProcess()
        {
            throw new NotImplementedException();
        }
    }
}
