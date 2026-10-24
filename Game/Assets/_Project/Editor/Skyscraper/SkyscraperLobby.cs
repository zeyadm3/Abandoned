using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;

namespace Abandoned.EditorTools
{
    public static class SkyscraperLobby
    {
        public static void Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("Lobby",root);
            A.Deck(g,0,a.Marble);
            A.Deck(Group("LobbyCeiling",g),6,a.WallPlaster);
            // A direct, six-metre-wide approach leads from the street to the functioning central car.
            Box("EntryWalkOffMat",g,new Vector3(0,.013f,-8.3f),new Vector3(5.8f,.022f,2.5f),a.GraniteDark,false);
            foreach(float x in new[]{-3.3f,3.3f})
                Box("EntryStoneInlay",g,new Vector3(x,.016f,-6.2f),new Vector3(.045f,.02f,7.3f),a.Brass,false);
            foreach(float x in new[]{-6f,6f})
            {
                Box("BookmatchedStonePier",g,new Vector3(x,2.8f,12),new Vector3(.65f,5.6f,.65f),a.Marble);
                Box("PierShadowPlinth",g,new Vector3(x,.12f,12),new Vector3(.7f,.24f,.7f),a.GraniteDark,false);
            }
            Reception(g,a);
            SkyscraperLobbyDetails.Build(g,a);
        }

        private static void Reception(Transform g,SkyscraperArt a)
        {
            Transform reception=Group("Concierge",g);
            Box("WalnutReceptionWall",reception,new Vector3(12.3f,2.8f,-2.65f),new Vector3(10.5f,5.6f,.2f),a.LobbyWalnut);
            for(float x=7.1f;x<17.5f;x+=1.15f)
                Box("WalnutPanelShadowJoint",reception,new Vector3(x,2.8f,-2.765f),new Vector3(.012f,5.5f,.015f),a.GraniteDark,false);
            Label(reception,"CorporateIdentity","C L O U D L I N E",new Vector3(12,3.35f,-2.79f),Quaternion.identity,.085f,new Color(.83f,.81f,.74f));
            Label(reception,"CorporateSubline","TOWER CONCIERGE  /  APEX RESTAURANT",new Vector3(12,2.75f,-2.79f),Quaternion.identity,.024f,new Color(.75f,.72f,.62f));
            Box("ReceptionShadowPlinth",reception,new Vector3(11,.12f,-6.5f),new Vector3(6.7f,.24f,1.05f),a.GraniteDark);
            Box("ReceptionStoneFront",reception,new Vector3(11,.66f,-6.5f),new Vector3(7.1f,1.08f,1.25f),a.Marble);
            Box("StoneTop",reception,new Vector3(11,1.23f,-6.5f),new Vector3(7.25f,.08f,1.36f),a.Marble);
            foreach(float x in new[]{7.4f,14.6f})
                Box("WalnutDeskReturn",reception,new Vector3(x,.7f,-6.3f),new Vector3(.1f,1.1f,1.55f),a.LobbyWalnut);
            Box("RecessedBrassDeskReveal",reception,new Vector3(11,.27f,-7.14f),new Vector3(6.9f,.035f,.03f),a.Brass,false);
            Box("ReceptionMonitor",reception,new Vector3(12.2f,1.52f,-6.3f),new Vector3(.85f,.48f,.07f),a.GraniteDark,false);
            Box("MonitorFoot",reception,new Vector3(12.2f,1.3f,-6.3f),new Vector3(.26f,.06f,.22f),a.LiftSteel,false);
            Box("VisitorRegister",reception,new Vector3(10,1.285f,-6.65f),new Vector3(.35f,.025f,.25f),a.Leather,false);
            Box("ReceptionTelephone",reception,new Vector3(13.5f,1.31f,-6.4f),new Vector3(.26f,.09f,.2f),a.GraniteDark,false);
            SkyscraperFineDining.Planter(g,a,new Vector3(16.3f,0,-4.5f));
        }
    }
}
