using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What makes one station itself, for <see cref="StationBuild"/>: its plan, look, loot, fittings, lot and sound.
    /// The build calls these in order; everything they place goes through the <see cref="StationContext"/>.
    /// </summary>
    public abstract class StationRecipe
    {
        public abstract StationPlan Plan { get; }
        public abstract LevelAtmosphere.Mood Mood { get; }
        /// <summary>Ceiling-light colour.</summary>
        public abstract Color LampColour { get; }
        /// <summary>The interior haze (colour, density) inside the footprint.</summary>
        public abstract (Color colour, float density) InteriorHaze { get; }
        public abstract IReadOnlyList<Vector3> CrewSpawns { get; }
        /// <summary>Where the crew comes in (the truck end of the carry route): monsters never start near it.</summary>
        public abstract Vector3 Entrance { get; }
        public virtual float MinThreatDistance => 22f;
        /// <summary>Groups under the root the NavMesh bake leaves out, besides the lot, truck, roof and lights.</summary>
        public virtual IEnumerable<string> NavMeshIgnored => new string[0];
        public abstract (Vector3 position, Vector3 lookAt) Camera { get; }

        public abstract StationArt CreateArt();
        /// <summary>Definitions, models and prefabs for the station's loot; runs before the scene opens.</summary>
        public abstract void EnsureLoot();
        /// <summary>The station's contract location asset, in its own data folder (the open world lists it).</summary>
        public virtual void Location() { }
        /// <summary>Rooms, hiding places, breakers, special structures and the loot and monster points inside.</summary>
        public abstract void Dress(StationContext ctx);
        /// <summary>The lot round the building and the truck.</summary>
        public abstract void Site(StationContext ctx);
        /// <summary>Ambient sound: the level bed and the station's own soundscape.</summary>
        public abstract void Sound(Transform root);
    }
}
