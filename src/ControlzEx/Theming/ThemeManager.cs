namespace ControlzEx.Theming
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    /// A class that allows for the detection and alteration of a theme and accent.
    /// </summary>
    public static class ThemeManager
    {
        public const string AccentKeyName = "ControlzEx.Accent.Name";

        private static readonly Dictionary<string, CombinedAccent> accents = new Dictionary<string, CombinedAccent>();
        private static readonly Dictionary<string, CombinedAppTheme> appThemes = new Dictionary<string, CombinedAppTheme>();

        /// <summary>
        /// Gets a list of all of default accents.
        /// </summary>
        public static IReadOnlyList<CombinedAccent> Accents => accents.Values.ToList();

        /// <summary>
        /// Gets a list of all default themes.
        /// </summary>
        public static IReadOnlyList<CombinedAppTheme> AppThemes => appThemes.Values.ToList();

        /// <summary>
        /// Adds an accent with the given name.
        /// </summary>
        /// <returns>true if the accent does not exists and can be added.</returns>
        public static bool AddAccent(string name, Uri resourceAddress)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (resourceAddress == null)
            {
                throw new ArgumentNullException(nameof(resourceAddress));
            }

            return AddAccent(new Accent(name, resourceAddress));
        }

        /// <summary>
        /// Adds an accent with the given name.
        /// </summary>
        /// <returns>true if the accent does not exists and can be added.</returns>
        public static bool AddAccent(Accent accent)
        {
            var combinedAccent = accents.ContainsKey(accent.Name)
                ? accents[accent.Name]
                : null;

            if (combinedAccent == null)
            {
                combinedAccent = new CombinedAccent(accent.Name);
                accents.Add(accent.Name, combinedAccent);
            }

            combinedAccent.Accents.Add(accent);

            return true;
        }

        /// <summary>
        /// Adds an app theme with the given name.
        /// </summary>
        /// <returns>true if the app theme does not exists and can be added.</returns>
        public static bool AddAppTheme(string name, Uri resourceAddress)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (resourceAddress == null)
            {
                throw new ArgumentNullException(nameof(resourceAddress));
            }

            return AddAppTheme(new AppTheme(name, resourceAddress));
        }

        /// <summary>
        /// Adds an app theme with the given name.
        /// </summary>
        /// <returns>true if the app theme does not exists and can be added.</returns>
        public static bool AddAppTheme(AppTheme appTheme)
        {
            var combinedAppTheme = appThemes.ContainsKey(appTheme.Name)
                ? appThemes[appTheme.Name]
                : null;

            if (combinedAppTheme == null)
            {
                combinedAppTheme = new CombinedAppTheme(appTheme.Name);
                appThemes.Add(appTheme.Name, combinedAppTheme);
            }

            combinedAppTheme.AppThemes.Add(appTheme);

            return true;
        }

        /// <summary>
        /// Gets app theme with the given resource dictionary.
        /// </summary>
        /// <param name="resources"><see cref="ResourceDictionary"/> from which the theme should be retrieved.</param>
        /// <returns>AppTheme</returns>
        public static AppTheme GetAppTheme(ResourceDictionary resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            return appThemes.Values.SelectMany(x => x.AppThemes).FirstOrDefault(x => AreResourceDictionarySourcesEqual(x.Resources.Source, resources.Source));
        }

        /// <summary>
        /// Gets app theme with the given name and theme type (light or dark).
        /// </summary>
        /// <returns>AppTheme</returns>
        public static CombinedAppTheme GetAppTheme(string appThemeName)
        {
            if (appThemeName == null)
            {
                throw new ArgumentNullException(nameof(appThemeName));
            }

            return appThemes.Values.FirstOrDefault(x => x.Name.Equals(appThemeName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the inverse <see cref="AppTheme" /> of the given <see cref="AppTheme"/>.
        /// This method relies on the "Dark" or "Light" affix to be present.
        /// </summary>
        /// <param name="appTheme">The app theme.</param>
        /// <returns>The inverse <see cref="AppTheme"/> or <c>null</c> if it couldn't be found.</returns>
        /// <remarks>
        /// Returns BaseLight, if BaseDark is given or vice versa.
        /// Custom Themes must end with "Dark" or "Light" for this to work, for example "CustomDark" and "CustomLight".
        /// </remarks>
        public static CombinedAppTheme GetInverseAppTheme(CombinedAppTheme appTheme)
        {
            if (appTheme == null)
            {
                throw new ArgumentNullException(nameof(appTheme));
            }

            if (appTheme.Name.EndsWith("dark", StringComparison.OrdinalIgnoreCase))
            {
                return GetAppTheme(appTheme.Name.ToLower().Replace("dark", string.Empty) + "light");
            }

            if (appTheme.Name.EndsWith("light", StringComparison.OrdinalIgnoreCase))
            {
                return GetAppTheme(appTheme.Name.ToLower().Replace("light", string.Empty) + "dark");
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Accent"/> with the given name.
        /// </summary>
        /// <returns>The <see cref="Accent"/> or <c>null</c>, if the app theme wasn't found</returns>
        public static CombinedAccent GetAccent(string accentName)
        {
            if (accentName == null)
            {
                throw new ArgumentNullException(nameof(accentName));
            }

            return accents.Values.FirstOrDefault(x => x.Name.Equals(accentName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the <see cref="Accent"/> with the given resource dictionary.
        /// </summary>
        /// <param name="resources"><see cref="ResourceDictionary"/> from which the accent should be retrieved.</param>
        /// <returns>The <see cref="Accent"/> or <c>null</c>, if the accent wasn't found.</returns>
        public static Accent GetAccent(ResourceDictionary resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            var builtInAccent = accents.Values.SelectMany(x => x.Accents).FirstOrDefault(x => AreResourceDictionarySourcesEqual(x.Resources.Source, resources.Source));
            if (builtInAccent != null)
            {
                return builtInAccent;
            }

            // support dynamically created runtime resource dictionaries
            if (resources.Source == null)
            {
                if (IsAccentDictionary(resources))
                {
                    return new Accent($"Runtime accent {Guid.NewGuid()}", resources);
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified resource dictionary represents an <see cref="Accent"/>.
        /// <para />
        /// This might include runtime accents which do not have a resource uri.
        /// </summary>
        /// <param name="resources">The resources.</param>
        /// <returns><c>true</c> if the resource dictionary is an <see cref="Accent"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">resources</exception>
        public static bool IsAccentDictionary(ResourceDictionary resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            // Note: do not use contains, because that will look in all merged dictionaries as well. We need to check
            // out the actual keys of the current resource dictionary
            if (resources.Keys.OfType<string>().Any(keyAsString => string.Equals(keyAsString, AccentKeyName)) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a resource from the detected AppStyle.
        /// </summary>
        /// <param name="window">The window to check. If this is null, the Application's sources will be checked.</param>
        /// <param name="key">The key to check against.</param>
        /// <returns>The resource object or null, if the resource wasn't found.</returns>
        public static object GetResourceFromAppStyle(Window window, string key)
        {
            var appStyle = window != null 
                               ? DetectAppStyle(window) 
                               : DetectAppStyle(Application.Current);

            if (appStyle == null 
                && window != null)
            {
                appStyle = DetectAppStyle(Application.Current); //no resources in the window's resources.
            }

            if (appStyle == null)
            {
                // nothing to do here, we can't found an app style (make sure all custom themes are added!)
                return null;
            }

            //next check the accent
            var accentResource = appStyle.Item2.GetResource(key);

            if (accentResource != null)
            {
                return accentResource;
            }

            return appStyle.Item1.GetResource(key);
        }

        /// <summary>
        /// Change the theme for the whole application.
        /// </summary>
        [SecurityCritical]
        public static void ChangeAppTheme(Application app, string themeName)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (themeName == null)
            {
                throw new ArgumentNullException(nameof(themeName));
            }

            var oldTheme = DetectAppStyle(app);
            CombinedAppTheme matched;
            if ((matched = GetAppTheme(themeName)) != null)
            {
                ChangeAppStyle(app.Resources, oldTheme, oldTheme.Item2, matched);
            }
        }

        /// <summary>
        /// Change theme for the given window.
        /// </summary>
        [SecurityCritical]
        public static void ChangeAppTheme(Window window, string themeName)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            if (themeName == null)
            {
                throw new ArgumentNullException(nameof(themeName));
            }

            var oldTheme = DetectAppStyle(window);
            CombinedAppTheme matched;
            if ((matched = GetAppTheme(themeName)) != null)
            {
                ChangeAppStyle(window.Resources, oldTheme, oldTheme.Item2, matched);
            }
        }

        /// <summary>
        /// Change accent and theme for the whole application.
        /// </summary>
        /// <param name="app">The instance of Application to change.</param>
        /// <param name="newAccent">The accent to apply.</param>
        /// <param name="newTheme">The theme to apply.</param>
        [SecurityCritical]
        public static void ChangeAppStyle(Application app, CombinedAccent newAccent, CombinedAppTheme newTheme)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var oldTheme = DetectAppStyle(app);
            ChangeAppStyle(app.Resources, oldTheme, newAccent, newTheme);
        }

        /// <summary>
        /// Change accent and theme for the given window.
        /// </summary>
        /// <param name="window">The Window to change.</param>
        /// <param name="newAccent">The accent to apply.</param>
        /// <param name="newTheme">The theme to apply.</param>
        [SecurityCritical]
        public static void ChangeAppStyle(Window window, CombinedAccent newAccent, CombinedAppTheme newTheme)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            var oldTheme = DetectAppStyle(window);
            ChangeAppStyle(window.Resources, oldTheme, newAccent, newTheme);
        }

        [SecurityCritical]
        private static void ChangeAppStyle(ResourceDictionary resources, Tuple<CombinedAppTheme, CombinedAccent> oldThemeInfo, CombinedAccent newCombinedAccent, CombinedAppTheme newCombinedAppTheme)
        {
            var themeChanged = false;
            if (oldThemeInfo != null)
            {
                var oldCombinedAccent = oldThemeInfo.Item2;
                if (oldCombinedAccent != null && oldCombinedAccent.Name != newCombinedAccent.Name)
                {
                    foreach (var accent in newCombinedAccent.Accents)
                    {
                        resources.MergedDictionaries.Add(accent.Resources);
                    }

                    foreach (var oldAccent in oldCombinedAccent.Accents)
                    {
                        var key = oldAccent.Resources.Source.ToString().ToLower();
                        var oldAccentResources = resources.MergedDictionaries.Where(x => x.Source != null && x.Source.ToString().ToLower() == key)
                                                          .ToList();

                        foreach (var oldAccentResource in oldAccentResources)
                        {
                            resources.MergedDictionaries.Remove(oldAccentResource);
                        }
                    }

                    themeChanged = true;
                }

                var oldCombinedAppTheme = oldThemeInfo.Item1;
                if (oldCombinedAppTheme != null && oldCombinedAppTheme != newCombinedAppTheme)
                {
                    foreach (var appTheme in newCombinedAppTheme.AppThemes)
                    {
                        resources.MergedDictionaries.Add(appTheme.Resources);
                    }

                    foreach (var oldAppTheme in oldCombinedAppTheme.AppThemes)
                    {
                        var key = oldAppTheme.Resources.Source.ToString().ToLower();
                        var oldAppThemeResources = resources.MergedDictionaries.Where(x => x.Source != null && x.Source.ToString().ToLower() == key)
                                                            .ToList();

                        foreach (var oldAccentResource in oldAppThemeResources)
                        {
                            resources.MergedDictionaries.Remove(oldAccentResource);
                        }
                    }

                    themeChanged = true;
                }
            }
            else
            {
                ChangeAppStyle(resources, newCombinedAccent, newCombinedAppTheme);

                themeChanged = true;
            }

            if (themeChanged)
            {
                OnThemeChanged(newCombinedAccent, newCombinedAppTheme);
            }
        }

        /// <summary>
        /// Changes the accent and theme of a ResourceDictionary directly.
        /// </summary>
        /// <param name="resources">The ResourceDictionary to modify.</param>
        /// <param name="newAccent">The accent to apply to the ResourceDictionary.</param>
        /// <param name="newTheme">The theme to apply to the ResourceDictionary.</param>
        [SecurityCritical]
        public static void ChangeAppStyle(ResourceDictionary resources, CombinedAccent newAccent, CombinedAppTheme newTheme)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            if (newAccent == null)
            {
                throw new ArgumentNullException(nameof(newAccent));
            }

            if (newTheme == null)
            {
                throw new ArgumentNullException(nameof(newTheme));
            }

            foreach (var accent in newAccent.Accents)
            {
                ApplyResourceDictionary(accent.Resources, resources);    
            }
            
            foreach (var appTheme in newTheme.AppThemes)
            {
                ApplyResourceDictionary(appTheme.Resources, resources);    
            }
        }

        [SecurityCritical]

        private static void ApplyResourceDictionary(ResourceDictionary newRd, ResourceDictionary oldRd)
        {
            oldRd.BeginInit();

            foreach (DictionaryEntry r in newRd)
            {
                if (oldRd.Contains(r.Key))
                {
                    oldRd.Remove(r.Key);
                }

                oldRd.Add(r.Key, r.Value);
            }

            oldRd.EndInit();
        }

        /// <summary>
        /// Scans the window resources and returns it's accent and theme.
        /// </summary>
        public static Tuple<CombinedAppTheme, CombinedAccent> DetectAppStyle()
        {
            try
            {
                var style = DetectAppStyle(Application.Current.MainWindow);

                if (style != null)
                {
                    return style;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to detect app style on main window.{Environment.NewLine}{ex}");
            }

            return DetectAppStyle(Application.Current);
        }

        /// <summary>
        /// Scans the window resources and returns it's accent and theme.
        /// </summary>
        /// <param name="window">The Window to scan.</param>
        public static Tuple<CombinedAppTheme, CombinedAccent> DetectAppStyle(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            var detectedStyle = DetectAppStyle(window.Resources);
            if (detectedStyle == null)
            {
                detectedStyle = DetectAppStyle(Application.Current.Resources);
            }

            return detectedStyle;
        }

        /// <summary>
        /// Scans the application resources and returns it's accent and theme.
        /// </summary>
        /// <param name="app">The Application instance to scan.</param>
        public static Tuple<CombinedAppTheme, CombinedAccent> DetectAppStyle(Application app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return DetectAppStyle(app.Resources);
        }

        /// <summary>
        /// Scans a resources and returns it's accent and theme.
        /// </summary>
        /// <param name="resources">The ResourceDictionary to check.</param>
        private static Tuple<CombinedAppTheme, CombinedAccent> DetectAppStyle(ResourceDictionary resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            CombinedAppTheme currentTheme = null;
            Tuple<CombinedAppTheme, CombinedAccent> detectedAccentTheme = null;

            if (DetectThemeFromResources(resources, ref currentTheme))
            {
                if (GetThemeFromResources(resources, currentTheme, ref detectedAccentTheme))
                {
                    return new Tuple<CombinedAppTheme, CombinedAccent>(detectedAccentTheme.Item1, detectedAccentTheme.Item2);
                }
            }

            return null;
        }

        private static bool DetectThemeFromResources(ResourceDictionary dict, ref CombinedAppTheme detectedTheme)
        {
            AppTheme matched;
            if ((matched = GetAppTheme(dict)) != null)
            {
                detectedTheme = appThemes[matched.Name];
                return true;
            }

            foreach (var rd in dict.MergedDictionaries.Reverse())
            {
                if (DetectThemeFromResources(rd, ref detectedTheme))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool GetThemeFromResources(ResourceDictionary dict, CombinedAppTheme presetTheme, ref Tuple<CombinedAppTheme, CombinedAccent> detectedAccentTheme)
        {
            var currentTheme = presetTheme;

            Accent matched;
            if ((matched = GetAccent(dict)) != null)
            {
                detectedAccentTheme = Tuple.Create(currentTheme, accents[matched.Name]);
                return true;
            }

            foreach (var rd in dict.MergedDictionaries.Reverse())
            {
                if (GetThemeFromResources(rd, presetTheme, ref detectedAccentTheme))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This event fires if accent color and theme was changed
        /// this should be using the weak event pattern, but for now it's enough
        /// </summary>
        public static event EventHandler<OnThemeChangedEventArgs> IsThemeChanged;

        /// <summary>
        /// Invalidates global colors and resources.
        /// Sometimes the ContextMenu is not changing the colors, so this will fix it.
        /// </summary>
        [SecurityCritical]
        private static void OnThemeChanged(CombinedAccent newAccent, CombinedAppTheme newTheme)
        {
            IsThemeChanged?.Invoke(Application.Current, new OnThemeChangedEventArgs(newTheme, newAccent));
        }

        private static bool AreResourceDictionarySourcesEqual(Uri first, Uri second)
        {
            return Uri.Compare(first, second,
                 UriComponents.Host | UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #region WindowsAppModeSetting

        /// <summary>
        /// Synchronizes the current <see cref="AppTheme"/> with the "app mode" setting from windows.
        /// </summary>
        public static void SyncAppThemeWithWindowsAppModeSetting()
        {
            var appThemeName = AppsUseLightTheme()
                               ? AppTheme.Light
                               : AppTheme.Dark;

            ChangeAppTheme(Application.Current, appThemeName);
        }

        private static bool isAutomaticWindowsAppModeSettingSyncEnabled;

        /// <summary>
        /// Gets or sets wether changes to the "app mode" setting from windows should be detected at runtime and the current <see cref="AppTheme"/> be changed accordingly.
        /// </summary>
        public static bool IsAutomaticWindowsAppModeSettingSyncEnabled
        {
            get { return isAutomaticWindowsAppModeSettingSyncEnabled; }

            set
            {
                if (value == isAutomaticWindowsAppModeSettingSyncEnabled)
                {
                    return;
                }

                isAutomaticWindowsAppModeSettingSyncEnabled = value;

                if (isAutomaticWindowsAppModeSettingSyncEnabled)
                {
                    SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
                }
                else
                {
                    SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
                }
            }
        }

        private static void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                SyncAppThemeWithWindowsAppModeSetting();
            }
        }

        private static bool AppsUseLightTheme()
        {
            try
            {
                return Convert.ToBoolean(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", true));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }

            return false;
        }

        #endregion WindowsAppModeSetting
    }

    /// <summary>
    /// Class which is used as argument for an event to signal theme changes.
    /// </summary>
    public class OnThemeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public OnThemeChangedEventArgs(CombinedAppTheme appTheme, CombinedAccent accent)
        {
            this.AppTheme = appTheme;
            this.Accent = accent;
        }

        /// <summary>
        /// The new theme.
        /// </summary>
        public CombinedAppTheme AppTheme { get; set; }

        /// <summary>
        /// The new accent
        /// </summary>
        public CombinedAccent Accent { get; set; }
    }
}