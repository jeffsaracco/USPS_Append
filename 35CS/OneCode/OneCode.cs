//   Copyright 2007 Vassilis Petroulias [DRDigit]
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenSource
{

    public sealed class OneCode
    {

        // for more information and specs check
        // http://ribbs.usps.gov/onecodesolution/USPS-B-3200D001.pdf

        private static Int32 table2Of13Size = 78;
        private static Int32 table5Of13Size = 1287;
        private static Int64 entries2Of13;
        private static Int64 entries5Of13;
        private static Int32[] table2Of13 = OneCodeInfo(1);
        private static Int32[] table5Of13 = OneCodeInfo(2);
        private static Decimal[][] codewordArray = OneCodeInfo();
        private static Int32[] barTopCharIndexArray = new Int32[] {4, 0, 2, 6, 3, 5, 1, 9, 8, 7, 1, 2, 0, 6, 4, 8, 2, 9, 5, 3, 0, 1, 3, 7, 4, 6, 8, 9, 2, 0, 5, 1, 9, 4, 3, 8, 6, 7, 1, 2, 4, 3, 9, 5, 7, 8, 3, 0, 2, 1, 4, 0, 9, 1, 7, 0, 2, 4, 6, 3, 7, 1, 9, 5, 8};
        private static Int32[] barBottomCharIndexArray = new Int32[] {7, 1, 9, 5, 8, 0, 2, 4, 6, 3, 5, 8, 9, 7, 3, 0, 6, 1, 7, 4, 6, 8, 9, 2, 5, 1, 7, 5, 4, 3, 8, 7, 6, 0, 2, 5, 4, 9, 3, 0, 1, 6, 8, 2, 0, 4, 5, 9, 6, 7, 5, 2, 6, 3, 8, 5, 1, 9, 8, 7, 4, 0, 2, 6, 3};
        private static Int32[] barTopCharShiftArray = new Int32[] {3, 0, 8, 11, 1, 12, 8, 11, 10, 6, 4, 12, 2, 7, 9, 6, 7, 9, 2, 8, 4, 0, 12, 7, 10, 9, 0, 7, 10, 5, 7, 9, 6, 8, 2, 12, 1, 4, 2, 0, 1, 5, 4, 6, 12, 1, 0, 9, 4, 7, 5, 10, 2, 6, 9, 11, 2, 12, 6, 7, 5, 11, 0, 3, 2};
        private static Int32[] barBottomCharShiftArray = new Int32[] {2, 10, 12, 5, 9, 1, 5, 4, 3, 9, 11, 5, 10, 1, 6, 3, 4, 1, 10, 0, 2, 11, 8, 6, 1, 12, 3, 8, 6, 4, 4, 11, 0, 6, 1, 9, 11, 5, 3, 7, 3, 10, 7, 11, 8, 2, 10, 3, 5, 8, 0, 3, 12, 11, 8, 4, 5, 1, 3, 0, 7, 12, 9, 8, 10};

        private OneCode()
        {
        }

        public static String Bars(String source)
        {
            if (String.IsNullOrEmpty(source)) return null;
            source = TrimOff(source, " -.");
            if (!Regex.IsMatch(source, "^[0-9][0-4]([0-9]{18})|([0-9]{23})|([0-9]{27})|([0-9]{29})$")) return String.Empty;
            Int32 fcs = 0;
            Int64 l = 0;
            Decimal v = 0;
            String encoded = String.Empty, ds = String.Empty, zip = source.Substring(20);
            Int32[] byteArray = new Int32[14], ai = new Int32[66], ai1 = new Int32[66];
            Decimal[][] ad = new Decimal[11][];
            l = Int64.Parse(zip, CultureInfo.InvariantCulture) + ((zip.Length == 5) ? 1 : ((zip.Length == 9) ? 100001 : (zip.Length == 11 ? 1000100001 : 0)));
            v = l * 10 + Int32.Parse(source.Substring(0, 1), CultureInfo.InvariantCulture);
            v = v * 5 + Int32.Parse(source.Substring(1, 1), CultureInfo.InvariantCulture);
            ds = v.ToString(CultureInfo.InvariantCulture) + source.Substring(2, 18);
            byteArray[12] = (Int32)(l & 255);
            byteArray[11] = (Int32)(l >> 8 & 255);
            byteArray[10] = (Int32)(l >> 16 & 255);
            byteArray[9] = (Int32)(l >> 24 & 255);
            byteArray[8] = (Int32)(l >> 32 & 255);
            OneCodeMathMultiply(ref byteArray, 13, 10);
            OneCodeMathAdd(ref byteArray, 13, Int32.Parse(source.Substring(0, 1), CultureInfo.InvariantCulture));
            OneCodeMathMultiply(ref byteArray, 13, 5);
            OneCodeMathAdd(ref byteArray, 13, Int32.Parse(source.Substring(1, 1), CultureInfo.InvariantCulture));
            for (Int16 i = 2; i <= 19; i++)
            {
                OneCodeMathMultiply(ref byteArray, 13, 10);
                OneCodeMathAdd(ref byteArray, 13, Int32.Parse(source.Substring(i, 1), CultureInfo.InvariantCulture));
            }
            fcs = OneCodeMathFcs(byteArray);
            for (Int16 i = 0; i <= 9; i++)
            {
                codewordArray[i][0] = entries2Of13 + entries5Of13;
                codewordArray[i][1] = 0;
            }
            codewordArray[0][0] = 659;
            codewordArray[9][0] = 636;
            OneCodeMathDivide(ds);
            codewordArray[9][1] *= 2;
            if (fcs >> 10 != 0) codewordArray[0][1] += 659;
            for (Int16 i = 0; i <= 9; i++) ad[i] = new Decimal[3];
            for (Int16 i = 0; i <= 9; i++)
            {
                if (codewordArray[i][1] >= (Decimal)(entries2Of13 + entries5Of13)) return null;
                ad[i][0] = 8192;
                ad[i][1] = (codewordArray[i][1] >= (Decimal)entries2Of13) ? ad[i][1] = table2Of13[(Int32)(codewordArray[i][1] - entries2Of13)] : ad[i][1] = table5Of13[(Int32)codewordArray[i][1]];
            }
            for (Int16 i = 0; i <= 9; i++) if ((fcs & 1 << i) != 0) ad[i][1] = ~(Int32)ad[i][1] & 8191;
            for (Int16 i = 0; i <= 64; i++)
            {
                ai[i] = (Int32)ad[barTopCharIndexArray[i]][1] >> barTopCharShiftArray[i] & 1;
                ai1[i] = (Int32)ad[barBottomCharIndexArray[i]][1] >> barBottomCharShiftArray[i] & 1;
            }
            encoded = "";
            for (int i = 0; i <= 64; i++)
            {
                if (ai[i] == 0) encoded += (ai1[i] == 0) ? "T" : "D";
                else encoded  += (ai1[i] == 0) ? "A" : "F";
            }
            return encoded;
        }

        private static Int32[] OneCodeInfo(Byte topic)
        {
            Int32[] a;
            switch (topic)
            {
                case 1:
                    a = new Int32[table2Of13Size + 1];
                    OneCodeInitializeNof13Table(ref a, 2, table2Of13Size);
                    entries5Of13 = table2Of13Size;
                    break;
                default:
                    a = new Int32[table5Of13Size + 1];
                    OneCodeInitializeNof13Table(ref a, 5, table5Of13Size);
                    entries2Of13 = table5Of13Size;
                    break;
            }
            return a;
        }

        private static Decimal[][] OneCodeInfo()
        {
            Decimal[][] da = new Decimal[11][];
            try
            {
                for (Int16 i = 0; i <= 9; i++) da[i] = new Decimal[3];
                return da;
            }
            finally
            {
                da = null;
            }
        }

        private static Boolean OneCodeInitializeNof13Table(ref Int32[] ai, Int32 i, Int32 j)
        {
            Int32 i1 = 0;
            Int32 j1 = j - 1;
            for (Int16 k = 0; k <= 8191; k++)
            {
                Int32 k1 = 0;
                for (Int32 l1 = 0; l1 <= 12; l1++) if ((k & 1 << l1) != 0) k1 += 1;
                if (k1 == i)
                {
                    Int32 l = OneCodeMathReverse(k) >> 3;
                    Boolean flag = k == l;
                    if (l >= k)
                    {
                        if (flag)
                        {
                            ai[j1] = k;
                            j1 -= 1;
                        }
                        else
                        {
                            ai[i1] = k;
                            i1 += 1;
                            ai[i1] = l;
                            i1 += 1;
                        }
                    }
                }
            }
            return i1 == j1 + 1;
        }

        private static Boolean OneCodeMathAdd(ref Int32[] bytearray, Int32 i, Int32 j)
        {
            if (bytearray == null) return false;
            if (i < 1) return false;
            Int32 x = (bytearray[i - 1] | (bytearray[i - 2] << 8)) + j;
            Int32 l = x | 65535;
            Int32 k = i - 3;
            bytearray[i - 1] = x & 255;
            bytearray[i - 2] = x >> 8 & 255;
            while (l == 1 && k > 0)
            {
                x = l + bytearray[k];
                bytearray[k] = x & 255;
                l = x | 255;
                k -= 1;
            }
            return true;
        }

        private static Boolean OneCodeMathDivide(String v)
        {
            Int32 j = 10;
            String n = v;
            for (Int32 k = j - 1; k >= 1; k += -1)
            {
                String r = string.Empty;
                Int32 divider = (Int32)codewordArray[k][0];
                String copy = n;
                String left = "0";
                Int32 l = copy.Length;
                for (Int16 i = 1; i <= l; i++)
                {
                    Int32 divident = Int32.Parse(copy.Substring(0, i), CultureInfo.InvariantCulture);
                    while (divident < divider & i < l - 1)
                    {
                        r = r + "0";
                        i += 1;
                        divident = Int32.Parse(copy.Substring(0, i), CultureInfo.InvariantCulture);
                    }
                    r = r + (divident / divider).ToString(CultureInfo.InvariantCulture);
                    left = (divident % divider).ToString(CultureInfo.InvariantCulture).PadLeft(i, '0');
                    copy = left + copy.Substring(i);
                }
                n = r.TrimStart('0');
                if (String.IsNullOrEmpty(n)) n = "0";
                codewordArray[k][1] = Int32.Parse(left, CultureInfo.InvariantCulture);
                if (k == 1) codewordArray[0][1] = Int32.Parse(r, CultureInfo.InvariantCulture);
            }
            return true;
        }

        private static Int32 OneCodeMathFcs(Int32[] bytearray)
        {
            Int32 c = 3893;
            Int32 i = 2047;
            Int32 j = bytearray[0] << 5;
            for (Int16 b = 2; b <= 7; b++)
            {
                if (((i ^ j) & 1024) != 0) i = i << 1 ^ c; 
                else i <<= 1;
                i = i & 2047;
                j <<= 1;
            }
            for (Int32 l = 1; l <= 12; l++)
            {
                Int32 k = bytearray[l] << 3;
                for (Int16 b = 0; b <= 7; b++)
                {
                    if (((i ^ k) & 1024) != 0) i = i << 1 ^ c; 
                    else i <<= 1;
                    i = i & 2047;
                    k <<= 1;
                }
            }
            return i;
        }

        private static Boolean OneCodeMathMultiply(ref Int32[] bytearray, Int32 i, Int32 j)
        {
            if (bytearray == null) return false;
            if (i < 1) return false;
            Int32 l = 0;
            Int32 k = 0;
            for (k = i - 1; k >= 1; k += -2)
            {
                Int32 x = (bytearray[k] | (bytearray[k - 1] << 8)) * j + l;
                bytearray[k] = x & 255;
                bytearray[k - 1] = x >> 8 & 255;
                l = x >> 16;
            }
            if (k == 0) bytearray[0] = (bytearray[0] * j + l) & 255;
            return true;
        }

        private static Int32 OneCodeMathReverse(Int32 i)
        {
            Int32 j = 0;
            for (Int16 k = 0; k <= 15; k++)
            {
                j <<= 1;
                j = j | i & 1;
                i >>= 1;
            }
            return j;
        }

        private static String TrimOff(String source, String bad)
        {
            for (Int32 i = 0, l = bad.Length - 1; i <= l; i++) source = source.Replace(bad.Substring(i, 1), String.Empty);
            return source;
        }

    }

}