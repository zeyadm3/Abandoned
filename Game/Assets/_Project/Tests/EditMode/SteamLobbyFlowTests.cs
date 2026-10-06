using System.Threading.Tasks;
using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class SteamLobbyFlowTests
    {
        private const ulong HostId = 76561190000000099UL;
        private FakeSteamLobbies lobbies;
        private FakeLobbySession session;
        private SteamLobbyFlow flow;

        [SetUp]
        public void SetUp()
        {
            lobbies = new FakeSteamLobbies();
            session = new FakeLobbySession();
            flow = new SteamLobbyFlow(lobbies, session);
        }

        // The fake completes synchronously, so these tasks are already done when they return.
        private static T Done<T>(Task<T> task)
        {
            Assert.IsTrue(task.IsCompleted, "fake lobby calls complete immediately");
            return task.Result;
        }

        private FakeSteamLobbies.FakeLobby FriendsLobby(string version = "0.3.0+abc1234", int max = 4)
        {
            FakeSteamLobbies.FakeLobby lobby = lobbies.Add(HostId, "Host", max);
            lobby.Data[SteamLobbyFlow.VersionKey] = version;
            lobby.Data[SteamLobbyFlow.HostKey] = HostId.ToString();
            return lobby;
        }

        [Test]
        public void HostingOverSteamOpensAFriendsOnlyLobbyTaggedWithOurBuild()
        {
            session.Host();
            Assert.IsTrue(flow.OnHostStarted().IsCompleted);
            Assert.IsTrue(flow.InLobby);
            FakeSteamLobbies.FakeLobby lobby = lobbies.Lobbies[flow.LobbyId];
            Assert.IsTrue(lobby.FriendsOnly);
            Assert.AreEqual(4, lobby.MaxMembers, "lobby size = MaxPlayers, so Steam itself refuses a fifth");
            Assert.AreEqual(session.CompatibilityKey, lobby.Data[SteamLobbyFlow.VersionKey]);
            Assert.AreEqual(session.LocalSteamId.ToString(), lobby.Data[SteamLobbyFlow.HostKey]);
            Assert.AreEqual(1, flow.Members.Count);
            Assert.IsEmpty(flow.LastError);
        }

        [Test]
        public void UnityTransportHostingOpensNoLobby()
        {
            session.UsesSteamTransport = false;
            session.Host();
            flow.OnHostStarted();
            Assert.IsFalse(flow.InLobby);
            Assert.IsEmpty(lobbies.Lobbies);
        }

        [Test]
        public void SteamRefusingTheLobbyIsAClearError()
        {
            lobbies.RefuseCreate = true;
            session.Host();
            flow.OnHostStarted();
            Assert.IsFalse(flow.InLobby);
            Assert.AreEqual(LobbyMessages.CreateFailed, flow.LastError);
        }

        [Test]
        public void HostStoppingWhileSteamCreatesTheLobbyLeavesItAgain()
        {
            lobbies.CreateGate = new TaskCompletionSource<bool>();
            session.Host();
            Task pending = flow.OnHostStarted();
            session.Stop();
            flow.OnSessionStopped();
            lobbies.CreateGate.SetResult(true);
            Assert.IsTrue(pending.IsCompleted);
            Assert.IsFalse(flow.InLobby);
            Assert.AreEqual(1, lobbies.Left.Count, "the late lobby is left, not abandoned open");
        }

        [Test]
        public void EndingTheSessionLeavesTheLobby()
        {
            session.Host();
            flow.OnHostStarted();
            ulong id = flow.LobbyId;
            session.Stop();
            flow.OnSessionStopped();
            Assert.IsFalse(flow.InLobby);
            CollectionAssert.Contains(lobbies.Left, id);
        }

        [Test]
        public void HostInvitesThroughTheOverlayAndSeesMembersArrive()
        {
            session.Host();
            flow.OnHostStarted();
            int changes = 0;
            flow.Changed += () => changes++;
            flow.OpenInviteOverlay();
            CollectionAssert.AreEqual(new[] { flow.LobbyId }, lobbies.InvitesOpened);
            lobbies.MemberJoins(flow.LobbyId, 42UL, "Friend");
            Assert.AreEqual(2, flow.Members.Count);
            Assert.AreEqual("Friend", flow.Members[1].Name);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void AcceptingAnInviteJoinsTheLobbyAndConnectsToItsHost()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby();
            lobbies.AcceptInvite(lobbies.IdOf(lobby));
            Assert.AreEqual(HostId, session.JoinedHost);
            Assert.IsTrue(flow.InLobby);
            Assert.IsEmpty(flow.LastError);
            Assert.AreEqual(2, lobby.Members.Count);
        }

        [Test]
        public void AnotherBuildIsRefusedBeforeConnecting()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby("0.3.0+fffffff");
            Assert.IsFalse(Done(flow.JoinAsync(lobbies.IdOf(lobby))));
            Assert.AreEqual(0UL, session.JoinedHost);
            Assert.IsFalse(flow.InLobby);
            StringAssert.Contains("0.3.0 (fffffff)", flow.LastError);
            StringAssert.Contains("0.3.0 (abc1234)", flow.LastError);
            CollectionAssert.Contains(lobbies.Left, lobbies.IdOf(lobby));
        }

        [Test]
        public void AFullLobbyIsRefused()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby(max: 1);
            Assert.IsFalse(Done(flow.JoinAsync(lobbies.IdOf(lobby))));
            Assert.AreEqual(LobbyMessages.Full, flow.LastError);
            Assert.AreEqual(0UL, session.JoinedHost);
        }

        [Test]
        public void InvitesWhileAlreadyPlayingAreRefused()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby();
            session.Host();
            lobbies.AcceptInvite(lobbies.IdOf(lobby));
            Assert.AreEqual(LobbyMessages.AlreadyInGame, flow.LastError);
            Assert.AreEqual(0UL, session.JoinedHost);
        }

        [Test]
        public void ALobbyWithoutHostDataFallsBackToItsOwner()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby();
            lobby.Data.Remove(SteamLobbyFlow.HostKey);
            Assert.IsTrue(Done(flow.JoinAsync(lobbies.IdOf(lobby))));
            Assert.AreEqual(HostId, session.JoinedHost);
        }

        [Test]
        public void ConnectFailureLeavesTheLobbyWithTheSessionsReason()
        {
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby();
            session.RefuseJoin = "Steam isn't running - start Steam and try again.";
            Assert.IsFalse(Done(flow.JoinAsync(lobbies.IdOf(lobby))));
            Assert.IsFalse(flow.InLobby);
            Assert.AreEqual(session.RefuseJoin, flow.LastError);
            CollectionAssert.Contains(lobbies.Left, lobbies.IdOf(lobby));
        }

        [Test]
        public void SteamJoinErrorsReachThePlayer()
        {
            lobbies.JoinError = LobbyMessages.ForRoomEnter("NotAllowed");
            FakeSteamLobbies.FakeLobby lobby = FriendsLobby();
            Assert.IsFalse(Done(flow.JoinAsync(lobbies.IdOf(lobby))));
            StringAssert.Contains("friends-only", flow.LastError);
            Assert.AreEqual(LobbyMessages.Gone, LobbyMessages.ForRoomEnter("DoesntExist"));
        }

        [Test]
        public void ClosingTheLobbyForARunOnlyAppliesOnTheHost()
        {
            session.Host();
            flow.OnHostStarted();
            flow.SetJoinable(false);
            Assert.IsFalse(lobbies.Lobbies[flow.LobbyId].Joinable);
            flow.SetJoinable(true);
            Assert.IsTrue(lobbies.Lobbies[flow.LobbyId].Joinable);
        }

        [Test]
        public void DetachLeavesAndStopsListening()
        {
            session.Host();
            flow.OnHostStarted();
            ulong id = flow.LobbyId;
            flow.Detach();
            CollectionAssert.Contains(lobbies.Left, id);
            session.Stop();
            lobbies.AcceptInvite(lobbies.IdOf(FriendsLobby()));
            Assert.AreEqual(0UL, session.JoinedHost, "a detached flow ignores invites");
        }

        [Test]
        public void HostingWhileAJoinIsInFlightKeepsTheHostsLobby()
        {
            FakeSteamLobbies.FakeLobby friends = FriendsLobby();
            var steamAnswers = new TaskCompletionSource<bool>();
            lobbies.JoinGate = steamAnswers;
            Task<bool> joining = flow.JoinAsync(lobbies.IdOf(friends));
            lobbies.JoinGate = null;

            session.Host();
            flow.OnHostStarted();
            ulong own = flow.LobbyId;
            Assert.AreNotEqual(0UL, own);

            steamAnswers.SetResult(true);
            Assert.IsTrue(joining.IsCompleted);
            Assert.IsFalse(joining.Result);
            Assert.AreEqual(own, flow.LobbyId, "our own lobby is still the one we're in");
            Assert.AreEqual(LobbyMessages.AlreadyInGame, flow.LastError);
            Assert.AreEqual(0UL, session.JoinedHost);
            CollectionAssert.Contains(lobbies.Left, lobbies.IdOf(friends), "the late join is left again");
        }
    }
}
