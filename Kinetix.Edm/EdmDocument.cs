namespace Kinetix.Edm
{
    /// <summary>
    /// Document de la GED.
    /// </summary>
    public class EdmDocument
    {
        /// <summary>
        /// Créé une nouvelle instance de EdmDocument.
        /// </summary>
        public EdmDocument()
        {
            Fields = new EdmFields();
        }

        /// <summary>
        /// Clé identifiant le document dans la GED.
        /// </summary>
        public object EdmId
        {
            get;
            set;
        }

        /// <summary>
        /// Nom du document.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Contenu du document.
        /// </summary>
        public byte[] Content
        {
            get;
            set;
        }

        /// <summary>
        /// Champs du document dans la GED.
        /// </summary>
        public EdmFields Fields
        {
            get;
            private set;
        }
    }
}
