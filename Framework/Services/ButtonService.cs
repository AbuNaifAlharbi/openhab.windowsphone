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
    public static class ButtonService
    {
        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.RegisterAttached(
            "IsPressed",            // Name of the property
            typeof(bool),           // Type of the property
            typeof(ButtonService),  // Type of the provider of the registered attached property
            null);                  // Callback invoked in case the property value has changed
        
        public static bool GetIsPressed(DependencyObject aObject)
        {
            return (bool)aObject.GetValue(IsPressedProperty);
        }

        public static readonly DependencyProperty MonitorIsPressedProperty = DependencyProperty.RegisterAttached(
            "MonitorIsPressed",                                     // Name of the property
            typeof(bool),                                           // Type of the property
            typeof(ButtonService),                                  // Type of the provider of the registered attached property
            new PropertyMetadata(false, OnMonitorIsPressedChanged));// Callback invoked in case the property value has changed

        public static bool GetMonitorIsPressed(DependencyObject aObject)
        {
            return (bool)aObject.GetValue(MonitorIsPressedProperty);
        }
        public static void SetMonitorIsPressed(DependencyObject aObject, bool aMonitorIsPressed)
        {
            aObject.SetValue(MonitorIsPressedProperty, aMonitorIsPressed);
        }

        private static void OnMonitorIsPressedChanged(DependencyObject aSender, DependencyPropertyChangedEventArgs aArgs)
        {
            var btn = aSender as System.Windows.Controls.Primitives.ButtonBase;
            if (btn == null)
                return;

            if (aArgs.OldValue != null && aArgs.OldValue.Equals(true))
            {
                btn.ManipulationStarted -= OnSetIsPressed;
                btn.ManipulationCompleted -= OnClearIsPressed;
            }
            if (aArgs.NewValue != null && aArgs.NewValue.Equals(true))
            {
                btn.ManipulationStarted += OnSetIsPressed;
                btn.ManipulationCompleted += OnClearIsPressed;
            }
        }

        private static void OnSetIsPressed(object aSender, ManipulationStartedEventArgs aArgs)
        {
            var btn = aSender as System.Windows.Controls.Primitives.ButtonBase;
            if (btn == null)
                return;

            btn.SetValue(IsPressedProperty, true);
        }
        private static void OnClearIsPressed(object aSender, ManipulationCompletedEventArgs aArgs)
        {
            var btn = aSender as System.Windows.Controls.Primitives.ButtonBase;
            if (btn == null)
                return;

            btn.SetValue(IsPressedProperty, false);
        }
    }
}
