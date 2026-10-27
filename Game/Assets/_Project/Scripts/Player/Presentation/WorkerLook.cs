using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// The worker's skinned look (2026 remake): a head (one of the eight characters) and an outfit for that character's
    /// build, both on the crew skeleton. The standing body binds them to WorkerRig's joints and the ragdoll to bone
    /// helpers inside its bodies, so swapping a character or outfit is only a mesh and material change, never new bones.
    /// The outfit's tint material takes the player's colour (the old coverall colour) through a property block.
    /// </summary>
    public class WorkerLook : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private SkinnedMeshRenderer head;
        [SerializeField] private SkinnedMeshRenderer outfit;
        private MaterialPropertyBlock block;

        public SkinnedMeshRenderer Head => head;
        public SkinnedMeshRenderer Outfit => outfit;

        public void Apply(CosmeticDefinition character, CosmeticDefinition clothes, Color tint)
        {
            if (head != null && character != null && character.LookMesh != null)
            {
                head.sharedMesh = character.LookMesh;
                if (character.LookMaterials != null && character.LookMaterials.Length > 0) head.sharedMaterials = character.LookMaterials;
            }
            if (outfit == null || clothes == null) return;
            Mesh mesh = clothes.BuildMesh(character != null ? character.Build : 0);
            if (mesh != null) outfit.sharedMesh = mesh;
            if (clothes.LookMaterials != null && clothes.LookMaterials.Length > 0) outfit.sharedMaterials = clothes.LookMaterials;
            block ??= new MaterialPropertyBlock();
            for (int i = 0; i < outfit.sharedMaterials.Length; i++)
            {
                block.Clear();
                if (i == clothes.TintMaterial) block.SetColor(BaseColor, tint);
                outfit.SetPropertyBlock(block, i);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(SkinnedMeshRenderer headRenderer, SkinnedMeshRenderer outfitRenderer)
        {
            head = headRenderer;
            outfit = outfitRenderer;
        }
#endif
    }
}
