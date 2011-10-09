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
    [ProvideProjectFactory(typeof(MonoTouch26FlavorProjectFactory), "MonoTouch Flavor", "Mono Files (*.csproj);*.csproj", null, null, null)]
    [ProvideProjectFactory(typeof(MonoTouch28FlavorProjectFactory), "MonoTouch Flavor", "Mono Files (*.csproj);*.csproj", null, null, null)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSMonoTouchPkgString)]
    public sealed class VSMonoTouchPackage : Package
    {
        private DTE _dte;
        private BuildEvents _BuildEvents;

        protected override void Initialize()
        {
            RegisterProjectFactory(new MonoTouch26FlavorProjectFactory(this));
            RegisterProjectFactory(new MonoTouch28FlavorProjectFactory(this));
            
            _dte = GetGlobalService(typeof(SDTE)) as DTE;

            if (_dte != null)
            {
                _BuildEvents = _dte.Events.BuildEvents;
                _BuildEvents.OnBuildBegin += MakeXibsNone;
                _BuildEvents.OnBuildDone += MakeXibsPage;
            }
            else
            {
                throw new Exception();
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

        private void MakeXibsPage(vsBuildScope scope, vsBuildAction action)
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
                        var ext = Path.GetExtension(fileName);
                        if(!string.IsNullOrEmpty(ext) && ext.ToLower().Equals(".xib"))
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
            if (projectTypeGuids.Contains(GuidList.guidMonoTouchProjectFactory26)) return true;
            if (projectTypeGuids.Contains(GuidList.guidMonoTouchProjectFactory28)) return true;
            return false;
        }     
    }

    public abstract class MonoTouchFlavorProjectFactory : FlavoredProjectFactoryBase
    {
        protected readonly VSMonoTouchPackage _package;

        protected MonoTouchFlavorProjectFactory(VSMonoTouchPackage package)
        {
            _package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            return new MonoTouchFlavePackageProject(_package);
        }  
    }

    [ComVisible(false)]
    [Guid(GuidList.guidMonoTouchProjectFactory26)]
    public class MonoTouch26FlavorProjectFactory : MonoTouchFlavorProjectFactory
    {
        public MonoTouch26FlavorProjectFactory(VSMonoTouchPackage package) : base(package) {}
    }

    [ComVisible(false)]
    [Guid(GuidList.guidMonoTouchProjectFactory28)]
    public class MonoTouch28FlavorProjectFactory : MonoTouchFlavorProjectFactory
    {
        public MonoTouch28FlavorProjectFactory(VSMonoTouchPackage package) : base(package) {}
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