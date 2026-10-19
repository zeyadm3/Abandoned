using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;

namespace Abandoned.EditorTools
{
    /// <summary>Open timber shelving with individual fitted spines; the original furniture footprints stay solid.</summary>
    public static class IslandLibraryShelves
    {
        public static void Build(Transform parent, string name, Vector3 at, Vector3 size, IslandPrisonArt art)
        {
            bool alongZ = size.z > size.x;
            float width = alongZ ? size.z : size.x, depth = alongZ ? size.x : size.z;
            string key = $"LibraryShelving_{width:0.00}_{size.y:0.00}_{depth:0.00}".Replace(',', '.');
            var kit = IslandMeshKit.Get(key, g => Model(g, width, size.y, depth, art));
            Transform holder = IslandMeshKit.Place(kit, name, parent, at, Quaternion.Euler(0f, alongZ ? 90f : 0f, 0f));
            IslandMeshKit.Solid(holder, Vector3.up * size.y * .5f, new Vector3(width, size.y, depth));
            holder.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Individual book spines and shelf mouldings share the bookcase's fitted solid collision.");
        }

        private static void Model(Transform g, float width, float height, float depth, IslandPrisonArt art)
        {
            Box("Back", g, new Vector3(0f, height * .5f, depth * .5f - .025f), new Vector3(width, height, .05f), art.DarkWood, false);
            foreach (float x in new[] { -width * .5f + .035f, width * .5f - .035f })
                Box("Side", g, new Vector3(x, height * .5f, 0f), new Vector3(.07f, height, depth), art.Wood, false);
            int bays = Mathf.Max(1, Mathf.RoundToInt(width / 1.1f));
            for (int i = 1; i < bays; i++)
                Box("Upright", g, new Vector3(-width * .5f + width * i / bays, height * .5f, 0f), new Vector3(.055f, height, depth), art.DarkWood, false);
            Box("Crown", g, new Vector3(0f, height - .025f, -.025f), new Vector3(width + .035f, .09f, depth + .04f), art.Wood, false);
            Box("Plinth", g, new Vector3(0f, .065f, -.015f), new Vector3(width + .015f, .13f, depth + .02f), art.DarkWood, false);
            int row = 0;
            for (float y = .32f; y < height - .2f; y += .45f, row++)
            {
                Box("ShelfBoard", g, new Vector3(0f, y, 0f), new Vector3(width - .075f, .035f, depth - .025f), art.Wood, false);
                Box("ShelfNosing", g, new Vector3(0f, y - .018f, -depth * .5f - .007f), new Vector3(width - .06f, .06f, .035f), art.DarkWood, false);
                for (int bay = 0; bay < bays; bay++)
                {
                    float start = -width * .5f + bay * width / bays + .09f;
                    float end = -width * .5f + (bay + 1) * width / bays - .075f;
                    int book = 0;
                    for (float x = start; x < end - .08f; book++)
                    {
                        float hash = K.Hash(book + bay * 31, row + 310);
                        float bw = Mathf.Min(.055f + hash * .075f, end - x), bh = Mathf.Min(.18f + K.Hash(book, row + 74) * .17f, height - y - .12f);
                        if (K.Hash(book + row * 13, bay + 983) < .19f) { x += bw * 1.8f; continue; }
                        float z = -depth * .5f + .025f + K.Hash(book, row + 980) * .025f;
                        Material cover = ((book + row + bay) % 5) switch
                        {
                            0 => art.Books, 1 => art.Dado, 2 => art.Canvas, 3 => art.HullTrim, _ => art.DarkWood,
                        };
                        Box("BoundSpine", g, new Vector3(x + bw * .5f, y + .025f + bh * .5f, z), new Vector3(bw, bh, .04f), cover, false);
                        Box("PageBlock", g, new Vector3(x + bw * .5f, y + .025f + bh * .5f, z + .1f), new Vector3(bw - .009f, bh - .018f, .17f), art.Paper, false);
                        if (hash > .3f)
                            Box("SpineBand", g, new Vector3(x + bw * .5f, y + .025f + bh * .72f, z - .021f), new Vector3(bw * .7f, .012f, .004f), art.Canvas, false);
                        x += bw + .012f;
                    }
                    Box("ShelfLabelHolder", g, new Vector3((start + end) * .5f, y - .015f, -depth * .5f - .029f), new Vector3(.14f, .035f, .009f), art.Brass, false);
                }
            }
        }
    }
}
