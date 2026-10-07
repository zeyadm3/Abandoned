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
            MenuKit.Text(panel, "HOW TO PLAY", "heading");
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("scroll");
            panel.Add(scroll);
            MenuKit.Text(scroll, "THE JOB", "section");
            MenuKit.Text(scroll, "Your salvage company takes jobs in abandoned buildings. Carry loot out to the truck, pull the lever, " +
                                 "and the haul is appraised. Make the quota or the company pays a penalty; three misses in a row and it goes bankrupt.");
            MenuKit.Text(scroll, "THE BUILDING", "section");
            MenuKit.Text(scroll, "Floors, stairs and walkways have a load limit. Weight cracks them (you'll hear creaks and groans), " +
                                 "hard impacts damage them, and a failing floor collapses with everything on it. Spread out, and think before " +
                                 "the whole crew stands on the bridge with the piano.");
            MenuKit.Text(scroll, "THE LOOT", "section");
            MenuKit.Text(scroll, "Heavy things slow you down; big things need a crew on the handles (or a slow drag). Fragile things lose value " +
                                 "every time they hit something. Small things go in your pockets, but you lose those if you don't make it out.");
            MenuKit.Text(scroll, "THE DARK", "section");
            MenuKit.Text(scroll, "You're not alone in there. Noise draws attention: running, dropping things, and shouting. Talk with your crew " +
                                 "(proximity voice) and use the radio when you split up. The dead watch as ghosts until the next job.");
            MenuKit.Text(scroll, "KEYS", "section");
            keys = MenuKit.Text(scroll, "");
            MenuKit.Button(panel, "Back", menu.Back, Audio.SoundId.UiBack);
        }

        /// <summary>Keys follow the player's bindings, so they're filled in when the page opens.</summary>
        public void Refresh()
        {
            string K(string action) => InputBindings.Display(action);
            keys.text = $"Move {K("Move")}   Sprint {K("Sprint")}   Crouch {K("Crouch")}   Jump {K("Jump")}\n" +
                        $"Pick up / use {K("Interact")}   Throw (hold) {K("Use")}   Drop {K("Drop")}   Pockets {K("Inventory")}\n" +
                        $"Flashlight {K("Flashlight")}   Hand slots {K("HandSlot1")} / {K("HandSlot2")}   Scan {K("Scan")}\n" +
                        $"Talk {K("PushToTalk")}   Radio {K("Radio")}   Menu Esc";
        }
    }
}
