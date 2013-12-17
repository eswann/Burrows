// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Burrows.Diagnostics.Introspection
{
    using System;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;

    public class StandardDiagnosticsInfo
    {
        public void WriteCommonItems(IDiagnosticsProbe probe)
        {
            AddHostValues(probe);
            AddOperatingSystemValues(probe);
            AddProcessValues(probe);
            AddRunningInFullTrustValue(probe);
        }

        static void AddHostValues(IDiagnosticsProbe probe)
        {
            probe.Add("host.machine_name", Environment.MachineName);
            probe.Add("net.version", Environment.Version);
        }

        static void AddProcessValues(IDiagnosticsProbe probe)
        {
            probe.Add("process.id", Process.GetCurrentProcess().Id);
            probe.Add("process.bits", Environment.Is64BitProcess ? "x64" : "x32");
        }

        static void AddOperatingSystemValues(IDiagnosticsProbe probe)
        {
            probe.Add("os.version", Environment.OSVersion);
            probe.Add("os.bits", Environment.Is64BitOperatingSystem ? "x64" : "x32");
        }

        static void AddRunningInFullTrustValue(IDiagnosticsProbe probe)
        {
            probe.Add("process.fulltrust", IsRunningInFullTrust());
        }

        static bool IsRunningInFullTrust()
        {
            try
            {
                new FileIOPermission(PermissionState.Unrestricted);
            }
            catch (SecurityException)
            {
                return false;
            }
            return true;
        }
    }
}