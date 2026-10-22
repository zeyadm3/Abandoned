using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Host: what happens on the road between locations. While the van is moving outside every location, every so often
    /// it rolls a road event: a monster on the road, a hazard blocking it (fallen tree, wreck), or a jump scare (a
    /// cosmetic fright on each machine). The events themselves are hooks to fill (Docs/OPEN_WORLD_PLAN.md).
    /// </summary>
    public class RoadEventDirector : MonoBehaviour
    {
        public enum RoadEvent { Monster, Hazard, JumpScare }

        [SerializeField] private Transform van;
        [SerializeField] private PoiStreamer streamer;
        [Tooltip("Seconds between road events (random in this range).")]
        [SerializeField] private Vector2 every = new(45f, 120f);
        [SerializeField, Range(0f, 1f)] private float monsterWeight = .35f;
        [SerializeField, Range(0f, 1f)] private float hazardWeight = .3f;
        [SerializeField, Range(0f, 1f)] private float jumpScareWeight = .35f;
        [Tooltip("How far ahead of the van an event is placed (m).")]
        [SerializeField] private float ahead = 80f;

        public event System.Action<RoadEvent, Vector3> Triggered;

        private float next;

        private void Start() => next = Time.time + Random.Range(every.x, every.y);

        private void Update()
        {
            if (van == null || Time.time < next) return;
            next = Time.time + Random.Range(every.x, every.y);
            float total = monsterWeight + hazardWeight + jumpScareWeight, pick = Random.value * total;
            RoadEvent e = pick < monsterWeight ? RoadEvent.Monster : pick < monsterWeight + hazardWeight ? RoadEvent.Hazard : RoadEvent.JumpScare;
            Triggered?.Invoke(e, van.position + van.forward * ahead);
        }
    }
}
