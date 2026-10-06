using System;
using System.Collections;
using System.IO;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Multi-process localhost nettest driver. Inert unless the game is launched with
    /// <c>-nettest host|client</c> (see <see cref="NetTestArgs"/>): then it hosts or joins over Unity
    /// Transport on 127.0.0.1, runs the named scenario, writes a JSON <see cref="NetTestResult"/> and
    /// quits with exit code 0 (passed) or 1. Tools/nettest.sh launches 1 host + 3 clients this way.
    /// </summary>
    public sealed class NetTestRunner : MonoBehaviour
    {
        private const string Loopback = "127.0.0.1";
        private const float BootstrapTimeout = 15f;
        // Four headless instances share one Mac; uncapped frame rates would starve each other.
        private const int FrameRate = 60;

        private NetTestArgs args;
        private float started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LaunchFromCommandLine()
        {
            NetTestArgs parsed = NetTestArgs.Parse(Environment.GetCommandLineArgs());
            if (!parsed.Active) return;
            var go = new GameObject("NetTestRunner");
            DontDestroyOnLoad(go);
            go.AddComponent<NetTestRunner>().args = parsed;
        }

        private IEnumerator Start()
        {
            started = Time.realtimeSinceStartup;
            Application.runInBackground = true;
            Application.targetFrameRate = FrameRate;
            QualitySettings.vSyncCount = 0;
            Debug.Log($"[NetTest] {args.Role} scenario '{args.Scenario}' port {args.Port} -> {args.OutPath}");

            var result = new NetTestResult
            {
                scenario = args.Scenario,
                role = args.Role.ToString().ToLowerInvariant(),
                version = VersionInfo.Display,
            };
            yield return Run(result);

            result.seconds = Time.realtimeSinceStartup - started;
            result.passed = result.errors.Count == 0;
            Write(result);
            Debug.Log($"[NetTest] RESULT {(result.passed ? "PASS" : "FAIL")} {result.role} client {result.clientId} " +
                      $"in {result.seconds:0.0} s{(result.passed ? "" : ": " + string.Join("; ", result.errors))}");

            if (NetworkBootstrap.Instance != null) NetworkBootstrap.Instance.Disconnect();
            yield return null;
            Application.Quit(result.passed ? 0 : 1);
        }

        private IEnumerator Run(NetTestResult result)
        {
            string problem = args.Problem();
            if (problem != null) { result.Fail(problem); yield break; }
            if (!NetTestScenarios.TryCreate(args.Scenario, out INetTestScenario scenario))
            {
                result.Fail($"unknown scenario '{args.Scenario}' (known: {string.Join(", ", NetTestScenarios.Names)})");
                yield break;
            }

            if (scenario is INetTestSetup setup) setup.Prepare();

            float deadline = Time.realtimeSinceStartup + BootstrapTimeout;
            while (NetworkBootstrap.Instance == null && Time.realtimeSinceStartup < deadline) yield return null;
            NetworkBootstrap bootstrap = NetworkBootstrap.Instance;
            if (bootstrap == null) { result.Fail("no NetworkBootstrap in the first scene"); yield break; }

            bootstrap.SelectTransport(TransportMode.UnityTransport);
            bool ok = args.Role == NetTestRole.Host
                ? bootstrap.StartHost(args.Port, Loopback)
                : bootstrap.StartClient(Loopback, args.Port);
            if (!ok) { result.Fail($"couldn't start the session: {bootstrap.LastError}"); yield break; }
            if (args.Role == NetTestRole.Host) SignalReady(bootstrap.HostPort);

            var channel = new NetTestChannel(bootstrap.Manager);
            channel.Open();
            var context = new NetTestContext(bootstrap, channel, result, args.Clients);
            yield return RunWithTimeout(args.Role == NetTestRole.Host ? scenario.RunHost(context) : scenario.RunClient(context),
                result, args.Timeout - (Time.realtimeSinceStartup - started));
            // A scenario that ends the session (robust) records its own id; the manager may be gone by now.
            if (bootstrap != null && bootstrap.Manager != null && bootstrap.Manager.IsListening)
                result.clientId = bootstrap.Manager.LocalClientId;
            channel.Close();
        }

        private IEnumerator RunWithTimeout(IEnumerator body, NetTestResult result, float timeout)
        {
            bool finished = false;
            Coroutine running = StartCoroutine(Track(body, () => finished = true));
            float end = Time.realtimeSinceStartup + timeout;
            while (!finished && Time.realtimeSinceStartup < end) yield return null;
            if (finished) yield break;
            StopCoroutine(running);
            result.Fail($"scenario didn't finish within {args.Timeout:0} s");
        }

        private static IEnumerator Track(IEnumerator body, Action onDone)
        {
            yield return body;
            onDone();
        }

        /// <summary>
        /// Tools/nettest.sh launches clients only once this file exists: a slow host start (loaded Mac,
        /// cold disk cache) would otherwise outlast the clients' connect attempts.
        /// </summary>
        private void SignalReady(ushort port)
        {
            if (string.IsNullOrEmpty(args.OutPath)) return;
            try
            {
                File.WriteAllText(args.OutPath + ".ready", port.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetTest] couldn't write the ready file: {e.Message}");
            }
        }

        private void Write(NetTestResult result)
        {
            string json = JsonUtility.ToJson(result, true);
            if (string.IsNullOrEmpty(args.OutPath))
            {
                Debug.Log("[NetTest] result:\n" + json);
                return;
            }
            try
            {
                string folder = Path.GetDirectoryName(Path.GetFullPath(args.OutPath));
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(args.OutPath, json);
            }
            catch (Exception e)
            {
                // The script treats a missing file as a failure, so logging is enough here.
                Debug.LogError($"[NetTest] couldn't write {args.OutPath}: {e.Message}");
            }
        }
    }
}
