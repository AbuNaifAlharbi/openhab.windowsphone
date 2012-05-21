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
 
// Written by Osama Zaqout
//
// You can do whatever you want with this code, but please keep this comment intact.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Framework.Controls
{
    public class DataTemplateSelector : ContentControl
    {
        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.RegisterAttached( 
            "DataType",                     // Name of the property
            typeof( string ),               // Type of the property
            typeof( DataTemplateSelector ), // Type of the provider of the registered attached property
            new PropertyMetadata( null ) ); // Callback invoked in case the property value has changed

        public static void SetDataType(DependencyObject aObject, string aDataType)
        {
            aObject.SetValue(DataTypeProperty, aDataType);
        }

        public static string GetDataType(DependencyObject aObject)
        {
            return (string)aObject.GetValue(DataTypeProperty);
        }

        protected override void OnContentChanged(object aOldContent, object aNewContent)
        {
            base.OnContentChanged(aOldContent, aNewContent);

            if (aNewContent != null)
            {
                var newType = aNewContent.GetType();

                if (aOldContent != null)
                {
                    if (aOldContent.GetType() == newType)
                        return;
                }
            }

            SelectTemplate(aNewContent);
        }

        private void SelectTemplate(object aContent)
        {
            if (aContent == null)
                return;

            if (mRegisteredTemplates == null)
            {
                mRegisteredTemplates = new Dictionary<string, DataTemplate>();
                RegisterTemplates(this);
            }

            var type = aContent.GetType();
            DataTemplate dt;
            if (mRegisteredTemplates.TryGetValue(type.FullName, out dt))
                ContentTemplate = dt;
            else if (mRegisteredTemplates.TryGetValue(type.Name, out dt))
                ContentTemplate = dt;
        }

        private Dictionary<string,DataTemplate> mRegisteredTemplates;
        private void RegisterTemplates(FrameworkElement aElement)
        {
            RegisterTemplates(aElement.Resources);

            var parent = VisualTreeHelper.GetParent(aElement) as FrameworkElement;
            if (parent != null)
                RegisterTemplates(parent);
        }

        private void RegisterTemplates(ResourceDictionary aDictionary)
        {
            foreach (DictionaryEntry resource in aDictionary)
            {
                var dt = resource.Value as DataTemplate;
                if (dt != null)
                {
                    var type = GetDataType(dt);
                    if (type != null)
                    {
                        mRegisteredTemplates[type] = dt;
                    }
                }
            }

            foreach (var dictionary in aDictionary.MergedDictionaries)
            {
                RegisterTemplates(dictionary);
            }
        }
    }
}
