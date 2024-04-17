using System;
using System.Collections.Generic;

namespace Unity.XR.Management.AndroidManifest.Editor
{
    /// <summary>
    /// This class contains lists of Android manifest elements that need to be added, overriden or removed from the application manifest.
    /// </summary>
    public class ManifestRequirement : IEquatable<ManifestRequirement>
    {
        /// <summary>
        /// Set of supported <see cref="UnityEngine.XR.Management.XRLoader"/> types by these requirements.
        /// If none of the listed loaders is active at the moment of building, the requirements will be ignored.
        /// </summary>
        public HashSet<Type> SupportedXRLoaders { get; set; } = new HashSet<Type>();

        /// <summary>
        /// List of <see cref="ManifestElement"/> elements that will be added to the Android manifest.
        /// Each entry represents a single element within its specified node path, and it won't overwrite or override any other element to be added.
        /// </summary>
        public List<ManifestElement> NewElements { get; set; } = new List<ManifestElement>();

        /// <summary>
        /// List of <see cref="ManifestElement"/> elements whose attirbutes will be merged or overriden with existing the Android manifest elements.
        /// If the manifest element doesn't exist in the file, it will be created.
        /// </summary>
        public List<ManifestElement> OverrideElements { get; set; } = new List<ManifestElement>();

        /// <summary>
        /// List of <see cref="ManifestElement"/> elements which will be removed from the Android manifest. 
        /// Entries not found will be ignored.
        /// Only entries that specify the same attributes and its respective values in the manifest will be taken in account for deletion.
        /// </summary>
        public List<ManifestElement> RemoveElements { get; set; } = new List<ManifestElement>();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is ManifestRequirement && Equals(obj as ManifestRequirement);
        }

        /// <inheritdoc/>
        public bool Equals(ManifestRequirement other)
        {
            return other != null &&
                ((NewElements == null && other.NewElements == null) || (NewElements != null && NewElements.Equals(other.NewElements))) &&
                ((OverrideElements == null && other.OverrideElements == null) || (OverrideElements != null && OverrideElements.Equals(other.OverrideElements))) &&
                ((RemoveElements == null && other.RemoveElements == null) || (RemoveElements != null && RemoveElements.Equals(other.RemoveElements)));
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
#if UNITY_2021_3_OR_NEWER
            return HashCode.Combine(NewElements, OverrideElements, RemoveElements);
#else
            // Unfortunately, 2020.3 compiles against .NET 2.0, which doesn't include the more reasonable HashCode class.
            // Still, a hash function is needed for comparing these objects, so going on with the option outlined here:
            // https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            int factor = 31;
            int hash = 17;
            hash = hash * factor + (NewElements != null ? NewElements.GetHashCode() : 0);
            hash = hash * factor + (OverrideElements != null ? OverrideElements.GetHashCode() : 0);
            hash = hash * factor + (RemoveElements != null ? RemoveElements.GetHashCode() : 0);
            return hash;
#endif // UNITY_2021_3_OR_NEWER
        }
    }
}
