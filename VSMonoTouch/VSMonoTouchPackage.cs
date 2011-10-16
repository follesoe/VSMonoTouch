using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using EnvDTE;
using Microsoft.VisualStudio;
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
    [ProvideProjectFactory(typeof(MonoTouch26FlavorProjectFactory), "MonoTouch Flavor", "Mono Files (*.csproj);*.csproj", "csproj", "csproj", null)]
    [ProvideProjectFactory(typeof(MonoTouch28FlavorProjectFactory), "MonoTouch Flavor", "Mono Files (*.csproj);*.csproj", "csproj", "csproj", null)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidVSMonoTouchPkgString)]
    public sealed class VSMonoTouchPackage : Package
    {
        private DTE _dte;
        private BuildEvents _buildEvents;
        private readonly SolutionEvents _solutionEvents = new SolutionEvents();
        private uint _solutionEventsCookie;
        private IVsSolution _solution;

        protected override void Initialize()
        {
            RegisterProjectFactory(new MonoTouch26FlavorProjectFactory(this));
            RegisterProjectFactory(new MonoTouch28FlavorProjectFactory(this));           

            _dte = GetService(typeof(SDTE)) as DTE;
            if (_dte == null) throw new Exception("DTE Reference Not Found");

            _buildEvents = _dte.Events.BuildEvents;
            _buildEvents.OnBuildBegin += MakeXibsNone;
            _buildEvents.OnBuildDone += MakeXibsPage;

            _solution = (IVsSolution)GetService(typeof(SVsSolution));
            if (_solution == null) throw new Exception("IVSSolution Reference Not Found.");                
            _solution.AdviseSolutionEvents(_solutionEvents, out _solutionEventsCookie);
                        
            base.Initialize();
        }

        protected override int QueryClose(out bool canClose)
        {
            if (_solutionEventsCookie != 0 && _solution != null)
                _solution.UnadviseSolutionEvents(_solutionEventsCookie);

            return base.QueryClose(out canClose);
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
                if (IsMonoTouchProject(project)) FindXibs(project.ProjectItems, xibs);
            }
            else
            {
                NavigateProjectItems(project.ProjectItems, xibs);
            }
        }

        private void NavigateProjectItems(ProjectItems items, List<ProjectItem> xibs)
        {
            if (items == null) return;
            items.Cast<ProjectItem>().ToList().ForEach(pi =>
            {
                if (pi.SubProject != null) FindXibs(pi.SubProject, xibs);
            });
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

        internal static bool IsMonoTouchProject(Project project)
        {
            var projectTypeGuids = ProjectUtils.GetProjectTypeGuids(project);

            if (projectTypeGuids.Contains(GuidList.guidMonoTouchProjectFactory26)) return true;
            if (projectTypeGuids.Contains(GuidList.guidMonoTouchProjectFactory28)) return true;

            return false;
        }     
    }

    public sealed class SolutionEvents : IVsSolutionEvents
    {
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            const string targetFrameworkMoniker = "TargetFrameworkMoniker";

            object projectObj;
            pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out projectObj);
            var project = (Project) projectObj;

            if (VSMonoTouchPackage.IsMonoTouchProject(project))
            {
                var v10FrameworkName = (new FrameworkName(".NETFramework", new Version(1, 0))).FullName;
                var item = project.Properties.Item(targetFrameworkMoniker);
                if (item != null)
                {
                    if (item.Value == null || (string)item.Value != v10FrameworkName) item.Value = v10FrameworkName;
                }
                else
                {
                    project.Properties.Item(targetFrameworkMoniker).Value = v10FrameworkName;
                }
            }
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }
    }

    public abstract class MonoTouchFlavorProjectFactory : FlavoredProjectFactoryBase
    {
        protected readonly VSMonoTouchPackage Package;

        protected MonoTouchFlavorProjectFactory(VSMonoTouchPackage package)
        {
            Package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            return new MonoTouchFlavePackageProject(Package);
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