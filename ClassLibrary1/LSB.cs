using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;
using NAudio.Wave;
using System.Collections.Generic;
using System.Text;
using NAudio.Lame;


namespace Encode
{
    public class LSB
    {
        

        static void WriteWavHeader(Stream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            stream.Position = 0;

            // RIFF header.
            // Chunk ID.
            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

            // Chunk size.
            stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);

            // Format.
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);



            // Sub-chunk 1.
            // Sub-chunk 1 ID.
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

            // Sub-chunk 1 size.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // Audio format (floating point (3) or PCM (1)). Any other format indicates compression.
            stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

            // Channels.
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

            // Sample rate.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // Bytes rate.
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

            // Block align.
            stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

            // Bits per sample.
            stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);



            // Sub-chunk 2.
            // Sub-chunk 2 ID.
            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

            // Sub-chunk 2 size.
            stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
        }

        public static void ConvertWavToMp3(Stream wavFile)
        {

            using (var retMs = new FileStream("1.mp3",FileMode.Create))
            using (var rdr = new WaveFileReader(wavFile))
            using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                wtr.Flush();
                retMs.Flush();
            }


        }

        public static void WavToMP3(Stream wavFile)
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

                // Do conversion
                using (FileStream output = new FileStream("1.mp3",FileMode.Create))
                {
                    Yeti.MMedia.Mp3.Mp3Writer mp3 = new Yeti.MMedia.Mp3.Mp3Writer(output, fmt, conf);

                    int readCount;
                    while ((readCount = rdr.Read(buffer, 0, blen)) > 0)
                        mp3.Write(buffer, 0, readCount);

                    mp3.Close();
                    output.Flush();
                }
            }
        }


        static public void Hide(Stream messageStream, Stream keyStream, Stream sourceStream, Stream destinationStream)
        {
            var reader = new WaveFileReader(sourceStream);
            WaveFormat format = reader.WaveFormat;

            WriteWavHeader(destinationStream, false, (ushort)format.Channels, (ushort)format.BitsPerSample, format.SampleRate, (int)(reader.SampleCount));

            var waveBuffer = new byte[format.BitsPerSample/8];
            byte message, bit, waveByte;
            int messageBuffer; //receives the next byte of the message or -1
            int keyByte; //distance of the next carrier sample

            while ((messageBuffer = messageStream.ReadByte()) >= 0)
            {
                
                //read one byte of the message stream
                message = (byte)messageBuffer;
                //for each bit in [message]
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {

                    //read a byte from the key

                    keyByte = keyStream.ReadByte();


                    //skip a couple of samples

                    for (int n = 0; n < keyByte - 1; n++)
                    {
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                        destinationStream.Write(waveBuffer, 0, waveBuffer.Length);
                    }



                    //read one sample from the wave stream
                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length);


                    waveByte = waveBuffer[format.BitsPerSample/8 - 1];


                    //get the next bit from the current message byte...
                    bit = (byte)(((message & (byte)(1 << bitIndex)) > 0) ? 1 : 0);

                    //...place it in the last bit of the sample
                    if ((bit == 1) && ((waveByte % 2) == 0))
                    {
                        waveByte += 1;
                    }
                    else if ((bit == 0) && ((waveByte % 2) == 1))
                    {
                        waveByte -= 1;
                    }
                    waveBuffer[format.BitsPerSample/8 - 1] = waveByte;


                    //write the result to destinationStream
                    destinationStream.Write(waveBuffer, 0, waveBuffer.Length);
                }

            }
            sourceStream.CopyTo(destinationStream);
            destinationStream.Seek(0, SeekOrigin.Begin);

            WavToMP3(destinationStream);
        }

        // source stream- поток из исходого файла 
        // detination stream - куда записываеи
        // bytespersample - 

        /// <summary>
        /// Считывает из потока сообщение 
        /// </summary>
        /// <param name="messageStream"></param>
        /// <param name="keyStream"></param>
        /// <param name="sourceStream"></param>
        public static void Extract(Stream messageStream, Stream keyStream, Stream sourceStream)
        {
            WaveFormat format = (new WaveFileReader(sourceStream)).WaveFormat;

            byte[] waveBuffer = new byte[format.BitsPerSample/8];
            byte message, bit, waveByte;
            int messageLength = 12; //expected length of the message
            int keyByte; //distance of the next carrier sample


            //while ((messageLength == 0 || messageStream.Length < messageLength))
            while ((messageStream.Length < messageLength))
            {
                //clear the message-byte
                message = 0;

                //for each bit in [message]
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {

                    //read a byte from the key
                    keyByte = keyStream.ReadByte();

                    //skip a couple of samples
                    for (int n = 0; n < keyByte-1; n++)
                    {
                        //read one sample from the wave stream
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);

                    }

                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    waveByte = waveBuffer[format.BitsPerSample/8 - 1];


                    //get the last bit of the sample...
                    bit = (byte)(((waveByte % 2) == 0) ? 0 : 1);

                    //...write it into the message-byte
                    message += (byte)(bit << bitIndex);

                }
                //add the re-constructed byte to the message
                messageStream.WriteByte(message);



                if (messageLength == 0 && messageStream.Length == 4)
                {
                    //first 4 bytes contain the message's length
                    //...
                }
            }
            messageStream.Flush();
        }
    }
}
