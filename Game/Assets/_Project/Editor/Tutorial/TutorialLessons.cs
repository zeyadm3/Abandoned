using System.Collections.Generic;
using Abandoned.Tutorial;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The training job's default lessons, in order: what <see cref="TutorialContentBuilder"/> writes as step assets.
    /// After that the assets are the truth (edit, reorder or add lessons in the catalog without code). Every shown
    /// line here is English for the text sheets (Data/Text/Resources/Text/tutorial.txt), worded where it's shown.
    /// Keys name the house's salvage, areas, sections and spots (TutorialHouseGameplay, TutorialHouseStructure).
    /// Guide says where the arrows lead (empty = worked out from the check), Hud which HUD pieces get a frame, and
    /// Voice is what the trainer says (English only, never shown, never a key name: the card shows the keys).
    /// The recordings are named by a hash of Voice: re-run Tools/Audio/generate_tutorial_narration.py after editing one.
    /// </summary>
    public static class TutorialLessons
    {
        public readonly struct Lesson
        {
            public readonly string Id, Title, Text, Crew, Radio, Key, Area, BeatKey, Guide, Hud, Voice;
            public readonly int Chapter;
            public readonly TutorialCheck Check;
            public readonly float Amount, BeatAmount;
            public readonly TutorialBeat Beat;

            public Lesson(string id, int chapter, string title, string text, TutorialCheck check, string key = "", string area = "", float amount = 0f,
                string crew = "", string radio = "", TutorialBeat beat = TutorialBeat.None, string beatKey = "", float beatAmount = 0f,
                string guide = "", string hud = "", string voice = "")
            {
                Id = id; Chapter = chapter; Title = title; Text = text; Check = check; Key = key; Area = area; Amount = amount;
                Crew = crew; Radio = radio; Beat = beat; BeatKey = beatKey; BeatAmount = beatAmount;
                Guide = guide; Hud = hud; Voice = voice;
            }
        }

        public static readonly string[] Chapters =
        {
            "THE DRIVEWAY", "THE LIVING ROOM", "THE KITCHEN", "THE LAUNDRY", "THE HALLWAY",
            "THE BASEMENT", "THE VISITOR", "THE ENSUITE", "THE TRUCK",
        };

        public static readonly IReadOnlyList<Lesson> All = new List<Lesson>
        {
            // 0. The driveway: moving, the readouts, the walk up.
            new("driveway.look", 0, "Look around", "Move the mouse to look around. That's the job: 14 Larch Court. The bank wants it emptied before it sells.",
                TutorialCheck.Look, amount: 120f, radio: "DISPATCH: Training job, Larch Court. Nobody pays for this one. Take your time.",
                guide: "look area:porch",
                voice: "Alright, new hire. Have a look around. That's the house, fourteen Larch Court. The bank wants it emptied before it sells."),
            new("driveway.walk", 0, "Walk", "Walk up the driveway toward the house.", TutorialCheck.Walk, amount: 6f,
                guide: "go area:porch", voice: "Walk up the driveway. Follow the arrows on the ground."),
            new("driveway.sprint", 0, "Sprint", "Hold {key:Sprint} to sprint. It burns stamina, and running is loud.", TutorialCheck.Sprint, amount: 1.5f,
                guide: "go area:porch", hud: "vitals", voice: "Now sprint. Hold the key on your screen. It's quick, but it's loud, and it wears you out."),
            new("driveway.crouch", 0, "Crouch", "Press {key:Crouch} to crouch. Slow, but the quietest way to move.", TutorialCheck.Crouch, amount: 1.5f,
                guide: "go area:porch", voice: "Crouch. It's slow, but it's the quietest way to move."),
            new("driveway.jump", 0, "Jump", "Press {key:Jump} to jump.", TutorialCheck.Jump,
                guide: "go area:porch", voice: "And jump."),
            new("driveway.readouts", 0, "Your readouts", "Bottom left: your health and breath. Top: the compass. The rest you read from the world: listen to it.",
                TutorialCheck.Read, amount: 10f, guide: "none", hud: "vitals,compass,haul",
                voice: "Bottom left, your health and your breath. Up top, the haul and the compass. Everything else, you read from the house. Listen to it."),
            new("driveway.porch", 0, "The front door", "Go up to the front door. The porch has a ramp: loaded trolleys roll up it.", TutorialCheck.EnterArea, area: "porch",
                voice: "Head up to the front door. See the ramp on the porch? Loaded trolleys roll right up it."),

            // 1. The living room: light, scan, pick up, set down, throw, pockets, hand slots.
            new("living.enter", 1, "Inside", "Go inside.", TutorialCheck.EnterArea, area: "living",
                radio: "DISPATCH: The power's been off since the bank took it. Use your light.", voice: "Go on in."),
            new("living.light", 1, "Flashlight", "Turn your flashlight on with {key:Flashlight}. The battery recharges while it's off.", TutorialCheck.Flashlight,
                hud: "battery", voice: "It's dark in here. Turn your flashlight on. The battery charges back up while it's off."),
            new("living.scan", 1, "Scan", "Press {key:Scan} to scan: everything worth taking shows its value, and you see what the truck holds.", TutorialCheck.Scan,
                hud: "cargo", voice: "Now scan. Everything worth taking shows you its price, and you'll see what the truck is holding."),
            new("living.lamp", 1, "Pick it up", "Look at the lamp on the side table and press {key:Interact} to pick it up.", TutorialCheck.HoldItem, key: "lamp",
                voice: "See the lamp on the side table? Look right at it and pick it up."),
            new("living.drop", 1, "Set it down", "Press {key:Drop} to set it down. Every knock costs value; fragile things lose the most.", TutorialCheck.DropItem, key: "lamp",
                voice: "Set it down. Gently. Every knock costs money, and fragile things lose the most."),
            new("living.throw", 1, "Throw", "Pick up the cushion on the sofa and throw it with {key:Use}. Throwing is fast and loud, and breaks fragile things.",
                TutorialCheck.Throw, amount: 3f, guide: "take item:cushion",
                voice: "Grab the cushion off the sofa and throw it. Throwing is fast, but it's loud, and it breaks anything fragile."),
            new("living.pocket", 1, "Pockets", "Small things go straight into your pockets. Pick up the cash on the coffee table.", TutorialCheck.PocketItem, key: "cash",
                hud: "pockets", voice: "Small stuff goes straight into your pockets. Grab the cash off the coffee table."),
            new("living.inventory", 1, "Your four places", "Hold {key:Inventory} to see them: your gear and your pockets share four places.", TutorialCheck.Pockets,
                hud: "pockets", voice: "Hold the inventory key to see your four places. Your gear and your pockets share them."),
            new("living.slots", 1, "Switch", "Press {key:HandSlot1} or {key:HandSlot2} (or scroll) to choose what's in your hand.", TutorialCheck.SwitchSlot,
                hud: "pockets", voice: "Now switch what's in your hand. The number keys, or the scroll wheel."),

            // 2. The kitchen and dining room: fragile, the truck, unloading pockets, noise, heavy, the trolley.
            new("kitchen.vase", 2, "Fragile", "The vase on the dining table is worth a fortune while it's whole. Pick it up. Scan it as you go: every knock takes value off.",
                TutorialCheck.HoldItem, key: "vase", radio: "DISPATCH: That vase is worth more than the truck. Don't run with it.",
                voice: "The vase on the dining table is worth a fortune, as long as it's in one piece. Pick it up."),
            new("kitchen.vase_truck", 2, "To the truck", "Carry it out to the truck: up the ramp into the cargo bay, and set it down. Walk, don't run.",
                TutorialCheck.ItemInTruck, key: "vase",
                voice: "Carry it out to the truck. Up the ramp, into the cargo bay, and set it down. Walk. Don't run."),
            new("kitchen.unload", 2, "Empty your pockets", "Pockets count in the truck, but they fill up. In the cargo bay, hold {key:Inventory}, choose the cash's place and press {key:Drop}.",
                TutorialCheck.DropItem, key: "cash", guide: "load truck", hud: "pockets",
                voice: "Pockets fill up fast. While you're in the cargo bay, open your inventory, pick the cash, and drop it."),
            new("kitchen.noise", 2, "Noise", "Running, dropping and breaking things all make noise. Something in houses like this hunts by sound.", TutorialCheck.Read, amount: 9f,
                guide: "none", voice: "Running, dropping, breaking things. It all makes noise. And something in houses like this hunts by sound."),
            new("kitchen.trolley", 2, "Heavy things", "The fridge is too heavy to lift alone. Take the hand trolley from the kit crates beside the truck ({key:Interact}).",
                TutorialCheck.HasGear, key: "hand_trolley",
                crew: "The fridge needs two of you, or a hand trolley from the kit crates beside the truck ({key:Interact}). Take one anyway.",
                voice: "The fridge is too heavy to carry alone. Grab the hand trolley from the kit crates by the truck."),
            new("kitchen.fridge", 2, "Move the fridge", "With the trolley in a hand slot, take hold of the fridge ({key:Interact}) and drag it out of its corner. Heavy loads are slow.",
                TutorialCheck.MoveItem, key: "fridge", amount: 2f,
                crew: "Take a handle each ({key:Interact}) and lift the fridge together. You all move at the slowest carrier's pace.",
                voice: "With the trolley in your hand, take hold of the fridge and drag it out of its corner. Heavy loads are slow."),
            new("kitchen.fridge_truck", 2, "Haul it", "Out through the front door and up into the truck. Everything aboard counts toward the quota.",
                TutorialCheck.ItemInTruck, key: "fridge", hud: "haul",
                voice: "Now haul it out the front door and up into the truck. Everything aboard counts toward the quota."),

            // 3. The laundry alcove: doors, something small up high.
            new("laundry.shut", 3, "Doors", "Slide the laundry doors shut ({key:Interact}). A shut door blocks sight, and most things that hunt you can't open one.",
                TutorialCheck.DoorShut, amount: 0f,
                voice: "Slide the laundry doors shut. A shut door blocks sight, and most things that hunt you can't open one."),
            new("laundry.open", 3, "And open", "Open them again.", TutorialCheck.DoorOpen, amount: 0f, voice: "Good. Open them again."),
            new("laundry.watch", 3, "Look up", "There's a watch on the shelf above the machines. Pocket it.", TutorialCheck.PocketItem, key: "watch",
                voice: "There's a watch on the shelf above the machines. Pocket it."),

            // 4. The hallway and the west wing: load limits, backing off, crossing a cracking floor, a floor that fails.
            new("hall.floors", 4, "Floors", "Every floor has a load limit: you, what you carry, what you drag. Creaks mean near the limit, cracks mean it's weakening, and a sag means you have two seconds.",
                TutorialCheck.Read, amount: 11f, radio: "DISPATCH: That hallway's old timber over the basement. Listen to it.", guide: "look section:hallway",
                voice: "Listen up, this part matters. Every floor has a load limit. Creaks mean you're close. Cracks mean it's giving. And a sag means you've got two seconds."),
            new("hall.clock", 4, "The clock", "There's a grandfather clock in the north bedroom. Take hold of it with the trolley in a hand slot.",
                TutorialCheck.HoldItem, key: "clock", crew: "There's a grandfather clock in the north bedroom. Lift it together, or drag it on the trolley.",
                voice: "There's a grandfather clock in the north bedroom. Take hold of it with the trolley."),
            new("hall.listen", 4, "Listen", "Bring it through the hallway toward the kitchen. Listen to the floor.", TutorialCheck.SectionStage, key: "hallway", amount: 1f,
                voice: "Bring it down the hallway, toward the kitchen. And listen to that floor."),
            new("hall.back", 4, "Get off it", "Too much weight: it's cracking. Get everyone off the hallway.", TutorialCheck.OffSection, key: "hallway", amount: 2f,
                voice: "Hear that? It's cracking. Everybody off the hallway. Now."),
            new("hall.across", 4, "Across", "Now take the clock across into the kitchen in one go, with nobody else on the hallway. Don't stop on a cracking floor.",
                TutorialCheck.ItemInArea, key: "clock", area: "kitchen",
                voice: "Now take the clock across into the kitchen. One go, nobody else on the hallway, and don't stop on a cracking floor."),
            new("hall.necklace", 4, "The necklace", "There's a pearl necklace on the nightstand in the south bedroom. Go and get it.",
                TutorialCheck.HoldItem, key: "necklace", radio: "DISPATCH: South bedroom's had a leak for years. Careful.",
                beat: TutorialBeat.ArmCollapse, beatKey: TutorialHouseStructure.BedroomBKey,
                voice: "There's a pearl necklace on the nightstand in the south bedroom. Go and get it."),
            new("hall.fall", 4, "Floors fail", "When a floor sags, get off it: you have about two seconds. Falls hurt, more so with weight in your hands.",
                TutorialCheck.SectionDown, key: TutorialHouseStructure.BedroomBKey,
                voice: "That's what a floor sounds like when it goes. When one sags, get off it. You've got about two seconds."),

            // 5. The basement: getting down, the breaker, getting out.
            new("basement.down", 5, "The basement", "Get to the basement: down through the hole, or by the cellar stairs off the patio.",
                TutorialCheck.EnterArea, area: "basement", radio: "DISPATCH: There's a breaker panel on the furnace wall.",
                voice: "Get down to the basement. Through the hole, or round the back, by the cellar stairs off the patio."),
            new("basement.breaker", 5, "Breaker", "Throw the breaker on the furnace wall ({key:Interact}). Part of the house gets its lights back for a while, and the panel hums: things hear it.",
                TutorialCheck.Breaker,
                voice: "Throw the breaker on the furnace wall. You'll get some lights back for a while. But the panel hums, and things can hear it."),
            new("basement.out", 5, "Out", "Climb out by the cellar stairs onto the patio.", TutorialCheck.EnterArea, area: "patio",
                voice: "Now climb out by the cellar stairs, up onto the patio."),

            // 6. The visitor: something comes in, hide, wait, the crew.
            new("visitor.arrive", 6, "Company", "Something just came in through the patio door. It hunts by sound. Don't run.",
                TutorialCheck.Read, amount: 9f, radio: "DISPATCH: Movement at the back of the house. Get out of sight.",
                beat: TutorialBeat.SpawnThreat, beatKey: "threat_in", guide: "none",
                voice: "Something just came in through the patio door. It hunts by sound. Don't run."),
            new("visitor.hide", 6, "Hide", "Get to the master bedroom and hide in the closet ({key:Interact}, nothing big in your hands). It can't find you in there unless it watched you go in.",
                TutorialCheck.Hide,
                voice: "Get to the master bedroom and hide in the closet. Nothing big in your hands. It can't find you in there, unless it watched you go in."),
            new("visitor.wait", 6, "Wait it out", "Stay hidden. Look around to peek. Wait until it leaves.", TutorialCheck.ThreatGone,
                beat: TutorialBeat.ThreatLeave, beatKey: "threat_out", voice: "Stay put. You can look around to peek. Wait for it to leave."),
            new("visitor.crew", 6, "The crew", "Hold {key:PushToTalk} to talk to people near you, {key:Radio} for the radio. {key:Ping} marks a spot for everyone. If you die, you watch as a ghost until the job ends.",
                TutorialCheck.Read, amount: 11f, guide: "none",
                voice: "On a real job, you'll have a crew. Talk to anyone close by, use the radio for the rest, and ping things for everyone to see. If you die, you watch as a ghost until the job's done."),

            // 7. The ensuite: room in your pockets, the ring.
            new("ensuite.room", 7, "Make room", "Your four places are full. Drop something you're done with: hold {key:Inventory}, choose it, press {key:Drop}.", TutorialCheck.FreeSlot,
                hud: "pockets", voice: "Your four places are full. Drop something you're done with."),
            new("ensuite.ring", 7, "Small fortunes", "The ensuite is through the master bedroom. The ring on the vanity is the best thing in the house. Pocket it.",
                TutorialCheck.PocketItem, key: "ring", radio: "DISPATCH: Always check the bathrooms. People hide the good stuff.",
                voice: "The ensuite's through the master bedroom. The ring on the vanity is the best thing in this house. Pocket it."),
            new("ensuite.pockets", 7, "Pockets ride with you", "Whatever's in your pockets counts when you're aboard the truck. If you go down, it falls where you lie.",
                TutorialCheck.Read, amount: 9f, guide: "none", hud: "pockets",
                voice: "Whatever's in your pockets counts once you're aboard the truck. But if you go down, it drops right where you fall."),

            // 8. The truck: the last load, the quota, the lever.
            new("truck.clock", 8, "Load the clock", "Take the grandfather clock out to the truck and load it.", TutorialCheck.ItemInTruck, key: "clock",
                voice: "Take the grandfather clock out to the truck and load it."),
            new("truck.haul", 8, "The quota", "Scan ({key:Scan}) or read the truck's board. Keep loading until the haul meets the quota.", TutorialCheck.Haul,
                hud: "haul", voice: "Scan, or read the board on the truck. Keep loading until the haul meets the quota."),
            new("truck.lever", 8, "Go home", "Everyone into the cargo bay, then pull the yellow lever ({key:Interact}). It honks, then leaves with whoever is aboard.",
                TutorialCheck.Lever, radio: "DISPATCH: That'll do. Bring it home.",
                voice: "Everyone into the cargo bay, then pull the yellow lever. It honks, then it leaves, with whoever's aboard."),
        };
    }
}
