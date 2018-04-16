namespace ControlzEx.Theming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;

    public class CombinedAccent
    {
        public CombinedAccent(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public List<Accent> Accents { get; } = new List<Accent>();

        public object GetResource(string key)
        {
            foreach (var accent in this.Accents)
            {
                var resource = accent.Resources[key];

                if (resource != null)
                {
                    return resource;
                }
            }

            return null;
        }
    }

    /// <summary>
    ///     An object that represents the foreground color for a <see cref="AppTheme" />.
    /// </summary>
    [DebuggerDisplay("accent={Name}, res={Resources.Source}")]
    public class Accent
    {
        public static IReadOnlyList<string> DefaultAccentNames = new[]
                                                                 {
                                                                     "Amber",
                                                                     "Blue",
                                                                     "Brown",
                                                                     "Cobalt",
                                                                     "Crimson",
                                                                     "Cyan",
                                                                     "Emerald",
                                                                     "Green",
                                                                     "Indigo",
                                                                     "Lime",
                                                                     "Magenta",
                                                                     "Mauve",
                                                                     "Olive",
                                                                     "Orange",
                                                                     "Pink",
                                                                     "Purple",
                                                                     "Red",
                                                                     "Sienna",
                                                                     "Steel",
                                                                     "Taupe",
                                                                     "Teal",
                                                                     "Violet",
                                                                     "Yellow"
                                                                 };

        /// <summary>
        ///     Initializes a new instance of the <see cref="Accent" /> class.
        /// </summary>
        /// <param name="name">The name of the new Accent.</param>
        /// <param name="resourceAddress">The URI of the accent ResourceDictionary.</param>
        public Accent(string name, Uri resourceAddress)
            : this(name, new ResourceDictionary
                         {
                             Source = resourceAddress
                         })
        {
            if (resourceAddress == null)
            {
                throw new ArgumentNullException(nameof(resourceAddress));
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Accent" /> class.
        /// </summary>
        /// <param name="name">The name of the new Accent.</param>
        /// <param name="resources">The accent ResourceDictionary.</param>
        public Accent(string name, ResourceDictionary resources)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.Resources = resources;
        }

        /// <summary>
        ///     Gets/sets the name of the Accent.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The ResourceDictionary that represents this Accent.
        /// </summary>
        public ResourceDictionary Resources { get; }
    }
}