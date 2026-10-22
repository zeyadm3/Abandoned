using System.Collections.Generic;
using UnityEngine;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Strategic threat spawn coordinates designed for pacing vertical salvage runs.
    /// Threat nodes are situated to cut off stairwell retreats, lurk in shaft pits, and stalk dining halls.
    /// </summary>
    public static class SkyscraperThreatPlan
    {
        public static List<Vector3> Points()
        {
            return new List<Vector3>
            {
                // ---- Ground Plaza & Street Lobby -------------------------------------------------
                new(-16f, L.GroundY + 0.1f, -22f), // Forecourt west perimeter
                new( 16f, L.GroundY + 0.1f, -22f), // Forecourt east perimeter
                new(-10f, L.GroundY + 0.1f,  -4f), // Lobby security alcove
                new(  0f, L.GroundY + 0.1f,   0f), // Ground elevator shaft pit

                // ---- Continuous Fire Stairwell (Ambush Points) -----------------------------------
                new(-12f,  8.1f, -2.4f), // Stairwell story 2
                new(-12f, 20.1f, -2.4f), // Stairwell story 5 (Mechanical transfer)
                new(-12f, 32.1f, -2.4f), // Stairwell story 8
                new(-12f, 40.1f,  2.4f), // Stairwell story 10 (Half-landing before summit)

                // ---- Intermediate Floors (12m & 32m) ---------------------------------------------
                new( -4f, 12.1f,  6f), // Flooded floor burst pipe shadow
                new( -4f, 32.1f,  8f), // Looted corporate office cubicle maze

                // ---- Mechanical Deck (y = 20m) ---------------------------------------------------
                new(-12f, L.MechanicalY + 0.1f, 12f), // Transformer cage shadow
                new( 12f, L.MechanicalY + 0.1f, 12f), // Chiller maintenance aisle
                new(  8f, L.MechanicalY + 0.1f,  0f), // East catwalk overlook

                // ---- Restaurant Floor (y = 44m) --------------------------------------------------
                new( -6f, L.RestaurantY + 0.1f, -4f), // Sky lobby coat check
                new(-14f, L.RestaurantY + 0.1f, -2f), // Commercial kitchen prep corner
                new(-14f, L.RestaurantY + 0.1f, -6f), // Cold store walk-in freezer
                new( -8f, L.RestaurantY + 0.1f,  1f), // Kitchen dishwashing zone
                new(-12f, L.RestaurantY + 0.1f, 18f), // Dining room northwest window corner
                new(  0f, L.RestaurantY + 0.1f, 14f), // Central plinth statue shadow
                new( 12f, L.RestaurantY + 0.1f, 18f), // Dining room northeast window corner
                new( 14f, L.RestaurantY + 0.1f, -2f), // Cocktail bar service aisle
                new( 12f, L.RestaurantY + 0.1f, 14f), // Penthouse private dining room
                new(-16f, L.RestaurantY + 0.1f, 16f), // Outdoor observation terrace

                // ---- Roof Terrace & Window Gantry (y = 50m) ---------------------------------------
                new(  0f, L.RoofY + 0.1f,  8f), // Roof helipad center
                new(  0f, L.RoofY + 0.1f, 19f)  // Window washer gantry track edge
            };
        }
    }
}
