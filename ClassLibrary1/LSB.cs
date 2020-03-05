using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;
using NAudio.Wave;
using System.Collections.Generic;
using System.Text;
using NAudio.Lame;
using Yeti.Lame;

namespace Encode
{
    public class LSB
    {

        /// <summary>
        /// Записывает заоголовок в wav поток по данным
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="isFloatingPoint"></param>
        /// <param name="channelCount"></param>
        /// <param name="bitDepth"></param>
        /// <param name="sampleRate"></param>
        /// <param name="totalSampleCount"></param>
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

        /// <summary>
        /// Преобразует поток с wav файлом в mp3 
        /// </summary>
        /// <param name="wavFile"></param>
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
                using (FileStream output = new FileStream("1.mp3", FileMode.Create))
                {
                    Yeti.MMedia.Mp3.Mp3Writer mp3 = new Yeti.MMedia.Mp3.Mp3Writer(output, fmt, conf);

                    int readCount;
                    while ((readCount = rdr.Read(buffer, 0, blen)) > 0)
                        mp3.Write(buffer, 0, readCount);

                    output.Flush();
                    mp3.Close();
                }
            }
        }

        public static int intFromBytes(byte[] bytes)
        {
            int value = ((bytes[0] & 0xFF) << 24) + ((bytes[1] & 0xFF) << 16) + ((bytes[2] & 0xFF) << 8) + (bytes[3] & 0xFF);
            return value;
        }

        public static byte[] intToBytes(int value)
        {
            byte[] bytes = new byte[4];
            bytes[3] = (byte)value;
            bytes[2] = (byte)(value >> 8);
            bytes[1] = (byte)(value >> 16);
            bytes[0] = (byte)(value >> 24);
            return bytes;
        }

        public static byte[] Connect(byte[] list1,byte[] list2)
        {
            byte[] list = new byte[list1.Length + list2.Length];
            list1.CopyTo(list, 0);
            list2.CopyTo(list, list1.Length);
            return list;
        }

        static public string Hide(string messagePath, string keyPath, string sourcePath, string destinationPath)
        {
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var keyStream = new FileStream(keyPath, FileMode.Open))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
            using (var reader = new WaveFileReader(sourceStream))
            {
                WaveFormat format = reader.WaveFormat;

                WriteWavHeader(destinationStream, false, (ushort)format.Channels, (ushort)format.BitsPerSample, format.SampleRate, (int)(reader.SampleCount));

                var waveBuffer = new byte[format.BitsPerSample / 8];
                byte message, bit, waveByte;
                int messageBuffer; //receives the next byte of the message or -1
                int keyByte; //distance of the next carrier sample

                byte[] file = File.ReadAllBytes(messagePath);

                byte[] length = intToBytes(file.Length);

                //вставить длину в начало
                string str = "";
                using (MemoryStream messageStream = new MemoryStream(Connect(length, file)))
                {

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

                            Array.ForEach(waveBuffer, (t) => str += t+"+");
                            str += Environment.NewLine;

                            waveByte = waveBuffer[format.BitsPerSample / 8 - 1];


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
                            waveBuffer[format.BitsPerSample / 8 - 1] = waveByte;

                            Array.ForEach(waveBuffer, (t) => str += t+"-");
                            str += Environment.NewLine;
                            //write the result to destinationStream
                            destinationStream.Write(waveBuffer, 0, waveBuffer.Length);
                            //str += destinationStream.Position + Environment.NewLine;
                        }
                        str += Environment.NewLine;
                    }
                    sourceStream.CopyTo(destinationStream);
                    destinationStream.Seek(0, SeekOrigin.Begin);
                    destinationStream.Flush();


                    return str;
                    // Записываем в формате mp3 на диск
                    //WavToMP3(destinationStream);
                }
            }
        }


        public static string Extract(string encodemessagePath, string keyPath, string sourcePath)
        {
            using (var messageStream = new FileStream(encodemessagePath, FileMode.Create))
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var keyStream = new FileStream(keyPath, FileMode.Open))
            {
                /*
                using (var reader = new Mp3FileReader(sourcePath))
                {
                    WaveFileWriter.CreateWaveFile("converter.wav", reader);

                }
                */
                WaveFormat format = (new WaveFileReader(sourceStream)).WaveFormat;

                sourceStream.Seek(44, SeekOrigin.Begin);
                string str = "";

                byte[] waveBuffer = new byte[format.BitsPerSample / 8];
                byte message, bit, waveByte;
                int messageLength = 0; //expected length of the message
                int keyByte; //distance of the next carrier sample

                while ((messageLength == 0 || messageStream.Length < messageLength))
                {
                    //clear the message-byte
                    message = 0;

                    //for each bit in [message]
                    for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                    {

                        //read a byte from the key
                        keyByte = keyStream.ReadByte();

                        //skip a couple of samples
                        for (int n = 0; n < keyByte-1 ; n++)
                        {
                            //read one sample from the wave stream
                            sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                            
                        }

                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);


                        waveByte = waveBuffer[format.BitsPerSample / 8 - 1];

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
                        byte[] bytes = new byte[4];
                        messageStream.Seek(0, SeekOrigin.Begin);
                        messageStream.Read(bytes, 0, 4);
                        messageLength = intFromBytes(bytes);
                        messageStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                messageStream.Flush();
                return str;
            }
        }
    }
}
