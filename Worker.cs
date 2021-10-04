using System.Net.Sockets;

namespace TechMain_2021_web_server {
    class Worker {
        public Worker(TcpListener listener) {
            while (true) {
                TcpClient tcpClient = listener.AcceptTcpClient();
                Run(tcpClient);
            }
        }

        private void Run(TcpClient tcpClient) {
            Client client = new Client(tcpClient);
            client.Run();
            client.Close();
        }
    }
}