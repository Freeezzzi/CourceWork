using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Encode
{


    public class Read
    {
        public static void ReadWav(string path)
        {
            WavHeader header = new WavHeader();
            // Размер заголовка
            int headerSize = Marshal.SizeOf(header);

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[headerSize];
            fileStream.Read(buffer, 0, headerSize);

            // Чтобы не считывать каждое значение заголовка по отдельности,
            // воспользуемся выделением unmanaged блока памяти
            var headerPtr = Marshal.AllocHGlobal(headerSize);
            // Копируем считанные байты из файла в выделенный блок памяти
            Marshal.Copy(buffer, 0, headerPtr, headerSize);
            // Преобразовываем указатель на блок памяти к нашей структуре
            Marshal.PtrToStructure(headerPtr, header);

            // Выводим полученные данные
            Console.WriteLine($"Sample rate: {0}", header.SampleRate);
            Console.WriteLine($"Channels: {0}", header.NumChannels);
            Console.WriteLine($"Bits per sample: {0}", header.BitsPerSample);

            // Посчитаем длительность воспроизведения в секундах
            double durationSeconds = 1.0 * header.Subchunk2Size / (header.BitsPerSample / 8.0) / header.NumChannels / header.SampleRate;
            int durationMinutes = (int)Math.Floor(durationSeconds / 60);
            durationSeconds = durationSeconds - (durationMinutes * 60);
            Console.WriteLine($"Duration: {0:00}:{1:00}", durationMinutes, durationSeconds);


            // Освобождаем выделенный блок памяти
            Marshal.FreeHGlobal(headerPtr);
        }
    }
}
