using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The HQ yard's diesel pump (part of the world, not the HQ's scene). E with the van parked beside it starts filling:
    /// the host pumps a few litres a second and charges the company for each litre, until the tank is full, the money
    /// runs out, or whoever started it walks away from the pump.
    /// </summary>
    public class FuelPump : MonoBehaviour, IUsable
    {
        [Tooltip("How close (m) the van has to be parked, and the one filling it has to stay.")]
        [SerializeField] private float reach = 8f;
        [SerializeField] private int pricePerLitre = 4;

        private ulong filler = VanSeating.Empty;
        private float paidFor;

        public int PricePerLitre => pricePerLitre;

        public string UsePrompt(GameObject user)
        {
            WorldVan van = WorldVan.Current;
            if (van == null || !van.IsSpawned) return null;
            if (Vector3.Distance(van.transform.position, transform.position) > reach) return Loc.T("Park the van by the pump to fill up");
            VanNetState s = van.State;
            float tank = VanConfig.Load().TankLitres;
            if (s.Fuel >= tank - .05f) return Loc.T("The tank is full ({0:0} L)", s.Fuel);
            return Loc.T("Fill up the van: {0:0}/{1:0} L, ${2} a litre", s.Fuel, tank, pricePerLitre);
        }

        public void Use(GameObject user)
        {
            WorldVan van = WorldVan.Current;
            if (van == null || !van.IsSpawned) return;
            if (van.IsServer) ServerStart(van.NetworkManager.LocalClientId);
            else van.RequestPump();
        }

        /// <summary>Host: someone at the pump started filling.</summary>
        public void ServerStart(ulong client)
        {
            WorldVan van = WorldVan.Current;
            if (van == null || Vector3.Distance(van.transform.position, transform.position) > reach) return;
            filler = client;
        }

        private void Update()
        {
            WorldVan van = WorldVan.Current;
            if (filler == VanSeating.Empty || van == null || !van.IsServer) return;
            NetworkPlayer p = null;
            foreach (NetworkPlayer candidate in NetworkPlayer.All)
                if (candidate != null && candidate.IsSpawned && candidate.OwnerClientId == filler) p = candidate;
            VanConfig config = VanConfig.Load();
            float room = config.TankLitres - van.Condition.Fuel;
            if (p == null || p.IsDead || room <= 0f || Vector3.Distance(p.transform.position, transform.position) > reach
                || Vector3.Distance(van.transform.position, transform.position) > reach)
            {
                filler = VanSeating.Empty;
                return;
            }
            float litres = Mathf.Min(room, config.PumpLitresPerSecond * Time.deltaTime);
            paidFor += litres;
            int whole = Mathf.FloorToInt(paidFor);
            CompanyService company = CompanyService.Current;
            if (whole > 0 && company != null && company.IsSpawned && CompanyService.RulesApply)
            {
                if (!company.TryCharge(whole * pricePerLitre, "diesel"))
                {
                    filler = VanSeating.Empty;
                    return;
                }
                paidFor -= whole;
            }
            van.ServerRefuel(litres);
        }

        public static FuelPump Near(Vector3 at, float within)
        {
            foreach (FuelPump p in FindObjectsByType<FuelPump>(FindObjectsSortMode.None))
                if (Vector3.Distance(p.transform.position, at) <= within) return p;
            return null;
        }
    }
}
