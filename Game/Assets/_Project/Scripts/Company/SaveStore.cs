using System;
using System.IO;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Reads and writes the company file on the host's machine (Application.persistentDataPath, fixed
    /// by company name + bundle id). Writes are atomic (temp file + replace) so a crash mid-save never
    /// leaves half a company; an unreadable file is kept as .corrupt and a new company starts. A file it
    /// can't safely replace (locked, newer build) is never overwritten.
    /// </summary>
    public sealed class SaveStore
    {
        public const string FileName = "company.json";
        /// <summary>The demo keeps its own company, so the full game never inherits a demo save (or the reverse).</summary>
        public const string DemoFileName = "company_demo.json";

        private readonly string path;

        public SaveStore(string folder = null)
        {
            path = System.IO.Path.Combine(folder ?? Application.persistentDataPath, Core.Demo.IsDemo ? DemoFileName : FileName);
        }

        public string Path => path;
        public bool Exists => File.Exists(path);

        /// <summary>
        /// False after a load that couldn't safely be replaced: the file was locked or unreadable for a
        /// reason other than bad contents, it came from a newer build, or its .corrupt backup failed.
        /// The company then lives in memory only, and the file on disk is left alone.
        /// </summary>
        public bool Writable { get; private set; } = true;

        public CompanySave Load()
        {
            Writable = true;
            if (!File.Exists(path)) return CompanySave.New();
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // Locked for a moment (antivirus, indexer, cloud sync): not corrupt, and not ours to overwrite.
                return ReadOnly($"couldn't be read ({e.Message})");
            }

            CompanySave save = null;
            try { save = JsonUtility.FromJson<CompanySave>(text); }
            catch (Exception) { /* bad JSON: handled as corrupt below */ }
            if (save != null && save.version > CompanySave.CurrentVersion)
                return ReadOnly($"was written by a newer build (version {save.version})");
            if (save == null || save.version <= 0)
            {
                try
                {
                    File.Copy(path, path + ".corrupt", true);
                    Debug.LogWarning($"[Save] {path} is unreadable; kept it as .corrupt and started a new company.");
                }
                catch (Exception e)
                {
                    return ReadOnly($"is unreadable and couldn't be backed up ({e.Message})");
                }
                return CompanySave.New();
            }
            if (save.level < 1) save.level = 1;
            return save;
        }

        private CompanySave ReadOnly(string why)
        {
            Writable = false;
            Debug.LogWarning($"[Save] {path} {why}: playing with a new company that WON'T be saved, so the file stays as it is.");
            return CompanySave.New();
        }

        /// <summary>Writes the company; false (logged, never thrown) when it couldn't. The next save retries.</summary>
        public bool Save(CompanySave save)
        {
            if (!Writable) return false;
            string temp = path + ".tmp";
            try
            {
                string folder = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(temp, JsonUtility.ToJson(save, true));
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"[Save] Couldn't write {path} ({e.Message}); will try again at the next save.");
                return false;
            }
        }

        public void Delete()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
