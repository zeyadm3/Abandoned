using System.Collections.Generic;
using UnityEngine;
using F = Abandoned.EditorTools.Remake.KitFamily.Fit;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The map props the remake models (KitSwap), most specific first: a "StudentDesk" is a student desk before it is
    /// a desk. Names come from the saved maps' object names (Game/Logs/remake/maps/*_inventory.json and the survey).
    /// </summary>
    public static class KitFamilies
    {
        public static readonly IReadOnlyList<KitFamily> All = new List<KitFamily>
        {
            // Furniture.
            new("student_desk", @"student_?desk|school_?desk|pupil_?desk", new Vector3(0.7f, 0.75f, 0.55f)),
            new("office_chair", @"office_?chair|swivel|desk_?chair|task_?chair", new Vector3(0.65f, 1.0f, 0.65f), along: false),
            new("desk", @"desk|workstation", new Vector3(1.5f, 0.76f, 0.75f), againstWall: true, not: @"lamp|clutter|light|supplies|organi[sz]er|phone|top_?items|reception|nurse|help_?desk"),
            new("reception_counter", @"reception|nurse_?station|help_?desk|service_?counter|counter|cash_?wrap|front_?desk|bar_?top", new Vector3(2.5f, 1.05f, 0.8f), againstWall: true, not: @"weight|top_?items|stool", tolerance: 3f),
            new("chair", @"chair|seat|stool", new Vector3(0.48f, 0.85f, 0.5f), along: false, not: @"cinema|wheel|lift|arm_?chair|bolted|car_?seat|seating_?row"),
            new("bolted_seat", @"bolted_?seat|seating_?row|bleacher_?seat", new Vector3(1.5f, 0.9f, 0.6f), tolerance: 3f),
            new("armchair", @"arm_?chair|lounge_?chair|recliner", new Vector3(0.85f, 0.9f, 0.85f), along: false),
            new("sofa", @"sofa|couch|settee", new Vector3(2.0f, 0.85f, 0.9f), againstWall: true),
            new("folding_table", @"folding_?table|trestle", new Vector3(1.8f, 0.74f, 0.75f)),
            new("picnic_table", @"picnic", new Vector3(1.8f, 0.75f, 1.5f)),
            new("table", @"table", new Vector3(1.4f, 0.75f, 0.9f), not: @"portable|generator|pool|scanner|patient|desk|bed_?side|night|operating|turn_?table|stable|time_?table|tablet|vegetable|picnic|folding", tolerance: 2.5f),
            new("workbench", @"work_?bench|work_?top|tool_?bench", new Vector3(2.0f, 0.9f, 0.8f), againstWall: true),
            new("bench", @"bench", new Vector3(1.8f, 0.45f, 0.4f), not: @"work|tool|picnic|park|street|court"),
            new("park_bench", @"park_?bench|street_?bench|court_?bench|bus_?bench", new Vector3(1.8f, 0.85f, 0.6f)),
            new("hospital_bed", @"ward_?bed|icu_?bed|hospital_?bed|gurney|stretcher|exam_?bed|exam_?couch", new Vector3(2.1f, 0.9f, 1.0f), againstWall: true),
            new("bunk", @"bunk|cell_?bed|cot\b|cot_|prison_?bed", new Vector3(2.0f, 1.4f, 0.9f), againstWall: true, tolerance: 2.2f),
            new("bed", @"bed\b|bed_|^bed|double_?bed|single_?bed|motel_?bed", new Vector3(2.0f, 0.6f, 1.5f), againstWall: true, not: @"flower|bed_?side|bedrock|truck|bedding|seabed|river"),
            new("bedside", @"bed_?side|night_?stand", new Vector3(0.5f, 0.55f, 0.4f), againstWall: true, not: @"monitor"),
            new("locker", @"locker", new Vector3(0.9f, 1.8f, 0.5f), againstWall: true, not: @"lintel|room|bay|hiding|guard", tolerance: 2.5f),
            new("filing_cabinet", @"filing|file_?cabinet|archive_?drawer", new Vector3(0.5f, 1.3f, 0.65f), againstWall: true),
            new("cabinet", @"cabinet|cupboard|dresser|wardrobe|sideboard|credenza", new Vector3(1.0f, 1.2f, 0.5f), againstWall: true, not: @"filing|file|medicine_?cab|trophy|electrical|breaker|fuse", tolerance: 2.2f),
            new("bookcase", @"bookcase|book_?shel|library_?stack", new Vector3(1.0f, 2.0f, 0.35f), againstWall: true),
            new("shelving", @"shelving|shelves|shelf|rack|gondola|stack_?unit", new Vector3(1.8f, 2.0f, 0.6f), againstWall: true, not: @"hose|squat|turnout|server|crack|book|bike|hat|coat|wine|gun|key|mag|truck|wall_?shelf|roof_?rack|rack_?ing_?beam", tolerance: 2.5f),
            new("whiteboard", @"white_?board|black_?board|chalk_?board|notice_?board|cork_?board", new Vector3(2.4f, 1.2f, 0.05f), F.Hang, againstWall: true, tolerance: 2.5f),
            // Kitchens, bathrooms, services.
            new("sink", @"sink|basin|wash_?stand|lavatory", new Vector3(0.6f, 0.85f, 0.5f), againstWall: true, not: @"heat_?sink"),
            new("toilet", @"toilet|urinal|\bwc\b", new Vector3(0.4f, 0.8f, 0.7f), againstWall: true, not: @"paper|sign|roll|block|room|stall"),
            new("bathtub", @"bath_?tub|\btub\b|bathtub", new Vector3(1.7f, 0.55f, 0.75f), againstWall: true),
            new("fridge", @"fridge|refrigerator|freezer|cooler_?unit", new Vector3(0.75f, 1.8f, 0.75f), againstWall: true, not: @"walk_?in"),
            new("stove", @"stove|cooker|oven|range_?unit|hob\b|hob_|griddle|fryer", new Vector3(0.75f, 0.9f, 0.65f), againstWall: true, not: @"blast|reheat|soaking|furnace"),
            new("washer", @"washer|washing|dryer|laundry_?machine", new Vector3(0.7f, 0.9f, 0.7f), againstWall: true, not: @"pressure|window|dish"),
            new("vending", @"vending", new Vector3(1.0f, 1.85f, 0.85f), againstWall: true),
            new("radiator", @"radiator", new Vector3(1.2f, 0.6f, 0.15f), F.Stretch, againstWall: true, tolerance: 2.5f),
            new("hvac", @"hvac|air_?con|window_?ac|ac_?unit|condenser|air_?handler|rooftop_?unit", new Vector3(1.2f, 1.0f, 0.8f), F.Stretch, tolerance: 3f),
            // Stock and yard.
            new("crate", @"crate", new Vector3(0.8f, 0.7f, 0.8f), along: false, not: @"plastic|sorting|milk"),
            new("plastic_crate", @"plastic_?crate|sorting_?crate|milk_?crate|tote", new Vector3(0.4f, 0.3f, 0.3f), along: false),
            new("barrel", @"barrel|drum\b|drum_|oil_?drum|keg", new Vector3(0.6f, 0.9f, 0.6f), along: false, not: @"gun|brake|drum_?kit|snare"),
            new("pallet", @"pallet", new Vector3(1.2f, 0.15f, 1.0f), along: false, not: @"jack|truck|rack"),
            new("dumpster", @"dumpster|skip\b|skip_|waste_?container|compactor", new Vector3(1.9f, 1.4f, 1.3f), tolerance: 2.2f),
            new("trash_can", @"trash|litter|waste_?bin|garbage|bin\b|bin_|^bin", new Vector3(0.55f, 0.9f, 0.55f), along: false, not: @"cabin|bins_?row|recycl.*station|storage|parts|tool|sort|stock|scrap|grad|label|tote|hand"),
            new("cart", @"cart\b|cart_|trolley|hand_?truck", new Vector3(1.1f, 1.0f, 0.6f), not: @"torch|hand_?truck|go_?kart|cartridge|carton|cartoon|laundry", tolerance: 2.2f),
            // Street.
            new("car", @"parked_?car|deck_?car|^car\b|^car_|sedan|hatchback|station_?wagon|abandoned_?car", new Vector3(4.6f, 1.5f, 1.9f)),
            new("bus_shelter", @"bus_?shelter|bus_?stop", new Vector3(3.9f, 2.6f, 1.7f)),
            new("parking_meter", @"parking_?meter", new Vector3(0.2f, 1.5f, 0.17f), along: false),
            new("traffic_signal", @"traffic_?signal|signal_?head|traffic_?light", new Vector3(5.0f, 6.8f, 0.6f), F.Hang, along: false),
            new("stop_sign", @"stop_?sign", new Vector3(0.8f, 2.7f, 0.06f), along: false),
            new("utility_pole", @"utility_?pole|power_?pole|telegraph|power_?line_?pole", new Vector3(1.7f, 8.5f, 0.3f), along: false, tolerance: 2.2f),
            new("water_tank", @"water_?tank|water_?tower|cistern", new Vector3(3.5f, 5.0f, 3.5f), along: false, tolerance: 2.5f),
            // Architecture fittings (stretch to the opening they sit in).
            new("window", @"window", new Vector3(1.6f, 2.0f, 0.2f), F.Stretch, not: @"campaign|ac\b|_ac|aircon|bars|sill|blind|shutter|cleaning|display|frame_?only|board|sign|sticker|decal|glazing_?run|ribbon|head|lintel|reveal|surround|cill|band|jamb|mullion|transom|spandrel|ledge|box|grille|guard|well", tolerance: 3.5f, perMap: true),
        };
    }
}
