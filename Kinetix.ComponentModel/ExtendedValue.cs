using System;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Valeur et métadonnées associées.
    /// </summary>
    public sealed class ExtendedValue : IComparable
    {
        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <param name="metadata">Métadonnées.</param>
        public ExtendedValue(object value, object metadata)
        {
            Value = value;
            Metadata = metadata;
        }

        /// <summary>
        /// Valeur principale.
        /// </summary>
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Métadonnées.
        /// </summary>
        public object Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Test l'égalité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator ==(ExtendedValue source, ExtendedValue test) =>
            Equals(source, null) && Equals(test, null)
                ? Equals(source, test)
                : Equals(source, null) && !Equals(test, null)
                    ? test.Equals(source)
                    : source.Equals(test);

        /// <summary>
        /// Test l'inégalité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator !=(ExtendedValue source, ExtendedValue test) => !(source == test);

        /// <summary>
        /// Test l'inferiorité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator <(ExtendedValue source, ExtendedValue test)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.CompareTo(test) < 0;
        }

        /// <summary>
        /// Test la supériorité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator >(ExtendedValue source, ExtendedValue test)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.CompareTo(test) > 0;
        }

        /// <summary>
        /// Indique si l'ExtendedValue est égale a un objet donné.
        /// </summary>
        /// <param name="obj">L'objet à comparer.</param>
        /// <returns>Indique si les objets sont égaux.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ExtendedValue;
            return other == null ?
                base.Equals(obj) :
                object.Equals(Value, other.Value) && object.Equals(Metadata, other.Metadata);
        }

        /// <summary>
        /// Retourne le hash de l'instance.
        /// </summary>
        /// <returns>Le hash.</returns>
        /// <remarks>Requis par l'implémentation d'Equals.</remarks>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Comparaison.
        /// </summary>
        /// <param name="obj">L'objet.</param>
        /// <returns>Retourne 0 si équivalent, -1 si inférieur, 1 si supérieur.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var value = (ExtendedValue)obj;
            return Equals(Value, value.Value) && Equals(Metadata, value.Metadata)
                ? 0
                : Value == null
                    ? -1
                    : decimal.Compare((decimal)Value, (decimal)value.Value);
        }
    }
}
