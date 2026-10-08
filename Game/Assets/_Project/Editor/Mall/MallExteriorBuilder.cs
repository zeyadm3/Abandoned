using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallExteriorBuilder
    {
        public static void Build(Transform mall)
        {
            Transform exterior=Group("Exterior",mall),slabs=Group("GroundSlabs",exterior),parking=Group("Parking",exterior);
            Material asphalt=MeridianArt.Material("Asphalt"),stone=MeridianArt.Material("FacadeStone");
            MeridianParkingBuilder.Build(parking);
            Box("WestWalk",slabs,new Vector3(-6,-.2f,24),new Vector3(12,.4f,48),stone);
            Box("RearWalk",slabs,new Vector3(28,-.2f,54),new Vector3(80,.4f,12),asphalt);
            Box("Forecourt",slabs,new Vector3(28,-.197f,2),new Vector3(16,.4f,4),stone);
            Box("DockApron",slabs,new Vector3(58,-.12f,34),new Vector3(4,.25f,4),stone);
            // Setback gaps need real walking ground; the mall mask owns exactly where slabs remain absent.
            for(int x=0;x<TilesX;x++)for(int z=0;z<TilesZ;z++)if(!InFootprint(new Vector2Int(x,z)))
                Box("Setback",slabs,new Vector3(x*4+2,-.2f,z*4+2),new Vector3(4,.4f,4),stone);
            // The entry canopy and low island frame an oblique spawn view, not a featureless wall.
            if(!MeridianArt.UseKit) {
                Box("EntranceCanopy",exterior,new Vector3(28,4.2f,.9f),new Vector3(15,.25f,5),MeridianArt.Material("Metal"),false);
                foreach(float x in new[]{20.3f,35.7f})Box("CanopyPost",exterior,new Vector3(x,2,.4f),new Vector3(.28f,4,.28f),stone);
            }
            MeridianDressingSigns.Sign(exterior,new Vector3(12,7.2f,-.65f),Vector3.back,"DEAD CIRCUIT",6,.4f);
            MeridianDressingSigns.Sign(exterior,new Vector3(56.15f,2.5f,34),Vector3.right,"GOODS IN / CREW OUT",3,.3f);
            foreach(Transform t in exterior.GetComponentsInChildren<Transform>())
                if(t.GetComponent<Collider>()!=null&&t.GetComponent<SurfaceTag>()==null)
                    t.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            MarkStatic(exterior);
        }
        public static void Exits(Transform mall)
        {
            Transform exits=Group("Exits",mall);
            foreach(var e in Exterior) {
                if(!e.window)continue;
                Vector3 at=ExteriorPosition(e),outward=e.side=='W'?Vector3.left:e.side=='E'?Vector3.right:e.side=='N'?Vector3.forward:Vector3.back;
                Transform point=Group($"RopeExit_{e.floor}_{e.side}_{e.column}",exits);
                point.position=at+outward*.9f;
                // The ledge's top remains flush with the doorway; no jumping through a high window sill.
                Transform ledge=Group("Ledge",point);
                Vector3 size=e.side=='W'||e.side=='E'?new Vector3(1.8f,.25f,4):new Vector3(4,.25f,1.8f);
                var collider=ledge.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.down*.125f;collider.size=size;
                if(MeridianArt.UseKit)MeridianArt.Place("FloorConcrete",Group("Visual",ledge),Vector3.zero,0,new Vector3(size.x/4,1,size.z/4));
                else Box("Visual",ledge,collider.center,size,MeridianArt.Material("Concrete"),false);
                point.gameObject.AddComponent<MallFallbackDescent>().EditorSetup(Vector3.up*.6f,at.y+.6f,1.45f);
                Box("Rope",point,new Vector3(0,1.1f-at.y/2,0),new Vector3(.035f,at.y+2.2f,.035f),MeridianArt.Material("BoardWood"),false);
                MeridianDressingSigns.Sign(point,outward*-.55f+Vector3.up*1.8f,-outward,"CREW DESCENT",2.8f,.25f);
            }
        }
    }
}
