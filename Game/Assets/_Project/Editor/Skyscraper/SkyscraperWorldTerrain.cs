using System.Linq;
using Abandoned.World;
using UnityEditor;
using UnityEngine;
namespace Abandoned.EditorTools
{
    /// <summary>Fits only the expanded downtown site into existing world terrain; other sites retain their saved data.</summary>
    public static class SkyscraperWorldTerrain
    {
        public static void Fit(WorldPoi poi)
        {
            Rect site=poi.WorldGround;
            const float feather=100f;
            int changed=0;
            foreach(Terrain terrain in Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                TerrainData data=terrain.terrainData;if(data==null)continue;
                Vector3 origin=terrain.transform.position,size=data.size;
                Rect tile=new Rect(origin.x,origin.z,size.x,size.z);
                Rect reach=Rect.MinMaxRect(site.xMin-feather,site.yMin-feather,site.xMax+feather,site.yMax+feather);
                if(!tile.Overlaps(reach))continue;
                int res=data.heightmapResolution;
                float[,] heights=data.GetHeights(0,0,res,res);
                float target=(poi.worldPosition.y+poi.groundHeight-.05f-origin.y)/size.y;
                for(int z=0;z<res;z++)for(int x=0;x<res;x++)
                {
                    float wx=origin.x+x*size.x/(res-1),wz=origin.z+z*size.z/(res-1);
                    float dx=Mathf.Max(site.xMin-wx,0,wx-site.xMax),dz=Mathf.Max(site.yMin-wz,0,wz-site.yMax);
                    float distance=Mathf.Sqrt(dx*dx+dz*dz);
                    if(distance<feather)heights[z,x]=Mathf.Lerp(heights[z,x],target,1-Mathf.SmoothStep(0,1,distance/feather));
                }
                data.SetHeights(0,0,heights);
                int holesRes=data.holesResolution;
                bool[,] solid=data.GetHoles(0,0,holesRes,holesRes);
                for(int z=0;z<holesRes;z++)for(int x=0;x<holesRes;x++)
                    if(site.Contains(new Vector2(origin.x+(x+.5f)*size.x/holesRes,origin.z+(z+.5f)*size.z/holesRes)))solid[z,x]=false;
                data.SetHoles(0,0,solid);
                Rect clear=Rect.MinMaxRect(site.xMin-8,site.yMin-8,site.xMax+8,site.yMax+8);
                data.SetTreeInstances(data.treeInstances.Where(tree=>!clear.Contains(new Vector2(origin.x+tree.position.x*size.x,origin.z+tree.position.z*size.z))).ToArray(),true);
                int detailRes=data.detailResolution;
                for(int layer=0;layer<data.detailPrototypes.Length;layer++)
                {
                    int[,] density=data.GetDetailLayer(0,0,detailRes,detailRes,layer);
                    for(int z=0;z<detailRes;z++)for(int x=0;x<detailRes;x++)
                        if(clear.Contains(new Vector2(origin.x+(x+.5f)*size.x/detailRes,origin.z+(z+.5f)*size.z/detailRes)))density[z,x]=0;
                    data.SetDetailLayer(0,0,layer,density);
                }
                EditorUtility.SetDirty(data);terrain.Flush();changed++;
            }
            Debug.Log($"[Skyscraper] Fitted downtown terrain and removed forest from its street grid ({changed} world tiles).");
        }
    }
}
