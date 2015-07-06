using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Common
{
    public class Logger
    {
        Action<string> _fnOut, _fnErr;

        public Logger(Action<string> fnOut, Action<string> fnErr) {
            _fnOut = fnOut;
            _fnErr = fnErr;
        }

        public void Info(string format, params object[] args) {
            string t = string.Format(format, args);
            _fnOut(t);
        }

        public void Error(string format, params object[] args) {
            string t = string.Format(format, args);
            _fnErr(t);
        }
    }
}
