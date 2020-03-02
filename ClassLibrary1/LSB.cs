using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;

namespace Encode
{
    public class LSB
    {
        // source stream- поток из исходого файла 
        // detination stream - куда записываеи
        // bytespersample - 
        public void Hide(Stream messageStream, Stream keyStream)
        {

            byte[] waveBuffer = new byte[bytesPerSample];
            byte message, bit, waveByte;
            int messageBuffer; //receives the next byte of the message or -1
            int keyByte; //distance of the next carrier sample

            //loop over the message, hide each byte
            while ((messageBuffer = messageStream.ReadByte()) >= 0)
            {
                //read one byte of the message stream
                message = (byte)messageBuffer;

                //for each bit in [message]
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {

                    //read a byte from the key
                    keyByte = GetKeyValue(keyStream);

                    //skip a couple of samples
                    for (int n = 0; n < keyByte - 1; n++)
                    {
                        //copy one sample from the clean stream to the carrier stream
                        sourceStream.Copy( 
                            waveBuffer, 0,
                            waveBuffer.Length, destinationStream); 
                    }

                    //read one sample from the wave stream
                    sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    waveByte = waveBuffer[bytesPerSample - 1];

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

                    waveBuffer[bytesPerSample - 1] = waveByte;

                    //write the result to destinationStream
                    destinationStream.Write(waveBuffer, 0, bytesPerSample);
                }
            }

            //copy the rest of the wave without changes
            //...
        }

        public void Extract(Stream messageStream, Stream keyStream)
        {

            byte[] waveBuffer = new byte[bytesPerSample];
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
                    keyByte = GetKeyValue(keyStream);

                    //skip a couple of samples
                    for (int n = 0; n < keyByte; n++)
                    {
                        //read one sample from the wave stream
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
                    }
                    waveByte = waveBuffer[bytesPerSample - 1];

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
        }
    }
}
