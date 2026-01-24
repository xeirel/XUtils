using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;

namespace XUtils.StringUtils
{
    public static class XString
    {
        public static string ColorizeJson(this string json)
        {
            const string keyColor = "#4EC9B0";
            const string stringColor = "#D69D85";
            const string numberColor = "#B5CEA8";
            const string boolColor = "#569CD6";
            const string nullColor = "#808080";
            const string symbolColor = "#D4D4D4";

            json = Regex.Replace(json, @"(""[^""\\]*(?:\\.[^""\\]*)*"")(\s*:)", m =>
            {
                return $"<color={keyColor}>{m.Groups[1].Value}</color>{m.Groups[2].Value}";
            });
            json = Regex.Replace(json, @":\s*(""[^""\\]*(?:\\.[^""\\]*)*"")", m =>
            {
                return $": <color={stringColor}>{m.Groups[1].Value}</color>";
            });
            json = Regex.Replace(json, @":\s*(-?\d+(\.\d+)?)", m =>
            {
                return $": <color={numberColor}>{m.Groups[1].Value}</color>";
            });
            json = Regex.Replace(json, @":\s*(true|false)", m =>
            {
                return $": <color={boolColor}>{m.Groups[1].Value}</color>";
            });
            json = Regex.Replace(json, @":\s*(null)", m =>
            {
                return $": <color={nullColor}>null</color>";
            });
            json = Regex.Replace(json, @"([{}\[\],])", m =>
            {
                return $"<color={symbolColor}>{m.Groups[1].Value}</color>";
            });

            return json;
        }
        public static string GiveMoneyColor(this string input, string currencySymbol = "$", Color? color = null)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(currencySymbol))
            {
                return input;
            }

            var moneyColor = color ?? UnityEngine.Color.green;
            string escapedCurrency = Regex.Escape(currencySymbol);

            string pattern = $@"(\d{{1,}}(?:[\.,]\d{{3,}})*(?:[\.,]\d+)?){escapedCurrency}";

            return Regex.Replace(
                input,
                pattern,
                m => $"{m.Groups[1].Value}{currencySymbol}".Color(moneyColor));
        }
        public static string ToSmartString<T>(this T number, int maxDecimals = 2, bool useThousandSeparator = false) where T : struct, IConvertible
        {
            decimal value = Convert.ToDecimal(number);

            var culture = CultureInfo.CurrentCulture;

            string thousandSeparator = culture.NumberFormat.CurrencyGroupSeparator;
            string decimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator;

            if (value % 1 == 0)
            {
                int intValue = (int)value;
                return useThousandSeparator
                    ? intValue.ToString("N0", culture)
                    : intValue.ToString();
            }

            decimal roundedValue = System.Math.Round(value, maxDecimals, MidpointRounding.AwayFromZero);
            return useThousandSeparator
                ? roundedValue.ToString($"N{maxDecimals}", culture)
                : roundedValue.ToString();
        }
        public static string GetMD5Hash(this string _input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(_input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public static string GetHex<T>(this T strct) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(strct, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static T ResolveHex<T>(this string hex) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        public static string EncodeToBase64<T>(T input) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(input, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return Convert.ToBase64String(bytes);
        }
        public static T DecodeFromBase64<T>(string base64) where T : struct
        {
            byte[] bytes = Convert.FromBase64String(base64);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        public static string GetSHA256(this string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static string ToHex(this Color color, bool includeAlpha = false)
        {
            Color32 c = color;

            return includeAlpha
                ? $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"
                : $"#{c.r:X2}{c.g:X2}{c.b:X2}";
        }
        public static string RemoveWhitespaces(this string input)
        {
            return Regex.Replace(input, @"\s+", "");
        }
        public static Color ConvertToColor(this string name)
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(name, out Color color))
            {
                return color;
            }
            else
            {
                Debug.LogError($"Failed to parse color from string: {name}");
                return UnityEngine.Color.white;
            }
        }
        public static void SetTmpText(this TMP_Text tmp, string text, Color color)
        {
            tmp.text = text;
            tmp.color = color;
        }
        public static string RemoveTMPTags(this string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
        public static string ExtractCallerFromStack(this string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return "StackTrace is empty.";

            string[] lines = stackTrace.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("Assets"))
                {
                    int start = line.IndexOf("Assets", StringComparison.Ordinal);
                    if (start >= 0)
                    {
                        return line.Substring(start).Trim();
                    }
                }
            }

            return "Unknown";
        }
        public static string MorphString(string a, string b, float t)
        {
            t = Mathf.Clamp01(t);
            int maxLength = Mathf.Max(a.Length, b.Length);
            char[] result = new char[maxLength];
            int morphCount = Mathf.RoundToInt(t * maxLength);
            for (int i = 0; i < maxLength; i++)
            {
                if (i < morphCount)
                {
                    result[i] = i < b.Length ? b[i] : ' ';
                }
                else
                {
                    result[i] = i < a.Length ? a[i] : ' ';
                }
            }
            return new string(result);
        }
        public static int[] GetRandomOrder(int length)
        {
            int[] order = Enumerable.Range(0, length).ToArray();
            System.Random rnd = new System.Random();
            for (int i = order.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            return order;
        }
        public static string MorphStringRandom(string a, string b, float t)
        {
            int[] randomOrder = GetRandomOrder(Mathf.Max(a.Length, b.Length));
            t = Mathf.Clamp01(t);
            int maxLength = Mathf.Max(a.Length, b.Length);
            char[] result = new char[maxLength];
            int morphCount = Mathf.RoundToInt(t * maxLength);

            for (int i = 0; i < maxLength; i++)
            {
                int idx = (randomOrder != null && i < randomOrder.Length) ? randomOrder[i] : i;
                if (i < morphCount)
                    result[idx] = idx < b.Length ? b[idx] : ' ';
                else
                    result[idx] = idx < a.Length ? a[idx] : ' ';
            }
            return new string(result);
        }

        public static string Color(this string text, string color) =>
    $"<color={color}>{text}</color>";
        public static string Color(this char text, string color) =>
            $"<color={color}>{text}</color>";
        public static string Size(this string text, string size) =>
    $"<size={size}>{text}</color>";
        public static string Size(this char text, string size) =>
    $"<size={size}>{text}</color>";
        public static string Bold(this string text) =>
$"<b>{text}</b>";
        public static string Bold(this char text) =>
    $"<b>{text}</b>";
        public static string Italic(this string text) =>
$"<i>{text}</i>";
        public static string Italic(this char text) =>
$"<i>{text}</i>";
        public static string Color(this string text, Color color) =>
            $"<color=#{color.ToHexString().ToLower()}>{text}</color>";
        public static string Color(this char text, Color color) =>
            $"<color=#{color.ToHexString().ToLower()}>{text}</color>";
        public static string TrimStartOnce(this string str, char trimChar)
        {
            return str.StartsWith(trimChar) ? str.Substring(1) : str;
        }
    }
}