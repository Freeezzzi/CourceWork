using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using Yeti.MMedia;
using System.Numerics;
using WaveLib;
using System.Windows.Forms;

namespace PhaseCoding
{
    public static class PhaseCoding
    {
        public static string Hide(string messagePath, string key, string sourcePath, string destinationPath)
        {
            string mes = "";


            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(key)))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
            using (var reader = new WaveFileReader(sourceStream))
            {
                NAudio.Wave.WaveFormat format = reader.WaveFormat;
                Encode.LSB.WriteWavHeader(destinationStream, false, (ushort)format.Channels, (ushort)format.BitsPerSample, format.SampleRate, (int)(reader.SampleCount));
                byte[] data = new byte[4];
                byte message, bit;
                int messageBuffer; //receives the next byte of the message or -1

                reader.Seek(44, SeekOrigin.Begin);

                byte[] file = File.ReadAllBytes(messagePath);

                byte[] length = Encode.LSB.intToBytes(file.Length);

                using (MemoryStream messageStream = new MemoryStream(Encode.LSB.Connect(length, file)))
                {

                    while ((messageBuffer = messageStream.ReadByte()) >= 0 && data != null)
                    {
                        //read one byte of the message stream
                        message = (byte)messageBuffer;
                        //for each bit in [message]
                        for (int bitIndex = 0; bitIndex < 8 && (reader.Read(data, 0, data.Length) != -1); bitIndex++)
                        {
                            //mes += data[0] + " " + data[1] + " " + data[2] + " " + data[3] + " " + reader.Position + Environment.NewLine;
                            //get the next bit from the current message byte...
                            bit = (byte)(((message & (byte)(1 << bitIndex)) > 0) ? 1 : 0);
                            //2
                            Complex[] complexData = new Complex[data.Length];
                            for (int i = 0; i < complexData.Length; i++)
                            {
                                complexData[i] = new Complex(data[i], 0);
                            }

                            Complex[] fftdata = FFT.FFT.fft(complexData);
                            double[] A = new double[fftdata.Length];
                            double[] phi = new double[fftdata.Length];

                            for (int i = 0; i < fftdata.Length; i++)
                            {
                                A[i] = fftdata[i].Magnitude;
                                phi[i] = fftdata[i].Phase;
                            }
                            //mes += phi[0] + Environment.NewLine;
                            //3
                            double[] deltaphi = new double[phi.Length - 1];
                            for (int i = 0; i < deltaphi.Length; i++)
                            {
                                deltaphi[i] = phi[i + 1] - phi[i];
                            }


                            //4
                            double phidata = Math.PI / 2 - Math.PI * bit;


                            //5
                            phi[1] = phidata;
                            phi[2] = -phidata;

                            //6 
                            
                            for (int i = 1; i < phi.Length-1; i++)
                            {
                                phi[i] = phi[i - 1] + deltaphi[i]; 
                            }
                            //mes += phi[1] + Environment.NewLine;

                            //реконструируем сигнал
                            byte[] reconstructed = new byte[data.Length];
                            Complex[] ifft = new Complex[phi.Length];
                            for (int i = 0; i < phi.Length; i++)
                            {
                                ifft[i] = A[i] * (new Complex(Math.Cos(phi[i]), Math.Sin(phi[i])));
                            }
                            ifft = FFT.FFT.ifft(ifft);
                            for (int i = 0; i < reconstructed.Length; i++)
                            {
                                reconstructed[i] = (byte)ifft[i].Real;
                            }
                            mes += reconstructed[0] + " " + reconstructed[1] + " " + reconstructed[2] + " " + reconstructed[3] + " " + destinationStream.Position + Environment.NewLine;
                            destinationStream.Write(reconstructed, 0, 4);
                            
                        }
                    }
                    reader.CopyTo(destinationStream);
                    destinationStream.Seek(0, SeekOrigin.Begin);

                    destinationStream.Flush();
                }
                return mes;
            }
        }


        public static string Extract(string encodemessagePath, string key, string sourcePath)
        {
            string mes = "";

            using (var messageStream = new FileStream(encodemessagePath, FileMode.Create))
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(key)))
            using (var reader = new WaveFileReader(sourceStream))
            {
                //NAudio.Wave.WaveFormat format = (new WaveFileReader(sourceStream)).WaveFormat;

                reader.Seek(44, SeekOrigin.Begin);
                string str = "";

                byte[] data = new byte[4];
                byte message, bit, waveByte;
                int messageLength = 16; //expected length of the message
                int keyByte; //distance of the next carrier sample

                while ((messageLength == 0 || messageStream.Length < messageLength) && data != null)
                {
                    //clear the message-byte
                    message = 0;

                    //for each bit in [message]
                    for (int bitIndex = 0; bitIndex < 8 && (reader.Read(data, 0, data.Length) != -1); bitIndex++)
                    {
                        mes += data[0]+" " + data[1]+" " + data[2]+" " + data[3] +" "+reader.Position+ Environment.NewLine;
                        Complex[] complexData = new Complex[data.Length];
                        for (int i = 0; i < complexData.Length; i++)
                        {
                            complexData[i] = new Complex(data[i], 0);
                        }

                        Complex[] fftdata = FFT.FFT.fft(complexData);
                        double[] A = new double[fftdata.Length];
                        double[] phi = new double[fftdata.Length];

                        for (int i = 0; i < fftdata.Length; i++)
                        {
                            phi[i] = fftdata[i].Phase;
                        }

                        //mes += phi[1] + Environment.NewLine;
                        if (phi[1] > 0)
                        {
                            bit = 0;
                        }
                        else
                        {
                            bit = 1;
                        }


                        //...write it into the message-byte
                        message += (byte)(bit << bitIndex);
                    }
                    //add the re-constructed byte to the message
                    messageStream.WriteByte(message);

                    if (messageLength == 0 && messageStream.Length == 4)
                    {
                        //first 4 bytes contain the message's length
                        //...
                        byte[] bytes = new byte[4];
                        messageStream.Seek(0, SeekOrigin.Begin);
                        messageStream.Read(bytes, 0, 4);
                        messageLength = Encode.LSB.intFromBytes(bytes);
                        messageStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                messageStream.Flush();
            }
            return mes;
        }



        /// <summary>
        /// Преобразует поток с wav файлом в mp3 
        /// </summary>
        /// <param name="wavFile"></param>
        public static void WavToMP3(string wavPath, string Mp3path)
        {
            using (Stream source = new FileStream(wavPath, FileMode.Open))
            using (Stream Mp3file = new FileStream(Mp3path, FileMode.Create))
            using (NAudio.Wave.WaveFileReader rdr = new NAudio.Wave.WaveFileReader(source))
            {
                WaveLib.WaveFormat fmt = new WaveLib.WaveFormat(rdr.WaveFormat.SampleRate, rdr.WaveFormat.BitsPerSample, rdr.WaveFormat.Channels);

                // convert to MP3 at 96kbit/sec...
                Yeti.Lame.BE_CONFIG conf = new Yeti.Lame.BE_CONFIG(fmt, 96);

                // Allocate a 1-second buffer
                int blen = rdr.WaveFormat.AverageBytesPerSecond;
                byte[] buffer = new byte[blen];

                Yeti.MMedia.Mp3.Mp3Writer mp3 = new Yeti.MMedia.Mp3.Mp3Writer(Mp3file, fmt, conf);

                int readCount;
                while ((readCount = rdr.Read(buffer, 0, blen)) > 0)
                    mp3.Write(buffer, 0, readCount);

                Mp3file.Flush();
                mp3.Close();
            }
        }
    }
}

