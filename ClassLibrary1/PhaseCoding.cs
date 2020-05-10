using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using Yeti.MMedia;
using System;
using System.Numerics;
using WaveLib;

namespace PhaseCoding
{
    public class PhaseCoding
    {
        public static string Phase_Coding(string messagePath, string keyPath, string sourcePath, string destinationPath)
        {
            string mes = "";


            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var keyStream = new FileStream(keyPath, FileMode.Open))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
            using (var reader = new WaveFileReader(sourceStream))
            {
                NAudio.Wave.WaveFormat format = reader.WaveFormat;
                WaveWriter ww = new WaveWriter(destinationStream, new WaveLib.WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels));
                float[] f = new float[0];
                byte message, bit;
                int messageBuffer; //receives the next byte of the message or -1

                byte[] file = File.ReadAllBytes(messagePath);

                byte[] length = Encode.LSB.intToBytes(file.Length);

                //вставить длину в начало!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (MemoryStream messageStream = new MemoryStream(Encode.LSB.Connect(length, file)))
                {

                    while ((messageBuffer = messageStream.ReadByte()) >= 0 && f != null)
                    {
                        //read one byte of the message stream
                        message = (byte)messageBuffer;
                        //for each bit in [message]
                        for (int bitIndex = 0; bitIndex < 8 && (f = reader.ReadNextSampleFrame()) != null; bitIndex++)
                        {
                            //get the next bit from the current message byte...
                            bit = (byte)(((message & (byte)(1 << bitIndex)) > 0) ? 1 : 0);
                            mes += f.Length + Environment.NewLine;
                            //float[] S = FFT.FFT.FFT1(f, (ulong)f.Length, -1);
                            //float[] phi = new float[S.Length];
                            //float[] A = new float[S.Length];
                            for (int i = 0; i <8; i++)
                            {
                                //A[i] = Math.Abs(S[i]);
                                //phi[i] = 
                            }
                            

                        }
                    }
                }
                return mes;
            }
        }
        /// <summary>
        /// Преобразует поток с wav файлом в mp3 
        /// </summary>
        /// <param name="wavFile"></param>
        public static void WavToMP3(Stream wavFile, Stream Mp3file)
        {
            using (Stream source = wavFile)
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







/*
 * public static void WavToMP3(Stream wavFile, Stream Mp3file)
        {
            using (Stream source = wavFile)
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
 */
