using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperDowntown
    {
        public static readonly Rect Ground=Rect.MinMaxRect(-360,-338,360,382);
        public const int TowerCount=48;
        public static void Build(Transform exterior,SkyscraperArt art)
        {
            Transform city=Group("Downtown",exterior);
            var palette=new SkyscraperDowntownMaterials();
            SkyscraperDowntownStreets.Build(city,art,palette);
            for(int z=-3;z<=3;z++)for(int x=-3;x<=3;x++)
            {
                if(x==0 && z==0)continue;
                int seed=(x+4)*173+(z+4)*71;
                bool close=Mathf.Abs(x)<=1 && Mathf.Abs(z)<=1;
                float height=close?new[]{388f,366f,342f,396f,372f,354f,382f,326f}[seed%8]:108f+(seed*37%205);
                if(x==0 && z==1)height=396f;
                Vector3 at=new Vector3(x*100f,0,z*100f+22f);
                SkyscraperDowntownBuildings.Tower(city,art,palette,at,height,seed);
            }
            Debug.Log("[Skyscraper] Downtown: 48 neighbouring towers, 720m street district, heights 108–396m.");
        }
    }
}
