using System.Collections;
using GameKit.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameKit.UI.Tests.PlayMode
{
    public sealed class UiModalTests : InputTestFixture
    {
        private Keyboard _keyboard;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            ResetStack();
        }

        [TearDown]
        public override void TearDown()
        {
            ResetStack();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Open_ReachesFullScaleAndScrim()
        {
            var modal = CreateModal(out var root);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.IsTrue(modal.IsOpen);
            Assert.AreEqual(Vector3.one, modal.Panel.localScale);
            Assert.AreEqual(modal.Theme.ScrimColor, modal.ScrimColor);
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Close_DeactivatesAndCallsOnClosedOnce()
        {
            var modal = CreateModal(out var root);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            modal.Close();
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsFalse(modal.IsOpen);
            Assert.AreEqual(1, modal.ClosedCount);
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Open_DuringHide_EndsOpenWithoutOnClosed()
        {
            var modal = CreateModal(out var root);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            modal.Close();
            yield return new WaitForSecondsRealtime(0.04f);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsTrue(modal.IsOpen);
            Assert.AreEqual(Vector3.one, modal.Panel.localScale);
            Assert.AreEqual(0, modal.ClosedCount);
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Close_DuringShow_EndsClosed()
        {
            var modal = CreateModal(out var root);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.04f);
            modal.Close();
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsFalse(modal.IsOpen);
            Assert.AreEqual(Vector3.one * modal.Theme.Motion.CloseEndScale, modal.Panel.localScale);
            Assert.AreEqual(1, modal.ClosedCount);
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Back_ClosesOnlyTopModal()
        {
            var lower = CreateModal(out var lowerRoot);
            var top = CreateModal(out var topRoot);
            lower.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            top.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            InputSystem.Update();
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsTrue(lower.IsOpen);
            Assert.IsFalse(top.IsOpen);
            Object.Destroy(lowerRoot);
            Object.Destroy(topRoot);
        }

        [UnityTest]
        public IEnumerator Works_WithTimeScaleZero()
        {
            float oldTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            var modal = CreateModal(out var root);
            modal.Open();
            yield return new WaitForSecondsRealtime(0.35f);
            Assert.AreEqual(Vector3.one, modal.Panel.localScale);
            modal.Close();
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.IsFalse(modal.IsOpen);
            Time.timeScale = oldTimeScale;
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator UsesThemeMotion()
        {
            var modal = CreateModal(out var root);
            modal.Theme.Motion.OpenSeconds = 0.5f;
            modal.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.AreNotEqual(Vector3.one, modal.Panel.localScale);
            yield return new WaitForSecondsRealtime(0.35f);
            Assert.AreEqual(Vector3.one, modal.Panel.localScale);
            Object.Destroy(root);
        }

        private static CountingModal CreateModal(out GameObject root)
        {
            root = new GameObject("Modal", typeof(RectTransform), typeof(Image), typeof(Button));
            root.SetActive(false);
            var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(Button));
            panelObject.transform.SetParent(root.transform, false);
            var closeObject = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObject.transform.SetParent(panelObject.transform, false);
            var modal = root.AddComponent<CountingModal>();
            modal.TestTheme = ScriptableObject.CreateInstance<UiTheme>();
            var serialized = new UnityEditor.SerializedObject(modal);
            serialized.FindProperty("_panel").objectReferenceValue = panelObject.GetComponent<RectTransform>();
            serialized.FindProperty("_scrim").objectReferenceValue = root.GetComponent<Image>();
            serialized.FindProperty("_theme").objectReferenceValue = modal.TestTheme;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            root.SetActive(true);
            return modal;
        }

        private static void ResetStack() => typeof(UiModal).GetMethod("ResetStack",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);

        public sealed class CountingModal : UiModal
        {
            public int ClosedCount { get; private set; }
            public UiTheme Theme => TestTheme;
            public UiTheme TestTheme { get; set; }

            protected override void OnClosed() => ClosedCount++;
        }
    }
}
