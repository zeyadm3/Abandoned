using System;
using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>
    /// What one nettest process writes to its -nettestOut file (JsonUtility). Tools/nettest.sh fails
    /// the run unless every file exists and says passed. The host's file holds the cross-machine verdict.
    /// </summary>
    [Serializable]
    public class NetTestResult
    {
        public string scenario;
        public string role;
        public ulong clientId;
        public string version;
        public bool passed;
        public float seconds;
        public List<string> errors = new();
        public List<string> notes = new();
        public List<NetTestMove> moves = new();
        public List<NetTestView> views = new();
        public List<NetTestLootAction> lootActions = new();
        public List<NetTestLootView> lootViews = new();
        public List<NetTestSharedCarryAction> sharedActions = new();
        public NetTestSharedCarryHostView sharedHost;

        public void Fail(string error)
        {
            passed = false;
            errors.Add(error);
        }
    }
}
