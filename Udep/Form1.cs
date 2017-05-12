using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Threading;
using System.Net;

namespace Udep
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCaptureDevices;
        private VideoCaptureDevice FinalVideo;

        private byte[] imagear;
        private byte[] arr;
        private byte[] ahrs;


        UdpClient udpClient1 = new UdpClient();
        UdpClient udpClient2 = new UdpClient();
        UdpClient udpClient3 = new UdpClient(11002);
        ImageConverter converter = new ImageConverter();
        Bitmap video;
        Bitmap video2;

        private string[] portNames;
        SerialPort sp1;
        public Thread _t1;

        public bool flagaLatarka = false;


        public Form1()
        {
            InitializeComponent();
            axVLCPlugin21.playlist.add("rtsp://localhost:8554/stre");


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices)
            {
                comboBox1.Items.Add(VideoCaptureDevice.Name);
            }

            comboBox1.SelectedIndex = 0;
            FinalVideo = new VideoCaptureDevice();
        }

        private void axVLCPlugin21_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (FinalVideo.IsRunning == true) FinalVideo.Stop();

            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[comboBox1.SelectedIndex].MonikerString);
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            udpClient1.Connect(textBox1.Text, 11000);
            FinalVideo.Start();
        }

        private void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();
            video2 = new Bitmap(video);

            pictureBox1.Image = video2;

            imagear = (byte[])converter.ConvertTo(video, typeof(byte[]));
            //try {

            MemoryStream ms = new MemoryStream();
            Image image = Image.FromStream(new MemoryStream(imagear));
            image.Save(ms, ImageFormat.Jpeg);
            arr = ms.ToArray();

            //Console.WriteLine(arr.Length.ToString());
            udpClient1.Send(arr, arr.Length);
            //udpClient.Close();
            /*}
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }*/

        }

        private void button2_Click(object sender, EventArgs e)
        {            
            _t1.Abort();
            sp1.Close();
            FinalVideo.Stop();
            udpClient1.Close();
            udpClient2.Close();
            udpClient3.Close();
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            portNames = SerialPort.GetPortNames();

            sp1 = new SerialPort("COM" + textBox2.Text, 460800);
            _t1 = new Thread(_func1);
            _t1.Start();
        }

        public void _func1()
        {
            if (portNames.Length > 0)
            {
                sp1.Open();
            }
            else
            {
                MessageBox.Show("COM error");
                _t1.Abort();
            }

            IPEndPoint RemoteIpEndPoint3 = new IPEndPoint(IPAddress.Any, 11002);
            string doLatarki = "";

            while (true)
            {

                ahrs = Encoding.ASCII.GetBytes(sp1.ReadLine());
                udpClient2.Connect(textBox1.Text, 11001);
                udpClient2.Send(ahrs, ahrs.Length);
                Console.WriteLine(ahrs.Length);

                if(udpClient3.Available > 0)
                {
                    Byte[] receiveBytes = udpClient3.Receive(ref RemoteIpEndPoint3);
                    doLatarki = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine(doLatarki);
                    if (doLatarki == "1") sp1.WriteLine("on");
                    if (doLatarki == "0") sp1.WriteLine("off");
                }



                //sp1.WriteLine(Encoding.ASCII.GetString(receiveBytes));
            }
        }


    }
}
