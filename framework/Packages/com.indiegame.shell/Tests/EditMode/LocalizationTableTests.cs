using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IndieGame.Shell.Unity.Localization;
using NUnit.Framework;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class LocalizationTableTests
    {
        [Test] public void EveryRegisteredKeyExistsInBothLanguages()
        {
            var text=SampleLocalization.Load();
            foreach(var lang in new[]{"en","vi"}) foreach(var key in SampleLocalization.Keys)
            {
                var value=text.Lookup(lang,key);
                Assert.That(value.Success,Is.True,key);
                Assert.That(value.Value,Does.Not.Contain("["),lang+":"+key);
                Assert.That(value.Value,Is.Not.Empty,lang+":"+key);
                Assert.That(LocalizationService.FormatSafe(value.Value,1,2,3,4).Success,Is.True,key);
            }
            // Lookup records a miss even when English fallback succeeds.
            Assert.That(text.MissingKeys,Is.Empty);
        }
        [Test] public void RegistryCoversPresenterLiteralsAndDynamicFamilies()
        {
            var package=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Packages/com.indiegame.shell"));
            foreach(var file in Directory.GetFiles(Path.Combine(package,"Runtime/Unity/Screens"),"*.cs"))
            foreach(Match match in Regex.Matches(File.ReadAllText(file),"\"((?:shell|popup)\\.[a-zA-Z0-9_.]+)\""))
            {
                var key=match.Groups[1].Value;
                if(!key.EndsWith(".")) Assert.That(SampleLocalization.Keys,Does.Contain(key),Path.GetFileName(file));
            }
            foreach(var type in new[]{typeof(SaveError),typeof(DailyError),typeof(ModuleError),typeof(ShopError),typeof(ServiceError),typeof(AdOutcome)})
            foreach(var name in Enum.GetNames(type)) Assert.That(SampleLocalization.Keys,Does.Contain("shell.error."+name));
            foreach(var kind in new[]{"win","lose","revive","pause","leave"}) foreach(var suffix in new[]{"title","body"})
                Assert.That(SampleLocalization.Keys,Does.Contain("popup."+kind+"."+suffix));
            foreach(var action in new[]{"next","double","retry","revive","ad","gold","resume","sound","music","haptics","leave","confirm","cancel"})
                Assert.That(SampleLocalization.Keys,Does.Contain("popup.action."+action));
            foreach(var key in new[]{"shell.product.p","shell.quest.q","shell.booster.hint","shell.loading.tip"}) Assert.That(SampleLocalization.Keys,Does.Contain(key));
            var text=SampleLocalization.Load();
            foreach(var lang in new[]{"en","vi"}) foreach(var screen in new[]{"main","loading"})
                Assert.That(text.Lookup(lang,"shell."+screen+".title").Value.Length,Is.GreaterThan(60));
        }
    }
}
