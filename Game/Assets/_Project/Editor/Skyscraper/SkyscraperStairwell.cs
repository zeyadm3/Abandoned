using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A = Abandoned.EditorTools.SkyscraperArchitecture;

namespace Abandoned.EditorTools
{
    public static class SkyscraperStairwell
    {
        public const int FlightCount = (int)(SkyscraperLayout.RestaurantY/4f);
        public const int HalfFlights=(int)(SkyscraperLayout.RoofY/2f);
        public const float StoryRise = 4f;
        public static List<Transform> Build(Transform root, SkyscraperArt art, out List<Transform> weak)
        {
            Transform g = Group("StairwellTower", root);
            weak = new List<Transform>();
            var landings = new List<Transform>();
            Box("CoreWallWest", g, new Vector3(-15.75f, ((SkyscraperLayout.RoofY+3f)*.5f), 1f), new Vector3(.3f, (SkyscraperLayout.RoofY+3f), 12.6f), art.Concrete);
            Box("CoreWallNorth", g, new Vector3(-12f, ((SkyscraperLayout.RoofY+3f)*.5f), 7.15f), new Vector3(7.8f, (SkyscraperLayout.RoofY+3f), .3f), art.Concrete);
            Box("CoreWallSouth", g, new Vector3(-12f, ((SkyscraperLayout.RoofY+3f)*.5f), -5.15f), new Vector3(7.8f, (SkyscraperLayout.RoofY+3f), .3f), art.Concrete);
            // East wall leaves a 2.4m south doorway at each accessible story and a north roof doorway.
            Box("CoreWallEastMiddle", g, new Vector3(-8.25f, ((SkyscraperLayout.RoofY+3f)*.5f), 1f), new Vector3(.3f, (SkyscraperLayout.RoofY+3f), 5.6f), art.Concrete);
            EastStrip(g, art, -3.5f, new[] {0f, 120f, 200f, 280f, 404f, SkyscraperLayout.RoofY});
            EastStrip(g, art, 5.5f, new[] {SkyscraperLayout.UpperRestaurantY});
            for (int i = 0; i <= HalfFlights; i++)
            {
                float y = i * 2f;
                bool north = i % 2 == 1;
                float z = north ? 5.5f : -3.5f;
                Transform landing = A.Slab(g, new Vector3(-12f, y, z), new Vector2(7.2f, 3f), art.Concrete);
                landing.name = $"Landing_{i:00}";
                landings.Add(landing);
                if (i < HalfFlights) Flight(g, art, i, !north);
                if (north) A.Rail(g, new Vector3(-15.3f, y, 6.8f), new Vector3(-8.7f, y, 6.8f), art.Steel);
            }
            foreach (float y in new[] {0f,120f,200f,280f,404f})
            {
                A.Portal(g, new Vector3(-8.25f,y,-3.5f), 2.4f, 2.8f, art.Steel, true);
                Label(g, "ExitDirection", "EXIT / STAIRS", new Vector3(-8.02f,y+3.05f,-3.5f), Quaternion.Euler(0,-90,0), .09f, Color.white);
            }
            return landings;
        }
        private static void EastStrip(Transform g, SkyscraperArt art, float z, float[] doors)
        {
            float start = 0f;
            foreach (float y in doors)
            {
                if (y > start) Box("CoreWallEastSpandrel",g,new Vector3(-8.25f,(y+start)/2,z),new Vector3(.3f,y-start,3.4f),art.Concrete);
                start = y+2.8f;
            }
            if (start < (SkyscraperLayout.RoofY+3f)) Box("CoreWallEastSpandrel",g,new Vector3(-8.25f,((SkyscraperLayout.RoofY+3f)+start)/2,z),new Vector3(.3f,(SkyscraperLayout.RoofY+3f)-start,3.4f),art.Concrete);
        }
        private static void Flight(Transform parent, SkyscraperArt art, int i, bool north)
        {
            Transform g = Group($"Flight_{i:00}",parent);
            float x = north ? -13.65f : -10.35f, y = i*2f;
            float angle = Mathf.Atan2(2f,6f)*Mathf.Rad2Deg;
            var slope = Box("ContinuousCarryRamp",g,new Vector3(x,y+1f-.12f/Mathf.Cos(angle*Mathf.Deg2Rad),1f),
                new Vector3(2.7f,.24f,Mathf.Sqrt(40f)),art.Concrete);
            slope.transform.localRotation = Quaternion.Euler(north ? -angle : angle,0,0);
            for (int s=0;s<12;s++)
            {
                float z = north ? -2f+(s+.5f)*.5f : 4f-(s+.5f)*.5f;
                Box("Tread",g,new Vector3(x,y+(s+.5f)/6f+.025f,z),new Vector3(2.7f,.05f,.5f),art.Concrete,false);
                Box("Nosing",g,new Vector3(x,y+(s+.5f)/6f+.055f,z+(north ? .22f : -.22f)),new Vector3(2.65f,.012f,.035f),art.Brass,false);
            }
            float railX = north ? -12.22f : -11.78f;
            A.Rail(g,new Vector3(railX,y,north ? -2f : 4f),new Vector3(railX,y+2f,north ? 4f : -2f),art.Steel);
        }
    }
}
