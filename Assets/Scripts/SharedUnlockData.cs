using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SharedUnlockData
{
    public int version = 1;
    public bool unlockCharge;
    public bool unlockTank;
    public bool unlockJump;
    public long updatedAtUtc;
}

public static class SharedUnlockIO
{
    const string FolderName = "RUN-Shared";
    const string FileName = "unlock_data.json";

    public static SharedUnlockData Load()
    {
        string path = GetSharedPath();
        if (!File.Exists(path))
        {
            return new SharedUnlockData();
        }

        string json = File.ReadAllText(path);
        SharedUnlockData data = JsonUtility.FromJson<SharedUnlockData>(json);
        return data ?? new SharedUnlockData();
    }

    public static void Save(SharedUnlockData data)
    {
        if (data == null)
        {
            data = new SharedUnlockData();
        }

        data.updatedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string json = JsonUtility.ToJson(data, true);
        string path = GetSharedPath();
        string folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(path, json);
    }

    public static bool IsCharacterUnlocked(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        if (characterId == "athletic_zombie")
        {
            return true;
        }

        SharedUnlockData data = Load();
        switch (characterId)
        {
            case "charge_zombie":
                return data.unlockCharge;
            case "tank_zombie":
                return data.unlockTank;
            case "jump_zombie":
                return data.unlockJump;
            default:
                return true;
        }
    }

    public static bool LaunchPuzzle(string executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            return false;
        }

        string resolvedPath = executablePath;
        if (!Path.IsPathRooted(resolvedPath))
        {
            string runtimeRoot = GetRuntimeRoot();
            if (!string.IsNullOrEmpty(runtimeRoot))
            {
                resolvedPath = Path.GetFullPath(Path.Combine(runtimeRoot, resolvedPath));
            }
        }

        if (!File.Exists(resolvedPath))
        {
            string fallback = GuessDefaultPuzzlePath();
            if (!string.IsNullOrEmpty(fallback) && File.Exists(fallback))
            {
                resolvedPath = fallback;
            }
            else
            {
                Debug.LogWarning($"Puzzle executable not found: {resolvedPath}");
                return false;
            }
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = resolvedPath,
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to launch puzzle: {ex.Message}");
            return false;
        }
    }

    public static string GetSharedPath()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, FolderName, FileName);
    }

    public static string GuessDefaultPuzzlePath()
    {
        try
        {
            string runtimeRoot = GetRuntimeRoot();
            if (string.IsNullOrEmpty(runtimeRoot))
            {
                return string.Empty;
            }

            string[] candidates = new[]
            {
                Path.Combine(runtimeRoot, "JIEMI.exe"),
                Path.Combine(runtimeRoot, "..", "JIEMI", "JIEMI.exe"),
                Path.Combine(runtimeRoot, "..", "JIEMI", "build", "JIEMI.exe")
            };

            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    return full;
                }
            }

            return Path.GetFullPath(Path.Combine(runtimeRoot, "..", "JIEMI", "JIEMI.exe"));
        }
        catch
        {
            return string.Empty;
        }
    }

    static string GetRuntimeRoot()
    {
        try
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }
        catch
        {
            return string.Empty;
        }
    }
}
