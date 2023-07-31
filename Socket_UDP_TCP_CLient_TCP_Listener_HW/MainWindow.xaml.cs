using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Socket_UDP_TCP_CLient_TCP_Listener_HW { 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new UdpClient();
            try
            {
                var serverIp = IPAddress.Parse("127.0.0.1");
                var serverPort = 27001;
                var remoteEP = new IPEndPoint(serverIp, serverPort);

                byte[] buffer = { 4 };
                await client.SendAsync(buffer, buffer.Length, remoteEP);
                buffer = new byte[ushort.MaxValue - 29];
                var result = await client.ReceiveAsync();
                int imageSize = int.Parse(Encoding.ASCII.GetString(result.Buffer));

                var imageBuffer = new byte[imageSize];
                int totalBytesReceived = 0;

                while (totalBytesReceived < imageSize)
                {
                    result = await client.ReceiveAsync();
                    var receivedSize = result.Buffer.Length;
                    Buffer.BlockCopy(result.Buffer, 0, imageBuffer, totalBytesReceived, receivedSize);
                    totalBytesReceived += receivedSize;
                }
                BuildImage(imageBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        public void BuildImage(byte[] imageBuffer)
        {
            using (MemoryStream ms = new MemoryStream(imageBuffer))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                screenShotImage.Source = image;
            }
        }
    }
}
