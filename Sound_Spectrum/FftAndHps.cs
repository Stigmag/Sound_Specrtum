using System;
using System.Numerics;
using MathNet;


namespace Sound_Spectrum
{
    public class FFT
    {
        /// <summary>
        /// Вычисление поворачивающего модуля e^(-i*2*PI*k/N)
        /// </summary>
        /// <param name="k"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private static Complex w(int k, int N)
        {
            if (k % N == 0) return 1;
            double arg = -2 * Math.PI * k / N;
            return new Complex(Math.Cos(arg), Math.Sin(arg));
        }
        /// <summary>
        /// Возвращает спектр сигнала
        /// </summary>
        /// <param name="x">Массив значений сигнала. Количество значений должно быть степенью 2</param>
        /// <returns>Массив со значениями спектра сигнала</returns>
        /// 
        

public enum Direction
  {
    Forward = 1,
    Backward = -1
  }

  ;
  public class Tools
  {
    public static int Pow2(int power)
    {
      return ((power >= 0) && (power <= 30)) ? (1 << power) : 0;
    }

    public static int Log2(int x)
    {
      if (x <= 65536)
      {
        if (x <= 256)
        {
          if (x <= 16)
          {
            if (x <= 4)
            {
              if (x <= 2)
              {
                if (x <= 1)
                  return 0;
                return 1;
              }

              return 2;
            }

            if (x <= 8)
              return 3;
            return 4;
          }

          if (x <= 64)
          {
            if (x <= 32)
              return 5;
            return 6;
          }

          if (x <= 128)
            return 7;
          return 8;
        }

        if (x <= 4096)
        {
          if (x <= 1024)
          {
            if (x <= 512)
              return 9;
            return 10;
          }

          if (x <= 2048)
            return 11;
          return 12;
        }

        if (x <= 16384)
        {
          if (x <= 8192)
            return 13;
          return 14;
        }

        if (x <= 32768)
          return 15;
        return 16;
      }

      if (x <= 16777216)
      {
        if (x <= 1048576)
        {
          if (x <= 262144)
          {
            if (x <= 131072)
              return 17;
            return 18;
          }

          if (x <= 524288)
            return 19;
          return 20;
        }

        if (x <= 4194304)
        {
          if (x <= 2097152)
            return 21;
          return 22;
        }

        if (x <= 8388608)
          return 23;
        return 24;
      }

      if (x <= 268435456)
      {
        if (x <= 67108864)
        {
          if (x <= 33554432)
            return 25;
          return 26;
        }

        if (x <= 134217728)
          return 27;
        return 28;
      }

      if (x <= 1073741824)
      {
        if (x <= 536870912)
          return 29;
        return 30;
      }

      return 31;
    }

    public static bool IsPowerOf2(int x)
    {
      return (x > 0) ? ((x & (x - 1)) == 0) : false;
    }
  }


    public static void FastFourierTransform1d(Complex[] data, Direction direction)
    {
      int n = data.Length;
      int m = Tools.Log2(n);
      // reorder data first
      ReorderData(data);
      // compute FFT
      int tn = 1, tm;
      for (int k = 1; k <= m; k++)
      {
        Complex[] rotation = GetComplexRotation(k, direction);
        tm = tn;
        tn <<= 1;
        for (int i = 0; i < tm; i++)
        {
          Complex t = rotation[i];
          for (int even = i; even < n; even += tn)
          {
            int odd = even + tm;
            Complex ce = data[even];
            Complex co = data[odd];
            double tr = co.Real * t.Real - co.Imaginary * t.Imaginary;
            double ti = co.Real * t.Imaginary + co.Imaginary * t.Real;
            data[even] += new Complex(tr, ti);
            data[odd] = new Complex(ce.Real - tr, ce.Imaginary - ti);
          }
        }
      }

      if (direction == Direction.Forward)
      {
        for (int i = 0; i < data.Length; i++)
          data[i] /= (double)n;
      }
    }

    private const int minLength = 2;
    private const int maxLength = 16384;
    private const int minBits = 1;
    private const int maxBits = 14;
    private static int[][] reversedBits = new int[maxBits][];
    private static Complex[, ][] complexRotation = new Complex[maxBits, 2][];
    // Get array, indicating which data members should be swapped before FFT
    private static int[] GetReversedBits(int numberOfBits)
    {
      if ((numberOfBits < minBits) || (numberOfBits > maxBits))
        throw new ArgumentOutOfRangeException();
      // check if the array is already calculated
      if (reversedBits[numberOfBits - 1] == null)
      {
        int n = Tools.Pow2(numberOfBits);
        int[] rBits = new int[n];
        // calculate the array
        for (int i = 0; i < n; i++)
        {
          int oldBits = i;
          int newBits = 0;
          for (int j = 0; j < numberOfBits; j++)
          {
            newBits = (newBits << 1) | (oldBits & 1);
            oldBits = (oldBits >> 1);
          }

          rBits[i] = newBits;
        }

        reversedBits[numberOfBits - 1] = rBits;
      }

      return reversedBits[numberOfBits - 1];
    }

    // Get rotation of complex number
    private static Complex[] GetComplexRotation(int numberOfBits, Direction direction)
    {
      int directionIndex = (direction == Direction.Forward) ? 0 : 1;
      // check if the array is already calculated
      if (complexRotation[numberOfBits - 1, directionIndex] == null)
      {
        int n = 1 << (numberOfBits - 1);
        double uR = 1.0;
        double uI = 0.0;
        double angle = System.Math.PI / n * (int)direction;
        double wR = System.Math.Cos(angle);
        double wI = System.Math.Sin(angle);
        double t;
        Complex[] rotation = new Complex[n];
        for (int i = 0; i < n; i++)
        {
          rotation[i] = new Complex(uR, uI);
          t = uR * wI + uI * wR;
          uR = uR * wR - uI * wI;
          uI = t;
        }

        complexRotation[numberOfBits - 1, directionIndex] = rotation;
      }

      return complexRotation[numberOfBits - 1, directionIndex];
    }

    // Reorder data for FFT using
    private static void ReorderData(Complex[] data)
    {
      int len = data.Length;
            // check data length
            if ((len < minLength) || (len > maxLength) || (!Tools.IsPowerOf2(len)))
             throw new ArgumentException("Incorrect cInputFft length.");
      int[] rBits = GetReversedBits(Tools.Log2(len));
      for (int i = 0; i < len; i++)
      {
        int s = rBits[i];
        if (s > i)
        {
          Complex t = data[i];
          data[i] = data[s];
          data[s] = t;
        }
      }
    }
        public static int highestPowerof2(int n)
        {
            int res = 0;
            for (int i = n; i >= 1; i--)
            {
                // If i is a power of 2 
                if ((i & (i - 1)) == 0)
                {
                    res = i;
                    break;
                }
            }
            return res;
        }


        /////
        public static Complex[] fft(Complex[] x)
        {
            Complex[] X;
            int N = x.Length;
            if (N == 2)
            {
                X = new Complex[2];
                X[0] = x[0] + x[1];
                X[1] = x[0] - x[1];
            }
            else
            {
                Complex[] x_even = new Complex[N / 2];
                Complex[] x_odd = new Complex[N / 2];
                for (int i = 0; i < N / 2; i++)
                {
                    x_even[i] = x[2 * i];
                    x_odd[i] = x[2 * i + 1];
                }
                Complex[] X_even = fft(x_even);
                Complex[] X_odd = fft(x_odd);
                X = new Complex[N];
                for (int i = 0; i < N / 2; i++)
                {
                    X[i] = X_even[i] + w(i, N) * X_odd[i];
                    X[i + N / 2] = X_even[i] - w(i, N) * X_odd[i];
                }
            }
            return X;
        }
        /// <summary>
        /// Центровка массива значений полученных в fft (спектральная составляющая при нулевой частоте будет в центре массива)
        /// </summary>
        /// <param name="X">Массив значений полученный в fft</param>
        /// <returns></returns>
        /// 
        public static Complex[] HarmonicProductSpectrum(Complex[] data)
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
                   // array[i] = data[i].Real * hps2[i].Real * hps3[i].Real * hps4[i].Real * hps5[i].Real; // HERE
                    array[i] = Complex.Sqrt(data[i] * Complex.Conjugate(data[i]) * hps2[i] * Complex.Conjugate(hps2[i]) * hps3[i] * Complex.Conjugate(hps3[i]) *
                     hps4[i] * Complex.Conjugate(hps4[i]) * hps5[i] * Complex.Conjugate(hps5[i]));
                    
                }
            }
            return array;
        }

        public static Complex[] Downsample(Complex[] data, int n)
        {
            Complex[] array = new Complex[Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / n))];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = data[i * n].Real; // AND HERE
            }
            return array;
        }
        public static double FindF0(int Length, Complex [] magnitude)
        {
        double max_mag = float.MinValue;
        float max_index = -1;

        for (int i = 0; i < Length / 2; i++)
            if (magnitude[i].Real > max_mag)
            {
                max_mag =magnitude[i].Real;
                max_index = i;
            }
        var frequency = max_index * 8000 / 1024;
        return frequency;
        }
    
        public static Complex[] nfft(Complex[] X)
        {
            int N = X.Length;
            Complex[] X_n = new Complex[N];
            for (int i = 0; i < N / 2; i++)
            {
                X_n[i] = X[N / 2 + i];
                X_n[N / 2 + i] = X[i];
            }
            return X_n;
        }
    }
}