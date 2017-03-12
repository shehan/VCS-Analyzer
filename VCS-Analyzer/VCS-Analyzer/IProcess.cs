using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCSAnalyzer.Services;

namespace VCSAnalyzer
{
    public interface IProcess
    {
        void StartProcess();

        event EventHandler<NotificationArgs> NotificationIssued;

        void OnMessageIssued(NotificationArgs e);
    }
}
