using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Contracts;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.FireStationLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Millbrook Fire Station No. 4 for <see cref="StationBuild"/> (Docs/FIRE_STATION_PLAN.md): a grey winter afternoon,
    /// warm lamps inside, the crew's van at the haul lane's door, the hose tower and the pole.
    /// </summary>
    public sealed class FireStationRecipe : StationRecipe
    {
        public override StationPlan Plan => L.Plan;

        /// <summary>A grey winter afternoon with a low sun: the bay doors stand open on a pale, cold street.</summary>
        public override LevelAtmosphere.Mood Mood => new(
            sky: new Color(.16f, .17f, .19f), equator: new Color(.1f, .1f, .11f), ground: new Color(.05f, .045f, .04f),
            fog: new Color(.42f, .43f, .45f), fogDensity: .008f, bloom: .18f, vignette: .2f, grain: .12f,
            saturation: -22f, contrast: 10f, sunColor: new Color(1f, .86f, .7f), sunIntensity: .55f, sunEuler: new Vector3(22f, 230f, 0f));

        public override Color LampColour => new(1f, .9f, .75f);
        public override (Color colour, float density) InteriorHaze => (new Color(.14f, .13f, .12f), .015f);
        public override Vector3 Entrance => L.HaulDoor;

        /// <summary>On the apron between the van's ramp and the haul lane's door.</summary>
        public override IReadOnlyList<Vector3> CrewSpawns => new[]
        {
            new Vector3(8.5f, .05f, -5f), new Vector3(10f, .05f, -5f), new Vector3(14f, .05f, -5f), new Vector3(15.5f, .05f, -5f),
        };

        public override IEnumerable<string> NavMeshIgnored => new[]
        {
            FireStationTower.GroupName + "/Hoses", FireStationTower.GroupName + "/Lamps", FireStationTower.GroupName + "/Shell/Roof",
            "FirePole", StationAccess.GroupName,
        };

        public override (Vector3 position, Vector3 lookAt) Camera => (new Vector3(78f, 26f, -38f), new Vector3(28f, 4f, 18f));

        public override StationArt CreateArt() =>
            new(L.ArtFolder, "Fire", new Color(.95f, .6f, .5f), new Color(.6f, .08f, .06f), LampColour);

        public override void EnsureLoot() => FireStationLoot.Kit.EnsureContent();

        public override void Location()
        {
            PolishAssets.EnsureFolder(L.LootFolder);
            var location = SerializedWiring.LoadOrCreateAsset<LocationDefinition>(L.LootFolder + "/Location_FireStation.asset");
            location.EditorSetup("fire_station", "Millbrook Fire Station No. 4",
                "A 1930s firehouse: two engines and the ladder truck still in the bay, bunk rooms on a floor that sags over them, and a hose tower full of brass. The van backs up to the empty lane's door.",
                L.SceneName, 1, 1, 1.05f, 1f, -.03f, 1f, .1f);
            EditorUtility.SetDirty(location);
        }

        public override void Dress(StationContext ctx)
        {
            FireStationTower.Build(ctx);
            FireStationBay.Dress(ctx);
            FireStationGround.Dress(ctx);
            FireStationUpper.Dress(ctx);
            FireStationBasement.Dress(ctx);
        }

        public override void Site(StationContext ctx) => FireStationExterior.Build(ctx);

        public override void Sound(Transform root)
        {
            var ambience = new GameObject("Ambience");
            ambience.transform.SetParent(root, false);
            // The bay doors stand open: more wind than the precinct, a faint hum, the old building settling.
            ambience.AddComponent<LevelAmbience>().EditorSetup(.24f, .06f, .02f, new Vector2(20f, 50f));
            Transform scape = Group("Soundscape", root);
            Transform[] Points(string name, params Vector3[] at)
            {
                var list = new Transform[at.Length];
                for (int i = 0; i < at.Length; i++)
                {
                    list[i] = Group($"{name}_{i}", scape);
                    list[i].position = at[i];
                }
                return list;
            }
            scape.gameObject.AddComponent<FireStationSoundscape>().EditorSetup(
                Points("Radio", new Vector3(40.4f, 1.2f, 5.5f), new Vector3(4f, 2f, 4.1f), new Vector3(41f, 5.2f, 28f)),
                Points("Tower", new Vector3(4f, 18f, 34f), new Vector3(4f, 10f, 33f), new Vector3(4f, 4f, 36f)),
                Points("Drip", new Vector3(12f, .2f, 14f), new Vector3(62f, 4.5f, 4.5f), new Vector3(47.4f, .9f, 39.3f), new Vector3(58f, .3f, 35f),
                    new Vector3(50f, -3.5f, 34f)));
        }
    }
}
