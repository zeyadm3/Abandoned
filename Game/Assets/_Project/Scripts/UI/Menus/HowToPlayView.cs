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
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            MenuKit.Text(panel, Loc.T("RECOVERY MANUAL / READ BEFORE ENTRY"), "menu-record");
            MenuKit.Text(panel, Loc.T("HOW TO PLAY"), "heading");
            var scroll = MenuKit.Scroll(panel);
            MenuKit.Text(scroll, Loc.T("THE JOB"), "section");
            MenuKit.Text(scroll, Loc.T("Your salvage company takes jobs in abandoned buildings. Carry loot out to the truck, pull the lever, " +
                                 "and the haul is appraised. Make the quota or the company pays a penalty; three misses in a row and it goes bankrupt."));
            MenuKit.Text(scroll, Loc.T("THE BUILDING"), "section");
            MenuKit.Text(scroll, Loc.T("Floors, stairs and walkways have a load limit. Weight cracks them (you'll hear creaks and groans), " +
                                 "hard impacts damage them, and a failing floor collapses with everything on it. Spread out, and think before " +
                                 "the whole crew stands on the bridge with the piano. Falling hurts: one floor costs health, more if you were " +
                                 "carrying something heavy, and three floors kills."));
            MenuKit.Text(scroll, Loc.T("THE LOOT"), "section");
            MenuKit.Text(scroll, Loc.T("Heavy things slow you down; big things need a crew on the handles (or a slow drag). Fragile things lose value " +
                                 "every time they hit something. Scan to reveal prices, pocket values and the haul for a few seconds. " +
                                 "Hold Rotate and move the mouse to turn a held object. Tap Drop to release it, or hold Drop to lower it " +
                                 "gently onto clear support; keep holding until it settles, or release early to cancel. Small things " +
                                 "go in your pockets, but you lose those if you don't make it out."));
            MenuKit.Text(scroll, Loc.T("THE DARK"), "section");
            MenuKit.Text(scroll, Loc.T("You're not alone in there. Noise draws attention: running, dropping things, and shouting. Talk with your crew " +
                                 "(proximity voice) and use the radio when you split up. The dead watch as ghosts; a crewmate nearby can " +
                                 "bring them back with a single-use medkit."));
            MenuKit.Text(scroll, Loc.T("THE COMPANY"), "section");
            MenuKit.Text(scroll, Loc.T("At HQ, use the rotary phone on the central dispatch bench to choose a contract and read its briefing. " +
                                 "The leather field guide beside it records the monsters you have encountered. Check haul receipts at the scrap depot " +
                                 "and collect gear and upgrades at the maintenance bench."));
            MenuKit.Text(scroll, Loc.T("Every job costs running costs, whatever you bring back. Spend the rest at the HQ: gear from the shop (two hand " +
                                 "slots each; single-use gear is used with your hands empty), truck upgrades, better jobs as the company levels up. " +
                                 "Locked stores hold the good stock: bolt cutters or a crowbar get you in. A rope and pulley over a hole lowers " +
                                 "loot gently; a flatbed trolley lets a short crew drag even a piano. Check the answering machine."));
            MenuKit.Text(scroll, Loc.T("KEYS"), "section");
            keys = MenuKit.Text(scroll, "");
            MenuKit.Button(panel, Loc.T("Back"), menu.Back, Audio.SoundId.UiBack);
        }

        /// <summary>Keys follow the player's bindings, so they're filled in when the page opens.</summary>
        public void Refresh()
        {
            string K(string action) => InputBindings.Display(action);
            keys.text = Loc.T("Move {0}   Sprint {1}   Crouch {2}   Jump {3}", K("Move"), K("Sprint"), K("Crouch"), K("Jump")) + "\n" +
                        Loc.T("Pick up / use {0}   Throw (hold) {1}   Drop (tap) / set down (hold) {2}", K("Interact"), K("Use"), K("Drop")) + "\n" +
                        Loc.T("Rotate (hold + mouse) {0}   Pockets {1}", K("Rotate"), K("Inventory")) + "\n" +
                        Loc.T("Flashlight {0}   Hand slots {1} / {2}   Scan {3}", K("Flashlight"), K("HandSlot1"), K("HandSlot2"), K("Scan")) + "\n" +
                        Loc.T("Give gear {0}   Chat {1}   Talk {2}   Radio {3}   Menu Esc", K("GiveGear"), K("Chat"), K("PushToTalk"), K("Radio"));
        }
    }
}
