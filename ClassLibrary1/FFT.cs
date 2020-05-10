using System;
using System.Numerics;

namespace FFT
{
    public class FFT
    {
        static void SWAP(float a, float b) { float tempr = (a); (a) = (b); (b) = tempr; }



        //data -> float array that represent the array of complex samples
        //number_of_complex_samples -> number of samples (N^2 order number) 
        //isign -> 1 to calculate FFT and -1 to calculate Reverse FFT
        public static float[] FFT1(float[] data, UInt64 number_of_complex_samples, int sinal)
        {
            //variables for trigonometric recurrences
            UInt64 n, mmax, m, j, istep, i;
            float wtemp, wr, wpr, wpi, wi, theta, tempr, tempi;
            //the complex array is real+complex so the array
            //as a size n = 2* number of complex samples
            // real part is the data[index] and the complex part is the data[index+1]
            n = number_of_complex_samples * 2;
            float[] copyofdata = new float[data.Length];
            data.CopyTo(copyofdata, 0);

            //binary inversion (note that
            //the indexes start from 1 witch means that the
            //real part of the complex is on the odd-indexes
            //and the complex part is on the even-indexes
            j = 1;
            for (i = 1; i < n; i += 2)
            {
                if (j > i)
                {
                    //swap the real part
                    SWAP(copyofdata[j], copyofdata[i]);
                    //swap the complex part
                    SWAP(copyofdata[j + 1], copyofdata[i + 1]);
                }
                m = n / 2;
                while (m >= 2 && j > m)
                {
                    j -= m;
                    m = m / 2;
                }
                j += m;
            }



            //Danielson-Lanzcos routine
            mmax = 2;
            //external loop
            while (n > mmax)
            {
                istep = mmax << 1;
                theta = (float)(sinal * (2 * Math.PI / mmax));
                wtemp = (float)Math.Sin(0.5 * theta);
                wpr = (float)-2.0 * wtemp * wtemp;
                wpi = (float)Math.Sin(theta);
                wr = (float)1.0;
                wi = 0;
                //internal loops
                for (m = 1; m < mmax; m += 2)
                {
                    for (i = m; i <= n; i += istep)
                    {
                        j = i + mmax;
                        tempr = wr * copyofdata[j - 1] - wi * copyofdata[j];
                        tempi = wr * copyofdata[j] + wi * copyofdata[j - 1];
                        copyofdata[j - 1] = copyofdata[i - 1] - tempr;
                        copyofdata[j] = copyofdata[i] - tempi;
                        copyofdata[i - 1] += tempr;
                        copyofdata[i] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
            return copyofdata;
        }
        


















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
