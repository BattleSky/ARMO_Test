using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ARMO_Test1
{
    public static class NodeOperations
    {
        /// <summary>
        /// Поле для хранения последней проверенной директории
        /// </summary>
        private static string _pathToDirectory = "";
        /// <summary>
        /// Изъятие из очереди на отрисовку пути к файлу
        /// </summary>
        /// <returns>Путь к файлу</returns>
        private static string FileDequeue()
        {
            if (SearchFiles.FilesQueue == null) return null;
            SearchFiles.FilesQueue.TryDequeue(out var pathToFile);
            return pathToFile;
        }

        /// <summary>
        /// Просмотр верхнего в очереди пути к файлу
        /// </summary>
        /// <returns>Путь к файлу</returns>
        private static string FilePeek()
        {
            if (SearchFiles.FilesQueue == null) return null;
            SearchFiles.FilesQueue.TryPeek(out var pathToFile);
            return pathToFile;
        }

        /// <summary>
        /// Добавление Node в дерево TreeView в зависимости от пути
        /// </summary>
        /// <param name="tree">Дерево для отрисовки</param>
        public static void AddNodeFromPath(TreeView tree)
        {
            var pathToFile = FilePeek();
            var dirPathSplit = GetPathToDirectoryOfFile(pathToFile);
            if (dirPathSplit == null) return;

            if (tree.Nodes.Count == 0)
                tree.Nodes.Add(dirPathSplit[0], dirPathSplit[0] + @":\");
            
            var baseNode = tree.Nodes[0];
            baseNode.Expand();

            // Добавляем родительские ноды
            for (var i = 1; i < dirPathSplit.Length; i++)
            {
                //TODO: Складывает некорректно папки, надо их поднимать наверх как-то
                if (baseNode.Nodes.ContainsKey(dirPathSplit[i]))
                    baseNode = baseNode.Nodes.Find(dirPathSplit[i], false)[0];
                else
                    baseNode = baseNode.Nodes.Add(dirPathSplit[i], dirPathSplit[i]);
            }

            _pathToDirectory = Path.GetDirectoryName(pathToFile);
            var nodesInFolder = GetRangeOfNodes();
            baseNode.Nodes.AddRange(nodesInFolder);
        }

        /// <summary>
        /// Проверка, изъятия из очереди значений пути к файлам и составление массива TreeNode из этих значений.
        /// </summary>
        /// <returns>Возвращает массив TreeNode для добавления AddRange</returns>
        private static TreeNode[] GetRangeOfNodes()
        {
            //Изменяя этот параметр можно регулировать скорость разгребания очереди
            const int capacity = 10;
            var nodes = new List<TreeNode>();
            for (var i = 0; i < capacity; i++)
            {
                var pathToFile = FilePeek();
                var currentDir = Path.GetDirectoryName(pathToFile);
                if (currentDir == _pathToDirectory)
                {
                    nodes.Add(new TreeNode(Path.GetFileName(FileDequeue())));
                    SearchFiles.FilesFound++;
                }
                else
                {
                    _pathToDirectory = currentDir;
                    break;
                }
            }
            return nodes.ToArray();
        }

        /// <summary>
        /// Разбитие пути к файлу на массив папок.
        /// </summary>
        /// <param name="pathToFile">Полный путь к файлу</param>
        /// <returns>Массив папок</returns>
        public static string[] GetPathToDirectoryOfFile(string pathToFile)
        {
            var pathToDirOfFile = Path.GetDirectoryName(pathToFile);
            if (pathToDirOfFile == null) return null;
            var splitPath = pathToDirOfFile.Split(new[] { @":\\", @"\", @":\" }, StringSplitOptions.RemoveEmptyEntries);
            return splitPath.Length == 0 ? null : splitPath;
        }
    }
}