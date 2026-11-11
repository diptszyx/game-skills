using System;
using IndieGame.Shell;
using Xunit;

namespace IndieGame.Shell.Tests;

public sealed class LocalizationTests
{
    static LocalizationService Create() => new(new LocalizationConfig("en"));
    [Fact] public void JsonStringTablesLoadPerLanguageAndPreserveUnicode()
    {
        var l = Create(); Assert.True(l.LoadTable("en", "{\"hello\":\"Hello\"}").Success);
        Assert.True(l.LoadTable("vi", "{\"hello\":\"Xin chào\"}").Success);
        Assert.Equal("Hello", l.Lookup("en", "hello").Value); Assert.Equal("Xin chào", l.Lookup("vi", "hello").Value); Assert.Empty(l.MissingKeys);
    }
    [Fact] public void LookupFallsBackFromLanguageToDefaultThenBracketedKeyAndRecordsMisses()
    {
        var l = Create(); l.LoadTable("en", "{\"hello\":\"Hello\"}"); l.LoadTable("vi", "{}");
        Assert.Equal("Hello", l.Lookup("vi", "hello").Value); Assert.Equal("[absent]", l.Lookup("vi", "absent").Value);
        Assert.Equal("Hello", l.Lookup("xx", "hello").Value);
        Assert.Collection(l.MissingKeys, m => { Assert.Equal("vi", m.Language); Assert.Equal("hello", m.Key); },
            m => Assert.Equal("absent", m.Key), m => Assert.Equal("xx", m.Language));
        Assert.True(l.ClearMissingKeys().Success); Assert.Empty(l.MissingKeys);
    }
    [Fact] public void SafeFormattingHandlesArgumentsAndBadPlaceholdersWithoutThrowing()
    {
        var l = Create(); l.LoadTable("en", "{\"good\":\"Hi {0}, {1:N1}\",\"bad\":\"Hi {2}\",\"broken\":\"Hi {\"}");
        Assert.Equal("Hi Sam, 1.5", l.Format("en", "good", "Sam", 1.5).Value);
        var bad = l.Format("en", "bad", "Sam"); Assert.False(bad.Success); Assert.Equal("Hi {2}", bad.Value);
        Assert.False(l.Format("en", "broken").Success);
        Assert.False(LocalizationService.FormatSafe("{0}", new BadFormat()).Success);
        Assert.False(LocalizationService.FormatSafe(null!).Success);
    }
    sealed class BadFormat { public override string ToString() => throw new InvalidOperationException(); }
    [Fact] public void MalformedOrNonStringJsonTablesAreRejectedWithoutReplacingValidTable()
    {
        var l = Create(); l.LoadTable("en", "{\"key\":\"old\"}");
        foreach (var json in new[] { "", "{", "[]", "null", "{\"key\":1}", "{\"key\":null}", "{\"key\":\"a\",\"key\":\"b\"}" })
        { Assert.False(l.LoadTable("en", json).Success); Assert.Equal("old", l.Lookup("en", "key").Value); }
        Assert.False(l.LoadTable("", "{}").Success); Assert.Empty(l.MissingKeys);
        Assert.False(l.Lookup("", "key").Success); Assert.False(l.Lookup("en", null!).Success); Assert.Empty(l.MissingKeys);
        Assert.False(l.Format("en", "absent", null!).Success); Assert.Empty(l.MissingKeys);
    }
    [Fact] public void JsonDateLikeTextIsPreservedAsText()
    {
        var l = Create(); Assert.True(l.LoadTable("en", "{\"date\":\"2026-11-11T00:00:00Z\"}").Success);
        Assert.Equal("2026-11-11T00:00:00Z", l.Lookup("en", "date").Value);
    }
}
