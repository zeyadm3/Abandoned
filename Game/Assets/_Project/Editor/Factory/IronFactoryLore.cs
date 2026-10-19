using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Atmospheric storytelling notes and incident reports placed across the Iron Factory rooms and walkways.
    /// Gives authentic industrial flavor (blast furnace burn-throughs, carbon monoxide warnings, runaway rolling
    /// mills, and superstitious foundry workers) while rewarding thorough salvage crews.
    /// </summary>
    public static class IronFactoryLore
    {
        public static void Build(Transform root)
        {
            Transform loreGroup = Group("LoreNotes", root);
            Material paperMat = GetMaterial("IronFactory_NotePaper", new Color(.88f, .84f, .76f));
            Material clipboardMat = GetMaterial("IronFactory_ClipboardWood", new Color(.45f, .32f, .22f));

            // 1. Foremen's Office (Desk in cell x=20, z=22 -> ~82m, 90m)
            PlaceNote(loreGroup, new Vector3(82f, .85f, 90f),
                "INCIDENT REPORT: HEARTH BURN-THROUGH",
                "02:40 — Cooling jacket pressure dropped to zero on tuyere 6.\n" +
                "03:15 — Slag penetrated refractory brickwork. Molten iron began weeping into sub-hearth.\n" +
                "Superintendent ordered the tapping runner left open to divert liquid head into the channel.\n" +
                "Do not plug the taphole. If the iron freezes inside the furnace, the plant is dead.",
                paperMat, clipboardMat);

            // 2. Weighbridge Office (Table in cell x=2, z=1 -> ~8m, 6m)
            PlaceNote(loreGroup, new Vector3(8f, .85f, 6f),
                "WEIGHBRIDGE MANIFEST: NIGHT SHIFT",
                "MANIFEST 774-B / NIGHT RUN\n" +
                "Outbound: 14 flatcars of cast iron pig. 6 boxcars scrap.\n" +
                "Scale recorded an unaccounted +12 tons on the last hopper car.\n" +
                "Driver claimed the platform scale was hot from channel drafts.\n" +
                "Whatever was inside car 9 had its own heartbeat.",
                paperMat, clipboardMat);

            // 3. Breaker Hall (Console near wall in cell x=2, z=10 -> ~8m, 40m)
            PlaceNote(loreGroup, new Vector3(8f, 1.1f, 40f),
                "HIGH VOLTAGE WARNING: BUS 4",
                "DO NOT THROW BREAKER 4 WITHOUT CLEARING THE MACHINE FLOOR.\n" +
                "Breaker 4 energizes the steam hammer hydraulics and the soaking pit gas igniters.\n" +
                "Last month Henderson reset the switch while maintenance was inside the die guide.\n" +
                "We never found his wrench.",
                paperMat, clipboardMat);

            // 4. Pattern Shop (Workbench in cell x=27, z=10 -> ~110m, 40m)
            PlaceNote(loreGroup, new Vector3(110f, .85f, 40f),
                "PATTERN MASTER'S LOG",
                "Order: 15-ton ceremonial bronze bell.\n" +
                "The customer provided their own alloy ingot specifications.\n" +
                "Wood patterns keep splintering along the grain during sand ramming as if vibrating from within.\n" +
                "The apprentice refuses to enter the shop after sundown.\n" +
                "He says the bell mould rings before we pour the bronze.",
                paperMat, clipboardMat);

            // 5. Operator Pulpit (Console shelf at x=98m, y=10.9m, z=74m)
            PlaceNote(loreGroup, new Vector3(98f, 10.95f, 74f),
                "MILL OPERATOR LOG: EMERGENCY STOP",
                "Shift 3 handover:\n" +
                "Roll stand 2 screwdown motors are surging.\n" +
                "When the emergency stop cord is pulled, the rollers accelerate instead of braking.\n" +
                "Keep your hands off the runout table.\n" +
                "If an ingot jams in the roughing rolls, do not go down with a cutting torch.",
                paperMat, clipboardMat);

            // 6. Worker Locker Room (Bench at x=14m, z=90m)
            PlaceNote(loreGroup, new Vector3(14f, .55f, 90f),
                "SAFETY COMMITTEE MEMORANDUM",
                "RE: CARBON MONOXIDE LEVELS IN LOWER RUNNER BAYS\n" +
                "Management has denied the requisition for additional air respirators.\n" +
                "Workers experiencing dizziness, auditory hallucinations, or burning soles are advised to step onto Catwalk Tier 2.\n" +
                "Lava pit safety railing repairs remain deferred to Q4.",
                paperMat, clipboardMat);

            // 7. Cargo Train Station (West platform scale at x=44m, z=-3.2m)
            PlaceNote(loreGroup, new Vector3(44f, 1.25f, -2.5f),
                "TERMINAL NOTICE: DISCONTINUATION OF FREIGHT RAIL SERVICE",
                "NOTICE TO FOUNDRY CREWS & OUTBOUND EXPEDITION:\n" +
                "All freight locomotive movements on Spur Line 9 are suspended indefinitely pending facility condemnation.\n" +
                "Locomotive #402 evacuated the final train of finished steel coils on Thursday night.\n" +
                "No further rail cars will be routed to the loading platforms. Ingot staging is closed.\n" +
                "Leave all pending billets on the dunnage. Salvage contractors will begin plant demolition.",
                paperMat, clipboardMat);

            MarkStatic(loreGroup);
        }

        private static void PlaceNote(Transform parent, Vector3 position, string title, string body, Material paperMat, Material boardMat)
        {
            Transform noteRoot = Group("Note_" + title.Replace(" ", "_").Replace(":", ""), parent);
            noteRoot.position = position;

            // Wooden clipboard base
            Box("Clipboard", noteRoot, Vector3.zero, new Vector3(.32f, .02f, .42f), boardMat, false);

            // Paper sheet
            GameObject paper = Box("PaperSheet", noteRoot, Vector3.up * .015f, new Vector3(.26f, .01f, .36f), paperMat, false);

            // Steel clip
            Box("Clip", noteRoot, new Vector3(0f, .025f, .16f), new Vector3(.12f, .02f, .04f), boardMat, false);

            // Usable collider for interaction raycasts
            var col = paper.AddComponent<BoxCollider>();
            col.center = Vector3.zero;
            col.size = new Vector3(.4f, .25f, .45f);

            paper.AddComponent<MysteryNote>().EditorSetup(title, body);
        }
    }
}
