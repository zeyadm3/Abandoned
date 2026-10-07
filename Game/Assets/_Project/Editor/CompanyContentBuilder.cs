using Abandoned.Company;
using Abandoned.Contracts;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Company and contract data (GDD 13-14): configs, the first four modifiers, and the session's CompanyService prefab.</summary>
    public static class CompanyContentBuilder
    {
        public const string CompanyConfigPath = "Assets/_Project/Data/Company/CompanyConfig.asset";
        public const string ContractFolder = "Assets/_Project/Data/Contracts";
        public const string ContractConfigPath = ContractFolder + "/ContractConfig.asset";
        public const string CompanyServicePrefabPath = NetworkContentBuilder.NetworkPrefabFolder + "/CompanyService.prefab";

        [MenuItem("Tools/Abandoned/Create Company Content")]
        public static GameObject CreateMissing()
        {
            var company = LoadOrCreateAsset<CompanyConfig>(CompanyConfigPath);
            var contracts = LoadOrCreateAsset<ContractConfig>(ContractConfigPath);
            ContractModifier[] modifiers =
            {
                Modifier("power_off", "Power Off", "No electricity: the building is dark. Bring flashlights.", powerOff: true, bonus: 0.1f),
                Modifier("unstable", "Unstable", "Structural stability -20 %. Watch every step.", stability: -0.2f, bonus: 0.15f),
                Modifier("rush_job", "Rush Job", "A much shorter extraction window, a better bonus.", window: 0.6f, bonus: 0.25f),
                Modifier("fragile_collection", "Fragile Collection", "Lots of glass and antiques. Don't drop anything.", fragile: 3f, bonus: 0.1f),
                // M9.2 (GDD 14).
                Modifier("night", "Night Job", "After dark: the building is darker, and something extra is awake in there.", bonus: 0.2f,
                    night: true, extraThreats: 1, minLevel: 2),
                Modifier("heavy_jackpot", "Heavy Jackpot", "Word is there's more than one big prize in there. Bring a crew and a trolley.",
                    bonus: 0.1f, extraJackpots: 1),
                Modifier("picked_over", "Already Picked Over", "Someone got here first: much less loot, a better bonus for what's left.",
                    loot: 0.55f, bonus: 0.25f),
                Modifier("storm", "Storm", "The storm covers your footsteps, but the wind and rain are working on the structure too.",
                    bonus: 0.15f, hearing: 0.6f, decay: 1.4f, storm: true, minLevel: 2),
                // M10 (GDD 14 "sealed"; GDD 13 harder/elite modifiers at higher levels).
                Modifier("sealed", "Sealed", "The front is shuttered and every store is locked down. Bring bolt cutters or a crowbar; what's locked away is worth more.",
                    bonus: 0.15f, isSealed: true, minLevel: 2),
                Modifier("hot_property", "Hot Property", "Two big prizes and a buyer who won't wait: a short window, a fat bonus.",
                    window: 0.55f, bonus: 0.4f, extraJackpots: 1, minLevel: 8),
                Modifier("condemned", "Condemned", "Scheduled for demolition. Stability -30 %, something extra awake inside. Danger money.",
                    stability: -0.3f, bonus: 0.45f, extraThreats: 1, minLevel: 10),
            };
            // Append any modifier the board doesn't have yet (kept order; inspector edits stay).
            var current = new System.Collections.Generic.List<ContractModifier>(contracts.Modifiers ?? new ContractModifier[0]);
            bool added = false;
            foreach (ContractModifier m in modifiers)
                if (!current.Exists(c => c != null && c.Id == m.Id)) { current.Add(m); added = true; }
            if (added)
            {
                contracts.EditorSetModifiers(current.ToArray());
                EditorUtility.SetDirty(contracts);
            }

            var equipment = EquipmentContentBuilder.CreateMissing();
            var upgrades = CreateTruckUpgrades();
            var messages = CreateMessages();
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CompanyServicePrefabPath);
            if (existing != null && existing.GetComponent<CompanyService>() != null)
            {
                // Keep the prefab (its network hash); just make sure every reference is wired.
                var existingService = existing.GetComponent<CompanyService>();
                Set(existingService, "config", company);
                Set(existingService, "contracts", contracts);
                Set(existingService, "equipment", equipment);
                Set(existingService, "truckUpgrades", upgrades);
                Set(existingService, "messages", messages);
                PrefabUtility.SavePrefabAsset(existing);
                return existing;
            }
            var root = new GameObject("CompanyService");
            var no = root.AddComponent<Unity.Netcode.NetworkObject>();
            no.DontDestroyWithOwner = true;
            no.SynchronizeTransform = false;
            var service = root.AddComponent<CompanyService>();
            Set(service, "config", company);
            Set(service, "contracts", contracts);
            Set(service, "equipment", equipment);
            Set(service, "truckUpgrades", upgrades);
            Set(service, "messages", messages);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CompanyServicePrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        public const string MessagesPath = "Assets/_Project/Data/Company/CompanyMessages.asset";

        /// <summary>The boss's voicemails and the board's one-liners (M10.7). Created when missing; edits in the inspector stay.</summary>
        public static CompanyMessages CreateMessages()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CompanyMessages>(MessagesPath);
            if (existing != null) return existing;
            var m = ScriptableObject.CreateInstance<CompanyMessages>();
            m.EditorSetup(
                welcome: new[]
                {
                    "Morning. This is your boss. You're the new salvage crew. The last crew were also the new salvage crew. The board has jobs. The van has fuel. Mostly. Bring things back.",
                    "Welcome aboard. Company policy: if it's worth money, it's ours. If it's on fire, it's yours. Contract board's by the office. Don't touch my mug.",
                },
                met: new[]
                {
                    "Quota made. Good. That means I don't have to learn your names yet. Same again tomorrow.",
                    "The buyer called. He said the stuff was 'mostly intact'. That's the nicest thing he's ever said. Keep it up.",
                    "You made quota. I've told accounting not to celebrate. They weren't going to.",
                    "Decent haul. Somebody scraped the van though. I'm not saying it was you. I'm saying the van says it was you.",
                    "Quota met. The building you were in fell down an hour after you left. Great timing. Do that every time.",
                },
                big: new[]
                {
                    "Double quota. I had to sit down. I don't have a chair. I sat on a filing cabinet. It broke. Worth it.",
                    "That's the biggest haul this company's ever had. I'm putting it on the wall. The money, not you.",
                    "The buyer asked if we robbed a museum. I said 'not yet'. Lovely work.",
                },
                missed: new[]
                {
                    "You missed quota. I'm not angry. I'm writing it down, which is worse.",
                    "Short on quota. The difference is coming out of the company account, which is also where your pay comes from. Think about that.",
                    "So. Quota. Not met. The buyer laughed at me on the phone. I don't like being laughed at on the phone.",
                    "Missed it. Look, the floors are falling down, I get it. But the floors falling down is the job.",
                },
                warning: new[]
                {
                    "That's two in a row. One more and the bank owns this company, the van, and probably my shoes. Please. Bring. Things. Back.",
                    "Two missed quotas. I've started looking at other careers. You should too, if the next one goes like the last.",
                },
                bankrupt: new[]
                {
                    "Well. The bank took the company. I've started a new one. Same building, new name, same crew, because nobody else answered the ad. Fresh start. Don't make it stale.",
                    "We went under. I've signed the papers for a new company. The bank said 'again?'. Let's not hear them say it a third time.",
                },
                level: new[]
                {
                    "The buyers are taking us seriously now. Bigger jobs on the board. Bigger quotas too. That's how serious works.",
                    "We've moved up. There's new gear in the shop and worse buildings on the board. Congratulations, I suppose.",
                    "Promotion. For the company, not you. The shop's got more stock. The board's got more ways to die. Enjoy.",
                },
                quips: new[]
                {
                    "Client says the floors are 'mostly fine'.",
                    "Previous crew left in a hurry. Their van's still there.",
                    "Owner wants the piano. Owner doesn't say how.",
                    "Insurance says no. We say yes.",
                    "Some of it's glass. Most of it's glass.",
                    "Watch the third floor. Actually watch all the floors.",
                    "The power company says the lights might work. The power company also went bankrupt.",
                    "Locals say it makes noises at night. Locals say that about everything.",
                    "The roof is optional, apparently.",
                    "Bring back the statue and I'll stop shouting for a week.",
                    "Easy money. Famous last words, but easy money.",
                    "Structural survey attached. It's a drawing of a sad face.",
                });
            AssetDatabase.CreateAsset(m, MessagesPath);
            return m;
        }

        public const string TruckUpgradeFolder = "Assets/_Project/Data/Company/TruckUpgrades";
        public const string TruckUpgradeCatalogPath = "Assets/_Project/Data/Company/TruckUpgrades.asset";

        /// <summary>
        /// GDD 13 truck upgrades (M10.1), one asset per tier (created when missing, never overwritten) and the
        /// catalog in a fixed order: append only, an entry's index is its bit on the network.
        /// </summary>
        public static TruckUpgradeCatalog CreateTruckUpgrades()
        {
            if (!AssetDatabase.IsValidFolder(TruckUpgradeFolder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Company", "TruckUpgrades");
            var items = new System.Collections.Generic.List<TruckUpgradeDefinition>
            {
                Upgrade("cargo_1", "Bigger Bay", "Side boards and a roof rack: 35 % more cargo space.", TruckUpgradeKind.Cargo, 1, 12000, 2, 1.35f),
                Upgrade("floodlights", "Floodlights", "A light bar on the cab: the lot and the loading bay stay lit, power or no power.",
                    TruckUpgradeKind.Floodlights, 1, 8000, 3, 1f),
                Upgrade("engine_1", "Tuned Engine", "It starts first time: the truck leaves 7 s after the lever, not 10.", TruckUpgradeKind.Engine, 1, 10000, 4, 7f),
                Upgrade("cargo_2", "Box Truck", "A proper box truck: 75 % more cargo space than the van.", TruckUpgradeKind.Cargo, 2, 30000, 5, 1.75f),
                Upgrade("engine_2", "V8", "Leaves 4 s after the lever. Hold on to something.", TruckUpgradeKind.Engine, 2, 25000, 7, 4f),
                Upgrade("armor", "Armored Bay", "Steel plates and a cage: nothing that lives in there can touch you inside the truck.",
                    TruckUpgradeKind.Armor, 1, 40000, 9, 1f),
            };
            var catalog = LoadOrCreateAsset<TruckUpgradeCatalog>(TruckUpgradeCatalogPath);
            // Keep the existing order (network bits); append what's new.
            var current = new System.Collections.Generic.List<TruckUpgradeDefinition>(catalog.Items);
            current.RemoveAll(d => d == null);
            foreach (TruckUpgradeDefinition d in items)
                if (!current.Contains(d)) current.Add(d);
            catalog.EditorSet(current);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static TruckUpgradeDefinition Upgrade(string id, string displayName, string description, TruckUpgradeKind kind, int tier,
            int price, int level, float amount)
        {
            string path = $"{TruckUpgradeFolder}/TruckUpgrade_{id}.asset";
            var d = AssetDatabase.LoadAssetAtPath<TruckUpgradeDefinition>(path);
            if (d != null) return d;
            d = ScriptableObject.CreateInstance<TruckUpgradeDefinition>();
            d.EditorSetup(id, displayName, description, kind, tier, price, level, amount);
            AssetDatabase.CreateAsset(d, path);
            return d;
        }

        // Created when missing; never overwritten, so inspector tuning is kept.
        private static ContractModifier Modifier(string id, string displayName, string description, float stability = 0f,
            float window = 1f, float bonus = 0f, bool powerOff = false, float fragile = 1f, float loot = 1f, int minLevel = 1,
            int extraJackpots = 0, bool night = false, int extraThreats = 0, float hearing = 1f, float decay = 1f, bool storm = false,
            bool isSealed = false)
        {
            string path = $"{ContractFolder}/Modifier_{id}.asset";
            var m = AssetDatabase.LoadAssetAtPath<ContractModifier>(path);
            if (m != null) return m;
            m = ScriptableObject.CreateInstance<ContractModifier>();
            m.EditorSetup(id, displayName, description, stability, window, bonus, powerOff, fragile, loot, minLevel,
                extraJackpots, night, extraThreats, hearing, decay, storm, isSealed);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }
    }
}
