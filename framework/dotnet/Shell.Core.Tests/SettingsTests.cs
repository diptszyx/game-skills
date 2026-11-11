using IndieGame.Shell;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class SettingsTests
{
    static SettingsService Create() => new(new SettingsConfig(true, false, true, "en", new[] { "en", "vi" }));
    [Fact] public void SoundMusicAndHapticsCanEachBeSwitchedOnAndOff()
    {
        var s = Create();
        Assert.True(s.SetSound(false).Success); Assert.False(s.Sound);
        Assert.True(s.SetSound(true).Success); Assert.True(s.Sound);
        Assert.True(s.SetMusic(true).Success); Assert.True(s.Music);
        Assert.True(s.SetMusic(false).Success); Assert.False(s.Music);
        Assert.True(s.SetHaptics(false).Success); Assert.False(s.Haptics);
        Assert.True(s.SetHaptics(true).Success); Assert.True(s.Haptics);
    }
    [Fact] public void LanguageSupportsConfiguredCodesAndUnsupportedCodeFallsBackToDefault()
    {
        var s = Create(); Assert.True(s.SetLanguage("vi").Success); Assert.Equal("vi", s.Language);
        Assert.True(s.SetLanguage("xx").Success); Assert.Equal("en", s.Language);
        s.SetLanguage("vi"); Assert.False(s.SetLanguage(null!).Success); Assert.False(s.SetLanguage(" ").Success); Assert.Equal("vi", s.Language);
    }
    [Fact] public void SettingsPersistAsOneVersionedSectionAndKeepOtherSections()
    {
        var s = Create(); s.SetSound(false); s.SetMusic(true); s.SetHaptics(false); s.SetLanguage("vi");
        var d = new SaveDocument(); d.WriteSection("other", new { Keep = 42 }); Assert.True(s.Save(d).Success);
        var copy = Create(); Assert.True(copy.Load(d).Success);
        Assert.False(copy.Sound); Assert.True(copy.Music); Assert.False(copy.Haptics); Assert.Equal("vi", copy.Language);
        var section = d.ReadSection("settings", () => new JObject()).Value; Assert.Equal(1, section["Version"]);
        Assert.Equal(5, section.Count); Assert.Equal(42, d.ReadSection("other", () => new JObject()).Value["Keep"]);
    }
    [Fact] public void MissingSettingsSectionAndMissingOrUnknownFieldsUseConfiguredDefaults()
    {
        var s = Create(); var d = new SaveDocument(); s.SetSound(false); Assert.True(s.Load(d).Success); Assert.True(s.Sound);
        d.WriteSection("settings", new { Version = 1, Unknown = 10, Music = true }); Assert.True(s.Load(d).Success);
        Assert.True(s.Sound); Assert.True(s.Music); Assert.True(s.Haptics); Assert.Equal("en", s.Language);
        d.WriteSection("settings", new { Language = "xx" }); Assert.True(s.Load(d).Success); Assert.Equal("en", s.Language);
    }
    [Fact] public void MalformedFutureOrOverflowSettingsLeaveStateUnchanged()
    {
        var s = Create(); s.SetLanguage("vi"); s.SetSound(false); var d = new SaveDocument();
        foreach (var value in new object[] { "bad", new { Version = 2 }, new { Sound = -1 }, new { Music = "true" },
            new { Version = "9223372036854775808" }, new { Language = 9 } })
        {
            d.WriteSection("settings", value); Assert.False(s.Load(d).Success); Assert.False(s.Sound); Assert.Equal("vi", s.Language);
        }
        Assert.False(s.Load(null!).Success); Assert.False(s.Save(null!).Success);
    }
    [Fact] public void SettingsCopyLanguageConfigurationAndExposeReadOnlyLanguages()
    {
        var codes = new[] { "en", "vi" }; var c = new SettingsConfig(false, true, false, "en", codes); var s = new SettingsService(c);
        codes[1] = "xx"; s.SetLanguage("vi"); Assert.Equal("vi", s.Language);
    }
}
