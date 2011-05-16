using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Follesoe.VSMonoTouch
{
    public class ProjectUtils
    {
        public static string GetProjectTypeGuids(Project proj)
        {
            string projectTypeGuids = "";
            IVsHierarchy hierarchy;
            IVsAggregatableProject aggregatableProject;
            int result;

            object service = GetService(proj.DTE, typeof(IVsSolution));
            var solution = (IVsSolution)service;

            result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

            if (result == 0)
            {
                aggregatableProject = (IVsAggregatableProject)hierarchy;
                aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
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
            int hr;

            Guid SIDGuid = guid;
            Guid IIDGuid = SIDGuid;
            var serviceProvider = (IServiceProvider)serviceProviderObject;
            hr = serviceProvider.QueryService(ref SIDGuid, ref IIDGuid, out serviceIntPtr);

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
