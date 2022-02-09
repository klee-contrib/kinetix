using System;
using Moq;

namespace Kinetix.Services
{
    /// <summary>
    /// Usine à instances factices.
    /// </summary>
    internal class DummyFactory
    {
        /// <summary>
        /// Retourne une instance factice d'un type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Instance factice.</returns>
        public static object GetDummy(Type type)
        {
            /* On utilise Moq, qui est normalement destiné à faire des mock dans un environnement de tests. */
            var mockType = typeof(Mock<>).MakeGenericType(type);
            var mockInstance = (Mock)Activator.CreateInstance(mockType);
            return mockInstance.Object;
        }
    }
}
