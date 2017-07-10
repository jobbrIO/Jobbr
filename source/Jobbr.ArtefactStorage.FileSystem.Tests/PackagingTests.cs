﻿using System.Reflection;
using Jobbr.DevSupport.ReferencedVersionAsserter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.ArtefactStorage.FileSystem.Tests
{
    [TestClass]
    public class PackagingTests
    {
        private readonly bool isPre = Assembly.GetExecutingAssembly().GetInformalVersion().Contains("-");

        [TestMethod]
        public void Feature_NuSpec_IsCompilant()
        {
            var asserter = new Asserter(Asserter.ResolvePackagesConfig("Jobbr.ArtefactStorage.FileSystem"), Asserter.ResolveRootFile("Jobbr.ArtefactStorage.FileSystem.nuspec"));

            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.Registration"));
            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.ArtefactStorage"));

            if (this.isPre)
            {
                // This rule is only valid for Pre-Release versions because we only need exact match on PreRelease Versions
                asserter.Add(new ExactVersionMatchRule("Jobbr.ComponentModel.*"));
            }
            else
            {
                asserter.Add(new AllowNonBreakingChangesRule("Jobbr.ComponentModel.*"));
            }

            asserter.Add(new VersionIsIncludedInRange("Jobbr.ComponentModel.*"));

            asserter.Add(new NoMajorChangesInNuSpec("Jobbr.*"));

            var result = asserter.Validate();

            Assert.IsTrue(result.IsSuccessful, result.Message);
        }
    }
}
