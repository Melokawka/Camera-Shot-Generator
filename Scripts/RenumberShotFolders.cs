using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class RenumberShotFolders : MonoBehaviour
{
    public List<string> sourceFolders;
    public string targetFolder;
    public string folderPatternPrefix;
    void Start()
    {
        MergeAndRenumber(sourceFolders, targetFolder, folderPatternPrefix);
    }

    //[ContextMenu("Renumber Shot Folders")]
    public static async Task Renumber(string basePath, ShotTypes shotType)
    {
        await Task.Run(() =>
        {
            string[] environmentDirs = Directory.GetDirectories(basePath);

            foreach (string environmentDir in environmentDirs)
            {
                string environmentName = Path.GetFileName(environmentDir);
                Debug.LogWarning("Sprawdzam " + environmentName);

                string pattern = $@"^{shotType}Camera_(\d+)$";
                Regex regex = new Regex(pattern);

                var shotDirs = Directory.GetDirectories(environmentDir, $"{shotType}Camera_*", SearchOption.TopDirectoryOnly)
                    .Select(path => new DirectoryInfo(path))
                    .Where(dir => regex.IsMatch(dir.Name))
                    .OrderBy(dir =>
                    {
                        Match match = regex.Match(dir.Name);
                        return int.Parse(match.Groups[1].Value);
                    })
                    .ToList();

                // Krok 1: Zmień tymczasowo nazwy folderów
                for (int i = 0; i < shotDirs.Count; i++)
                {
                    string tempName = Path.Combine(environmentDir, $"TMP_{System.Guid.NewGuid()}");
                    Directory.Move(shotDirs[i].FullName, tempName);
                    shotDirs[i] = new DirectoryInfo(tempName); // zaktualizuj ścieżkę
                }

                // Krok 2: Przypisz finalne numery
                for (int i = 0; i < shotDirs.Count; i++)
                {
                    string newName = $"{shotType}Camera_{i}";
                    string newFullPath = Path.Combine(environmentDir, newName);
                    Directory.Move(shotDirs[i].FullName, newFullPath);
                    Debug.Log($"[{environmentName}] Zmieniono: {shotDirs[i].Name} → {newName}");
                }
            }
        });
    }

    public void MergeAndRenumber(List<string> sourceFolders, string targetFolder, string folderPatternPrefix)
    {
        // Stwórz folder docelowy jeśli nie istnieje
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        Regex regex = new Regex($@"^{Regex.Escape(folderPatternPrefix)}(\d+)$");

        List<DirectoryInfo> allMatchingFolders = new List<DirectoryInfo>();

        // Krok 1: Znajdź wszystkie foldery pasujące do wzorca wewnątrz folderów źródłowych
        foreach (string sourceFolder in sourceFolders)
        {
            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogWarning($"Folder nie istnieje: {sourceFolder}");
                continue;
            }

            var matchingDirs = Directory.GetDirectories(sourceFolder, $"{folderPatternPrefix}*", SearchOption.AllDirectories)
                .Select(path => new DirectoryInfo(path))
                .Where(dir => regex.IsMatch(dir.Name))
                .ToList();

            allMatchingFolders.AddRange(matchingDirs);
        }

        // Krok 2: Posortuj po numerze
        var sortedDirs = allMatchingFolders
            .OrderBy(dir =>
            {
                Match match = regex.Match(dir.Name);
                return int.Parse(match.Groups[1].Value);
            })
            .ToList();

        // Krok 3: Przenieś i ponumeruj
        for (int i = 0; i < sortedDirs.Count; i++)
        {
            string newFolderName = $"{folderPatternPrefix}{i}";
            string destinationPath = Path.Combine(targetFolder, newFolderName);

            if (Directory.Exists(destinationPath))
            {
                Debug.LogWarning($"Folder już istnieje: {destinationPath}, pomijam...");
                continue;
            }

            Directory.Move(sortedDirs[i].FullName, destinationPath);
            Debug.Log($"Przeniesiono: {sortedDirs[i].FullName} → {destinationPath}");
        }
    }
}