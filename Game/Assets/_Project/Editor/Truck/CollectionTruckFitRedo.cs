using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Fits the original referenced gameplay objects to a commercial truck's loading height.</summary>
    public static class CollectionTruckFitRedo
    {
        public const float DeckLift = .65f;
        private const float DeckTop = 1.05f, RampRun = 4.4f;

        public static void Apply(Transform truck)
        {
            Fit(truck, "BayFloor", new Vector3(0, .985f, 0), new Vector3(2.6f, .13f, 5));
            Fit(truck, "Wall_L", new Vector3(-1.35f, 2.15f, 0), new Vector3(.1f, 2.2f, 5));
            Fit(truck, "Wall_R", new Vector3(1.35f, 2.15f, 0), new Vector3(.1f, 2.2f, 5));
            Fit(truck, "Wall_Front", new Vector3(0, 2.15f, 2.55f), new Vector3(2.8f, 2.2f, .1f));
            Fit(truck, "Ignition", new Vector3(.95f, 2.15f, 2.38f), new Vector3(.22f, .22f, .12f));
            if (truck.Find("Ignition") is Transform ignition && ignition.TryGetComponent(out Renderer control))
                control.sharedMaterial = CollectionTruckMaterials.Get("Amber");
            Fit(truck, "DisplayFrame", new Vector3(-.48f, 2.52f, 2.47f), new Vector3(1.28f, .67f, .04f));
            Fit(truck, "Display", new Vector3(-.48f, 2.52f, 2.44f), new Vector3(1.20f, .60f, 1));
            if (truck.Find("DisplayFrame") is Transform frame && frame.TryGetComponent(out Renderer housing))
                housing.sharedMaterial = CollectionTruckMaterials.Get("Dark");

            Transform ramp = truck.Find("Ramp");
            if (ramp != null)
            {
                float angle = Mathf.Atan2(DeckTop, RampRun);
                ramp.localPosition = new Vector3(0, DeckTop * .5f - .05f * Mathf.Cos(angle), -2.5f - RampRun * .5f);
                ramp.localRotation = Quaternion.Euler(-angle * Mathf.Rad2Deg, 0, 0);
                ramp.localScale = new Vector3(2.6f, .1f, Mathf.Sqrt(RampRun * RampRun + DeckTop * DeckTop));
                if (ramp.TryGetComponent(out Renderer surface))
                {
                    surface.sharedMaterial = CollectionTruckMaterials.Get("Dark");
                    surface.shadowCastingMode = ShadowCastingMode.On;
                }
            }

            Transform bay = truck.Find("Bay");
            if (bay != null && bay.TryGetComponent(out BoxCollider trigger))
            {
                trigger.center = new Vector3(0, 2.15f, 0);
                trigger.size = new Vector3(2.6f, 2.2f, 5);
                truck.GetComponent<TruckCargo>().EditorSetup(trigger,
                    new Bounds(new Vector3(0, 2.15f, 1), new Vector3(3.2f, 3.5f, 8)), truck.Find("Ignition"));
            }
            FitLighting(truck);
            FitApproach(truck);
        }

        private static void FitLighting(Transform truck)
        {
            Fit(truck, "LightBar", new Vector3(0, 3.47f, 2.5f), new Vector3(1.95f, .10f, .17f));
            foreach (Transform child in truck)
            {
                if (child.name == "Floodlamp")
                {
                    child.localPosition = new Vector3(Mathf.Sign(child.localPosition.x) * .7f, 3.62f, 2.47f);
                    child.localScale = new Vector3(.32f, .22f, .10f);
                }
                else if (child.name == "Floodlight")
                    child.localPosition = new Vector3(Mathf.Sign(child.localPosition.x) * .7f, 3.65f, 2.39f);
            }
        }

        private static void FitApproach(Transform truck)
        {
            Transform mall = truck;
            while (mall.parent != null && mall.name != "Mall") mall = mall.parent;
            Transform collection = mall.Find("MeridianCollectionBay");
            if (collection == null) return;
            Transform lane = collection.Find("FrontCollectionApproach");
            if (lane != null)
            {
                Vector3 rear = truck.TransformPoint(new Vector3(0, 0, -2.5f));
                lane.position = rear - truck.forward * (RampRun + 2f);
                lane.rotation = Quaternion.LookRotation(-truck.forward);
            }
            // The former freestanding billboard overlapped the taller cargo body.
            for (int i = collection.childCount - 1; i >= 0; i--)
            {
                Transform child = collection.GetChild(i);
                if (child.name is "TailgateSignBracket" or "Sign_COLLECTION") Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void Fit(Transform root, string name, Vector3 position, Vector3 size)
        {
            Transform child = root.Find(name);
            if (child == null) return;
            child.localPosition = position;
            child.localScale = size;
        }
    }
}
