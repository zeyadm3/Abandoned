using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the Iron Factory's cosmetic ambient sound markers (L6): hanging chain rattle positions high up
    /// in the roof steel, and structural thermal stress groan positions around the molten channel and frame.
    /// Hooks them up to <see cref="FactorySoundscape"/>.
    /// </summary>
    public static class IronFactorySoundscape
    {
        public static void Build(Transform root)
        {
            Transform soundRoot = Group("Soundscape", root);
            var chains = new List<Transform>();
            var groans = new List<Transform>();

            // Chain rattle locations: high in the trusses, away from the ground haul path
            float[] trussX = { 12f, 36f, 84f, 108f };
            float[] trussZ = { 20f, 44f, 68f, 90f };
            int cIndex = 0;
            foreach (float x in trussX)
            foreach (float z in trussZ)
            {
                Transform pt = Group($"ChainNode_{cIndex++}", soundRoot);
                pt.position = new Vector3(x, 18.4f, z);
                chains.Add(pt);
            }

            // Thermal groan locations: along the hot molten channel banks and big columns
            Rect c = L.ChannelMeters;
            for (float z = c.yMin + 4f; z <= c.yMax - 4f; z += 12f)
            {
                Transform gW = Group($"Groan_W_{groans.Count}", soundRoot);
                gW.position = new Vector3(c.xMin - 1f, 1f, z);
                groans.Add(gW);

                Transform gE = Group($"Groan_E_{groans.Count}", soundRoot);
                gE.position = new Vector3(c.xMax + 1f, 1f, z);
                groans.Add(gE);
            }

            var soundscape = soundRoot.gameObject.AddComponent<FactorySoundscape>();
            soundscape.EditorSetup(chains.ToArray(), groans.ToArray());
        }
    }
}
