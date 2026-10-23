using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Interaction;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelInteriors
    {
        public static List<LootPointSpec> Build(Transform root, MotelKit kit, MotelStructure floors)
        {
            Transform rooms = MotelKit.Group("Interiors", root);
            var loot = new List<LootPointSpec>();
            for (int level = 0; level <2; level++)
            {
                for (int room = 0; room <8; room++)
                {
                    float x = MotelLayout.RoomX(room);
                    float y = level * 4f;
                    Transform r = MotelKit.Group("Room_" + MotelLayout.RoomNumber(level, room), rooms);
                    MotelGuestProps.Dress(r, kit, floors, room, level);
                    MotelHiding.Build(r, kit, new Vector3(x + .4f, y, 17.6f), 90f, 96 + level * 8 + room, floors.At(new Vector3(x + .4f, y, 17.6f)));
                    loot.Add(new LootPointSpec($"Motel_{level}_{room}_Bedside", "motel_guest", new Vector3(x + 4.8f, y + .05f, 19f), CarryClass.Pocket, CarryClass.TwoHand));
                    loot.Add(new LootPointSpec($"Motel_{level}_{room}_Luggage", "motel_guest", new Vector3(x + 4.8f, y + .05f, 21.3f), CarryClass.OneHand, CarryClass.Heavy));
                    loot.Add(new LootPointSpec($"Motel_{level}_{room}_Bathroom", "motel_bath", new Vector3(x + 3.5f, y + .05f, 26.5f), CarryClass.Pocket, CarryClass.OneHand));
                    if (room == 7)
                    {
                        loot.Add(new LootPointSpec("Motel_SuitePrize_" + level, "motel_suite_prize", new Vector3(x + 4f, y + .05f, 20f), CarryClass.Huge, CarryClass.Huge, true));
                    }
                }
            }
            MotelAmenities.Dress(rooms, kit, floors, loot);
            MotelAnnex.Dress(rooms, kit, floors, loot);
            MotelCommonDetails.Build(root, kit, floors);
            return loot;
        }
    }
}
