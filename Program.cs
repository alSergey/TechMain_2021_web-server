using System;
using System.Threading;

namespace TechMain_2021_web_server {
    class Program {
        static void Main(string[] args) {
            int threadCount = Environment.ProcessorCount;
            Console.WriteLine("Start Server " + threadCount);
            
            Server server = new Server(8080, threadCount);
            server.Start();
        }
    }
}
