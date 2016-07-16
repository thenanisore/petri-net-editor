using System;
using System.Text;

namespace PNDApp.Models
{
    /// <summary>
    /// Represents a system log that contains 
    /// updating information about the running application.
    /// </summary>
    public class Log
    {
        private readonly StringBuilder _log;

        /// <summary>
        /// Constructs a new system log.
        /// </summary>
        public Log()
        {
            _log = new StringBuilder();
            MakeRecord("Log started.");
        }

        /// <summary>
        /// Returns string that contains the whole log.
        /// </summary>
        public string GetLog
        {
            get { return _log.ToString(); }
        }

        /// <summary>
        /// Makes a new record in the current log.
        /// </summary>
        public void MakeRecord(string message)
        {
            var record = new StringBuilder();
            // Add current date and time.
            record.AppendFormat(" {0}", DateTime.Now);
            // Add message.
            record.AppendFormat(" | {0}{1}", message, Environment.NewLine);
            _log.Append(record);
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        public void ClearLog()
        {
            _log.Clear();
            MakeRecord("Log started.");
        }
    }
}
