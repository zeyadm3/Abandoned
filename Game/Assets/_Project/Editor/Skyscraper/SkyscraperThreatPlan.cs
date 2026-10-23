using System.Collections.Generic;
using UnityEngine;
namespace Abandoned.EditorTools
{
    public static class SkyscraperThreatPlan
    {
        public static List<Vector3> Points()
        {
            var points=new List<Vector3>{new(-20,.1f,-16),new(20,.1f,-16),new(6,.1f,18)};
            foreach(float y in new[]{40f,200f,280f,400f}) points.Add(new Vector3(-12,y+.1f,-3.5f));
            foreach(float y in new[]{120f,200f,280f}) points.Add(new Vector3(3,y+.1f,17));
            foreach(Vector2 at in new[]{new Vector2(0,17),new Vector2(-10,10),new Vector2(-12,-6),
                new Vector2(10,4),new Vector2(10,19),new Vector2(6,-6)}) points.Add(new Vector3(at.x,404.1f,at.y));
            points.Add(new Vector3(0,410.1f,15));
            return points;
        }
    }
}
