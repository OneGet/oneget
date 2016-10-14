
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Platform abstractions and platform specific implementations
    /// </summary>
    internal sealed class PlatformUtility
    {
        private static readonly Lazy<PlatformUtility> _platformUtility = new Lazy<PlatformUtility>(() => new PlatformUtility());

        internal static PlatformUtility Instance
        {
            get { return _platformUtility.Value; }
        }

        private PlatformUtility()
        {
        }

        internal static IEnumerable<XElement> LoadFrom(string filename)
        {

            if (OSInformation.IsWindowsPowerShell)
            {
                return WindowsPowerShellObject.LoadFrom(filename);
            }
            else
            {
                return PowerShellCoreObject.LoadFrom(filename);
            }
        }
    }

    /// <summary>
    /// Applys to Windows inbox components: FullClr and Nano Server
    /// </summary>
    internal class WindowsPowerShellObject 
    {
        private static readonly byte[] Utf = {0xef, 0xbb, 0xbf};

        internal static IEnumerable<XElement> LoadFrom(string filename)
        {

            if (!OSInformation.IsWindowsPowerShell)
            {
                // apply it for FullClr or Nano server only
                return Enumerable.Empty<XElement>();
            }
            var manifests = new List<XElement>();

            using (
                DisposableModule dll = NativeMethods.LoadLibraryEx(filename, Unused.Nothing,
                    LoadLibraryFlags.AsImageResource | LoadLibraryFlags.AsDatafile))
            {
                // if we get back a valid module handle
                if (!dll.IsInvalid)
                {
                    // search all the 'manifest' resources
                    if (NativeMethods.EnumResourceNamesEx(dll, ResourceType.Manifest, (m, type, id, param) =>
                    {
                        // for each manifest, check the language
                        NativeMethods.EnumResourceLanguagesEx(m, type, id,
                            (m1, resourceType, resourceId, language, unused) =>
                            {
                                // find the specific resource
                                var resource = NativeMethods.FindResourceEx(m1, resourceType, resourceId, language);
                                if (!resource.IsInvalid)
                                {
                                    // get a handle to the resource data
                                    var resourceData = NativeMethods.LoadResource(m1, resource);
                                    if (!resourceData.IsInvalid)
                                    {
                                        // copy the resource text out of the resource data
                                        try
                                        {
                                            var dataSize = NativeMethods.SizeofResource(m1, resource);
                                            var dataPointer = NativeMethods.LockResource(resourceData);

                                            // make sure that the pointer and size are legit.
                                            if (dataSize > 0 && dataPointer != IntPtr.Zero)
                                            {
                                                var data = new byte[dataSize];
                                                Marshal.Copy(dataPointer, data, 0, data.Length);
                                                var bomPresent = (data.Length >= 3 && data[0] == Utf[0] &&
                                                                  data[1] == Utf[1] && data[2] == Utf[2]);

                                                // create an XElement for the data returned.
                                                // IIRC, manifests are always UTF-8, n'est-ce pas?
                                                manifests.Add(
                                                    XElement.Parse(Encoding.UTF8.GetString(data, bomPresent ? 3 : 0,
                                                        bomPresent ? data.Length - 3 : data.Length)));
                                            }
                                        }
                                        catch
                                        {
                                            // skip it if it doesn't load.
                                        }
                                    }
                                }
                                return true;
                            }, Unused.Nothing, ResourceEnumFlags.None, LanguageId.None);

                        return true;
                    }, Unused.Nothing, ResourceEnumFlags.None, 0))
                    {
                    }
                }
            }
            return manifests;
        }
     
    }

    /// <summary>
    /// Applys to PowerShellCore (Windows, Linux, Mac)
    /// </summary>
    internal class PowerShellCoreObject
    {
        internal static IEnumerable<XElement> LoadFrom(string filename)
        {
            // apply it for FullClr or Nano server only
            return Enumerable.Empty<XElement>();
        }
    }
}
