using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Facts about the running build, stamped by the Editor build script just before a player build
    /// and reset to editor defaults right after it, so the committed asset never changes. Lives in a
    /// Resources folder because the game needs it before any scene reference exists (menus, crash log,
    /// version check on join).
    /// </summary>
    public class BuildInfo : ScriptableObject
    {
        public const string ResourcePath = "BuildInfo";
        public const string EditorCommit = "editor";

        [Tooltip("Short git commit the build was made from ('editor' when running in the editor).")]
        [SerializeField] private string commit = EditorCommit;
        [Tooltip("UTC build time, ISO 8601 (empty in the editor).")]
        [SerializeField] private string builtAtUtc = "";
        [Tooltip("Build flavour: Dev, Shareable or Release (empty in the editor).")]
        [SerializeField] private string flavor = "";

        public string Commit => commit;
        public string BuiltAtUtc => builtAtUtc;
        public string Flavor => flavor;

        /// <summary>Editor-only: the build script stamps the asset before building and resets it after.</summary>
        public void Stamp(string newCommit, string newBuiltAtUtc, string newFlavor)
        {
            commit = newCommit;
            builtAtUtc = newBuiltAtUtc;
            flavor = newFlavor;
        }
    }
}
