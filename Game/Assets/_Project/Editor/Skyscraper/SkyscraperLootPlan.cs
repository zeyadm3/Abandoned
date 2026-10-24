using System.Collections.Generic;
using Abandoned.Interaction;
using UnityEngine;
using Tags=Abandoned.EditorTools.SkyscraperLootItems;
namespace Abandoned.EditorTools
{
    public static class SkyscraperLootPlan
    {
        public static List<LootPointSpec> Points()
        {
            var points=new List<LootPointSpec>();
            void Add(string name,string tag,Vector3 at,CarryClass min=CarryClass.Pocket,CarryClass max=CarryClass.OneHand,bool jackpot=false)
                =>points.Add(new LootPointSpec(name,tag,at,min,max,jackpot));
            int i=0;
            foreach(Vector2 table in SkyscraperArchitecture.DiningTables)
            {
                // Leave the dressed place settings free and spawn salvage on the middle of the stone top.
                Add($"Dining_{i++}",Tags.TagDining,new Vector3(table.x,404.91f,table.y));
            }
            Add("HostLedger",Tags.TagDining,new Vector3(3.2f,405.15f,-7.5f));
            foreach(float z in new[]{0f,3f,6f})
                Add($"Bar_{z}",Tags.TagBar,new Vector3(14.7f,405.27f,z),CarryClass.OneHand,CarryClass.TwoHand);
            foreach(float z in new[]{-1f,5f})
                Add($"Backbar_{z}",Tags.TagBar,new Vector3(17.2f,405.25f,z));
            foreach(float z in new[]{12f,15f})
                Add($"KitchenIsland_{z}",Tags.TagKitchen,new Vector3(-12.8f,405.04f,z),CarryClass.OneHand,CarryClass.TwoHand);
            Add("KitchenCookline",Tags.TagKitchen,new Vector3(-16.6f,405.15f,12),CarryClass.TwoHand,CarryClass.Heavy);
            Add("KitchenWash",Tags.TagKitchen,new Vector3(-10,405.05f,20.8f),CarryClass.OneHand,CarryClass.TwoHand);
            Add("ColdStore",Tags.TagKitchen,new Vector3(-16,404.15f,20),CarryClass.TwoHand,CarryClass.TwoHand);
            Add("CellarTasting",Tags.TagCellar,new Vector3(-10,405.04f,-7.8f));
            Add("CellarReserve",Tags.TagCellar,new Vector3(-13,405.08f,-9.3f));
            Add("CellarGrandCask",Tags.TagCellar,new Vector3(-15,404.65f,-7),CarryClass.Huge,CarryClass.Huge,true);
            Add("VIPTable",Tags.TagExecutive,new Vector3(13,SkyscraperLayout.UpperRestaurantY+.99f,17.5f));
            Add("VIPSafe",Tags.TagExecutive,new Vector3(16.2f,SkyscraperLayout.UpperRestaurantY+.8f,14.3f),CarryClass.Heavy,CarryClass.Heavy,true);
            Add("VIPCredenza",Tags.TagExecutive,new Vector3(16,SkyscraperLayout.UpperRestaurantY+1.1f,20.7f));
            Add("Concierge",Tags.TagExecutive,new Vector3(10,1.32f,-6.5f));
            Add("MechanicalWorkbench",Tags.TagKitchen,new Vector3(-5,201.05f,9),CarryClass.OneHand,CarryClass.TwoHand);
            Add("MechanicalTools",Tags.TagKitchen,new Vector3(6,201.05f,6),CarryClass.OneHand,CarryClass.TwoHand);
            foreach(float y in new[]{120f,280f})
            {
                Add($"OfficeDesk_{y}",Tags.TagExecutive,new Vector3(-5,y+1.04f,8.8f));
                Add($"OfficeDeskEast_{y}",Tags.TagExecutive,new Vector3(7,y+1.04f,8.8f));
            }
            foreach(Vector2 table in SkyscraperUpperDining.GalleryTables)
                Add($"Gallery_{table.x}_{table.y}",Tags.TagDining,new Vector3(table.x,SkyscraperLayout.UpperRestaurantY+.99f,table.y));
            Add("ChefsFinishingCounter",Tags.TagKitchen,new Vector3(-13,SkyscraperLayout.UpperRestaurantY+1.2f,20.7f));
            Add("RoofSurvey",Tags.TagExecutive,new Vector3(-5,SkyscraperLayout.RoofY+1.04f,18),CarryClass.OneHand,CarryClass.TwoHand);
            return points;
        }
    }
}
