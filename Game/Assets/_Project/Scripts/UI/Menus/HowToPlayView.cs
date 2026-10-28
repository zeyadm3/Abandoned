using Abandoned.Core;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>The pause menu's "How to play" (M8.2): the goal, the keys (the player's own) and the rules of the building.</summary>
    public class HowToPlayView
    {
        private readonly Label keys;

        public VisualElement Root { get; }

        public HowToPlayView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Page(Root, wide: true);
            MenuKit.Text(panel, Loc.T("HOW TO PLAY"), "heading");
            var scroll = MenuKit.Scroll(panel);
            MenuKit.Text(scroll, Loc.T("THE JOB"), "section");
            MenuKit.Text(scroll, Loc.T("Your salvage company takes jobs in abandoned buildings. Carry loot out to the truck, pull the lever, " +
                                 "and the haul is appraised. Make the quota or the company pays a penalty; three misses in a row and it goes bankrupt."));
            MenuKit.Text(scroll, Loc.T("Use the ignition to start a six-second departure. Only its starter can cancel. Be inside the truck when it leaves."));
            MenuKit.Text(scroll, Loc.T("THE BUILDING"), "section");
            MenuKit.Text(scroll, Loc.T("Floors, stairs and walkways have a load limit. Weight cracks them (you'll hear creaks and groans), " +
                                 "hard impacts damage them, and a failing floor collapses with everything on it. Spread out, and think before " +
                                 "the whole crew stands on the bridge with the piano. Falling hurts: one floor costs health, more if you were " +
                                 "carrying something heavy, and three floors kills."));
            MenuKit.Text(scroll, Loc.T("THE LOOT"), "section");
            MenuKit.Text(scroll, Loc.T("Scan for estimated value, weight and fragility. The truck reveals the real value."));
            MenuKit.Text(scroll, Loc.T("Four pockets hold tools and small loot. Select with the wheel or pocket keys. Toggle inventory to inspect or discard; full pockets never swap items automatically."));
            MenuKit.Text(scroll, Loc.T("Tap Drop to gently set an item down. Hold and release Use to throw. Hold Rotate and move the mouse to turn it."));
            MenuKit.Text(scroll, Loc.T("One person can drag heavy loot. Share the handles to lift it; fewer carriers take more weight."));
            MenuKit.Text(scroll, Loc.T("THE DARK"), "section");
            MenuKit.Text(scroll, Loc.T("You're not alone in there. Noise draws attention: running, dropping things, and shouting. Talk with your crew " +
                                 "(proximity voice) and use the radio when you split up. The dead watch as ghosts; a crewmate nearby can " +
                                 "bring them back with a single-use medkit."));
            MenuKit.Text(scroll, Loc.T("Hiding spots vary. Hold Rotate and move the mouse to peek; peeking can expose you."));
            MenuKit.Text(scroll, Loc.T("THE COMPANY"), "section");
            MenuKit.Text(scroll, Loc.T("At HQ, use the rotary phone on the central dispatch bench to choose a contract and read its briefing. " +
                                 "The leather field guide beside it records the monsters you have encountered. Check haul receipts at the scrap depot " +
                                 "and collect gear and upgrades at the maintenance bench."));
            MenuKit.Text(scroll, Loc.T("Buy gear and truck upgrades at HQ. Your four pockets are shared with loot; use consumables with empty hands."));
            MenuKit.Text(scroll, Loc.T("KEYS"), "section");
            keys = MenuKit.Text(scroll, "");
            MenuKit.Button(panel, Loc.T("Back"), menu.Back, Audio.SoundId.UiBack);
        }

        /// <summary>Keys follow the player's bindings, so they're filled in when the page opens.</summary>
        public void Refresh()
        {
            string K(string action) => InputBindings.Display(action);
            keys.text = Loc.T("Move {0}   Sprint {1}   Crouch {2}   Jump {3}", K("Move"), K("Sprint"), K("Crouch"), K("Jump")) + "\n" +
                        Loc.T("Pick up / use {0}   Throw (hold) {1}   Set down {2}", K("Interact"), K("Use"), K("Drop")) + "\n" +
                        Loc.T("Rotate (hold + mouse) {0}   Pockets {1}", K("Rotate"), K("Inventory")) + "\n" +
                        Loc.T("Flashlight {0}   Pockets {1} / {2} / {3} / {4}   Scan {5}", K("Flashlight"), K("HandSlot1"), K("HandSlot2"), K("HandSlot3"), K("HandSlot4"), K("Scan")) + "\n" +
                        Loc.T("Give gear {0}   Chat {1}   Talk {2}   Radio {3}   Menu Esc", K("GiveGear"), K("Chat"), K("PushToTalk"), K("Radio"));
        }
    }
}
