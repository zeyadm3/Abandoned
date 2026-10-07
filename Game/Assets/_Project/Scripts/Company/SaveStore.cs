using System;
using System.IO;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Reads and writes the company file on the host's machine (Application.persistentDataPath, fixed
    /// by company name + bundle id). Writes are atomic (temp file + replace) so a crash mid-save never
    /// leaves half a company; an unreadable file is kept as .corrupt and a new company starts.
    /// </summary>
    public sealed class SaveStore
    {
        public const string FileName = "company.json";

        private readonly string path;

        public SaveStore(string folder = null)
        {
            path = System.IO.Path.Combine(folder ?? Application.persistentDataPath, FileName);
        }

        public string Path => path;
        public bool Exists => File.Exists(path);

        public CompanySave Load()
        {
            if (!File.Exists(path)) return CompanySave.New();
            try
            {
                var save = JsonUtility.FromJson<CompanySave>(File.ReadAllText(path));
                if (save == null || save.version <= 0 || save.version > CompanySave.CurrentVersion) throw new InvalidDataException($"version {save?.version}");
                if (save.level < 1) save.level = 1;
                return save;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] {path} is unreadable ({e.Message}); keeping it as .corrupt and starting a new company.");
                try { File.Copy(path, path + ".corrupt", true); } catch { /* best effort */ }
                return CompanySave.New();
            }
        }

        public void Save(CompanySave save)
        {
            string folder = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(save, true));
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }

        public void Delete()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
