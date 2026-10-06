using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Runs several coroutines side by side (one per in-process "machine") inside a test.</summary>
    public class TestCoroutineHost : MonoBehaviour
    {
        public static TestCoroutineHost Create() => new GameObject("TestCoroutineHost").AddComponent<TestCoroutineHost>();
    }
}
