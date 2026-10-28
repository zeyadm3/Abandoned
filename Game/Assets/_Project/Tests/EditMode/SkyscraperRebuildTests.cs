using System.Linq;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Tests
{
    public class SkyscraperRebuildTests
    {
        [OneTimeSetUp]
        public void Open()
        {
            EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath, OpenSceneMode.Single);
            Physics.SyncTransforms();
        }

        private static Vector3 OnNav(Vector3 at)
        {
            Assert.IsTrue(NavMesh.SamplePosition(at, out NavMeshHit hit, .65f, NavMesh.AllAreas), "Missing walkable surface at " + at);
            Assert.That(Mathf.Abs(hit.position.y - at.y), Is.LessThan(.5f), "Sample jumped to another floor at " + at);
            return hit.position;
        }

        [TestCase(0f)] [TestCase(120f)] [TestCase(200f)] [TestCase(280f)] [TestCase(404f)]
        public void StairExitHasAFullHeightClearOpening(float y)
        {
            Assert.IsFalse(Physics.CheckCapsule(new Vector3(-8.25f,y+.55f,-3.5f),new Vector3(-8.25f,y+1.65f,-3.5f),.42f,
                ~0,QueryTriggerInteraction.Ignore), "Stair doorway is blocked at " + y);
        }

        [TestCase(0f)] [TestCase(120f)] [TestCase(200f)] [TestCase(280f)] [TestCase(404f)]
        public void StairExitConnectsToItsFloor(float y)
        {
            var path=new NavMeshPath();
            Assert.IsTrue(NavMesh.CalculatePath(OnNav(new Vector3(-12,y,-3.5f)),OnNav(new Vector3(-5,y,-3.5f)),NavMesh.AllAreas,path));
            Assert.That(path.status,Is.EqualTo(NavMeshPathStatus.PathComplete),"Stair exit disconnected at "+y);
        }

        [TestCase(120f)] [TestCase(200f)] [TestCase(280f)] [TestCase(404f)] [TestCase(SkyscraperLayout.UpperRestaurantY)] [TestCase(SkyscraperLayout.RoofY)]
        public void ContinuousStairsReachEachDestinationFromTheStreet(float y)
        {
            var path=new NavMeshPath();
            Vector3 end=new Vector3(-7,y,y==SkyscraperLayout.UpperRestaurantY?5.5f:-3.5f);
            Assert.IsTrue(NavMesh.CalculatePath(OnNav(new Vector3(-4,0,-18)),OnNav(end),NavMesh.AllAreas,path));
            Assert.That(path.status,Is.EqualTo(NavMeshPathStatus.PathComplete),"No complete extraction route to "+end);
        }

        [TestCase(11.5f,-7f)] [TestCase(16f,-7f)] [TestCase(-12f,-7f)] [TestCase(-10.5f,10f)] [TestCase(0f,18f)] [TestCase(12f,11f)] [TestCase(10f,19f)]
        public void RestaurantRoomsConnectToTheLiftLobby(float x,float z)
        {
            var path=new NavMeshPath();
            Assert.IsTrue(NavMesh.CalculatePath(OnNav(new Vector3(0,404,-6)),OnNav(new Vector3(x,404,z)),NavMesh.AllAreas,path));
            Assert.That(path.status,Is.EqualTo(NavMeshPathStatus.PathComplete));
        }

        [TestCase(0f)] [TestCase(200f)] [TestCase(404f)]
        public void LiftCallButtonsAreExposedOnTheLanding(float y)
        {
            Assert.IsTrue(Physics.Raycast(new Vector3(1.75f,y+1.2f,-3.6f),Vector3.forward,out RaycastHit hit,1.3f,~0,QueryTriggerInteraction.Ignore));
            Assert.IsNotNull(hit.collider.GetComponent<Abandoned.Skyscraper.SkyscraperElevatorButton>(),"The call button is covered by "+hit.collider.name);
        }

        [Test]
        public void NoStationarySlabCrossesTheCabTravelVolume()
        {
            foreach(float y in new[]{6f,120f,123.5f,200f,205f,280f,283.5f,404f})
                Assert.IsFalse(Physics.CheckBox(new Vector3(0,y-.2f,0),new Vector3(1.65f,.17f,1.65f),Quaternion.identity,~0,QueryTriggerInteraction.Ignore),"Shaft blocked at "+y);
        }

        [Test]
        public void CabinWallsAndControlsTravelWithTheDeck()
        {
            Transform deck=GameObject.Find("Skyscraper").transform.Find("ElevatorShaft/ElevatorCab/CabDeck");
            Transform body=deck.Find("CabBody"), panel=deck.Find("CarButtonPanel");
            Assert.IsNotNull(body);Assert.IsNotNull(panel);
            Vector3 before=body.position;
            try
            {
                deck.GetComponent<MovingPlatform>().MoveTo(deck.position+Vector3.up*20);
                Assert.That(body.position.y-before.y,Is.EqualTo(20).Within(.001f));
                Assert.That(panel.position.y,Is.EqualTo(21.2f).Within(.001f));
            }
            finally { deck.position=Vector3.zero; Physics.SyncTransforms(); }
        }

        [TestCase(0f)] [TestCase(200f)] [TestCase(404f)]
        public void ParkedCabHasContinuousSupportAcrossItsLandingSill(float y)
        {
            Transform deck=GameObject.Find("Skyscraper").transform.Find("ElevatorShaft/ElevatorCab/CabDeck");
            Vector3 before=deck.position;
            try
            {
                deck.GetComponent<MovingPlatform>().MoveTo(new Vector3(0,y,0));Physics.SyncTransforms();
                foreach(float x in new[]{-.7f,0,.7f})
                for(float z=-1.5f;z>=-2.7f;z-=.05f)
                {
                    Assert.IsTrue(Physics.Raycast(new Vector3(x,y+.15f,z),Vector3.down,out RaycastHit hit,.35f,~0,QueryTriggerInteraction.Ignore),"Uncovered shaft gap at "+x+","+z+" on "+y);
                    Assert.That(hit.point.y,Is.EqualTo(y).Within(.025f));
                }
            }
            finally{deck.position=before;Physics.SyncTransforms();}
        }

        [Test]
        public void CorporateLobbyLeavesTheRealLiftAndStairApproachClear()
        {
            Assert.IsNotNull(GameObject.Find("Skyscraper").transform.Find("Lobby/Concierge/WalnutReceptionWall"));
            Assert.IsFalse(Physics.CheckCapsule(new Vector3(0,.55f,-2.5f),new Vector3(0,1.65f,-2.5f),.35f,~0,QueryTriggerInteraction.Ignore));
            Transform cab=GameObject.Find("Skyscraper").transform.Find("ElevatorShaft/ElevatorCab/CabDeck");
            Assert.IsNotNull(cab.Find("CabPassengerDoors").GetComponent<Abandoned.Skyscraper.SkyscraperElevatorDoor>());
        }

        [Test]
        public void SkyscraperSalvageHasOneConfiguredNetworkLootComponentPerPrefab()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<Abandoned.Loot.LootCatalog>(SkyscraperLoot.DataFolder+"/SkyscraperCatalog.asset");
            foreach(var entry in catalog.Entries)
            {
                var components=entry.prefab.GetComponents<Abandoned.Networking.NetworkLoot>();
                Assert.That(components.Length,Is.EqualTo(1),entry.prefab.name+" has duplicate loot network behaviours.");
                Assert.IsNotNull(components[0].Config,entry.prefab.name+" has no network loot configuration.");
            }
        }

        [Test]
        public void RestaurantSitsAtCloudHeightOnATrueSkyscraper()
        {
            Assert.That(SkyscraperLayout.RestaurantY,Is.EqualTo(404f));
            Assert.That(SkyscraperLayout.RoofY,Is.EqualTo(416f));
            var cloud=Object.FindFirstObjectByType<Abandoned.Skyscraper.SkyscraperCloudLayer>();
            Assert.IsNotNull(cloud);
            Assert.That(cloud.GetComponent<Renderer>().bounds.max.y,Is.GreaterThan(SkyscraperLayout.RestaurantY));
            Assert.That(cloud.GetComponent<Renderer>().bounds.min.y,Is.LessThan(SkyscraperLayout.RestaurantY));
            Assert.IsNull(cloud.GetComponent<Collider>());
        }


        [TestCase(-10f,6f)] [TestCase(-10f,10f)] [TestCase(10f,19f)] [TestCase(12f,11f)] [TestCase(0f,-3f)]
        public void UpperDiningRoomsHaveACompleteRouteFromArrival(float x,float z)
        {
            var path=new NavMeshPath();
            Assert.IsTrue(NavMesh.CalculatePath(OnNav(new Vector3(0,404,-6)),
                OnNav(new Vector3(x,SkyscraperLayout.UpperRestaurantY,z)),NavMesh.AllAreas,path));
            Assert.That(path.status,Is.EqualTo(NavMeshPathStatus.PathComplete),"Upper room is isolated at "+x+","+z);
        }

        [Test]
        public void GrandStairConnectsBothStoriesWithoutDetouringThroughTheFireCore()
        {
            var path=new NavMeshPath();
            Assert.IsTrue(NavMesh.CalculatePath(OnNav(new Vector3(5.8f,404,-7)),
                OnNav(new Vector3(8.6f,410,-8.4f)),NavMesh.AllAreas,path));
            Assert.That(path.status,Is.EqualTo(NavMeshPathStatus.PathComplete));
            float length=0;
            for(int i=1;i<path.corners.Length;i++)length+=Vector3.Distance(path.corners[i-1],path.corners[i]);
            Assert.That(length,Is.LessThan(32f),"The route bypasses the grand staircase.");
        }

        [TestCase(true)] [TestCase(false)]
        public void GrandStairFlightsHaveCarryWidthAndStandingHeadroom(bool north)
        {
            for(int i=1;i<20;i++)
            {
                float t=i/20f;
                Vector3 at=new Vector3(north?5.8f:8.6f,404+(north?0:3)+t*3,north?-6.5f+t*9:2.5f-t*9);
                Assert.IsFalse(Physics.CheckCapsule(at+Vector3.up*.62f,at+Vector3.up*1.65f,.40f,~0,QueryTriggerInteraction.Ignore),
                    "Carry route obstructed at "+at);
            }
        }

        [Test]
        public void AtriumIsOpenAndGalleryTilesParticipateInStructuralSimulation()
        {
            Assert.IsFalse(Physics.CheckBox(new Vector3(0,409.8f,12),new Vector3(SkyscraperArchitecture.DiningAtrium.width*.5f-.5f,.15f,5.5f),Quaternion.identity,~0,QueryTriggerInteraction.Ignore));
            Transform upper=GameObject.Find("Skyscraper").transform.Find("UpperDining_410m/FloorSlabs");
            Assert.IsNotNull(upper);
            Assert.IsTrue(upper.Cast<Transform>().All(t=>t.GetComponent<StructuralSection>()!=null));
            Assert.IsTrue(Object.FindObjectsByType<StructuralSection>(FindObjectsSortMode.None).All(s=>s.Visual!=null),
                "Structural slabs must have a visual to hide on collapse.");
            Transform chandelier=GameObject.Find("Skyscraper").transform.Find("Lights/SignatureAtriumChandelier");
            Assert.IsNotNull(chandelier);
            Material glow=AssetDatabase.LoadAssetAtPath<Material>(SkyscraperArt.MaterialsFolder+"/Lamp_WarmGlow.mat");
            Assert.IsTrue(glow.IsKeywordEnabled("_EMISSION"),"Chandelier emission must survive material reimport.");
        }

        [Test]
        public void CloudsCompileAndCannotOccludeTheWholeBuilding()
        {
            Shader shader=Shader.Find("Abandoned/SummitClouds");
            Assert.IsNotNull(shader);Assert.IsFalse(ShaderUtil.ShaderHasError(shader));
            var cloud=Object.FindFirstObjectByType<Abandoned.Skyscraper.SkyscraperCloudLayer>();
            Assert.That(GameObjectUtility.GetStaticEditorFlags(cloud.gameObject)&StaticEditorFlags.OccluderStatic,Is.EqualTo((StaticEditorFlags)0));
        }

        [Test]
        public void DowntownHasTallNeighboursOnEverySideWithoutBlockingTheEntrance()
        {
            Transform downtown=GameObject.Find("Skyscraper").transform.Find("Exterior/Downtown");
            Assert.IsNotNull(downtown);
            var towers=downtown.Cast<Transform>().Where(t=>t.name.StartsWith("CityTower_")).ToArray();
            Assert.That(towers.Length,Is.EqualTo(48));
            foreach(Vector3 axis in new[]{Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
                Assert.IsTrue(towers.Any(t=>Vector3.Dot(t.position,axis)>60 && t.GetComponent<BoxCollider>().size.y>300),"Missing high-rise neighbours along "+axis);
            Assert.IsFalse(Physics.CheckCapsule(new Vector3(0,.55f,-10),new Vector3(0,1.65f,-10),.42f,~0,QueryTriggerInteraction.Ignore));
        }

        [Test]
        public void TallStairNavigationUsesTwoBoundedSurfacesAndAPhysicalFlightJoin()
        {
            var root=GameObject.Find("Skyscraper");
            var surfaces=root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>();
            Assert.That(surfaces.Length,Is.EqualTo(2));
            Assert.IsTrue(surfaces.All(surface=>surface.collectObjects==Unity.AI.Navigation.CollectObjects.Volume && surface.size.y<252));
            var join=root.GetComponentInChildren<Unity.AI.Navigation.NavMeshLink>();
            Assert.IsNotNull(join);Assert.IsTrue(join.bidirectional);
            foreach(Vector3 local in new[]{join.startPoint,join.endPoint})
                Assert.IsTrue(Physics.Raycast(join.transform.TransformPoint(local)+Vector3.up*.3f,Vector3.down,.5f,~0,QueryTriggerInteraction.Ignore));
        }

        [Test]
        public void MainStairsRemainProtectedWhenWindowBaysCollapse()
        {
            var stairs=GameObject.Find("Skyscraper").transform.Find("StairwellTower");
            var sections=stairs.GetComponentsInChildren<StructuralSection>();
            Assert.That(sections.Length,Is.EqualTo(SkyscraperStairwell.HalfFlights+1));
            Assert.IsTrue(sections.All(s=>!s.CanCollapse && s.GetComponent<SectionKeepClear>()!=null));
            Assert.That(Object.FindObjectsByType<StructuralSection>(FindObjectsSortMode.None).Count(s=>s.CanCollapse),Is.GreaterThan(0));
        }
    }
}
