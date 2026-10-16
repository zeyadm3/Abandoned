using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>
    /// The single source of truth for job destinations. Keep this asset append-only in spirit: contract ids
    /// travel over the network and are useful in saved run diagnostics, while display text stays localizable.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Contracts/Location Catalog", fileName = "LocationCatalog")]
    public sealed class LocationCatalog : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "LocationCatalog";

        [SerializeField] private LocationDefinition[] locations = Array.Empty<LocationDefinition>();

        private static LocationCatalog current;
        private static LocationDefinition legacyMall;

        public IReadOnlyList<LocationDefinition> All => locations;

        /// <summary>The project's shipped catalog, for UI and systems that only have a scene or active contract.</summary>
        public static LocationCatalog Current
        {
            get
            {
                if (current == null) current = Resources.Load<LocationCatalog>(ResourcePath);
                return current;
            }
        }

        /// <summary>The first safe fallback destination. It is normally the Mall definition.</summary>
        public LocationDefinition Default => Find(ContractGenerator.MallId) ?? locations.FirstOrDefault(x => x != null);

        public LocationDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return locations.FirstOrDefault(x => x != null && string.Equals(x.Id, id, StringComparison.Ordinal));
        }

        public LocationDefinition FindByScene(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return null;
            return locations.FirstOrDefault(x => x != null && string.Equals(x.Scene, scene, StringComparison.Ordinal));
        }

        /// <summary>Finds the authored definition behind a networked contract, including contracts made before ids existed.</summary>
        public LocationDefinition Resolve(Contract contract) => Find(contract.LocationId) ?? FindByScene(contract.Scene);

        /// <summary>Definitions whose clearance requirement is met and which can actually be offered.</summary>
        public IReadOnlyList<LocationDefinition> Available(int companyLevel)
        {
            int level = Mathf.Max(1, companyLevel);
            return locations.Where(x => x != null && x.BoardWeight > 0 && x.MinCompanyLevel <= level &&
                                        !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Scene)).ToArray();
        }

        internal static LocationDefinition LegacyMall
        {
            get
            {
                if (legacyMall == null) legacyMall = LocationDefinition.LegacyMall();
                return legacyMall;
            }
        }

        public void Validate(List<string> errors)
        {
            if (locations == null || locations.Length == 0)
            {
                errors.Add($"{name}: no locations.");
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var scenes = new HashSet<string>(StringComparer.Ordinal);
            foreach (LocationDefinition location in locations)
            {
                if (location == null)
                {
                    errors.Add($"{name}: an entry is empty.");
                    continue;
                }
                if (!ids.Add(location.Id)) errors.Add($"{name}: duplicate location id '{location.Id}'.");
                if (!scenes.Add(location.Scene)) errors.Add($"{name}: duplicate location scene '{location.Scene}'.");
            }
            if (Find(ContractGenerator.MallId) == null) errors.Add($"{name}: needs the '{ContractGenerator.MallId}' fallback location.");
        }

#if UNITY_EDITOR
        public void EditorSet(LocationDefinition[] value) => locations = value ?? Array.Empty<LocationDefinition>();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
            legacyMall = null;
        }
    }
}
