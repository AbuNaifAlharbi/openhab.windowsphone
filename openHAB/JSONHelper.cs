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
using System.Collections.Generic;

namespace openHAB
{
    public static class JSONHelper
    {
        public static Dictionary<string, object> ParseJSON(this string aString)
        {
            int pos = 1;

            return Parse(aString, ref pos) as Dictionary<string, object>;
        }

        private static string ParseString(string aData, ref int aPosition)
        {
            System.Diagnostics.Debug.Assert(aData[aPosition] == '"');

            ++aPosition;

            var str = string.Empty;
            while (aPosition< aData.Length && aData[aPosition] != '"')
                str += aData[aPosition++];

            System.Diagnostics.Debug.Assert(aPosition < aData.Length);

            ++aPosition;

            return str;
        }

        private static Dictionary<string,object> Parse(string aData, ref int aPosition)
        {
            var result = new Dictionary<string, object>();

            while (aPosition < aData.Length)
            {
                var name = ParseString(aData, ref aPosition);

                System.Diagnostics.Debug.Assert(aData[aPosition] == ':');

                ++aPosition;

                result[name] = ParseObject(aData, ref aPosition);

                switch (aData[aPosition])
                {
                    case ',':
                        ++aPosition;
                        break;

                    case '}':
                        ++aPosition;
                        return result;

                    default:
                        System.Diagnostics.Debug.Assert(false);
                        return null;
                }
            }

            return result;
        }

        private static object ParseObject(string aData,ref int aPosition)
        {
            switch (aData[aPosition])
            {
                case '"':
                    return ParseString(aData, ref aPosition);

                case '{':
                    ++aPosition;
                    return Parse(aData, ref aPosition);

                case '[':
                    {
                        var lst = new List<Dictionary<string, object>>();

                        ++aPosition;
                        while (aPosition < aData.Length)
                        {
                            var obj = ParseObject(aData, ref aPosition);

                            System.Diagnostics.Debug.Assert(obj is Dictionary<string, object>);

                            lst.Add(obj as Dictionary<string, object>);

                            switch (aData[aPosition])
                            {
                                case ',':
                                    ++aPosition;
                                    break;

                                case ']':
                                    ++aPosition;
                                    return lst;

                            }
                        }
                    }
                    break;
            }

            System.Diagnostics.Debug.Assert(false);

            return null;
        }    
    }
}
