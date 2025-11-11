using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EAR_recognize_eye_C_
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private CascadeClassifier _eyeCascade;
        private bool _isRunning = false;

        public Form1()
        {
            InitializeComponent();
            _eyeCascade = new CascadeClassifier("cascades\\haarcascade_eye.xml"); // --> neyronka
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                _capture = new VideoCapture(0);
                _capture.ImageGrabbed += ProcessFrame;
                _capture.Start();
                _isRunning = true;
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame);
            var image = frame.ToImage<Bgr, byte>().Flip(FlipType.Horizontal);
            var gray = image.Convert<Gray, byte>();

            var eyes = _eyeCascade.DetectMultiScale(gray, 1.1, 5, Size.Empty); // --> gozun tapilmagi

            foreach (var eye in eyes)
            {
                image.Draw(eye, new Bgr(Color.LimeGreen), 2);
            }

            pictureBox1.Image = image.ToBitmap();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_capture == null)
            {
                MessageBox.Show("Сначала нажмите Старт");
                return;
            }

            Mat frame = new Mat();
            _capture.Retrieve(frame);
            var image = frame.ToImage<Bgr, byte>().Flip(FlipType.Horizontal); //--> firlanma(zerkalni)
            string imagePath = "temp.jpg";
            image.Save(imagePath);

            ProcessStartInfo psi = new ProcessStartInfo //-->python
            {
                FileName = @"C:\Users\MikeTyson\AppData\Local\Programs\Python\Python310\python.exe",
                Arguments = $"eyeustalost.py temp.jpg",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try // --> algoriitm obrabotki
            {
                using (Process process = Process.Start(psi))
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd().Trim();

                    if (result.StartsWith("FATIGUE:"))
                    {
                        string percentStr = result.Substring("FATIGUE:".Length);
                        if (int.TryParse(percentStr, out int percent))
                        {
                            label1.Text = $"Усталость: {percent}%";
                            label1.ForeColor = percent >= 70 ? Color.Red :
                                               percent >= 40 ? Color.Orange :
                                               Color.Green;
                        }
                        else
                        {
                            label1.Text = "Ошибка разбора процента";
                            label1.ForeColor = Color.Gray;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске Python: " + ex.Message);
            }
        }
    }
}
