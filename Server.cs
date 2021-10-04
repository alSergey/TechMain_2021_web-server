using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TechMain_2021_web_server {
    class Server {
        public Server(int port, int threadCount) {
            this.__port = port;
            this.__threadCount = threadCount;
        }

        ~Server() {
            if (__tcpListener != null) {
                __tcpListener.Stop();
            }
        }

        public void Start() {
            this.__CreateThreads();

            this.__tcpListener = new TcpListener(IPAddress.Any, this.__port);
            this.__tcpListener.Start();

            this.__StartThreads(this.__tcpListener);
            this.__JoinThreads();
        }

        private void __CreateWorker(Object listener) {
            new Worker((TcpListener)listener);
        }

        private void __CreateThreads() {
            this.__threadPool = new Thread[this.__threadCount];
            for (int i = 0; i != this.__threadCount; i++) {
                this.__threadPool[i] = new Thread(new ParameterizedThreadStart(__CreateWorker));
            }
        }

        private void __StartThreads(Object listener) {
            for (int i = 0; i != this.__threadCount; i++) {
                this.__threadPool[i].Start(listener);
            }
        }

         private void __JoinThreads() {
            for (int i = 0; i != this.__threadCount; i++) {
                this.__threadPool[i].Join();
            }
        }
        
        private int __port;
        private int __threadCount;
        private TcpListener __tcpListener;
        private Thread[] __threadPool;
    }
}