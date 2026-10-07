using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallProps
    {
        public static int Place(Transform root,Transform tiles)
        {
            Transform props=GreyboxFactory.Group("Props",root);
            int count=0;
            for(int f=0;f<Floors;f++)
            foreach(Zone zone in ZonesByFloor[f])
            {
                if(zone.Kind!=Kind.Store||zone.Name=="ServicePassage")continue;
                for(int z=zone.Area.yMin+1;z<zone.Area.yMax;z+=2)
                {
                    var c=new Vector2Int(zone.Area.xMin,z);
                    if(IsVoid(c,f)||Flights.Any(fl=>fl.Floor==f&&fl.Tiles.Contains(c))||MallLootPoints.IsJackpotTile(c,f))continue;
                    StructuralSection section=tiles.Find($"Floor_{f}/{MallBuilder.TileName(c,f)}")?.GetComponent<StructuralSection>();
                    string model=zone.Tag switch{"clothing"=>"mannequin","cinema"=>"CinemaSeat","office"=>"SecurityDesk","furniture"=>"bench",_=>"shelf"};
                    GameObject prop=CustomMallArt.Place(model,props,TileTopCenter(c,f)+new Vector3(-1.25f,0,0.3f),Quaternion.Euler(0,90,0));
                    prop.AddComponent<SectionProp>().EditorSetup(section);
                    var box=prop.AddComponent<BoxCollider>();box.center=Vector3.up*0.8f;box.size=new Vector3(0.65f,1.6f,1.5f);
                    count++;
                }
            }
            CustomMallArt.Place("fountain",props,new Vector3(28,0,26),Quaternion.identity);
            foreach(Vector3 at in new[]{new Vector3(18,0,16),new Vector3(38,0,28)})
            {
                GameObject kiosk=CustomMallArt.Place("kiosk",props,at,Quaternion.identity);
                var box=kiosk.AddComponent<BoxCollider>();box.center=new Vector3(0,1.3f,0);box.size=new Vector3(2.6f,2.6f,1.9f);count++;
            }
            for(int i=0;i<6;i++)
            {
                CustomMallArt.Place("FoodCourtTable",props,new Vector3(4+i*5,0,44),Quaternion.Euler(0,i*13,0));
                CustomMallArt.Place("counter",props,new Vector3(3+i*6,0,47),Quaternion.identity);
            }
            for(int i=0;i<3;i++)CustomMallArt.Place("bench",props,new Vector3(18+10*i,0,8),Quaternion.identity);
            for(int row=0;row<6;row++)for(int col=0;col<3;col++)
                CustomMallArt.Place("CinemaSeat",props,new Vector3(2+col*1.2f,8,8+row*3),Quaternion.Euler(0,180,0));
            MallMysteryBuilder.Sign(props,new Vector3(7.5f,10,1),Vector3.forward,"THE LAST SCREENING",7,1.8f);
            return count+30;
        }
        public static void ParkVehicles(Transform exterior)
        {
            Vehicles.Park(exterior,Vehicles.Delivery,new Vector3(7,0,-12),82,new Vector3(2.2f,2.6f,4.6f));
            Vehicles.Park(exterior,Vehicles.Van,new Vector3(46,0,-13),-15,new Vector3(2,2.1f,4.4f));
        }
    }
}
