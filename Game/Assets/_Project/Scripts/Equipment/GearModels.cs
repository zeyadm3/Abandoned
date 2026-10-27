using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The remodelled gear (2026 remake, Tools/Blender/remake/gear): one model per equipment kind, grip at the origin,
    /// +Z forward, like the code-built ToolModels it replaces. ToolModels.Build uses an entry when there is one and
    /// falls back to its own meshes otherwise, so new gear still needs no new code. Data/Equipment/Resources/GearModels.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Gear Models", fileName = "GearModels")]
    public class GearModels : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            [SerializeField] private EquipmentKind kind;
            [SerializeField] private GameObject model;

            public EquipmentKind Kind => kind;
            public GameObject Model => model;

            public Entry(EquipmentKind kind, GameObject model)
            {
                this.kind = kind;
                this.model = model;
            }
        }

        [SerializeField] private List<Entry> models = new();

        private static GearModels instance;
        private static bool loaded;

        public static GameObject For(EquipmentKind kind)
        {
            if (!loaded)
            {
                loaded = true;
                instance = Resources.Load<GearModels>("GearModels");
            }
            if (instance == null) return null;
            foreach (Entry e in instance.models)
                if (e.Kind == kind && e.Model != null) return e.Model;
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            loaded = false;
        }

#if UNITY_EDITOR
        public void EditorSet(List<Entry> entries) => models = entries;
#endif
    }
}
