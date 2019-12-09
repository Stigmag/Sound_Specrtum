using System;
using System.Numerics;

namespace Sound_Spectrum
{
    public class FftEventArgs : EventArgs
    {
        public FftEventArgs(Complex[] result)
        {
            this.Result = result;
        }
        public Complex[] Result { get; private set; }

        public Complex[] HarmonicProductSpectrum(Complex[] data)
        {
            Complex[] hps2 = Downsample(data, 2);
            Complex[] hps3 = Downsample(data, 3);
            Complex[] hps4 = Downsample(data, 4);
            Complex[] hps5 = Downsample(data, 5);
            Complex[] array = new Complex[hps5.Length];
            for (int i = 0; i < array.Length; i++)
            {
                checked
                {
                        array[i] = data[i].Real * hps2[i].Real * hps3[i].Real * hps4[i].Real * hps5[i].Real; // HERE
                }
            }
            return array;
        }

        public Complex[] Downsample(Complex[] data, int n)
        {
            Complex[] array = new Complex[Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / n))];
            for (int i = 0; i < array.Length; i++)
            {
                    array[i] = data[i * n].Real; // AND HERE
            }
            return array;
        }
    }
}