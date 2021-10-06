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
            HttpStatusCode parseStatus = this.__ParseRequest();
              if (parseStatus != HttpStatusCode.OK) {
                this.__SendHeaders(parseStatus);
                return;
            }

            HttpStatusCode statusMethod = this.__ParseMethod();
            if (statusMethod != HttpStatusCode.OK) {
                this.__SendHeaders(statusMethod);
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

        private HttpStatusCode __ParseRequest() {
            string request = this.__GetRequest();
            if (request == "") {
                return HttpStatusCode.BadRequest;
            }

            this.__request = request.Split(" ");
            return HttpStatusCode.OK;
        }

        private HttpStatusCode __ParseMethod() {
            this.__method = this.__GetMethod();
            if (this.__method != "GET" && this.__method != "HEAD") {
                return HttpStatusCode.MethodNotAllowed;
            }

            return HttpStatusCode.OK;
        }

        private HttpStatusCode __FindFile() {
            string url = this.__GetURL();

            int i = url.IndexOf("?");
            if (i >= 0) {
                url = url.Substring(0, i);
            }

            bool isIndexFile = false;
            if (url.EndsWith("/") && !url.Contains('.')) {
                isIndexFile = true;
                url += "index.html";
            }

            this.__filePath = "." + url;

            // Если у пользователя недостаточно прав на этот файл, то функция вернет false и не будет кидать исключение
            if (!File.Exists(this.__filePath)) {
                if (isIndexFile) {
                    return HttpStatusCode.Forbidden;
                }
                
                return HttpStatusCode.NotFound;
            }

            bool isAllowedDirectory = false;
            try {
                isAllowedDirectory = Path.GetFullPath(this.__filePath).StartsWith(Directory.GetCurrentDirectory());
            } catch (Exception e) {
                Console.WriteLine(e);
                return HttpStatusCode.Forbidden;
            }
            if (!isAllowedDirectory) {
                return HttpStatusCode.Forbidden;
            }
            
            return HttpStatusCode.OK;
        }

        private string __GetRequest() {
            string request = "";

            int count;
            byte[] buff = new byte[128];

            while ((count = this.__stream.Read(buff, 0, buff.Length)) > 0) {
                request += Encoding.ASCII.GetString(buff, 0, count);

                int i = request.IndexOf("\r\n");
                if (i >= 0) {
                    return request.Substring(0, i);
                }
            }
           
           return "";
        }

        private void __SendHeaders(HttpStatusCode statusCode) {
            string header = $"{this.__GetProtocol()} {(int)statusCode} {statusCode}\r\nServer: superPuperServer\r\nDate: {DateTime.Now.ToString()}\r\nConnection: keep-alive\r\n";

            if (statusCode == HttpStatusCode.OK) {
                try {
                    FileInfo file = new FileInfo(this.__filePath);
                    header += "Content-Type: " + this.__GetExtension(file.Extension) + "\r\n" + "Content-Length: " + file.Length + "\r\n";
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }

            header += "\r\n";

            byte[] buff = Encoding.ASCII.GetBytes(header);
            this.__stream.Write(buff, 0, buff.Length);
        }

        private void __SendFile() {
            try {
                FileStream file = new FileStream(this.__filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                int count = 0;
                byte[] buff = new byte[4096];

                while (file.Position < file.Length) {
                    count = file.Read(buff, 0, buff.Length);
                    this.__stream.Write(buff, 0, count);
                }

                file.Close();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private string __GetMethod() {
            return this.__request != null ? this.__request[0] : "GET";
        }

        private string __GetURL() {
            return this.__request != null ? Uri.UnescapeDataString(this.__request[1]) : "/";
        }

        private string __GetProtocol() {
            return this.__request != null ? this.__request[2] : "HTTP/1.1";
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
        private string __filePath;
    }
}