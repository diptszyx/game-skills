using GameKit.UI;
using NUnit.Framework;
using UnityEngine;

namespace GameKit.UI.Tests
{
    public sealed class UiSafeAreaTests
    {
        [Test]
        public void AnchorsFor_FullScreen_ReturnsUnitAnchors()
        {
            var result = UiSafeArea.AnchorsFor(new Rect(0, 0, 720, 1600), new Vector2(720, 1600));
            Assert.AreEqual(Vector2.zero, result.min);
            Assert.AreEqual(Vector2.one, result.max);
        }

        [Test]
        public void AnchorsFor_TopNotch_UsesSafeArea()
        {
            var result = UiSafeArea.AnchorsFor(new Rect(0, 0, 720, 1500), new Vector2(720, 1600));
            Assert.AreEqual(1500f / 1600f, result.max.y, 0.0001f);
        }
    }
}
