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
                        writer.Write(true);                                    //устанавливаем флаг записи в файл 

                        bool isBusy = reader.ReadBoolean();                    //читаем флаг занятости файла (контрольный флаг)

                        if (isBusy == false)                                   //если флаг занятости файла не установлен
                        {
                            stream.Seek(1, SeekOrigin.Begin);
                            writer.Write(true);                                //устанавливаем флаг занятости файла

                            JsonSerializer.Serialize(stream, data);

                            writer.Write('\0');                                //разделяем новые и старые данные, если
                                                                               //новые не полностью перезаписали старые 
                            stream.Seek(0, SeekOrigin.Begin);
                            writer.Write(false);                               //снимаем флаг записи
                            writer.Write(false);                               //снимаем контрольный флаг
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
                        bool isWrite = reader.ReadBoolean();
                        bool isBusy = reader.ReadBoolean();

                        if (isBusy == false && isWrite == false)                         //если флаги не установлены
                        {
                            stream.Seek(1, SeekOrigin.Begin);
                            writer.Write(true);                                          //устанавливаем флаг занятости файла         

                            char[] buffer = new char[128];
                            StringBuilder builder = new StringBuilder();

                            while (true)
                            {
                                buffer = reader.ReadChars(buffer.Length);

                                if (buffer[0] == '\0')
                                    break;

                                builder.Append(buffer);
                            }

                            data = builder.ToString();

                            stream.Seek(1, SeekOrigin.Begin);
                            writer.Write(false);                                       //снимаем контрольный флаг
                        }
                    }
                }
            });

            return String.IsNullOrWhiteSpace(data) == false ? data.Substring(0, data.IndexOf('\0')) : null;
        }

        ~MemoryFile()
        {
            if (mmf != null)
                mmf.Dispose();
        }
    }
}