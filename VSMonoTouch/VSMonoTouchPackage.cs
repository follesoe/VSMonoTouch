using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Follesoe.VSMonoTouch
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    [ProvideProjectFactory(typeof(MonoTouchFlavorProjectFactory), "MonoTouch Flavor", "Mono Files (*.csproj);*.csproj", null, null, null)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSMonoTouchPkgString)]
    public sealed class VSMonoTouchPackage : Package
    {
        private DTE _dte;

        protected override void Initialize()
        {
            RegisterProjectFactory(new MonoTouchFlavorProjectFactory(this));
            
            _dte = GetGlobalService(typeof(SDTE)) as DTE;         

            if (_dte != null)
            {
                _dte.Events.BuildEvents.OnBuildBegin += MakeXibsNone;
                _dte.Events.BuildEvents.OnBuildDone += MakeXibsPage;
            }

            base.Initialize();
        }

        private void MakeXibsNone(vsBuildScope scope, vsBuildAction action)
        {
            var xibs = FindAllXibsInSolution();
            foreach(var xib in xibs)
            {
                xib.Properties.Item("ItemType").Value = "None";
            }
        }

        private void MakeXibsPage(vsBuildScope Scope, vsBuildAction Action)
        {
            var xibs = FindAllXibsInSolution();
            foreach (var xib in xibs)
            {
                xib.Properties.Item("ItemType").Value = "Page";
            }
        }

        private IEnumerable<ProjectItem> FindAllXibsInSolution()
        {
            var xibs = new List<ProjectItem>();
            foreach (Project project in _dte.Solution.Projects)
            {
                FindXibs(project, xibs);
            }
            return xibs;
        }

        private void FindXibs(Project project, List<ProjectItem> xibs)
        {            
            if (project.ConfigurationManager != null)
            {
                if (IsMonoTouchProject(project))
                {
                    var vsProject = (VSLangProj.VSProject)project.Object;                    
                    FindXibs(project.ProjectItems, xibs);
                }
            }
            else
            {
                NavigateProjectItems(project.ProjectItems, xibs);
            }
        }

        private void NavigateProjectItems(ProjectItems items, List<ProjectItem> xibs)
        {
            foreach (ProjectItem item in items)
            {
                if (item.SubProject != null)
                {
                    FindXibs(item.SubProject, xibs);
                }
            }
        }

        private static void FindXibs(ProjectItems items, List<ProjectItem> xibs)
        {
            foreach (ProjectItem item in items)
            {                
                for (short i = 0; i < item.FileCount; ++i)
                {
                    string fileName = item.FileNames[i];
                    if(Directory.Exists(fileName))
                    {
                        FindXibs(item.ProjectItems, xibs);       
                    } 
                    else if(File.Exists(fileName))
                    {
                        if(Path.GetExtension(fileName).ToLower().Equals(".xib"))
                        {
                            xibs.Add(item);
                        }
                    }
                }
            }       
        }

        private static bool IsMonoTouchProject(Project project)
        {
            string projectTypeGuids = ProjectUtils.GetProjectTypeGuids(project);
            return projectTypeGuids.Contains(GuidList.guidMonoTouchProjectFactory);
        }     
    }

    [ComVisible(false)]
    [Guid(GuidList.guidMonoTouchProjectFactory)]
    public class MonoTouchFlavorProjectFactory : FlavoredProjectFactoryBase
    {        
        private readonly VSMonoTouchPackage _package;

        public MonoTouchFlavorProjectFactory(VSMonoTouchPackage package)
        {
            _package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            return new MonoTouchFlavePackageProject(_package);
        }    
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid(PackageProjectGuid)]
    public class MonoTouchFlavePackageProject : FlavoredProjectBase
    {
        private const string PackageProjectGuid = "628E6A0A-36B0-4a79-BB2E-3E1B3BB38C82";
        
        private readonly VSMonoTouchPackage _package;

        public MonoTouchFlavePackageProject(VSMonoTouchPackage package)
        {
            _package = package; 
        }
 
        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            if (serviceProvider == null)
            {
                serviceProvider = _package;
            }
            base.SetInnerProject(innerIUnknown);
        }
    }
}