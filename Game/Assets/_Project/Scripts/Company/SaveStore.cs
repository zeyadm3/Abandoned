using Abandoned.Core;
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

        public const int Slots = 3;
        private const string SlotKey = "save.slot";

        private readonly string path;

        /// <summary>
        /// Which save this machine hosts (1-<see cref="Slots"/>, QA B-24): your crew's record with the company
        /// (you work for it; it isn't yours). Slot 1 is the original company.json, so older saves stay where they
        /// were. The demo always has its own single file.
        /// </summary>
        public static int Slot
        {
            get => Mathf.Clamp(Core.Prefs.GetInt(SlotKey, 1), 1, Slots);
            set
            {
                Core.Prefs.SetInt(SlotKey, Mathf.Clamp(value, 1, Slots));
                Core.Prefs.Save();
            }
        }

        public SaveStore(string folder = null) : this(folder, Slot) { }

        public SaveStore(string folder, int slot)
        {
            path = System.IO.Path.Combine(folder ?? Application.persistentDataPath, FileFor(slot));
        }

        public static string FileFor(int slot) =>
            Core.Demo.IsDemo ? DemoFileName : slot <= 1 ? FileName : $"company_{Mathf.Clamp(slot, 1, Slots)}.json";

        /// <summary>A one-line description of a slot for the menu ("Empty", or level, money and jobs done).</summary>
        public static string Describe(int slot, string folder = null)
        {
            string file = System.IO.Path.Combine(folder ?? Application.persistentDataPath, FileFor(slot));
            if (!File.Exists(file)) return Loc.T("Empty: a new hire");
            try
            {
                CompanySave save = JsonUtility.FromJson<CompanySave>(File.ReadAllText(file));
                return save == null ? Loc.T("Unreadable") : Loc.T("Level {0} - ${1:N0} - {2} jobs", save.level, save.money, save.runs);
            }
            catch (Exception)
            {
                return Loc.T("Unreadable");
            }
        }

        /// <summary>Any save slot on this machine holds a company (the 2026 hiring treats its player as hired).</summary>
        public static bool AnyExists
        {
            get
            {
                for (int slot = 1; slot <= Slots; slot++)
                    if (File.Exists(System.IO.Path.Combine(Application.persistentDataPath, FileFor(slot)))) return true;
                return false;
            }
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
            // A hand-edited or partly written file can name a list as null; everything after load expects lists.
            save.equipment ??= new();
            save.unlocks ??= new();
            save.catalogue ??= new();
            save.truckLook ??= new();
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
            // Every failure, not only IO ones: File.Replace also throws PlatformNotSupported/Security/Argument
            // errors on odd file systems, and an exception escaping here would stop payday before the
            // crew is sent its outcome (the caller publishes state right after saving).
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Couldn't write {path} ({e.GetType().Name}: {e.Message}); will try again at the next save.");
                return false;
            }
        }

        public void Delete()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
