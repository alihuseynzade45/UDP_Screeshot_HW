
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Text;
using System.Runtime.InteropServices;

internal class Program
{
    private static void Main(string[] args)
    {
        var listener = new UdpClient(27001);
        Console.WriteLine("The server is listening for connections...");

        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = listener.Receive(ref remoteEP);
            Console.WriteLine("The client is connected.");

            byte[] screenshotData = GetScreenshot();


            byte[] imageSizeBytes = Encoding.ASCII.GetBytes(screenshotData.Length.ToString());
            listener.Send(imageSizeBytes, imageSizeBytes.Length, remoteEP);


            int bufferSize = ushort.MaxValue - 29;
            int totalBytesSent = 0;

            while (totalBytesSent < screenshotData.Length)
            {
                int remainingBytes = screenshotData.Length - totalBytesSent;
                int sendSize = Math.Min(bufferSize, remainingBytes);
                byte[] buffer = new byte[sendSize];
                Buffer.BlockCopy(screenshotData, totalBytesSent, buffer, 0, sendSize);
                listener.Send(buffer, buffer.Length, remoteEP);
                totalBytesSent += sendSize;
            }
            Console.WriteLine("Screenshot sent.");
        }
    }

    private static byte[] GetScreenshot()
    {
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;


        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        Bitmap Image = new Bitmap(screenWidth, screenHeight);

        Size s = new Size(Image.Width, Image.Height);


        using (var bitmap = new Bitmap(screenWidth, screenHeight))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }
    }
}
