using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>A fixed weight pressing down at its own position, for structure tests.</summary>
    public class TestLoad : MonoBehaviour, ILoadSource
    {
        public float weight = 100f;

        public float LoadWeight => weight;

        public void GetLoadPoints(List<LoadPoint> points) => points.Add(new LoadPoint(transform.position));

        private void OnEnable() => LoadSources.Register(this);

        private void OnDisable() => LoadSources.Unregister(this);

        public static TestLoad Create(Vector3 position, float weight)
        {
            var go = new GameObject($"TestLoad_{weight}kg");
            go.transform.position = position;
            var load = go.AddComponent<TestLoad>();
            load.weight = weight;
            return load;
        }
    }
}
