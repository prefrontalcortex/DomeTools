using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.Management.AndroidManifest.Editor
{
    /// <summary>
    /// This class holds information for a single Android manifest element, including its path and attributes.
    /// </summary>
    public class ManifestElement
    {
        /// <summary>
        ///     <para>
        ///         List of element names representing the full XML path to the element. It must include last the element that this object represents.
        ///     </para>
        /// </summary>
        /// 
        /// <remarks>
        ///     <para>
        ///         The order in which the elements are added is important,
        ///         as each list member represents an XML element name in the order,
        ///         so specifying a list as { "manifest", "application" } is not the same 
        ///         as { "application", "manifest"}.
        ///     </para>
        ///     <example>
        ///         Example for accessing a meta-data element:
        ///         <code>
        ///             new ManifestElement {
        ///                 ElementPath = new List() {
        ///                     "manifest", "application", "meta-data"
        ///                 }
        ///             }
        ///         </code>
        ///     </example>
        /// </remarks>
        public List<string> ElementPath { get; set; }

        /// <summary>
        /// Dictionary of Name-Value pairs of the represented element's attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }
    }
}
