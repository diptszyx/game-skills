using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
namespace IndieGame.Shell.Tests
{
    public sealed class KeyboardAvoidanceTests
    {
        const float Screen=1600, Margin=24;
        [Test] public void NoKeyboardMeansNoShift()
            =>Assert.That(KeyboardAvoidance.Shift(300,1100,700,0,Screen,Margin),Is.EqualTo(0));
        [Test] public void PanelAboveKeyboardDoesNotMove()
            =>Assert.That(KeyboardAvoidance.Shift(800,1400,1000,670,Screen,Margin),Is.EqualTo(0));
        [Test] public void OverlappingPanelRisesByOverlapPlusMargin()
            =>Assert.That(KeyboardAvoidance.Shift(500,1100,900,670,Screen,Margin),Is.EqualTo(694-500));
        [Test] public void ShiftIsLimitedByRoomAboveThePanel()
            =>Assert.That(KeyboardAvoidance.Shift(300,1500,1200,670,Screen,Margin),Is.EqualTo(100));
        [Test] public void FocusedFieldStaysClearOfKeyboardEvenWithoutRoomAbove()
            =>Assert.That(KeyboardAvoidance.Shift(300,1600,500,670,Screen,Margin),Is.EqualTo(694-500));
    }
}
