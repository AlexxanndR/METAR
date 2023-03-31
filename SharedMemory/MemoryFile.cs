using System;
using System.IO.Enumeration;
using System.IO.MemoryMappedFiles;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace SharedMemory
{
    public class MemoryFile
    {
        public const int MMF_MAX_SIZE = 1024;
        public const int MMF_VIEW_SIZE = 1024;

        private readonly string fileName;
        private MemoryMappedFile mmf; 

        public MemoryFile(string fileName)
        {
            this.fileName = fileName;
        }

        public void CreateNonPersisted()
        {
            mmf = MemoryMappedFile.CreateNew(fileName, MMF_MAX_SIZE);

            using (MemoryMappedViewStream stream = mmf.CreateViewStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(false);
            }
        }

        public void OpenNonPersisted()
        {
            mmf = MemoryMappedFile.OpenExisting(fileName);
        }

        public async Task WriteDataAsync(object data)
        {
            await Task.Run(() =>
            {
                using (MemoryMappedViewStream stream = mmf.CreateViewStream(0, MMF_VIEW_SIZE))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        if (reader.ReadBoolean() == false)                     //если флаг занятости файла не установлен
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            writer.Write(true);                                //устанавливаем флаг занятости файла

                            JsonSerializer.Serialize(stream, data);

                            writer.Write('\0');

                            stream.Seek(0, SeekOrigin.Begin);
                            writer.Write(false);                               //снимаем флаг занятости
                        }
                    }
                }
            });
        }

        public async Task<string?> ReadDataAsync()
        {
            string? data = null;

            await Task.Run(() =>
            {
                using (MemoryMappedViewStream stream = mmf.CreateViewStream(0, MMF_VIEW_SIZE))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        bool isMemoryBusy = reader.ReadBoolean();

                        if (isMemoryBusy == false)                                   //если флаг занятости файла не установлен
                        {
                            Thread.Sleep(100);                                       //ждем некоторое время

                            stream.Seek(0, SeekOrigin.Begin);

                            if ((isMemoryBusy = reader.ReadBoolean()) == true)      //если флаг занятости был установлен, выходим из функции
                                throw new Exception("Memory mapped file is busy.");

                            stream.Seek(0, SeekOrigin.Begin);
                            writer.Write(true);                                      //устанавливаем флаг занятости файла

                            char[] buffer = new char[128];
                            StringBuilder builder = new StringBuilder();

                            while (true)
                            {
                                buffer = reader.ReadChars(128);
                                builder.Append(buffer);

                                if (buffer[buffer.Length - 1] == '\0')
                                    break;
                            }

                            data = builder.ToString();

                            stream.Seek(0, SeekOrigin.Begin);
                            writer.Write(false);                                     //снимаем флаг занятости
                        }
                    }
                }
            });

            return data?.Substring(0, data.IndexOf('\0'));
        }

        ~MemoryFile()
        {
            if (mmf != null)
                mmf.Dispose();
        }
    }
}