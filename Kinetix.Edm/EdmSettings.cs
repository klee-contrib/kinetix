namespace Kinetix.Edm
{
    /// <summary>
    /// Configuration d'une datasource de GED.
    /// </summary>
    public class EdmSettings
    {
        /// <summary>
        /// Nom de la datasource.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// URL du site web de la GED.
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// Nom de la bibliothèque des documents.
        /// </summary>
        public string Library
        {
            get;
            set;
        }
    }
}
