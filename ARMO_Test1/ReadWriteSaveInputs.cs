using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ARMO_Test1
{
    [Serializable]
    public class ReadWriteSaveInputs
    {
        public string DirectoryPath { get; private set; }
        public string Regex { get; private set; }
        [NonSerialized] private readonly string _pathToFile = @"input.dat";


        public ReadWriteSaveInputs()
        {
            var result = Deserialize();
            DirectoryPath = result.DirectoryPath;
            Regex = result.Regex;
        }

        /// <summary>
        /// Записывает последние указанные значения для пути к файлу и REGEX-выражение
        /// </summary>
        public void Serialize()
        {
            var formatter = new BinaryFormatter();
            using var fileStream = new FileStream(_pathToFile, FileMode.OpenOrCreate);
            formatter.Serialize(fileStream, this);
        }

        private ReadWriteSaveInputs Deserialize()
        {
            var formatter = new BinaryFormatter();
            using var fileStream = new FileStream(_pathToFile, FileMode.OpenOrCreate);
            var inputs = this;
            try
            {
                inputs = (ReadWriteSaveInputs) formatter.Deserialize(fileStream);
            }
            catch
            {
                fileStream.Close();
                //MessageBox.Show("Возникла проблема с чтением файла\n" + _pathToFile);
                File.Delete(_pathToFile);
            }

            return inputs;
        }

        /// <summary>
        /// Перезаписывает хранимые значения для указанного в поле директории и REGEX-выражения
        /// </summary>
        /// <param name="directoryPath">Путь к директории для поиска</param>
        /// <param name="regex">Указанное REGEX-выражение</param>
        public void Update(string directoryPath, string regex)
        {
            DirectoryPath = directoryPath;
            Regex = regex;
        }
    }
}
