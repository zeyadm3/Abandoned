using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallWalls
    {
        public const float Thickness=.24f;
        public const float WideWidth=4f,WideHeight=3.35f;
        public static int Build(Transform parent,int floor,Material wall,Material frame,Material rail)
        {
            var permanent=Group($"Walls_{floor}",parent);
            var rails=Group($"Railings_{floor}",parent);
            int count=0;
            for(int x=0;x<TilesX;x++) for(int z=0;z<TilesZ;z++)
            {
                var c=new Vector2Int(x,z);
                if(!InFootprint(c)) continue;
                foreach(var (dir,side) in new[]{(Vector2Int.left,'W'),(Vector2Int.right,'E'),(Vector2Int.up,'N'),(Vector2Int.down,'S')})
                {
                    var n=c+dir;
                    Vector3 centre=TileTopCenter(c,floor)+new Vector3(dir.x,0,dir.y)*2;
                    if(!InFootprint(n))
                    {
                        var exit=Exterior.Where(e=>e.floor==floor&&e.side==side&&e.column==(dir.x==0?x:z)).ToArray();
                        WallOpening? opening=exit.Length>0?new WallOpening(WideWidth,0,WideHeight):null;
                        Panel(permanent,$"Outside_{floor}_{x}_{z}_{side}",centre,dir.x!=0,StoryHeight,opening,wall,frame);
                        count++;continue;
                    }
                    if(dir==Vector2Int.left||dir==Vector2Int.down) continue;
                    bool aVoid=IsVoid(c,floor),bVoid=IsVoid(n,floor);
                    if(aVoid&&bVoid) continue;
                    if(aVoid||bVoid)
                    {
                        if(IsFlightOpening(c,n,floor)) continue;
                        Railing(rails,$"Rail_{floor}_{x}_{z}_{side}",centre,dir.x!=0,rail);
                        count++;continue;
                    }
                    Zone? a=ZoneAt(c,floor),b=ZoneAt(n,floor);
                    if(!a.HasValue||!b.HasValue||SameSpace(a.Value,b.Value)) continue;
                    Door? door=DoorBetween(c,n,floor);
                    WallOpening? hole=door.HasValue?new WallOpening(door.Value.Wide?WideWidth:1.8f,0,door.Value.Wide?WideHeight:2.5f):null;
                    Panel(permanent,$"Partition_{floor}_{x}_{z}_{side}",centre,dir.x!=0,StoryHeight-SlabThickness,hole,wall,frame);
                    count++;
                }
            }
            MarkStatic(permanent);
            return count;
        }
        private static void Panel(Transform parent,string name,Vector3 centre,bool alongZ,float height,WallOpening? hole,Material wall,Material frame)
        {
            Transform panel=Group(name,parent);panel.localPosition=centre;
            panel.localRotation=Quaternion.Euler(0,alongZ?90:0,0);
            if(!hole.HasValue) {Box("Solid",panel,Vector3.up*height/2,new Vector3(4,height,Thickness),wall);return;}
            WallOpening h=hole.Value;float edge=(4-h.Width)/2,top=h.Bottom+h.Height;
            if(edge>.001f) foreach(float sign in new[]{-1f,1f}) Box("Jamb",panel,new Vector3(sign*(h.Width/2+edge/2),height/2,0),new Vector3(edge,height,Thickness),wall);
            if(top<height) Box("Header",panel,new Vector3(0,(top+height)/2,0),new Vector3(h.Width,height-top,Thickness),wall);
            if(h.Bottom>0) Box("Sill",panel,new Vector3(0,h.Bottom/2,0),new Vector3(h.Width,h.Bottom,Thickness),wall);
            Box("LintelTrim",panel,new Vector3(0,top+.04f,0),new Vector3(h.Width,.08f,Thickness+.04f),frame,false);
        }
        private static void Railing(Transform parent,string name,Vector3 centre,bool alongZ,Material rail)
        {
            var edge=Box(name,parent,centre+Vector3.up*.525f,alongZ?new Vector3(.1f,1.05f,4):new Vector3(4,1.05f,.1f),rail);
            // These edges move with their adjacent section, so they cannot be occluders or batched statics.
            var tiles=parent.root.Find("Tiles");
            if(tiles!=null)
            {
                var support=tiles.GetComponentsInChildren<Abandoned.Structure.StructuralSection>().OrderBy(s=>Vector3.Distance(s.transform.position,centre)).FirstOrDefault();
                if(support!=null) edge.AddComponent<Abandoned.Structure.SectionProp>().EditorSetup(support);
            }
        }
    }
}
