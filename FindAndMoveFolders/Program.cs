using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FindAndMoveFolders
{
    class Program
    {
        static void Main()
        {
            try
            {
                var foldersToMove = GetFoldersHasSubname();
                if (foldersToMove.Count > 0)
                {
                    MoveFolders(foldersToMove);
                    Console.WriteLine($"Перемещено {foldersToMove.Count} папок.");
                }
                else
                {
                    Console.WriteLine("Папки с указанным подназванием не найдены.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        private static string GetMovePath()
        {
            Console.WriteLine("Укажите путь куда будут перемещены папки:");
            var path = Console.ReadLine()?.Trim();

            while (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                Console.WriteLine("Путь не существует или введен некорректно. Введите путь снова:");
                path = Console.ReadLine()?.Trim();
            }

            return path;
        }

        private static string[] GetFolders()
        {
            Console.WriteLine("Укажите путь к папкам:");
            var path = Console.ReadLine()?.Trim();

            while (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                Console.WriteLine("Путь не существует или введен некорректно. Введите путь снова:");
                path = Console.ReadLine()?.Trim();
            }

            return Directory.GetDirectories(path);
        }

        private static List<string> GetFoldersHasSubname()
        {
            Console.WriteLine("Укажите подстроку для поиска в именах файлов внутри папок:");
            var subname = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(subname))
            {
                Console.WriteLine("Подстрока не указана. Поиск не будет выполнен.");
                return new List<string>();
            }

            var folders = GetFolders();
            var foldersWithSubname = new List<string>();

            Console.WriteLine($"Начинается проверка {folders.Length} папок...");

            for (int i = 0; i < folders.Length; i++)
            {
                var percentage = (double)(i + 1) / folders.Length * 100;
                Console.Write($"\r{percentage:0.0}% - Проверено {i + 1} из {folders.Length} папок");

                var folder = folders[i];

                if (CheckFolderHasSubname(folder, subname))
                    foldersWithSubname.Add(folder);
            }

            Console.WriteLine("\r100% - Проверка завершена");
            return foldersWithSubname;
        }

        private static bool CheckFolderHasSubname(string path, string subname)
        {
            var directoriesToCheck = new Queue<string>();
            directoriesToCheck.Enqueue(path);

            while (directoriesToCheck.Count > 0)
            {
                var currentDir = directoriesToCheck.Dequeue();

                try
                {
                    // Проверяем файлы в текущей директории
                    var files = Directory.GetFiles(currentDir);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        if (fileName.IndexOf(subname, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }

                    // Добавляем поддиректории для проверки
                    var subDirs = Directory.GetDirectories(currentDir);
                    foreach (var dir in subDirs)
                    {
                        directoriesToCheck.Enqueue(dir);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Пропускаем директории без доступа
                    continue;
                }
                catch (Exception)
                {
                    // Пропускаем директории с ошибками доступа
                    continue;
                }
            }

            return false;
        }

        private static void MoveFolders(List<string> foldersToMove)
        {
            Console.WriteLine($"\nНайдено {foldersToMove.Count} папок для перемещения.");

            if (foldersToMove.Count == 0)
                return;

            var destinationPath = GetMovePath();
            var movedCount = 0;

            Console.WriteLine($"\nНачинается перемещение {foldersToMove.Count} папок...");

            for (int i = 0; i < foldersToMove.Count; i++)
            {
                try
                {
                    var folderPath = foldersToMove[i];
                    var folderName = Path.GetFileName(folderPath);
                    var destinationFolderPath = Path.Combine(destinationPath, folderName);

                    // Проверяем, существует ли уже папка в целевом расположении
                    if (Directory.Exists(destinationFolderPath))
                    {
                        Console.WriteLine($"\nПапка '{folderName}' уже существует в целевом расположении. Добавляем суффикс...");

                        int counter = 1;
                        string newFolderName;
                        do
                        {
                            newFolderName = $"{folderName}_{counter}";
                            destinationFolderPath = Path.Combine(destinationPath, newFolderName);
                            counter++;
                        } while (Directory.Exists(destinationFolderPath));

                        Console.WriteLine($"Переименовываем в '{newFolderName}'");
                    }

                    var percentage = (double)(i + 1) / foldersToMove.Count * 100;
                    Console.Write($"\r{percentage:0.0}% - Перемещение папки {i + 1} из {foldersToMove.Count}: {folderName}");

                    Directory.Move(folderPath, destinationFolderPath);
                    movedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nОшибка при перемещении папки: {ex.Message}");
                }
            }

            Console.WriteLine($"\r100% - Перемещение завершено");
            Console.WriteLine($"Успешно перемещено {movedCount} из {foldersToMove.Count} папок");
        }
    }
}