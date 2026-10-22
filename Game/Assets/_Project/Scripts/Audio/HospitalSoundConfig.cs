using UnityEngine;

namespace Abandoned.Audio
{
    public sealed class HospitalSoundConfig : ScriptableObject
    {
        [SerializeField] private Vector2 monitorEvery = new(6f, 13f);
        [SerializeField] private Vector2 announcementEvery = new(35f, 75f);
        [SerializeField] private Vector2 trolleyEvery = new(55f, 105f);
        [SerializeField, Range(0f, 1f)] private float monitorLevel = .18f;
        [SerializeField, Range(0f, 1f)] private float announcementLevel = .32f;
        [SerializeField, Range(0f, 1f)] private float dripLevel = .28f;
        [SerializeField] private float trolleySeconds = 5f;

        public Vector2 MonitorEvery => monitorEvery;
        public Vector2 AnnouncementEvery => announcementEvery;
        public Vector2 TrolleyEvery => trolleyEvery;
        public float MonitorLevel => monitorLevel;
        public float AnnouncementLevel => announcementLevel;
        public float DripLevel => dripLevel;
        public float TrolleySeconds => trolleySeconds;
    }
}
