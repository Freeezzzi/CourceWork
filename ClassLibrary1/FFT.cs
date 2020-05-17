using System;
using System.Numerics;

namespace FFT
{
    public class FFT
    {
        // compute the FFT of x[], assuming its length n is a power of 2
        public static Complex[] fft(Complex[] x)
        {
            int n = x.Length;

            // base case
            if (n == 1) return new Complex[] { x[0] };

            // radix 2 Cooley-Tukey FFT
            if (n % 2 != 0)
            {
                throw new ArgumentException("n is not a power of 2");
            }

            // compute FFT of even terms
            Complex[] even = new Complex[n / 2];
            for (int k = 0; k < n / 2; k++)
            {
                even[k] = x[2 * k];
            }
            Complex[] evenFFT = fft(even);

            // compute FFT of odd terms
            Complex[] odd = even;  // reuse the array (to avoid n log n space)
            for (int k = 0; k < n / 2; k++)
            {
                odd[k] = x[2 * k + 1];
            }
            Complex[] oddFFT = fft(odd);

            // combine
            Complex[] y = new Complex[n];
            for (int k = 0; k < n / 2; k++)
            {
                double kth = -2 * k * Math.PI / n;
                Complex wk = new Complex(Math.Cos(kth), Math.Sin(kth));
                //y[k] = evenFFT[k].plus(wk.times(oddFFT[k]));
                y[k] = evenFFT[k] + Times(wk, oddFFT[k]);
                //y[k + n / 2] = evenFFT[k].minus(wk.times(oddFFT[k]));
                y[k + n / 2] = evenFFT[k] - Times(wk, oddFFT[k]);
            }
            return y;
        }


        // compute the inverse FFT of x[], assuming its length n is a power of 2
        public static Complex[] ifft(Complex[] x)
        {
            int n = x.Length;
            Complex[] y = new Complex[n];

            // take conjugate
            for (int i = 0; i < n; i++)
            {
                y[i] = Complex.Conjugate(x[i]);
            }

            // compute forward FFT
            y = fft(y);

            // take conjugate again
            for (int i = 0; i < n; i++)
            {
                y[i] = Complex.Conjugate(y[i]);
            }

            // divide by n
            for (int i = 0; i < n; i++)
            {
                y[i] = new Complex(y[i].Real * (1.0 / n), y[i].Imaginary * (1.0 / n));
            }

            return y;

        }

        // compute the circular convolution of x and y
        public static Complex[] cconvolve(Complex[] x, Complex[] y)
        {

            // should probably pad x and y with 0s so that they have same length
            // and are powers of 2
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Dimensions don't agree");
            }

            int n = x.Length;

            // compute FFT of each sequence
            Complex[] a = fft(x);
            Complex[] b = fft(y);

            // point-wise multiply
            Complex[] c = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                //c[i] = a[i].times(b[i]);
                c[i] = Times(a[i], b[i]);
            }

            // compute inverse FFT
            return ifft(c);
        }


        // compute the linear convolution of x and y
        public static Complex[] convolve(Complex[] x, Complex[] y)
        {
            Complex ZERO = new Complex(0, 0);

            Complex[] a = new Complex[2 * x.Length];
            for (int i = 0; i < x.Length; i++) a[i] = x[i];
            for (int i = x.Length; i < 2 * x.Length; i++) a[i] = ZERO;

            Complex[] b = new Complex[2 * y.Length];
            for (int i = 0; i < y.Length; i++) b[i] = y[i];
            for (int i = y.Length; i < 2 * y.Length; i++) b[i] = ZERO;

            return cconvolve(a, b);
        }

        public static Complex Times(Complex a, Complex b)
        {
            double real = a.Real * b.Real - a.Imaginary * b.Imaginary;
            double imag = a.Real * b.Imaginary + a.Imaginary * b.Real;
            return new Complex(real, imag);
        }

        // compute the DFT of x[] via brute force (n^2 time)
        public static Complex[] dft(Complex[] x)
        {
            int n = x.Length;
            Complex ZERO = new Complex(0, 0);
            Complex[] y = new Complex[n];
            for (int k = 0; k < n; k++)
            {
                y[k] = ZERO;
                for (int j = 0; j < n; j++)
                {
                    int power = (k * j) % n;
                    double kth = -2 * power * Math.PI / n;
                    Complex wkj = new Complex(Math.Cos(kth), Math.Sin(kth));
                    //y[k] = y[k].plus(x[j].times(wkj));
                    y[k] = y[k] + Times(x[j], wkj);
                }
            }
            return y;
        }
    }
}