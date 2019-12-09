using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Signals;
using System.Numerics;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using System.Threading;

namespace Sound_Spectrum
{
   
    public partial class Form1 : Form
    {
        //! Почистить константы! (некоторые возможно ненужны) также при остановке записи надо вызвать waveIn.Dispose( сейчас не так)
        static int Fs = 8000; // Частота дискретизвции !В данной программе ТОЛЬКО целые числа
        static double T = 1.0 / Fs; // Шаг дискретизации
        static int N; //Длина сигнала (точек)
        static double Fn = Fs/2;// Частота Найквиста
        
        // WaveIn - поток для записи
        WaveIn waveIn;
        GraphPane myPane;
        WavFile OurWav;
        string OurWavPath;
        WaveFileWriter writer;
        //Получение данных из входного буфера и обработка полученных с микрофона данных
        public Form1()
        {
            InitializeComponent();

          //  button1.Click += button1_Click;
           // button2.Click += button2_Click;
            openFileDialogWav.Filter = "Audio files(*.wav)|*.wav|All files(*.*)|*.*";
            saveFileDialogWav.Filter = "Audio files(*.wav)|*.wav|All files(*.*)|*.*";
        }
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //данные из буфера распределяем в массив чтобы в нем они были в формате ?PCM?
                byte[] buffer = e.Buffer;
                N = buffer.Length;
                int bytesRecorded = e.BytesRecorded;

                int res = FFT.highestPowerof2(bytesRecorded);
                bytesRecorded = res * 2;
                Complex[] sig = new Complex[bytesRecorded/2];
                Complex[] sig2 = new Complex[bytesRecorded / 2];
                for (int i = 0, j = 0; i < e.BytesRecorded; i += 2, j++)
                {       if (bytesRecorded / 2 != i)
                    {
                        short sample = (short)((buffer[i + 1] << 8) | buffer[i + 0]);
                        sig[j] = sample / 32768f;
                    }
                    else sig[j] = 0;
                    
                    
                 //   sig2[j] = sample / 32768f; 
                }

               // Transform.FourierForward(sig, FourierOptions.Matlab);
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
               // Complex [] siq2 = new Complex[bytesRecorded/2];
              //  siq2 = FFT.fft(sig2);
                // обнуляем спектр на небольших частотах (там постоянная составляющая и вообще много помех)
                for (int i = 0; i < 35*sig.Length/Fn; i++)
                {
                    sig[i] = 0;
                }
                //sig2 = FFT.HarmonicProductSpectrum(sig);
                
                //double d =
                //label2.Text = FFT.FindF0(sig2.Length, sig2).ToString();
                FFT.FastFourierTransform1d(sig, FFT.Direction.Forward);
                CreateGraph(zedGraphControl1,sig);
            }
        }
        //Окончание записи
        public void FFTandDraw(byte[] buffer, int bytesRecorded)
        {   // Array<byte> mas=       
            N = buffer.Length;
            //buffer.
            Complex[] sig = new Complex[bytesRecorded / 2];
            //    Complex[] sig2 = new Complex[bytesRecorded / 2];
            for (int i = 0, j = 0; i < bytesRecorded; i += 2, j++)
            {
                short sample = (short)((buffer[i + 1] << 8) | buffer[i + 0]);
                sig[j] = sample / 32768f;
                //   sig2[j] = sample / 32768f; 
            }

            Transform.FourierForward(sig, FourierOptions.Matlab);
             Complex [] siq2 = new Complex[bytesRecorded/2];
            //  siq2 = FFT.fft(sig2);
            // обнуляем спектр на небольших частотах (там постоянная составляющая и вообще много помех)
            for (int i = 0; i < 35 * sig.Length / Fn; i++)
            {
                sig[i] = 0;
            }
            siq2 = FFT.HarmonicProductSpectrum(sig);

            CreateGraph(zedGraphControl1, siq2);
        }
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
               // writer.Close();
               // writer = null;
            }
        }
        void StopRecording()
        {
            MessageBox.Show("StopRecording");
            waveIn.StopRecording();
        }

      //  public Form1()
        //{
          //  InitializeComponent();
        //}

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetSize();
        }


        private void SetSize()
        {
            zedGraphControl1.Location = new Point(10, 10);
            // Leave a small margin around the outside of the control
            zedGraphControl1.Size = new Size(ClientRectangle.Width - 20,
                                    ClientRectangle.Height - 20);
        }

        // Build the Chart
        private void CreateGraph(ZedGraphControl zgc, Complex[] sig)
        {
            // get a reference to the GraphPane
            myPane = zgc.GraphPane;
            myPane.XAxis.Scale.MajorStep = 200.0;
          //  myPane.XAxis.Scale.MinorStep = 200.0;
            myPane.CurveList.Clear();
            PointPairList list1 = new PointPairList();
            double K = sig.Length/2;
            for (int i = 0; i < K; i++)
            {
              //  double a = i * Fn / K;
               // double b = Complex.Abs(sig[i]) / N * 2;
                list1.Add(i*Fn/K, Complex.Abs(sig[i])/N*2);
            }

            // Generate a red curve 
            LineItem myCurve = myPane.AddCurve("", list1, Color.Red, SymbolType.None);
            // Set the Titles
            myPane.Title.Text = "Амплитудный спектр звука";
            myPane.XAxis.Title.Text = "Частота, Гц";
            myPane.YAxis.Title.Text = "Спектр, Дб";

            // Обновляем изображение и оси
            zgc.AxisChange();
            zgc.Invalidate();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Start Recording");
                waveIn = new WaveIn();
                //Дефолтное устройство для записи (если оно имеется)
                waveIn.DeviceNumber = 0;
                //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                waveIn.DataAvailable += waveIn_DataAvailable;
                //Прикрепляем обработчик завершения записи
                waveIn.RecordingStopped += waveIn_RecordingStopped;
                //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                waveIn.WaveFormat = new WaveFormat((int)Fs, 1);
                //Инициализируем объект WaveFileWriter
                
            if (saveFileDialogWav.ShowDialog() == DialogResult.Cancel)
                return;

            if (!string.IsNullOrEmpty(saveFileDialogWav.FileName) && File.Exists(saveFileDialogWav.FileName))
            {
                writer = new WaveFileWriter(saveFileDialogWav.FileName, waveIn.WaveFormat);
            }
                //Начало записи
                waveIn.StartRecording();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
            
                        // Setup the graph
            /*CreateGraph(zedGraphControl1);
            // Size the control to fill the form with a margin
            SetSize();*/ // эта часть не нужна программа узнает о прочитаном звуке с помощью событий.
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                StopRecording();
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //

        }

        private void saveFileDialogWav_FileOk(object sender, CancelEventArgs e)
        {
            //
        }

        private void button3_Click(object sender, EventArgs e)
        {
        /*    if (saveFileDialogWav.ShowDialog() == DialogResult.Cancel)
                return;

            if (!string.IsNullOrEmpty(saveFileDialogWav.FileName) && File.Exists(saveFileDialogWav.FileName))
            {
                // сохраняем текст в файл
                OurWavPath = saveFileDialogWav.FileName.ToString();
                //OurWav.AudioDataArray=
               // OurWav.WriteWav(OurWav);
            }
         //   System.IO.File.WriteAllText(filename, textBox1.Text);
            MessageBox.Show("Директория выбрана");*/
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialogWav.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            OurWav = new WavFile();
            OurWav.path = openFileDialogWav.FileName;
            // читаем файл в строку
            OurWav.openWav(OurWav);
            MessageBox.Show("Файл открыт");
            for (int i = 0; i < OurWav.AudioDataArray.Length; i += 1600)
            {
                OurWav.AudioDataArray.Skip(i);
                FFTandDraw(OurWav.AudioDataArray, 1600);
                //Thread.Sleep(1000);
            }
       //     Transform.FourierForward(OurWav.AudioDataArray, FourierOptions.Matlab);
           // for (int i = 0; i < OurWav.AudioDataArray.Length/1024; i++)

       //         CreateGraph(zedGraphControl1, OurWav.AudioDataArray);
        }
    }
}
