using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    public class MovementConfigTests
    {
        [Test]
        public void DefaultsAreValid()
        {
            var config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            var errors = new List<string>();
            config.Validate(errors);
            Assert.IsEmpty(errors);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void CrouchTallerThanStandingIsReported()
        {
            var config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("<CrouchHeight>k__BackingField").floatValue = 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var errors = new List<string>();
            config.Validate(errors);
            Assert.That(errors, Has.Some.Contains("CrouchHeight"));
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DebugViewToggleRaisesChanged()
        {
            DebugView.SetVisible(false);
            bool? seen = null;
            void OnChanged(bool v) => seen = v;
            DebugView.Changed += OnChanged;
            DebugView.Toggle();
            DebugView.Changed -= OnChanged;
            Assert.AreEqual(true, seen);
            DebugView.SetVisible(false);
        }
    }
}
