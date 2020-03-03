using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;
using NAudio.Wave;


namespace Encode
{
    public class LSB
    {
        /*
        static readonly string path = $"C:\\Users\\Alex Novoselov\\Desktop\\1.wav";
        //WaveFormat format;
        static WaveFileReader sourceStream = new WaveFileReader(path);
        public static WaveFormat format = sourceStream.WaveFormat;
        static WaveFileWriter destinationStream = new WaveFileWriter($"C:\\Users\\Alex Novoselov\\Desktop\\2.wav", format);
        */




























        static public void Hide(Stream messageStream, Stream keyStream, Stream sourceStream, Stream destinationStream1)
        {
            WaveFormat format = (new WaveFileReader(sourceStream)).WaveFormat;

            WaveFileWriter destinationStream = new WaveFileWriter(destinationStream1, format);


            var waveBuffer = new byte[format.BitsPerSample / 8];
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
                    /*
                    for (int n = 0; n < keyByte - 1; n++)
                    {
                        //copy one sample from the clean stream to the carrier stream

                        sourceStream.CopyTo(
                            destinationStream, waveBuffer.Length);
                    }
                    */



                    //read one sample from the wave stream
                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length - (waveBuffer.Length % format.BlockAlign));

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

                    //write the result to destinationStream
                    destinationStream.Write(waveBuffer, 0, format.BitsPerSample / 8);
                }
                destinationStream.Flush();
            }














        }

        // source stream- поток из исходого файла 
        // detination stream - куда записываеи
        // bytespersample - 
      

        public void Extract(Stream messageStream, Stream keyStream)
        {
            byte[] waveBuffer = new byte[1337 / 8];
            byte message, bit, waveByte;
            int messageLength = 55; //expected length of the message
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
                    /*
                    //skip a couple of samples
                    for (int n = 0; n < keyByte; n++)
                    {
                        //read one sample from the wave stream
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    }
                    */
                    waveByte = waveBuffer[1337 / 8 - 1];



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
