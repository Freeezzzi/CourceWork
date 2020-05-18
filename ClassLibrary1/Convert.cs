using System.Diagnostics;
using System.IO;
using NAudio.Lame;
using NAudio.Wave;
using Yeti.Lame;

namespace PhaseCoding
{
    public class Convert
    {
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

        public static void Mp3ToWav(string Mp3Path,string WavPath)
        {
            using (Mp3FileReader reader = new Mp3FileReader(Mp3Path))
            {
                WaveFileWriter.CreateWaveFile(WavPath, reader);
            }
        }
    }
}

