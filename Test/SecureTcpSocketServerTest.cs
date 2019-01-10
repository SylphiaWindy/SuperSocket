using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using NUnit.Framework;
using SuperSocket.SocketBase;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace SuperSocket.Test
{
    [TestFixture]
    public class SecureTcpSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "SecureTestServer.config";
            }
        }
        
        [Test]
        public void TestLoadNewCertificate()
        {
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            TestWelcome(serverAddress, serverConfig.Name);

            File.Copy(serverConfig.Certificate.FilePath, $"{serverConfig.Certificate.FilePath}.bak", true);
            File.Copy(serverConfig.Certificate.FilePath.Replace(".pfx", "_new.pfx"), serverConfig.Certificate.FilePath, true);
            var server = BootStrap.GetServerByName("TestServer") as IAppServer;
            server.ReloadCertificate();

            TestWelcome(serverAddress, serverConfig.Name);

            File.Copy($"{serverConfig.Certificate.FilePath}.bak", serverConfig.Certificate.FilePath, true);
        }

        protected override Stream GetSocketStream(System.Net.Sockets.Socket socket)
        {
            SslStream stream = new SslStream(new NetworkStream(socket), false, new RemoteCertificateValidationCallback(ValidateRemoteCertificate));
            stream.AuthenticateAsClient("supersocket");
            Console.WriteLine($"{stream.RemoteCertificate.Subject} expires at {stream.RemoteCertificate.GetExpirationDateString()}");
            return stream;
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
