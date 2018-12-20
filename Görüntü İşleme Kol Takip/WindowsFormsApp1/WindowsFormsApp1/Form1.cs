using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using Point = System.Drawing.Point;
using System.IO.Ports;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCapTureDevices;
        private VideoCaptureDevice Finalvideo;
        public Form1()
        {
            InitializeComponent();
        }
        int R;
        int G;
        int B;      
        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCapTureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCapTureDevices)
            {
                comboBox1.Items.Add(VideoCaptureDevice.Name);
            }
            comboBox1.SelectedIndex = 0;

            LoadPorts(); //PC de bağlı portlar listeye eklenir.
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Finalvideo = new VideoCaptureDevice(VideoCapTureDevices[comboBox1.SelectedIndex].MonikerString);
            Finalvideo.NewFrame += new NewFrameEventHandler(Finalvideo_NewFrame);
            Finalvideo.DesiredFrameRate = 20;//saniyede kaç görüntü alsın isteniyorsa
            Finalvideo.DesiredFrameSize = new Size(320, 240);//görüntü boyutları
            Finalvideo.Start();
        }
        void Finalvideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image1 = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;
            if (rdiobtnKirmizi.Checked)
            {
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                filter.CenterColor = new RGB(Color.FromArgb(215, 0, 0));
                filter.Radius = 100;
                filter.ApplyInPlace(image1);            
                nesnebul(image1);               
            }  
            if (rdbtnElleBelirleme.Checked)
            {
                EuclideanColorFiltering filter = new EuclideanColorFiltering();
                filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
                filter.Radius = 100;
                filter.ApplyInPlace(image1);
                nesnebul(image1);
            } 
        }
        public void nesnebul(Bitmap image)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            BitmapData objectsData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage = grayscaleFilter.Apply(new UnmanagedImage(objectsData));
            image.UnlockBits(objectsData);
            blobCounter.ProcessImage(image);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            Blob[] blobs = blobCounter.GetObjectsInformation();
            pictureBox2.Image = image;
            if (rdiobtnCokCisimTakibi.Checked)
            {
                for (int i = 0; rects.Length > i; i++)
                {
                    Rectangle objectRect = rects[i];
                    Graphics g = pictureBox1.CreateGraphics();
                    if (objectRect.Width * objectRect.Height < 2000 && objectRect.Width * objectRect.Height > 1000)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                        {
                            g.DrawRectangle(pen, objectRect);
                            g.DrawString((i + 1).ToString(), new Font("Arial", 12), Brushes.Red, objectRect);
                        }
                        //Cizdirilen Dikdörtgenin Koordinatlari aliniyor.
                        int objectX = objectRect.X + (objectRect.Width / 2);
                        int objectY = objectRect.Y + (objectRect.Height / 2);
                        //g.DrawString(objectX.ToString() + "X" + objectY.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(250, 1));
                        //g.DrawString(objectRect.Width.ToString() + "X" + objectRect.Height.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(250, 1));
                        //label1.Text = objectRect.Width.ToString();
                        //label2.Text = objectRect.Height.ToString();
                        //Control.CheckForIllegalCrossThreadCalls = false;
                        
                        if (objectY < 80)
                        {
                            if (objectX < 160)
                                WriteToPort("a");
                            else
                                WriteToPort("d");
                        }
                        else if(objectY > 160)
                        {
                            if (objectX > 160)
                                WriteToPort("f");
                            else
                                WriteToPort("c");
                        }
                        else
                        {
                            if (objectX < 160)
                                WriteToPort("b");
                            else
                                WriteToPort("e");
                        }
                    }
                    g.Dispose();
                }
            }
        }
        public void WriteToPort(string text)
        {
            byte[] bytes = serialPort1.Encoding.GetBytes(text);
            serialPort1.Write(bytes , 0, bytes.Length);
        }
        public void LoadPorts()
        {
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    serialPort1.PortName = "COM" + i.ToString();
                    serialPort1.Open();
                    cbPort.Items.Add(serialPort1.PortName);
                    serialPort1.Close();
                }
                catch (Exception)
                { continue; }
            }
        }
        private Point[] ToPointsArray(List<IntPoint> points)
        {          
            Point[] array = new Point[points.Count];
            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new Point(points[i].X, points[i].Y);
            }
            return array;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (Finalvideo.IsRunning)
            {
                Finalvideo.Stop();              
            }
        }     
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            R = trackBar1.Value;
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            G = trackBar2.Value;
        }
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            B = trackBar3.Value;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (Finalvideo.IsRunning)
            {
                Finalvideo.Stop();
            }
            Application.Exit();
        }

        private void cbPort_SelectedValueChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();
           
            serialPort1.PortName = cbPort.Text;
            serialPort1.Open();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}