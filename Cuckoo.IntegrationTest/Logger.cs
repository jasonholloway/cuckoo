using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace Cuckoo.IntegrationTest
{
    class Logger : ILogger
    {
        string _params;
        IEventSource _eventSource;

        List<string> _lErrors = new List<string>();
        List<string> _lMessages = new List<string>();

        public void Initialize(IEventSource eventSource) {
            _eventSource = eventSource;

            _eventSource.MessageRaised += eventSource_MessageRaised;
            _eventSource.ErrorRaised += eventSource_ErrorRaised;
            _eventSource.AnyEventRaised += eventSource_AnyEventRaised;
        }

        void eventSource_AnyEventRaised(object sender, BuildEventArgs e) {

        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e) {
            _lErrors.Add(e.Message);
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e) {
            _lMessages.Add(e.Message);
        }

        public string Parameters {
            get {
                return _params;
            }
            set {
                _params = value;
            }
        }

        public void Shutdown() {
            if(_eventSource != null) {
                _eventSource.MessageRaised -= eventSource_MessageRaised;
                _eventSource.ErrorRaised -= eventSource_ErrorRaised;
                _eventSource.AnyEventRaised -= eventSource_AnyEventRaised;
            }
        }

        public LoggerVerbosity Verbosity {
            get {
                return LoggerVerbosity.Detailed;
            }
            set {
                throw new NotImplementedException();
            }
        }
    }

}
