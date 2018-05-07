using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE80;
using MSBuild = Microsoft.Build.Evaluation;

namespace FixNuGetPackagePaths
{
    internal abstract class ProjectKindsConstant
    {
        public const string vsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
    }

    public static class DteHelper
    {
        private static IEnumerable<EnvDTE.Project> GetProjects(EnvDTE.Project project)
        {
            if (project.Kind == ProjectKindsConstant.vsProjectKindSolutionFolder)
            {
                return project.ProjectItems
                    .Cast<EnvDTE.ProjectItem>()
                    .Select(x => x.SubProject)
                    .Where(x => x != null)
                    .SelectMany(GetProjects);
            }

            const string projectKindMisc = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";
            if (project.Kind == projectKindMisc)
            {
                return new EnvDTE.Project[0];
            }

            return new [] { project };
        }

        public static IEnumerable<EnvDTE.Project> GetAllProjects(this EnvDTE.Solution sln)
        {
            return sln.Projects
                .Cast<EnvDTE.Project>()
                .SelectMany(GetProjects);
        }

        public static MSBuild.Project AsMsBuildProject(this EnvDTE.Project project)
        {
            return MSBuild.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(project.FullName).FirstOrDefault() ??
                   MSBuild.ProjectCollection.GlobalProjectCollection.LoadProject(project.FullName);
        }

        public static string GetSolutionDir(this EnvDTE.DTE dte)
        {
            return Path.GetDirectoryName(dte.Solution.FullName);
        }
    }
}