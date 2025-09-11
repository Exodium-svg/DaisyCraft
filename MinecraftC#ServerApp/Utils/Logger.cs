using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaisyCraft.Utils
{
    public class Logger
    {

        IEnumerable<Stream> outputs;
        public Logger(IEnumerable<Stream> outputs) => this.outputs = outputs;
        private string GetTimestamp() => DateTime.Now.ToString("HH:mm:ss");
        public void Info(string message) => Write($"{GetTimestamp()} [INFO] {message}");
        public void Warn(string message) => Write($"{GetTimestamp()} [WARN] {message}");
        public void Error(string message) => Write($"{GetTimestamp()} [ERROR] {message}");
        public void Exception(Exception e) => Write($"{GetTimestamp()} [EXCEPTION] {e.ToString()}");
        public void Exception(string message, Exception e) => Write($"{GetTimestamp()} [EXCEPTION] {message}\n{e.ToString()}");
        private void Write(string message, string? color = null)
        {
            lock (outputs)
            {
                foreach (var output in outputs)
                {
                    string colorFormat = string.Empty;

                    if (output == Console.OpenStandardOutput() && !string.IsNullOrEmpty(color))
                        colorFormat = color;

                    output.Write(Encoding.UTF8.GetBytes($"{colorFormat}{message}" + Environment.NewLine));
                }
            }
        }
    }
}
