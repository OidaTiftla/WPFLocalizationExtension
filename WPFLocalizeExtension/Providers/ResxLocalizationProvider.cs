﻿#region Copyright information

// <copyright file="ResxLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>

#endregion Copyright information

namespace WPFLocalizeExtension.Providers {

    #region Uses

    using Engine;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Resources;
    using System.Windows;
    using XAMLMarkupExtensions.Base;

    #endregion Uses

    /// <summary>
    /// A singleton RESX provider that uses attached properties and the Parent property to iterate through the visual tree.
    /// </summary>
    public class ResxLocalizationProvider : ResxLocalizationProviderBase {

        #region Dependency Properties

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, DefaultDictionaryChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, DefaultAssemblyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> IgnoreCase to set the case sensitivity.
        /// </summary>
        public static readonly DependencyProperty IgnoreCaseProperty =
            DependencyProperty.RegisterAttached(
                "IgnoreCase",
                typeof(bool),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(true, IgnoreCaseChanged));

        #endregion Dependency Properties

        #region Dependency Property Callback

        /// <summary>
        /// Indicates, that the <see cref="DefaultDictionaryProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void DefaultDictionaryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Instance.FallbackDictionary = e.NewValue != null ? e.NewValue.ToString() : null;
            Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// Indicates, that the <see cref="DefaultAssemblyProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void DefaultAssemblyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Instance.FallbackAssembly = e.NewValue != null ? e.NewValue.ToString() : null;
            Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// Indicates, that the <see cref="IgnoreCaseProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void IgnoreCaseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Instance.IgnoreCase = (bool)e.NewValue;
            Instance.OnProviderChanged(obj);
        }

        #endregion Dependency Property Callback

        #region Dependency Property Management

        #region Get

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to get the default dictionary from.</param>
        /// <returns>The default dictionary.</returns>
        public static string GetDefaultDictionary(DependencyObject obj) {
            return obj.GetValueSync<string>(DefaultDictionaryProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj) {
            return obj.GetValueSync<string>(DefaultAssemblyProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> ignore case flag.
        /// </summary>
        /// <param name="obj">The dependency object to get the ignore case flag from.</param>
        /// <returns>The ignore case flag.</returns>
        public static bool GetIgnoreCase(DependencyObject obj) {
            return obj.GetValueSync<bool>(IgnoreCaseProperty);
        }

        #endregion Get

        #region Set

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to set the default dictionary to.</param>
        /// <param name="value">The dictionary.</param>
        public static void SetDefaultDictionary(DependencyObject obj, string value) {
            obj.SetValueSync(DefaultDictionaryProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value) {
            obj.SetValueSync(DefaultAssemblyProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> ignore case flag.
        /// </summary>
        /// <param name="obj">The dependency object to set the ignore case flag to.</param>
        /// <param name="value">The ignore case flag.</param>
        public static void SetIgnoreCase(DependencyObject obj, bool value) {
            obj.SetValueSync(IgnoreCaseProperty, value);
        }

        #endregion Set

        #endregion Dependency Property Management

        #region Variables

        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private readonly ParentNotifiers _parentNotifiers = new ParentNotifiers();

        /// <summary>
        /// To use when no assembly is specified.
        /// </summary>
        public string FallbackAssembly { get; set; }

        /// <summary>
        /// To use when no dictionary is specified.
        /// </summary>
        public string FallbackDictionary { get; set; }

        #endregion Variables

        #region Singleton Variables, Properties & Constructor

        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static ResxLocalizationProvider instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="ResxLocalizationProvider"/> singleton.
        /// </summary>
        public static ResxLocalizationProvider Instance {
            get {
                if (instance == null) {
                    lock (InstanceLock) {
                        if (instance == null)
                            instance = new ResxLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// Resets the instance that is used for the ResxLocationProvider
        /// </summary>
        public static void Reset() {
            lock (InstanceLock) {
                instance = null;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private ResxLocalizationProvider() {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo>();
            AvailableCultures.Add(CultureInfo.InvariantCulture);
        }

        #endregion Singleton Variables, Properties & Constructor

        #region Abstract assembly & dictionary lookup

        /// <summary>
        /// An action that will be called when a parent of one of the observed target objects changed.
        /// </summary>
        /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
        private void ParentChangedAction(DependencyObject obj) {
            OnProviderChanged(obj);
        }

        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected override string GetAssembly(DependencyObject target) {
            if (target == null)
                return FallbackAssembly;

            var assembly = target.GetValueOrRegisterParentNotifier<string>(DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers);
            return String.IsNullOrEmpty(assembly) ? FallbackAssembly : assembly;
        }

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected override string GetDictionary(DependencyObject target) {
            if (target == null)
                return FallbackDictionary;

            var dictionary = target.GetValueOrRegisterParentNotifier<string>(DefaultDictionaryProperty, ParentChangedAction, _parentNotifiers);
            return String.IsNullOrEmpty(dictionary) ? FallbackDictionary : dictionary;
        }

        #endregion Abstract assembly & dictionary lookup
    }
}