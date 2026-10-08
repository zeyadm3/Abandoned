using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Authoring remains deterministic and prints the actual accepted density after mesh clearance checks.</summary>
    public static class MeridianDressingDensity
    {
        public static void Place(Transform mall, Transform group)
        {
            var clearance = new MeridianDressingClearance(mall);
            MeridianDressingStoreDensity.Place(clearance, Group("FittedStores", group));
            MeridianDressingCinemaDensity.Place(clearance, Group("CinemaInterior", group));
            MeridianDressingPublicDensity.Place(clearance, Group("PublicSpaces", group));
            clearance.Report();
        }
    }
}
