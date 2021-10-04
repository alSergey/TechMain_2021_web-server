using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TechMain_2021_web_server {
    class Client {

        public Client(TcpClient client) {
            this.__client = client;
            this.__stream = this.__client.GetStream();
        }

        public void Run() {
            this.__ParseRequest();

            HttpStatusCode statusMethod = this.__ParseMethod();
            if (statusMethod != HttpStatusCode.OK) {
                this.__SendHeaders(statusMethod);
                return;
            }

            HttpStatusCode urlStatus = this.__ParseURL();
            if (urlStatus != HttpStatusCode.OK) {
                this.__SendHeaders(urlStatus);
                return;
            }

            HttpStatusCode fileStatus = this.__FindFile();
            if (fileStatus != HttpStatusCode.OK) {
                 this.__SendHeaders(fileStatus);
                return;
            }

            this.__SendHeaders(HttpStatusCode.OK);
            if (this.__method == "HEAD") {
                return;
            }

            this.__SendFile();
        }

        public void Close() {
            this.__client.Close();
        }

        private void __ParseRequest() {
            this.__request = this.__GetRequest().Split("\r\n");
        }

        private HttpStatusCode __ParseMethod() {
            this.__method = this.__GetMethod();
            if (this.__method != "GET" && this.__method != "HEAD") {
                return HttpStatusCode.MethodNotAllowed;
            }

            return HttpStatusCode.OK;
        }

        private HttpStatusCode __ParseURL() {
            this.__url = this.__GetURL();
            if (this.__url.Contains("../")) {
                return HttpStatusCode.Forbidden;
            }

            if (this.__url.EndsWith("/") && !this.__url.Contains('.')) {
                this.__url += "index.html";
            }

            int i = this.__url.IndexOf("?");
            if (i > 0) {
                this.__url = this.__url.Substring(0, i);
            }
            
            return HttpStatusCode.OK;
        }

        public HttpStatusCode __FindFile() {
            this.__filePath =  "." + __url;

            if (!File.Exists(this.__filePath)) {
                if (this.__filePath.Contains("index.html")) {
                    return HttpStatusCode.Forbidden;
                }
                
                return HttpStatusCode.NotFound;
            }

            return HttpStatusCode.OK;
        }

        private string __GetRequest() {
            string request = "";

            int count;
            byte[] buff = new byte[1024];

            while ((count = this.__stream.Read(buff, 0, buff.Length)) > 0) {
                request += Encoding.ASCII.GetString(buff, 0, count);

                if (request.Contains("\r\n\r\n")) {
                    break;
                }
            }

            return request;
        }

        private void __SendHeaders(HttpStatusCode statusCode) {
            string header = $"{this.__GetProtocol()} {(int)statusCode} {statusCode}\r\nServer: superPuperServer\r\nDate: {DateTime.Now.ToString()}\r\nConnection: keep-alive\r\n";
            if (statusCode == HttpStatusCode.OK) {
                FileInfo file = new FileInfo(this.__filePath);

                header += "Content-Type: " + this.__GetExtension(file.Extension) + "\r\n" + "Content-Length: " + file.Length + "\r\n";
            }

            header += "\r\n";

            byte[] buff = Encoding.ASCII.GetBytes(header);
            this.__stream.Write(buff, 0, buff.Length);
        }

        private void __SendFile() {
            FileStream file = new FileStream(this.__filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            int count = 0;
            byte[] buff = new byte[1024];

            while (file.Position < file.Length) {
                count = file.Read(buff, 0, buff.Length);
                this.__stream.Write(buff, 0, count);
            }

            file.Close();
        }

        private string __GetMethod() {
            string[] HEAD = this.__request[0].Split(" ");
            return HEAD[0];
        }

        private string __GetURL() {
            string[] HEAD = this.__request[0].Split(" ");
            return Uri.UnescapeDataString(HEAD[1]);
        }

        private string __GetProtocol() {
            // string[] HEAD = this.__request[0].Split(" ");
            // return HEAD[2];
            return "HTTP/1.1";
        }

        private string __GetExtension(string extension) {
            switch (extension) {
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".swf":
                    return "application/x-shockwave-flash";
                default:
                    if (extension.Length > 1) {
                        return "application/" + extension.Substring(1);
                    }

                return "application/unknown";
            }
        }

        private TcpClient __client;
        private NetworkStream __stream;
        private string[] __request;
        private string __method;
        private string __url;
        private string __filePath;
    }
}