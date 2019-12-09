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

namespace Sound_Spectrum
{
    public class WavFile
    {
        public string path;
        //-----WaveHeader-----
        public char[] sGroupID; // RIFF
        public uint dwFileLength; // total file length minus 8, which is taken up by RIFF
        public char[] sRiffType;// always WAVE

        //-----WaveFormatChunk-----
        public char[] sFChunkID;         // Four bytes: "fmt "
        public uint dwFChunkSize;        // Length of header in bytes
        public ushort wFormatTag;       // 1 (MS PCM)
        public ushort wChannels;        // Number of channels
        public uint dwSamplesPerSec;    // Frequency of the audio in Hz... 44100
        public uint dwAvgBytesPerSec;   // for estimating RAM allocation
        public ushort wBlockAlign;      // sample frame size, in bytes
        public ushort wBitsPerSample;    // bits per sample

        //-----WaveDataChunk-----
        public char[] sDChunkID;     // "data"
        public uint dwDChunkSize;    // Length of header in bytes
        public byte dataStartPos;  // audio data start position
       //public Complex[] AudioDataArray;
        public byte[] AudioDataArray;
        public WavFile()
        {
            //path = Environment.CurrentDirectory;
            //-----WaveHeader-----
            dwFileLength = 0;
            sGroupID = "RIFF".ToCharArray();
            sRiffType = "WAVE".ToCharArray();

            //-----WaveFormatChunk-----
            sFChunkID = "fmt ".ToCharArray();
            dwFChunkSize = 16;
            wFormatTag = 1;
            wChannels = 2;
            dwSamplesPerSec = 44100;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;

            //-----WaveDataChunk-----
            dataStartPos = 44;
            dwDChunkSize = 0;
            sDChunkID = "data".ToCharArray();
        }
       public void WriteWav(WavFile wavFile)
        {
            FileStream fsr = new FileStream(wavFile.path, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fsr);
            string path = wavFile.path;
            path = path.Insert(path.Length - 4, "(+)");
            FileStream fsw = null;
            try
            {
                fsw = new FileStream(path, FileMode.CreateNew);
            }
            catch (IOException)
            {
                fsw = new FileStream(path, FileMode.Truncate);
            }
            BinaryWriter w = new BinaryWriter(fsw);
            int pos = 0, len = (int)r.BaseStream.Length; short temp;

            while (pos < len)
            {
                temp = (short)r.ReadInt16();
                //Working with the temp
                w.Write(temp);
                pos += 2;
            }
            r.Close(); w.Close();
            fsr.Close(); fsw.Close();
        }


        // Returns left and right double arrays. 'right' will be null if sound is mono.
       public void openWav(WavFile wavFile)
       {
           //path
           FileStream fsr = new FileStream(wavFile.path, FileMode.Open, FileAccess.Read);
           BinaryReader r = new BinaryReader(fsr);
           try
           {
               wavFile.sGroupID = r.ReadChars(4);
               wavFile.dwFileLength = r.ReadUInt32();
               wavFile.sRiffType = r.ReadChars(4);
               wavFile.sFChunkID = r.ReadChars(4);
               wavFile.dwFChunkSize = r.ReadUInt32();
               wavFile.wFormatTag = r.ReadUInt16();
               wavFile.wChannels = r.ReadUInt16();
               wavFile.dwSamplesPerSec = r.ReadUInt32();
               wavFile.dwAvgBytesPerSec = r.ReadUInt32();
               wavFile.wBlockAlign = r.ReadUInt16();
               wavFile.wBitsPerSample = r.ReadUInt16();
               wavFile.sDChunkID = r.ReadChars(4);
               wavFile.dwDChunkSize = r.ReadUInt32();
               wavFile.dataStartPos = (byte)r.BaseStream.Position;

               int n = (int)(wavFile.dwFileLength / wavFile.wChannels * 8 / wavFile.wBitsPerSample);
               wavFile.AudioDataArray = new byte[dwFileLength-44];

               for (int i = 0; i < dwFileLength-44; i++)
               {
                   wavFile.AudioDataArray [i]= r.ReadByte();
                   //wavFile.AudioDataArray[i] = sample / 32768f;  
               }
           }
           finally
           {
               r.Close();
               fsr.Close();

           }
       }
    }
}