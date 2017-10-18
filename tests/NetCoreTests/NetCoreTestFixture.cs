﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Buildalyzer;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace NetCoreTests
{
    [TestFixture]
    public class NetCoreTestFixture
    {
        private static readonly string[] ProjectFiles =
        {
#if Is_Windows
            @"LegacyFrameworkProject\LegacyFrameworkProject.csproj",
            @"LegacyFrameworkProjectWithReference\LegacyFrameworkProjectWithReference.csproj",
#endif
            @"SdkNetCoreProject\SdkNetCoreProject.csproj",
            @"SdkNetCoreProjectImport\SdkNetCoreProjectImport.csproj",
            @"SdkNetStandardProject\SdkNetStandardProject.csproj",
            @"SdkNetStandardProjectImport\SdkNetStandardProjectImport.csproj"
        };

        [TestCaseSource(nameof(ProjectFiles))]
        public void LoadsProject(string projectFile)
        {
            // Given
            StringBuilder log = new StringBuilder();
            ProjectAnalyzer analyzer = GetProjectAnalyzer(projectFile, log);

            // When
            Project project = analyzer.Load();

            // Then
            project.ShouldNotBeNull(log.ToString());
        }

        [TestCaseSource(nameof(ProjectFiles))]
        public void CompilesProject(string projectFile)
        {
            // Given
            StringBuilder log = new StringBuilder();
            ProjectAnalyzer analyzer = GetProjectAnalyzer(projectFile, log);

            // When
            ProjectInstance projectInstance = analyzer.Compile();

            // Then
            projectInstance.ShouldNotBeNull(log.ToString());
        }

        [TestCaseSource(nameof(ProjectFiles))]
        public void GetsSourceFiles(string projectFile)
        {
            // Given
            StringBuilder log = new StringBuilder();
            ProjectAnalyzer analyzer = GetProjectAnalyzer(projectFile, log);

            // When
            IReadOnlyList<string> sourceFiles = analyzer.GetSourceFiles();

            // Then
            sourceFiles.ShouldContain(x => x.EndsWith("Class1.cs"));
        }

        [Test]
        public void GetsProjectsInSolution()
        {
            // Given
            StringBuilder log = new StringBuilder();

            var path = GetProjectPath("TestProjects.sln");
            // When
            try
            {
                AnalyzerManager manager = new AnalyzerManager(path, log);
                var res = ProjectFiles.Select(GetProjectPath).ToList();
            
                // Then
                manager.Projects.Keys.ShouldBe(ProjectFiles.Select(GetProjectPath), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

            
        }

        private ProjectAnalyzer GetProjectAnalyzer(string projectFile, StringBuilder log) =>
            new AnalyzerManager(log).GetProject(GetProjectPath(projectFile));

        private static string GetProjectPath(string file)
        {
            var path = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(typeof(NetCoreTestFixture).Assembly.Location),
                    @"..\..\..\..\projects\" + file));

            return path.Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}
