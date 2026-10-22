using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Contracts;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.PoliceStationLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Millbrook 3rd Precinct for <see cref="StationBuild"/> (Docs/POLICE_STATION_PLAN.md): a wet evening after
    /// rain, cold fluorescent light inside, the truck at the sally port.
    /// </summary>
    public sealed class PoliceStationRecipe : StationRecipe
    {
        public override StationPlan Plan => L.Plan;

        /// <summary>Evening after rain: a low grey-blue sky, sodium lamps outside, the haze of a shut building inside.</summary>
        public override LevelAtmosphere.Mood Mood => new(
            sky: new Color(.075f, .085f, .105f), equator: new Color(.045f, .05f, .06f), ground: new Color(.02f, .02f, .022f),
            fog: new Color(.15f, .16f, .18f), fogDensity: .018f, bloom: .24f, vignette: .24f, grain: .17f,
            saturation: -28f, contrast: 12f, sunColor: new Color(.72f, .8f, 1f), sunIntensity: .14f, sunEuler: new Vector3(38f, 215f, 0f));

        public override Color LampColour => new(.86f, .93f, 1f);
        public override (Color colour, float density) InteriorHaze => (new Color(.09f, .1f, .11f), .02f);
        public override Vector3 Entrance => L.SallyDoor;

        public override IReadOnlyList<Vector3> CrewSpawns => new[]
        {
            new Vector3(59.4f, .05f, 6.4f), new Vector3(59.4f, .05f, 8.4f), new Vector3(59.4f, .05f, 11.6f), new Vector3(59.4f, .05f, 13.6f),
        };

        public override (Vector3 position, Vector3 lookAt) Camera => (new Vector3(88f, 24f, -26f), new Vector3(30f, 2f, 18f));

        public override StationArt CreateArt() =>
            new(L.ArtFolder, "Police", new Color(.86f, .62f, .52f), new Color(.16f, .24f, .42f), LampColour);

        public override void EnsureLoot() => PoliceStationLoot.Kit.EnsureContent();

        public override void Location()
        {
            PolishAssets.EnsureFolder(L.LootFolder);
            var location = SerializedWiring.LoadOrCreateAsset<LocationDefinition>(L.LootFolder + "/Location_PoliceStation.asset");
            location.EditorSetup("police_station", "Millbrook 3rd Precinct",
                "A brick precinct with a basement armoury, a cell block and an evidence room whose floor won't take much more. The truck backs into the sally port.",
                L.SceneName, 2, 1, 1.05f, 1f, -.02f, 1f, .1f);
            EditorUtility.SetDirty(location);
        }

        public override void Dress(StationContext ctx)
        {
            PoliceStationGround.Dress(ctx);
            PoliceStationUpper.Dress(ctx);
            PoliceStationBasement.Dress(ctx);
        }

        public override void Site(StationContext ctx) => PoliceStationExterior.Build(ctx);

        public override void Sound(Transform root)
        {
            var ambience = new GameObject("Ambience");
            ambience.transform.SetParent(root, false);
            // Wind round a shut brick building, the fluorescent hum, the building settling now and then.
            ambience.AddComponent<LevelAmbience>().EditorSetup(.2f, .1f, .03f, new Vector2(18f, 46f));
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
            scape.gameObject.AddComponent<PoliceStationSoundscape>().EditorSetup(
                Points("Scanner", new Vector3(46f, 1.2f, 21f), new Vector3(34f, 1f, 6f), new Vector3(-13f, 1f, 9f), new Vector3(48f, 1.5f, 4f)),
                Points("Slam", new Vector3(14f, 1.5f, 30f), new Vector3(4f, -2.5f, 26f), new Vector3(32f, 1.5f, 38f), new Vector3(2f, 5.5f, 35f)),
                Points("Drip", new Vector3(10f, -3f, 26f), new Vector3(2f, -3f, 20f), new Vector3(3f, 1f, 38f), new Vector3(30f, -3f, 36f)));
        }
    }
}
