using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;

using System;
using System.Numerics;

namespace ClassLibrary1
{
    public class PhaseCoding
    {
        public static void Phase_Coding(string inputfile,string outputfile)
        {
            // Do conversion
            using (FileStream input = new FileStream(inputfile,FileMode.Open))
            using (FileStream output = new FileStream(outputfile, FileMode.Create))
            {
                WavToMP3(input, output);
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
