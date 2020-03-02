using System;
using System.Runtime.InteropServices;

namespace Encode
{
    [StructLayout(LayoutKind.Sequential)]
    // Структура, описывающая заголовок WAV файла.
    internal class WavHeader
    {
        // WAV-формат начинается с RIFF-заголовка:

        // Содержит символы "RIFF" в ASCII кодировке
        // (0x52494646 в big-endian представлении)
        public UInt32 ChunkId;

        // 36 + subchunk2Size, или более точно:
        // 4 + (8 + subchunk1Size) + (8 + subchunk2Size)
        // Это оставшийся размер цепочки, начиная с этой позиции.
        // Иначе говоря, это размер файла - 8, то есть,
        // исключены поля chunkId и chunkSize.
        public UInt32 ChunkSize;

        // Содержит символы "WAVE"
        // (0x57415645 в big-endian представлении)
        public UInt32 Format;

        // Формат "WAVE" состоит из двух подцепочек: "fmt " и "data":
        // Подцепочка "fmt " описывает формат звуковых данных:

        // Содержит символы "fmt "
        // (0x666d7420 в big-endian представлении)
        public UInt32 Subchunk1Id;

        // 16 для формата PCM.
        // Это оставшийся размер подцепочки, начиная с этой позиции.
        public UInt32 Subchunk1Size;

        // Аудио формат, полный список можно получить здесь http://audiocoding.ru/wav_formats.txt
        // Для PCM = 1 (то есть, Линейное квантование).
        // Значения, отличающиеся от 1, обозначают некоторый формат сжатия.
        public UInt16 AudioFormat;

        // Количество каналов. Моно = 1, Стерео = 2 и т.д.
        public UInt16 NumChannels;

        // Частота дискретизации. 8000 Гц, 44100 Гц и т.д.
        public UInt32 SampleRate;

        // sampleRate * numChannels * bitsPerSample/8
        public UInt32 ByteRate;

        // numChannels * bitsPerSample/8
        // Количество байт для одного сэмпла, включая все каналы.
        public UInt16 BlockAlign;

        // Так называемая "глубиная" или точность звучания. 8 бит, 16 бит и т.д.
        public UInt16 BitsPerSample;

        // Подцепочка "data" содержит аудио-данные и их размер.

        // Содержит символы "data"
        // (0x64617461 в big-endian представлении)
        public UInt32 Subchunk2Id;

        // numSamples * numChannels * bitsPerSample/8
        // Количество байт в области данных.
        public UInt32 Subchunk2Size;

        // Далее следуют непосредственно Wav данные.
    }
}
