﻿using System;

namespace Kinetix.ComponentModel.Annotations
{
    /// <summary>
    /// Indique qu'une classe fournie les métadonnées des domaines.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DomainMetadataTypeAttribute : Attribute
    {
    }
}
