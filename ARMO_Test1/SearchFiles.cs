using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARMO_Test1
{
    public class SearchFiles
    {
        /// <summary>
        /// Очередь полных путей к файлам, прошедшим REGEX проверку. 
        /// </summary>
        public static ConcurrentQueue<string> FilesQueue { get; private set; }

        /// <summary>
        /// Свойство для отображения текущей папки, в которой ведется поиск.
        /// </summary>
        public static string CurrentFolder { get; private set; }

        /// <summary>
        /// Свойство для отображения количества просканированных файлов
        /// </summary>
        public static long AllFiles { get; private set; }

        /// <summary>
        /// Свойство для отображения количества найденных файлов
        /// </summary>
        public static long FilesFound { get; set; }

        /// <summary>
        /// Поле хранения статуса отмененного или законченного поиска.
        /// </summary>
        public static bool SearchCancelledOrDone;

        /// <summary>
        /// Метод инициализация поиска
        /// </summary>
        /// <param name="regexPattern">Шаблон REGEX-выражения</param>
        /// <param name="entryDir">Входная папка</param>
        /// <returns></returns>
        public static Task SearchByRegex(string regexPattern, string entryDir)
        {
            FilesQueue = new ConcurrentQueue<string>();
            AllFiles = FilesFound = 0;
            CurrentFolder = "";
            CheckDirectory(entryDir);
            CheckRegex(regexPattern);
            return Task.Run(async () =>
            {
                //Первый раз - файлы в родительской папке, далее рекурсивно
                await FilesSearchTask(regexPattern, entryDir);
                await SubDirSearch(regexPattern, entryDir);
                SearchCancelledOrDone = true;
            });
        }


        private static Task SubDirSearch(string reg, string entryDir)
        {
            return Task.Run(async () =>
            {
                var listOfSubDirs = new List<string>();
                try
                {
                    listOfSubDirs = Directory.GetDirectories(entryDir)
                        .ToList();
                }
                catch(UnauthorizedAccessException e)
                {
                    // Пустой catch для игнорирования исключения обращения в папку,
                    // если к ней не имеется доступ.
                }

                foreach (var subDirPath in listOfSubDirs)
                {
                    CurrentFolder = subDirPath;

                    // https://stackoverflow.com/questions/27238232/how-can-i-cancel-task-whenall

                    await Task.WhenAny(Task.WhenAll(FilesSearchTask(reg, subDirPath), SubDirSearch(reg, subDirPath)),
                        Task.Delay(Timeout.Infinite, ActionForm.CancellationToken.Token));
                }
            }, ActionForm.CancellationToken.Token);
        }

        private static Task FilesSearchTask(string reg, string entryDir)
        {
            return Task.Run(() => FilesSearchAndQueue(reg, entryDir), ActionForm.CancellationToken.Token);
        }

        private static void FilesSearchAndQueue(string reg, string entryDir)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(entryDir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    AllFiles++;
                    ActionForm.PauseEvent.WaitOne();
                    var filename = Path.GetFileName(file);
                    if (!Regex.IsMatch(filename, reg)) continue;
                    FilesQueue.Enqueue(file);
                }
            }
            catch
            {
                // ignored
            }
        }

        private static void CheckRegex(string regexPattern)
        {
            try
            {
                var regex = new Regex(regexPattern);
            }
            catch
            {
                SearchCancelledOrDone = true;
                MessageBox.Show("Некорректное REGEX-выражение");
                ActionForm.CancellationToken.Cancel();
            }
        }

        private static void CheckDirectory(string dirPath)
        {

            if (Directory.Exists(dirPath)) return;
            SearchCancelledOrDone = true;
            MessageBox.Show($"Директория \n{dirPath} \nне найдена");
        }

    }
}