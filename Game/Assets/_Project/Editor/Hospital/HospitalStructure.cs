using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;

namespace Abandoned.Hospital
{
    public sealed class HospitalStructure
    {
        private readonly Dictionary<Vector3Int,StructuralSection> tiles=new();
        private readonly HospitalKit k;
        private readonly StructureSceneSetup setup;
        public HospitalStructure(HospitalKit kit,StructureSceneSetup sections) { k=kit;setup=sections; }
        public StructuralSection At(Vector3 p) => tiles.TryGetValue(new Vector3Int(Mathf.FloorToInt(p.x/4),Mathf.RoundToInt(p.y/4),Mathf.FloorToInt(p.z/4)),out var s)?s:null;
        public void Build(Transform root)
        {
            Transform floors=HospitalKit.Group("Structure",root);
            for(int level=0;level<2;level++)
            for(int z=0;z<10;z++) for(int x=0;x<12;x++)
            {
                Transform t=HospitalKit.Group($"Slab_{level}_{x}_{z}",floors);
                t.localPosition=new Vector3(x*4+2,level*4,z*4+2);
                var c=t.gameObject.AddComponent<BoxCollider>(); c.center=Vector3.down*.16f;c.size=new Vector3(4,.32f,4);
                k.Box("Visual",t,c.center,c.size,k.Floor,false);
                bool keep=HospitalLayout.Protected(x,z),fall=level>0&&!keep;
                var s=setup.AddSection(t,SectionType.Floor,fall,fall?.82f:1f,fall?.85f:1f);
                t.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                if(keep)t.gameObject.AddComponent<SectionKeepClear>();
                if(fall)SectionNavCarver.EditorAdd(t.gameObject,c.size);
                tiles[new Vector3Int(x,level,z)]=s;
            }
            Stair(root,-2); Stair(root,50);
        }
        private void Stair(Transform parent,float x)
        {
            Transform t=HospitalKit.Group(x<0?"WestStair":"EastStair",parent);
            t.localPosition=new Vector3(x,0,8);
            Transform visual=HospitalKit.Group("Visual",t);
            Transform ramp=HospitalKit.Group("Ramp",t);
            ramp.localPosition=new Vector3(0,1.9f,4);ramp.localRotation=Quaternion.Euler(-26.565f,0,0);
            ramp.gameObject.AddComponent<BoxCollider>().size=new Vector3(3.2f,.2f,Mathf.Sqrt(80));
            for(int i=0;i<22;i++) k.Box("Tread",visual,new Vector3(0,(i+1)*4f/22-.04f,(i+.5f)*8f/22),new Vector3(3.2f,.08f,8f/22),k.Metal,false);
            foreach(float side in new[]{-1.65f,1.65f})
            {
                Transform guard=HospitalKit.Group("Rail",visual);guard.localPosition=new Vector3(side,2.55f,4);
                guard.localRotation=Quaternion.Euler(-26.565f,0,0);
                k.Box("Guard",guard,Vector3.zero,new Vector3(.06f,1.1f,Mathf.Sqrt(80)),k.Metal);
            }
            setup.AddSection(t,SectionType.Stair,false,fractured:false);t.gameObject.AddComponent<SectionKeepClear>();
            var link=t.gameObject.AddComponent<NavMeshLink>();link.startPoint=new Vector3(0,0,-.7f);link.endPoint=new Vector3(0,4,8.7f);link.width=2.7f;link.bidirectional=true;
            Landing(parent,x,0,6);Landing(parent,x,4,18);
            // The upper landing opens to the cross hall through a genuine side-wall opening.
        }
        private void Landing(Transform parent,float x,float y,float z)
        {
            Transform t=HospitalKit.Group("StairLanding",parent);t.localPosition=new Vector3(x,y,z);
            var c=t.gameObject.AddComponent<BoxCollider>();c.center=Vector3.down*.15f;c.size=new Vector3(4,.3f,4);
            k.Box("Visual",t,c.center,c.size,k.Metal,false);
            setup.AddSection(t,SectionType.Walkway,false,fractured:false);t.gameObject.AddComponent<SectionKeepClear>();
            float outer=x<0?-2:2;
            k.Rail(t,new Vector3(outer,0,-2),new Vector3(outer,0,2));
        }
    }
}
