using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Follesoe.VSMonoTouch
{
    public class ProjectUtils
    {
        public static string GetProjectTypeGuids(Project proj)
        {
            var projectTypeGuids = "";
            IVsHierarchy hierarchy;

            var service = GetService(proj.DTE, typeof(IVsSolution));
            var solution = (IVsSolution)service;

            var result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

            if (result == 0)
            {
                if (hierarchy is IVsAggregatableProjectCorrected)
                {
                    var aggregatableProject = (IVsAggregatableProjectCorrected)hierarchy;                    
                    aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
                }
            }

            return projectTypeGuids;

        }

        private static object GetService(object serviceProvider, Type type)
        {
            return GetService(serviceProvider, type.GUID);
        }

        private static object GetService(object serviceProviderObject, Guid guid)
        {

            object service = null;
            IntPtr serviceIntPtr;

            var sidGuid = guid;
            var iidGuid = sidGuid;
            var serviceProvider = (IServiceProvider)serviceProviderObject;
            var hr = serviceProvider.QueryService(ref sidGuid, ref iidGuid, out serviceIntPtr);

            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            else if (!serviceIntPtr.Equals(IntPtr.Zero))
            {
                service = Marshal.GetObjectForIUnknown(serviceIntPtr);
                Marshal.Release(serviceIntPtr);
            }

            return service;
        }
    }
}
