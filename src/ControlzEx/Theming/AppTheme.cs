namespace ControlzEx.Theming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;

    public class CombinedAppTheme
    {
        public CombinedAppTheme(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public List<AppTheme> AppThemes { get; } = new List<AppTheme>();

        public object GetResource(string key)
        {
            foreach (var accent in this.AppThemes)
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
    ///     Represents the background theme of the application.
    /// </summary>
    [DebuggerDisplay("apptheme={Name}, res={Resources.Source}")]
    public class AppTheme
    {
        public const string Light = "BaseLight";
        public const string Dark = "BaseDark";

        public static IReadOnlyList<string> DefaultAppThemeNames = new[]
                                                                   {
                                                                       Dark,
                                                                       Light
                                                                   };

        /// <summary>
        ///     Initializes a new instance of the AppTheme class.
        /// </summary>
        /// <param name="name">The name of the new AppTheme.</param>
        /// <param name="resourceAddress">The URI of the accent ResourceDictionary.</param>
        public AppTheme(string name, Uri resourceAddress)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (resourceAddress == null)
            {
                throw new ArgumentNullException(nameof(resourceAddress));
            }

            this.Name = name;
            this.Resources = new ResourceDictionary
                             {
                                 Source = resourceAddress
                             };
        }

        /// <summary>
        ///     The ResourceDictionary that represents this application theme.
        /// </summary>
        public ResourceDictionary Resources { get; }

        /// <summary>
        ///     Gets the name of the application theme.
        /// </summary>
        public string Name { get; }
    }
}