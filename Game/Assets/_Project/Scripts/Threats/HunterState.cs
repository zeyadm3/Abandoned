namespace Abandoned.Threats
{
    public enum HunterState : byte
    {
        Patrol,
        Chase,
        Search,
        Stunned,
        // 0.16.0 (append only: the value is replicated): at a shut door, breaking it down.
        Breach
    }
}
