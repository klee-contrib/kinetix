namespace Kinetix.Caching
{
    /// <summary>
    /// Cache status.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The cache is uninitialised. It cannot be used.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// The cache is alive. It can be used.
        /// </summary>
        Alive,

        /// <summary>
        /// The cache is shudown. It cannot be used.
        /// </summary>
        Shutdown
    }
}
