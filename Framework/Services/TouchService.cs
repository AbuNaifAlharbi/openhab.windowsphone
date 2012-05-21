/* openHAB, the open Home Automation Bus.
 * Copyright (C) 2010-${year}, openHAB.org <admin@openhab.org>
 * 
 * See the contributors.txt file in the distribution for a
 * full listing of individual contributors.
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.gnu.org/licenses>.
 * 
 * Additional permission under GNU GPL version 3 section 7
 * 
 * If you modify this Program, or any covered work, by linking or 
 * combining it with Eclipse (or a modified version of that library),
 * containing parts covered by the terms of the Eclipse Public License
 * (EPL), the licensors of this Program grant you additional permission
 * to convey the resulting work.
 */
 
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Framework.Services
{
    public static class TouchService
    {
        public static readonly DependencyProperty DoubleTapCommandProperty = DependencyProperty.RegisterAttached(
            "DoubleTapCommand",                                     // Name of the property
            typeof(ICommand),                                       // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null, OnDoubleTapCommandChanged)); // Callback invoked in case the property value has changed
        public static readonly DependencyProperty DoubleTapCommandParameterProperty = DependencyProperty.RegisterAttached(
            "DoubleTapCommandParameter",                            // Name of the property
            typeof(object),                                         // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null));                            // Callback invoked in case the property value has changed

        public static void SetDoubleTapCommand(DependencyObject aObject, ICommand aCommand)
        {
            aObject.SetValue(DoubleTapCommandProperty, aCommand);
        }
        public static ICommand GetDoubleTapCommand(DependencyObject aObject)
        {
            return (ICommand)aObject.GetValue(DoubleTapCommandProperty);
        }

        public static void SetDoubleTapCommandParameter(DependencyObject aObject, object aParameter)
        {
            aObject.SetValue(DoubleTapCommandParameterProperty, aParameter);
        }
        public static object GetDoubleTapCommandParameter(DependencyObject aObject)
        {
            return (object)aObject.GetValue(DoubleTapCommandParameterProperty);
        }

        private static void OnDoubleTapCommandChanged(DependencyObject aSender, DependencyPropertyChangedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            if (aArgs.OldValue != null)
                fw.DoubleTap -= OnDoubleTap;
            if(aArgs.NewValue != null)
                fw.DoubleTap += OnDoubleTap;
        }

        private static void OnDoubleTap(object aSender, GestureEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if(fw == null)
                return;

            var command = GetDoubleTapCommand(fw);
            var param = GetDoubleTapCommandParameter(fw);

            if (command.CanExecute(param))
                command.Execute(param);
        }

        public static readonly DependencyProperty TapCommandProperty = DependencyProperty.RegisterAttached(
            "TapCommand",                                     // Name of the property
            typeof(ICommand),                                       // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null, OnTapCommandChanged)); // Callback invoked in case the property value has changed
        public static readonly DependencyProperty TapCommandParameterProperty = DependencyProperty.RegisterAttached(
            "TapCommandParameter",                            // Name of the property
            typeof(object),                                         // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null));                            // Callback invoked in case the property value has changed

        public static void SetTapCommand(DependencyObject aObject, ICommand aCommand)
        {
            aObject.SetValue(TapCommandProperty, aCommand);
        }
        public static ICommand GetTapCommand(DependencyObject aObject)
        {
            return (ICommand)aObject.GetValue(TapCommandProperty);
        }

        public static void SetTapCommandParameter(DependencyObject aObject, object aParameter)
        {
            aObject.SetValue(TapCommandParameterProperty, aParameter);
        }
        public static object GetTapCommandParameter(DependencyObject aObject)
        {
            return (object)aObject.GetValue(TapCommandParameterProperty);
        }

        private static void OnTapCommandChanged(DependencyObject aSender, DependencyPropertyChangedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            if (aArgs.OldValue != null)
                fw.Tap -= OnTap;
            if (aArgs.NewValue != null)
                fw.Tap += OnTap;
        }

        private static void OnTap(object aSender, GestureEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            var command = GetTapCommand(fw);
            var param = GetTapCommandParameter(fw);

            if (command.CanExecute(param))
                command.Execute(param);
        }

        public static readonly DependencyProperty HoldCommandProperty = DependencyProperty.RegisterAttached(
            "HoldCommand",                                     // Name of the property
            typeof(ICommand),                                       // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null, OnHoldCommandChanged)); // Callback invoked in case the property value has changed
        public static readonly DependencyProperty HoldCommandParameterProperty = DependencyProperty.RegisterAttached(
            "HoldCommandParameter",                            // Name of the property
            typeof(object),                                         // Type of the property
            typeof(TouchService),                                   // Type of the provider of the registered attached property
            new PropertyMetadata(null));                            // Callback invoked in case the property value has changed

        public static void SetHoldCommand(DependencyObject aObject, ICommand aCommand)
        {
            aObject.SetValue(HoldCommandProperty, aCommand);
        }
        public static ICommand GetHoldCommand(DependencyObject aObject)
        {
            return (ICommand)aObject.GetValue(HoldCommandProperty);
        }

        public static void SetHoldCommandParameter(DependencyObject aObject, object aParameter)
        {
            aObject.SetValue(HoldCommandParameterProperty, aParameter);
        }
        public static object GetHoldCommandParameter(DependencyObject aObject)
        {
            return (object)aObject.GetValue(HoldCommandParameterProperty);
        }

        private static void OnHoldCommandChanged(DependencyObject aSender, DependencyPropertyChangedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            if (aArgs.OldValue != null)
                fw.Hold -= OnHold;
            if (aArgs.NewValue != null)
                fw.Hold += OnHold;
        }

        private static void OnHold(object aSender, GestureEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            var command = GetHoldCommand(fw);
            var param = GetHoldCommandParameter(fw);

            if (command.CanExecute(param))
                command.Execute(param);
        }

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.RegisterAttached(
            "IsPressed",                                     // Name of the property
            typeof(bool?),                                   // Type of the property
            typeof(TouchService),                            // Type of the provider of the registered attached property
            new PropertyMetadata(null, OnIsPressedChanged)); // Callback invoked in case the property value has changed

        public static void SetIsPressed(DependencyObject aObject, bool? aParameter)
        {
            aObject.SetValue(IsPressedProperty, aParameter);
        }
        public static bool? GetIsPressed(DependencyObject aObject)
        {
            return (bool?)aObject.GetValue(IsPressedProperty);
        }

        private static void OnIsPressedChanged(DependencyObject aSender, DependencyPropertyChangedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            var oldVal = (bool?)aArgs.OldValue;
            var newVal = (bool?)aArgs.NewValue;

            if (!oldVal.HasValue && newVal.HasValue)
            {
                fw.ManipulationStarted += OnManipulationStarted;
                fw.ManipulationCompleted += OnManipulationCompleted;
            }
            if (oldVal.HasValue && !newVal.HasValue)
            {
                fw.ManipulationStarted -= OnManipulationStarted;
                fw.ManipulationCompleted -= OnManipulationCompleted;
            }
        }

        private static void OnManipulationStarted(object aSender, ManipulationStartedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            SetIsPressed(fw, true);
        }
        private static void OnManipulationCompleted(object aSender, ManipulationCompletedEventArgs aArgs)
        {
            var fw = aSender as FrameworkElement;
            if (fw == null)
                return;

            SetIsPressed(fw, false);
        }
    }
}
