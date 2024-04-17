namespace Unity.XR.Management.AndroidManifest.Editor
{
    /// <summary>
    /// Interface for classes providing Android manifest entries.
    ///
    /// All implementers must be top level classes for the Android manifest processsor to call them
    /// Nested classes aren't supported.
    /// </summary>
    public interface IAndroidManifestRequirementProvider
    {
        /// <summary>
        /// Provides a <see cref="ManifestRequirement"/> object with the required Android manifest elements and its attributes needed to be
        /// added, overriden or removed.
        /// 
        /// </summary>
        /// <returns><see cref="ManifestRequirement"/> with element requirements data.</returns>
        ManifestRequirement ProvideManifestRequirement();
    }
}
