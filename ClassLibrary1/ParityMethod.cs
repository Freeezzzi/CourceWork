using System;
using System.IO;
using NAudio.Wave;
// Вроде нижняя библа не нужна 
namespace Encode
{
    public static class ParityMethod
    {
        public static int CountSum(byte[] bytes)
        {
            byte[] copy = new byte[2];
            bytes.CopyTo(copy, 0);
            int sum = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sum += copy[i] % 2;
                    copy[i] = (byte)(copy[i] / 2);
                }
            }
            return sum;
        }
        public static void Hide(string messagePath, string sourcePath, string destinationPath)
        {
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
            using (var reader = new WaveFileReader(sourceStream))
            {
                WaveFormat format = reader.WaveFormat;

                Encode.LSB.WriteWavHeader(destinationStream, false, (ushort)format.Channels, (ushort)format.BitsPerSample, format.SampleRate, (int)(reader.SampleCount));

                var waveBuffer = new byte[2];
                byte message, bit, waveByte;
                int messageBuffer; //receives the next byte of the message or -1

                byte[] file = File.ReadAllBytes(messagePath);

                byte[] length = Encode.LSB.intToBytes(file.Length);

                using (MemoryStream messageStream = new MemoryStream(Encode.LSB.Connect(length, file)))
                {

                    while ((messageBuffer = messageStream.ReadByte()) >= 0)
                    {

                        //read one byte of the message stream
                        message = (byte)messageBuffer;
                        //for each bit in [message]
                        for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                        {
                            //read one sample from the wave stream
                            sourceStream.Read(waveBuffer, 0, waveBuffer.Length);

                            waveByte = waveBuffer[1];

                            //get the next bit from the current message byte...
                            bit = (byte)(((message & (byte)(1 << bitIndex)) > 0) ? 1 : 0);

                            if (CountSum(waveBuffer) %2 != bit)
                            {
                                if ((waveByte % 2) == 0)
                                {
                                    waveByte += 1;
                                }
                                else if ((waveByte % 2) == 1)
                                {
                                    waveByte -= 1;
                                }
                            }
                            
                            
                            waveBuffer[1] = waveByte;
                            //write the result to destinationStream
                            destinationStream.Write(waveBuffer, 0, waveBuffer.Length);
                            //str += destinationStream.Position + Environment.NewLine;
                        }
                    }
                    sourceStream.CopyTo(destinationStream);
                    destinationStream.Seek(0, SeekOrigin.Begin);

                    destinationStream.Flush();
                }
            }
        }

        public static void Extract(string encodemessagePath, string sourcePath)
        {
            using (var messageStream = new FileStream(encodemessagePath, FileMode.Create))
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open))
            {
                WaveFormat format = (new WaveFileReader(sourceStream)).WaveFormat;

                sourceStream.Seek(44, SeekOrigin.Begin);

                byte[] waveBuffer = new byte[2];
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
                        sourceStream.Read(waveBuffer, 0, waveBuffer.Length);


                        waveByte = waveBuffer[1];

                        //get the last bit of the sample...
                        bit = (byte)(CountSum(waveBuffer) % 2 == 0 ? 0 : 1);

                        
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
        }
    }
}
