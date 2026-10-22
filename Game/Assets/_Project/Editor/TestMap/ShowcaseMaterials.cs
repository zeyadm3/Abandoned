using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap showcase's architecture materials. Each is named for its surface so MaterialRemaster classifies it
    /// (concrete, painted block, brick, timber floor...) and gives it that scan on Abandoned/Surface, mapped in world
    /// space (triplanar): the code-built boxes need no UVs and every wall gets the same texel size. The colour asked
    /// for is the average the finished surface shows.
    /// </summary>
    public static class ShowcaseMaterials
    {
        public const string Folder = "Assets/_Project/Art/TestMap/Materials";
        private static readonly Dictionary<string, Material> Made = new();

        /// <summary>Forget this build's materials, so the next build sets them up again from the scans.</summary>
        public static void Reset() => Made.Clear();

        public static Material ConcreteFloor => Surface("TestMap_Worn_Concrete_Floor", new Color(.47f, .46f, .44f));
        public static Material WoodFloor => Surface("TestMap_Oak_Wood_Floor", new Color(.42f, .31f, .22f));
        public static Material Joist => Surface("TestMap_Timber_Joist", new Color(.38f, .29f, .2f));
        public static Material BlockWall => Surface("TestMap_Painted_Block_Wall", new Color(.62f, .62f, .58f));
        public static Material PlasterWall => Surface("TestMap_Interior_Painted_Plaster_Wall", new Color(.66f, .64f, .59f));
        public static Material Brick => Surface("TestMap_Brick_Facade", new Color(.43f, .25f, .19f));
        public static Material CastConcrete => Surface("TestMap_Cast_Concrete_Pillar", new Color(.55f, .54f, .52f));
        public static Material Roof => Surface("TestMap_Cast_Concrete_Roof_Slab", new Color(.5f, .5f, .49f));
        public static Material Steel => Surface("TestMap_Steel_Beam", new Color(.36f, .37f, .38f));
        public static Material SafetyRail => Surface("TestMap_Safety_Painted_Steel_Rail", new Color(.86f, .66f, .12f));
        public static Material DoorFrame => Surface("TestMap_Painted_Steel_Door_Frame", new Color(.2f, .28f, .3f));
        public static Material Tread => Surface("TestMap_Tread_Plate_Stairs", new Color(.42f, .42f, .41f));
        public static Material Skirting => Surface("TestMap_Painted_Wood_Trim", new Color(.3f, .27f, .23f));
        public static Material Sill => Surface("TestMap_Precast_Concrete_Sill", new Color(.62f, .61f, .58f));
        public static Material Pavement => Surface("TestMap_Pavement_Concrete", new Color(.55f, .54f, .51f));
        public static Material Asphalt => Surface("TestMap_Asphalt_Carpark", new Color(.2f, .2f, .21f));
        public static Material Grass => Surface("TestMap_Grass_Lawn", new Color(.27f, .31f, .17f));
        public static Material Dirt => Surface("TestMap_Dirt_Ground", new Color(.33f, .28f, .21f));
        public static Material Gravel => Surface("TestMap_Gravel_Verge", new Color(.42f, .4f, .37f));
        public static Material Pallet => Surface("TestMap_Pallet_Wood", new Color(.52f, .41f, .28f));
        public static Material GalleryFloor => Surface("TestMap_Terrazzo_Gallery_Floor", new Color(.38f, .37f, .36f));
        public static Material GalleryWall => Surface("TestMap_Gallery_Interior_Painted_Plaster_Wall", new Color(.2f, .2f, .21f));
        public static Material GalleryCeiling => Surface("TestMap_Gallery_Acoustic_Ceiling", new Color(.5f, .5f, .49f));
        public static Material Plinth => Surface("TestMap_Smooth_Cast_Concrete_Plinth", new Color(.52f, .51f, .49f));

        public static Material Glass => Transparent("TestMap_Window_Glass", new Color(.62f, .7f, .72f, .22f));
        public static Material LinePaint => Plain("TestMap_Bay_Line_Stripe", new Color(.85f, .82f, .72f), .35f);
        public static Material SafetyLine => Plain("TestMap_Safety_Stripe", new Color(.85f, .64f, .1f), .4f);
        public static Material Housing => Plain("TestMap_Dark_Fixture_Housing", new Color(.08f, .08f, .085f), .55f, metallic: .6f);
        public static Material Glow => Emissive("TestMap_Fixture_Glow_Panel", new Color(1f, .95f, .86f), ShowcaseModels.TubeGlow);
        public static Material Plaque => Plain("TestMap_Brass_Plaque_Plate", new Color(.55f, .45f, .25f), .7f, metallic: 1f);

        /// <summary>A scanned surface (see the summary). The name decides which scan it gets.</summary>
        public static Material Surface(string name, Color average)
        {
            if (Made.TryGetValue(name, out Material made)) return made;
            Material material = Load(name);
            Made[name] = material;
            if (!MaterialRemaster.Apply(material, 0f, triplanar: true, moving: false, average))
            {
                Debug.LogWarning($"[Showcase] {name}: no scanned surface matched the name; it stays plain.");
                material.SetColor("_BaseColor", average);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material Plain(string name, Color colour, float smoothness, float metallic = 0f)
        {
            Material material = Load(name);
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Emissive(string name, Color colour, Color glow)
        {
            Material material = Plain(name, colour, .6f);
            material.SetColor("_EmissionColor", glow);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return material;
        }

        // Same set-up as the training house's glass (TutorialHouseMaterials): see-through, lit, both faces drawn.
        private static Material Transparent(string name, Color colour)
        {
            Material material = Plain(name, colour, .9f);
            material.SetFloat("_Surface", 1);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_Cull", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = 3000;
            return material;
        }

        private static Material Load(string name)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
