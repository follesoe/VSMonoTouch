// Guids.cs
// MUST match guids.h
using System;

namespace Follesoe.VSMonoTouch
{
    static class GuidList
    {
        public const string guidVSMonoTouchPkgString = "4e51e215-eb16-4614-b6d2-92e6e1d8c204";
        public const string guidVSMonoTouchCmdSetString = "3c2c3edd-b7a6-4566-8780-888f78686380";
        public const string guidSolutionFolder = "66A26720-8FB5-11D2-AA7E-00C04F688DDE";
        public const string guidMonoTouchProjectFactory26 = "E613F3A2-FE9C-494F-B74E-F63BCB86FEA6";
        public const string guidMonoTouchProjectFactory28 = "6BC8ED88-2882-458C-8E55-DFD12B67127B";

        public static readonly Guid guidVSMonoTouchCmdSet = new Guid(guidVSMonoTouchCmdSetString);
    };
}