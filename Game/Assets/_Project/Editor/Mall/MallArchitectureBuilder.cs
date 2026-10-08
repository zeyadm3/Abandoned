using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallArchitectureBuilder
    {
        public static void Build(Transform mall)
        {
            Transform walls=Group("Walls",mall);
            for(int floor=0;floor<Floors;floor++) MallWalls.Build(walls,floor,MeridianArt.Material("Plaster"),MeridianArt.Material("Trim"),MeridianArt.Material("Metal"));
            Transform columns=Group("Columns",mall);
            // Columns sit behind the inner edge: the four-metre haul strips remain clear.
            foreach(Vector3 at in new[]{new Vector3(20.45f,0,16.45f),new Vector3(35.55f,0,16.45f),new Vector3(20.45f,0,31.55f),new Vector3(35.55f,0,31.55f)})
            {
                if(MeridianArt.UseKit) {
                    var pillar=Group("Column",columns);pillar.localPosition=at;
                    var c=pillar.gameObject.AddComponent<BoxCollider>();c.center=Vector3.up*6;c.size=new Vector3(.7f,12,.7f);
                    for(int f=0;f<Floors;f++)MeridianArt.Place("Column",pillar,Vector3.up*FloorY(f),0,new Vector3(1,StoryHeight/3.75f,1));
                }
                else Box("Column",columns,at+Vector3.up*6,new Vector3(.7f,12,.7f),MeridianArt.Material("FacadeStone"));
            }
            MarkStatic(columns);
            Transform roof=Group("Roof",mall);
            for(int x=0;x<TilesX;x++) for(int z=0;z<TilesZ;z++) {
                var cell=new Vector2Int(x,z);if(!InFootprint(cell)||IsAtriumVoid(cell))continue;
                Box($"Roof_{x}_{z}",roof,TileTopCenter(cell,3)+Vector3.up*.125f,new Vector3(4,.25f,4),MeridianArt.Material("Concrete"));
            }
            var sky=Group("Skylight",roof);
            // Slender ribs and dirty panes cover the opening; there is no opaque deck over the atrium.
            for(int n=0;n<=4;n++) {
                Box("RibX",sky,new Vector3(20+n*4,12.45f,24),new Vector3(.1f,.22f,16),MeridianArt.Material("Metal"),false);
                Box("RibZ",sky,new Vector3(28,12.45f,16+n*4),new Vector3(16,.22f,.1f),MeridianArt.Material("Metal"),false);
            }
            for(int x=5;x<9;x++) for(int z=4;z<8;z++) if(IsAtriumVoid(new Vector2Int(x,z))&&(x+z)%5!=0)
                Box("DirtyGlass",sky,new Vector3(x*4+2,12.43f,z*4+2),new Vector3(3.9f,.035f,3.9f),MeridianArt.Material("Glass"),false);
            MarkStatic(roof);
            foreach(Renderer pane in sky.GetComponentsInChildren<Renderer>())
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(pane.gameObject,0);
            MallBasementBuilder.Build(mall);
        }
        public static void Ceilings(Transform mall)
        {
            Transform ceilings=Group("Ceilings",mall);
            for(int floor=-1;floor<Floors;floor++) for(int x=0;x<TilesX;x++) for(int z=0;z<TilesZ;z++)
            {
                var c=new Vector2Int(x,z);if(ZoneAt(c,floor)==null)continue;
                StructuralSection support=null;
                if(floor+1<Floors) {
                    support=mall.Find($"Tiles/Floor_{floor+1}/{MallBuilder.TileName(c,floor+1)}")?.GetComponent<StructuralSection>();
                    if(support==null)continue;
                } else if(IsAtriumVoid(c))continue;
                var group=Group($"Ceiling_{floor}_{x}_{z}",ceilings);group.localPosition=TileTopCenter(c,floor+1)-Vector3.up*(SlabThickness+.04f);
                if(MeridianArt.UseKit) MeridianArt.Place("CeilingGrid",group,Vector3.zero);
                else Box("Panels",group,Vector3.down*.015f,new Vector3(3.9f,.03f,3.9f),MeridianArt.Material("Ceiling"),false);
                if(support!=null)group.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            }
        }
        public static void BindRails(Transform mall)
        {
            var sections=mall.Find("Tiles").GetComponentsInChildren<StructuralSection>();
            foreach(Transform t in mall.Find("Walls").GetComponentsInChildren<Transform>())
            {
                if(!t.name.StartsWith("Rail_")||t.GetComponent<SectionProp>()!=null)continue;
                StructuralSection closest=null;float distance=float.PositiveInfinity;
                foreach(var s in sections) {
                    if(Mathf.Abs(s.transform.position.y-(t.position.y-.525f))>.1f)continue;
                    float d=Vector3.Distance(s.transform.position,t.position-Vector3.up*.525f);
                    if(d<distance){closest=s;distance=d;}
                }
                if(closest!=null)t.gameObject.AddComponent<SectionProp>().EditorSetup(closest);
            }
        }
    }
}
