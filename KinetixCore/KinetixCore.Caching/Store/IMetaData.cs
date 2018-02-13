namespace Kinetix.Caching.Store
{
    /// <summary>
    /// A type representing relevant metadata from an element, used by LfuPolicy for its operations.
    /// </summary>
    internal interface IMetaData
    {
        /// <summary>
        /// The key of this object.
        /// </summary>
        object Key
        {
            get;
        }

        /// <summary>
        /// The hit count for the element.
        /// </summary>
        long HitCount
        {
            get;
        }
    }
}
