using System.Collections;

namespace Abandoned.Networking
{
    /// <summary>
    /// One scripted multi-machine check. The host side drives and judges; the client side follows
    /// the host's messages. Both write findings into <see cref="NetTestContext.Result"/> and simply
    /// finish; the runner writes the file and quits. Add new ones in <see cref="NetTestScenarios"/>.
    /// </summary>
    public interface INetTestScenario
    {
        string Name { get; }
        IEnumerator RunHost(NetTestContext context);
        IEnumerator RunClient(NetTestContext context);
    }
}
