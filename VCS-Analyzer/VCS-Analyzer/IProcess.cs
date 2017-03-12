using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCS_Analyzer.Services;

namespace VCS_Analyzer
{
    public interface IProcess
    {
        void StartProcess();

        event EventHandler<NotificationArgs> NotificationIssued;
    }
}
