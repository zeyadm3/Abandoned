using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;

namespace Abandoned.Motel
{
    public sealed class MotelStructure
    {
        private readonly Dictionary<Vector3Int,StructuralSection> tiles=new();
        private readonly MotelKit k;
        private readonly StructureSceneSetup setup;
        public MotelStructure(MotelKit kit,StructureSceneSetup sections){k=kit;setup=sections;}
        public StructuralSection At(Vector3 p)=>tiles.TryGetValue(new Vector3Int(Mathf.FloorToInt(p.x/4),Mathf.RoundToInt(p.y/4),Mathf.FloorToInt(p.z/4)),out var s)?s:null;
        public void Build(Transform root)
        {
            Transform group=MotelKit.Group("Structure",root);
            for(int level=0;level<2;level++)for(int z=3;z<7;z++)for(int x=0;x<16;x++)
            {
                if(level==1&&!MotelLayout.UpperCell(x,z))continue;
                Transform t=MotelKit.Group($"Slab_{level}_{x}_{z}",group);t.localPosition=new Vector3(x*4+2,level*4,z*4+2);
                var c=t.gameObject.AddComponent<BoxCollider>();c.center=Vector3.down*.16f;c.size=new Vector3(4,.32f,4);
                k.Box("Visual",t,c.center,c.size,z==3?k.Metal:k.Floor,false);
                bool keep=MotelLayout.Protected(x,z),fall=level>0&&!keep;
                setup.AddSection(t,SectionType.Floor,fall,fall? .7f:1,fall? .78f:1);
                tiles[new Vector3Int(x,level,z)]=t.GetComponent<StructuralSection>();
                t.gameObject.AddComponent<SurfaceTag>().EditorSet(z==3?SurfaceMaterial.Metal:SurfaceMaterial.Carpet);
                if(keep)t.gameObject.AddComponent<SectionKeepClear>();
                if(fall)SectionNavCarver.EditorAdd(t.gameObject,c.size);
            }
            Stair(root,-2);Stair(root,66);
            Transform rails=MotelKit.Group("GalleryRails",root);
            k.Rail(rails,new Vector3(0,4,12),new Vector3(64,4,12));
            // The rail belongs to the protected gallery, not to a failing bedroom slab.
            for(float x=0;x<=64;x+=8)
                k.Box("GalleryColumn",rails,new Vector3(x,2,12),new Vector3(.16f,4,.16f),k.Metal);
        }
        private void Stair(Transform parent,float x)
        {
            Transform t=MotelKit.Group(x<0?"WestStair":"EastStair",parent);t.localPosition=new Vector3(x,0,4);
            Transform visual=MotelKit.Group("Visual",t),ramp=MotelKit.Group("Ramp",t);
            ramp.localPosition=new Vector3(0,1.9f,4);ramp.localRotation=Quaternion.Euler(-26.565f,0,0);
            ramp.gameObject.AddComponent<BoxCollider>().size=new Vector3(3.2f,.2f,Mathf.Sqrt(80));
            for(int i=0;i<22;i++)k.Box("Tread",visual,new Vector3(0,(i+1)*4f/22-.04f,(i+.5f)*8f/22),new Vector3(3.2f,.08f,8f/22),k.Metal,false);
            foreach(float side in new[]{-1.65f,1.65f})
            {
                Transform g=MotelKit.Group("StairGuard",visual);g.localPosition=new Vector3(side,2.55f,4);g.localRotation=Quaternion.Euler(-26.565f,0,0);
                k.Box("Guard",g,Vector3.zero,new Vector3(.06f,1.1f,Mathf.Sqrt(80)),k.Metal);
            }
            setup.AddSection(t,SectionType.Stair,false,fractured:false);t.gameObject.AddComponent<SectionKeepClear>();
            var link=t.gameObject.AddComponent<NavMeshLink>();link.startPoint=new Vector3(0,0,-.7f);link.endPoint=new Vector3(0,4,8.7f);link.width=2.7f;link.bidirectional=true;
            foreach(float y in new[]{0f,4f})
            {
                Transform land=MotelKit.Group("StairLanding",parent);land.localPosition=new Vector3(x,y,y==0?2:14);
                var c=land.gameObject.AddComponent<BoxCollider>();c.center=Vector3.down*.15f;c.size=new Vector3(4,.3f,4);
                k.Box("Visual",land,c.center,c.size,k.Metal,false);setup.AddSection(land,SectionType.Walkway,false,fractured:false);
                land.gameObject.AddComponent<SectionKeepClear>();
                k.Rail(land,new Vector3(x<0?-2:2,0,-2),new Vector3(x<0?-2:2,0,2));
            }
        }
    }
}
