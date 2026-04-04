using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;

public class ShotViewer : MonoBehaviour
{
    public string basePath = "CapturedFrames";  // Ustaw katalog z nagraniami
    public List<ShotTypes> shotTypes;  // Typ ujęcia np. ArcLeft, Static
    public List<string> environmentNames;

    private List<List<Texture2D>> framesPerFolder = new List<List<Texture2D>>();

    private List<string> folderPaths = new List<string>();

    private Vector2 scrollPos;
    public bool isLoaded = true;

    private List<string> environmentPerFolder = new List<string>();

    public void Start()
    {
        //LoadRandomFramesFromShots();
    }

    public void LoadRandomFramesFromShots()
    {
        string[] folders = {};

        foreach (string environmentName in environmentNames) {
            string environmentPath = Path.Combine(basePath, environmentName);

            foreach (ShotTypes shotType in shotTypes)
            {
                string searchPattern = $"{shotType}Camera_*";
                string[] unsortedFolders = Directory.GetDirectories(environmentPath, searchPattern, SearchOption.AllDirectories);

                var sortedFolders = unsortedFolders.OrderBy(path =>
                {
                    string folderName = Path.GetFileName(path);
                    string numberPart = new string(folderName.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
                    return int.TryParse(numberPart, out int val) ? val : int.MaxValue;
                });
                folders = folders.Concat(sortedFolders).ToArray();
            }
        }

        foreach (string folder in folders)
        {
            string[] frameFiles = Directory.GetFiles(folder, "frame_*.jpg");
            Debug.Log(frameFiles.Length);

            if (frameFiles.Length < 4) continue;

            int maxFrame = frameFiles
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Select(name =>
                {
                    string number = name.Substring("frame_".Length);
                    return int.TryParse(number, out int result) ? result : -1;
                })
                .Where(n => n >= 0)
                .Max();

            if (maxFrame < 3) continue;

            List<int> indices = new List<int>
            {
                0,
                Mathf.RoundToInt(maxFrame / 3f),
                Mathf.RoundToInt(2 * maxFrame / 3f),
                maxFrame
            };

            Debug.Log(folder);
            List<Texture2D> textures = indices
                .Select(index => Path.Combine(folder, $"frame_{index:D4}.jpg"))
                .Where(File.Exists)
                .Select(LoadTexture)
                .ToList();
            Debug.Log(textures.Count);

            if (textures.Count == 4)
            {
                framesPerFolder.Add(textures);
                Debug.Log("ok");
                folderPaths.Add(folder);

                string matchedEnvironment = environmentNames.FirstOrDefault(env => folder.Contains(Path.Combine(basePath, env)));
                environmentPerFolder.Add(matchedEnvironment);
            }
        }
    }

    Texture2D LoadTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

    void OnGUI()
    {
        if (!isLoaded) return;

        GUILayout.Space(5);
        GUILayout.Label($"Environments: {System.String.Join(", ", environmentNames)}");
        GUILayout.Space(5);
        GUILayout.Label($"Shots of type: {System.String.Join(", ", shotTypes)}");
        GUILayout.Space(5);

        if (GUILayout.Button("Finish and renumber", GUILayout.Width(150)))
        {
            foreach (ShotTypes shotType in shotTypes) 
                RenumberShotFolders.Renumber(basePath, shotType);
            EditorApplication.ExitPlaymode();
            return;
        }

        GUILayout.Space(5);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        string currentEnvironment = null;
        for (int i = 0; i < framesPerFolder.Count; i++)
        {
            string environment = environmentPerFolder[i];

            // Wyświetl nazwę środowiska tylko gdy się zmienia
            if (currentEnvironment != environment)
            {
                currentEnvironment = environment;
                GUILayout.Space(10);
                GUILayout.Label($"=== ENVIRONMENT: {currentEnvironment} ===", EditorStyles.boldLabel);
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileName(folderPaths[i]), GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete", GUILayout.Width(70)))
            {
                DeleteShotAtIndex(i);
                break;
            }
            GUILayout.EndHorizontal();

            var frames = framesPerFolder[i];
            GUILayout.BeginHorizontal();
            GUILayout.Box(frames[0], GUILayout.Width(256), GUILayout.Height(256));
            GUILayout.Box(frames[1], GUILayout.Width(256), GUILayout.Height(256));
            GUILayout.Box(frames[2], GUILayout.Width(256), GUILayout.Height(256));
            GUILayout.Box(frames[3], GUILayout.Width(256), GUILayout.Height(256));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndScrollView();
    }

    void DeleteShotAtIndex(int index)
    {
        if (index < 0 || index >= folderPaths.Count) return;

        string folder = folderPaths[index];
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);

        framesPerFolder.RemoveAt(index);
        folderPaths.RemoveAt(index);
    }
}
