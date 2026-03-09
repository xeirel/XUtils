using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace XUtils.Console
{
    public static class XConsoleRegistry
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        private static readonly Dictionary<string, CommandEntry> Commands = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, VariableEntry> Variables = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<object, TargetRegistration> TargetRegistrations = new();
        private static readonly HashSet<Type> StaticRegistrations = new();

        public static int CommandCount => Commands.Count;
        public static int VariableCount => Variables.Count;
        public static string[] CommandNames => Commands.Keys.ToArray();
        public static string[] VariableNames => Variables.Keys.ToArray();

        public static void Clear()
        {
            Commands.Clear();
            Variables.Clear();
            TargetRegistrations.Clear();
            StaticRegistrations.Clear();
        }

        public static void Register(object target)
        {
            if (target == null)
                return;

            Unregister(target);

            Type type = target.GetType();
            TargetRegistration registration = new();

            RegisterMethods(type, target, registration, includeInstanceMembers: true, includeStaticMembers: false);
            RegisterFields(type, target, registration, includeInstanceMembers: true, includeStaticMembers: false);
            RegisterProperties(type, target, registration, includeInstanceMembers: true, includeStaticMembers: false);

            if (registration.CommandNames.Count > 0 || registration.VariableNames.Count > 0)
                TargetRegistrations[target] = registration;

            if (!StaticRegistrations.Add(type))
                return;

            RegisterMethods(type, null, null, includeInstanceMembers: false, includeStaticMembers: true);
            RegisterFields(type, null, null, includeInstanceMembers: false, includeStaticMembers: true);
            RegisterProperties(type, null, null, includeInstanceMembers: false, includeStaticMembers: true);
        }

        public static void Unregister(object target)
        {
            if (target == null)
                return;

            if (!TargetRegistrations.TryGetValue(target, out TargetRegistration registration))
                return;

            foreach (string commandName in registration.CommandNames)
            {
                if (Commands.TryGetValue(commandName, out CommandEntry entry) && ReferenceEquals(entry.Owner, target))
                    Commands.Remove(commandName);
            }

            foreach (string variableName in registration.VariableNames)
            {
                if (Variables.TryGetValue(variableName, out VariableEntry entry) && ReferenceEquals(entry.Owner, target))
                    Variables.Remove(variableName);
            }

            TargetRegistrations.Remove(target);
        }

        public static bool TryExecute(string input, out string result)
        {
            result = string.Empty;

            List<string> tokens = Tokenize(input);
            if (tokens.Count == 0)
            {
                result = "Empty input.";
                return false;
            }

            string name = tokens[0];
            string[] args = new string[tokens.Count - 1];

            for (int i = 1; i < tokens.Count; i++)
                args[i - 1] = tokens[i];

            if (Commands.TryGetValue(name, out CommandEntry commandEntry))
                return TryExecuteCommand(commandEntry, args, out result);

            if (Variables.TryGetValue(name, out VariableEntry variableEntry))
                return TryExecuteVariable(variableEntry, args, out result);

            result = $"Unknown command or variable '{name}'.";
            return false;
        }

        public static IReadOnlyList<string> DescribeEntries(string filter = null)
        {
            List<string> lines = new();
            string normalizedFilter = filter?.Trim();

            foreach (KeyValuePair<string, CommandEntry> pair in Commands)
            {
                if (!MatchesFilter(pair.Key, normalizedFilter))
                    continue;

                lines.Add($"cmd  {pair.Value.Name}{BuildParameterSignature(pair.Value.Parameters)} - {pair.Value.Description}");
            }

            foreach (KeyValuePair<string, VariableEntry> pair in Variables)
            {
                if (!MatchesFilter(pair.Key, normalizedFilter))
                    continue;

                lines.Add($"var  {pair.Value.Name} : {GetFriendlyTypeName(pair.Value.ValueType)} - {pair.Value.Description}");
            }

            lines.Sort(StringComparer.OrdinalIgnoreCase);
            return lines;
        }

        private static void RegisterMethods(Type type, object target, TargetRegistration registration, bool includeInstanceMembers, bool includeStaticMembers)
        {
            foreach (MethodInfo method in type.GetMethods(MemberFlags))
            {
                bool isStatic = method.IsStatic;
                if ((isStatic && !includeStaticMembers) || (!isStatic && !includeInstanceMembers))
                    continue;

                DevCommandAttribute attribute = method.GetCustomAttribute<DevCommandAttribute>(inherit: true);
                if (attribute == null)
                    continue;

                string commandName = string.IsNullOrWhiteSpace(attribute.Name) ? method.Name : attribute.Name;
                CommandEntry entry = new()
                {
                    Name = commandName,
                    Description = attribute.Description,
                    Owner = target,
                    Target = isStatic ? null : target,
                    Method = method,
                    Parameters = method.GetParameters()
                };

                AddOrReplaceCommand(entry, registration);
            }
        }

        private static void RegisterFields(Type type, object target, TargetRegistration registration, bool includeInstanceMembers, bool includeStaticMembers)
        {
            foreach (FieldInfo field in type.GetFields(MemberFlags))
            {
                bool isStatic = field.IsStatic;
                if ((isStatic && !includeStaticMembers) || (!isStatic && !includeInstanceMembers))
                    continue;

                DevVariableAttribute attribute = field.GetCustomAttribute<DevVariableAttribute>(inherit: true);
                if (attribute == null)
                    continue;

                string variableName = string.IsNullOrWhiteSpace(attribute.Name) ? field.Name : attribute.Name;
                VariableEntry entry = new()
                {
                    Name = variableName,
                    Description = attribute.Description,
                    Owner = target,
                    Target = isStatic ? null : target,
                    Field = field,
                    ValueType = field.FieldType,
                    CanRead = true,
                    CanWrite = !field.IsInitOnly && !field.IsLiteral
                };

                AddOrReplaceVariable(entry, registration);
            }
        }

        private static void RegisterProperties(Type type, object target, TargetRegistration registration, bool includeInstanceMembers, bool includeStaticMembers)
        {
            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                MethodInfo accessor = property.GetMethod ?? property.SetMethod;
                if (accessor == null)
                    continue;

                bool isStatic = accessor.IsStatic;
                if ((isStatic && !includeStaticMembers) || (!isStatic && !includeInstanceMembers))
                    continue;

                DevVariableAttribute attribute = property.GetCustomAttribute<DevVariableAttribute>(inherit: true);
                if (attribute == null)
                    continue;

                string variableName = string.IsNullOrWhiteSpace(attribute.Name) ? property.Name : attribute.Name;
                VariableEntry entry = new()
                {
                    Name = variableName,
                    Description = attribute.Description,
                    Owner = target,
                    Target = isStatic ? null : target,
                    Property = property,
                    ValueType = property.PropertyType,
                    CanRead = property.CanRead,
                    CanWrite = property.CanWrite
                };

                AddOrReplaceVariable(entry, registration);
            }
        }

        private static void AddOrReplaceCommand(CommandEntry entry, TargetRegistration registration)
        {
            if (Commands.TryGetValue(entry.Name, out CommandEntry previousEntry) && previousEntry.Owner != null && TargetRegistrations.TryGetValue(previousEntry.Owner, out TargetRegistration previousRegistration))
                previousRegistration.CommandNames.Remove(entry.Name);

            Commands[entry.Name] = entry;
            registration?.CommandNames.Add(entry.Name);
        }

        private static void AddOrReplaceVariable(VariableEntry entry, TargetRegistration registration)
        {
            if (Variables.TryGetValue(entry.Name, out VariableEntry previousEntry) && previousEntry.Owner != null && TargetRegistrations.TryGetValue(previousEntry.Owner, out TargetRegistration previousRegistration))
                previousRegistration.VariableNames.Remove(entry.Name);

            Variables[entry.Name] = entry;
            registration?.VariableNames.Add(entry.Name);
        }

        private static bool TryExecuteCommand(CommandEntry entry, string[] args, out string result)
        {
            result = string.Empty;

            if (!IsTargetAlive(entry.Target))
            {
                result = $"Command '{entry.Name}' target is no longer valid.";
                return false;
            }

            if (!TryBuildArguments(entry.Parameters, args, out object[] values, out string argumentError))
            {
                result = argumentError;
                return false;
            }

            try
            {
                object returnValue = entry.Method.Invoke(entry.Target, values);
                if (entry.Method.ReturnType == typeof(void))
                {
                    result = $"Executed '{entry.Name}'.";
                    return true;
                }

                result = returnValue != null
                    ? $"{entry.Name} => {FormatValue(returnValue)}"
                    : $"{entry.Name} => null";

                return true;
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = exception.InnerException ?? exception;
                result = $"{entry.Name} failed: {innerException.Message}";
                return false;
            }
            catch (Exception exception)
            {
                result = $"{entry.Name} failed: {exception.Message}";
                return false;
            }
        }

        private static bool TryExecuteVariable(VariableEntry entry, string[] args, out string result)
        {
            result = string.Empty;

            if (!IsTargetAlive(entry.Target))
            {
                result = $"Variable '{entry.Name}' target is no longer valid.";
                return false;
            }

            if (args.Length == 0)
            {
                if (!entry.CanRead)
                {
                    result = $"Variable '{entry.Name}' is write-only.";
                    return false;
                }

                object currentValue = entry.GetValue();
                result = $"{entry.Name} = {FormatValue(currentValue)}";
                return true;
            }

            if (!entry.CanWrite)
            {
                result = $"Variable '{entry.Name}' is read-only.";
                return false;
            }

            int index = 0;
            if (!TryConsumeValue(args, entry.ValueType, ref index, captureRemainingString: true, out object convertedValue, out string error))
            {
                result = $"{entry.Name}: {error}";
                return false;
            }

            if (index != args.Length)
            {
                result = $"{entry.Name}: too many arguments supplied.";
                return false;
            }

            try
            {
                entry.SetValue(convertedValue);
                object currentValue = entry.CanRead ? entry.GetValue() : convertedValue;
                result = $"{entry.Name} = {FormatValue(currentValue)}";
                return true;
            }
            catch (Exception exception)
            {
                result = $"{entry.Name} failed: {exception.Message}";
                return false;
            }
        }

        private static bool TryBuildArguments(ParameterInfo[] parameters, string[] args, out object[] values, out string error)
        {
            values = new object[parameters.Length];
            error = string.Empty;
            int argIndex = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                bool captureRemainingString = i == parameters.Length - 1 && parameter.ParameterType == typeof(string);

                if (argIndex >= args.Length)
                {
                    if (!TryGetDefaultValue(parameter, out object defaultValue))
                    {
                        error = $"Missing required parameter '{parameter.Name}'.";
                        return false;
                    }

                    values[i] = defaultValue;
                    continue;
                }

                if (!TryConsumeValue(args, parameter.ParameterType, ref argIndex, captureRemainingString, out object value, out error))
                {
                    error = $"{parameter.Name}: {error}";
                    return false;
                }

                values[i] = value;
            }

            if (argIndex != args.Length)
            {
                error = "Too many arguments supplied.";
                return false;
            }

            return true;
        }

        private static bool TryConsumeValue(string[] args, Type targetType, ref int argIndex, bool captureRemainingString, out object value, out string error)
        {
            error = string.Empty;
            value = null;

            Type nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
                targetType = nullableType;

            if (targetType == typeof(string))
            {
                if (captureRemainingString)
                {
                    value = string.Join(" ", args, argIndex, args.Length - argIndex);
                    argIndex = args.Length;
                }
                else
                {
                    value = args[argIndex];
                    argIndex++;
                }

                return true;
            }

            if (targetType == typeof(Vector2))
                return TryConsumeVector2(args, ref argIndex, out value, out error);

            if (targetType == typeof(Vector3))
                return TryConsumeVector3(args, ref argIndex, out value, out error);

            if (targetType == typeof(Vector4))
                return TryConsumeVector4(args, ref argIndex, out value, out error);

            if (targetType == typeof(Color))
                return TryConsumeColor(args, ref argIndex, out value, out error);

            string token = args[argIndex];
            if (!TryParseSingleToken(token, targetType, out value))
            {
                error = $"Could not convert '{token}' to {GetFriendlyTypeName(targetType)}.";
                return false;
            }

            argIndex++;
            return true;
        }

        private static bool TryConsumeVector2(string[] args, ref int argIndex, out object value, out string error)
        {
            value = null;
            error = string.Empty;

            if (TryParseVector(args[argIndex], 2, out float[] values))
            {
                value = new Vector2(values[0], values[1]);
                argIndex++;
                return true;
            }

            if (argIndex + 1 < args.Length && TryParseFloat(args[argIndex], out float x) && TryParseFloat(args[argIndex + 1], out float y))
            {
                value = new Vector2(x, y);
                argIndex += 2;
                return true;
            }

            error = "Expected Vector2 as 'x y' or 'x,y'.";
            return false;
        }

        private static bool TryConsumeVector3(string[] args, ref int argIndex, out object value, out string error)
        {
            value = null;
            error = string.Empty;

            if (TryParseVector(args[argIndex], 3, out float[] values))
            {
                value = new Vector3(values[0], values[1], values[2]);
                argIndex++;
                return true;
            }

            if (argIndex + 2 < args.Length && TryParseFloat(args[argIndex], out float x) && TryParseFloat(args[argIndex + 1], out float y) && TryParseFloat(args[argIndex + 2], out float z))
            {
                value = new Vector3(x, y, z);
                argIndex += 3;
                return true;
            }

            error = "Expected Vector3 as 'x y z' or 'x,y,z'.";
            return false;
        }

        private static bool TryConsumeVector4(string[] args, ref int argIndex, out object value, out string error)
        {
            value = null;
            error = string.Empty;

            if (TryParseVector(args[argIndex], 4, out float[] values))
            {
                value = new Vector4(values[0], values[1], values[2], values[3]);
                argIndex++;
                return true;
            }

            if (argIndex + 3 < args.Length && TryParseFloat(args[argIndex], out float x) && TryParseFloat(args[argIndex + 1], out float y) && TryParseFloat(args[argIndex + 2], out float z) && TryParseFloat(args[argIndex + 3], out float w))
            {
                value = new Vector4(x, y, z, w);
                argIndex += 4;
                return true;
            }

            error = "Expected Vector4 as 'x y z w' or 'x,y,z,w'.";
            return false;
        }

        private static bool TryConsumeColor(string[] args, ref int argIndex, out object value, out string error)
        {
            value = null;
            error = string.Empty;
            string token = args[argIndex];

            if (ColorUtility.TryParseHtmlString(token, out Color htmlColor))
            {
                value = htmlColor;
                argIndex++;
                return true;
            }

            if (TryParseNamedColor(token, out Color namedColor))
            {
                value = namedColor;
                argIndex++;
                return true;
            }

            if (TryParseColor(token, out Color parsedColor))
            {
                value = parsedColor;
                argIndex++;
                return true;
            }

            if (argIndex + 2 < args.Length && TryParseFloat(args[argIndex], out float r) && TryParseFloat(args[argIndex + 1], out float g) && TryParseFloat(args[argIndex + 2], out float b))
            {
                float a = 1f;
                int consumed = 3;

                if (argIndex + 3 < args.Length && TryParseFloat(args[argIndex + 3], out float alpha))
                {
                    a = alpha;
                    consumed = 4;
                }

                value = new Color(r, g, b, a);
                argIndex += consumed;
                return true;
            }

            error = "Expected Color as '#RRGGBB', 'r g b [a]' or 'r,g,b[,a]'.";
            return false;
        }

        private static bool TryParseSingleToken(string token, Type targetType, out object value)
        {
            value = null;

            if (targetType.IsEnum)
            {
                try
                {
                    value = Enum.Parse(targetType, token, ignoreCase: true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (targetType == typeof(bool))
            {
                switch (token.ToLowerInvariant())
                {
                    case "1":
                    case "true":
                    case "on":
                    case "yes":
                        value = true;
                        return true;
                    case "0":
                    case "false":
                    case "off":
                    case "no":
                        value = false;
                        return true;
                    default:
                        return false;
                }
            }

            if (targetType == typeof(char) && token.Length == 1)
            {
                value = token[0];
                return true;
            }

            try
            {
                value = Convert.ChangeType(token, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetDefaultValue(ParameterInfo parameter, out object value)
        {
            if (parameter.HasDefaultValue)
            {
                value = parameter.DefaultValue;
                if (ReferenceEquals(value, DBNull.Value) || ReferenceEquals(value, Type.Missing))
                    value = GetDefault(parameter.ParameterType);
                return true;
            }

            if (parameter.IsOptional)
            {
                value = GetDefault(parameter.ParameterType);
                return true;
            }

            value = null;
            return false;
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool IsTargetAlive(object target)
        {
            if (target is UnityEngine.Object unityObject)
                return unityObject != null;

            return true;
        }

        private static bool MatchesFilter(string value, string filter)
        {
            return string.IsNullOrWhiteSpace(filter) || value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildParameterSignature(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder builder = new("(");
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                if (i > 0)
                    builder.Append(", ");

                builder.Append(parameter.Name);
                builder.Append(": ");
                builder.Append(GetFriendlyTypeName(parameter.ParameterType));

                if (TryGetDefaultValue(parameter, out object defaultValue))
                {
                    builder.Append(" = ");
                    builder.Append(FormatValue(defaultValue));
                }
            }

            builder.Append(')');
            return builder.ToString();
        }

        private static string GetFriendlyTypeName(Type type)
        {
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return $"{GetFriendlyTypeName(nullableType)}?";

            if (type == typeof(int))
                return "int";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(string))
                return "string";
            if (type == typeof(long))
                return "long";
            if (type == typeof(short))
                return "short";
            if (type == typeof(byte))
                return "byte";

            return type.Name;
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            switch (value)
            {
                case string text:
                    return text;
                case float number:
                    return number.ToString(CultureInfo.InvariantCulture);
                case double number:
                    return number.ToString(CultureInfo.InvariantCulture);
                case Vector2 vector2:
                    return vector2.ToString("F3", CultureInfo.InvariantCulture);
                case Vector3 vector3:
                    return vector3.ToString("F3", CultureInfo.InvariantCulture);
                case Vector4 vector4:
                    return vector4.ToString("F3", CultureInfo.InvariantCulture);
                case Color color:
                    return $"RGBA({color.r.ToString(CultureInfo.InvariantCulture)}, {color.g.ToString(CultureInfo.InvariantCulture)}, {color.b.ToString(CultureInfo.InvariantCulture)}, {color.a.ToString(CultureInfo.InvariantCulture)})";
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString();
            }
        }

        private static List<string> Tokenize(string input)
        {
            List<string> tokens = new();
            if (string.IsNullOrWhiteSpace(input))
                return tokens;

            StringBuilder currentToken = new();
            bool insideQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];

                if (character == '\\' && i + 1 < input.Length && input[i + 1] == '"')
                {
                    currentToken.Append('"');
                    i++;
                    continue;
                }

                if (character == '"')
                {
                    insideQuotes = !insideQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(character) && !insideQuotes)
                {
                    if (currentToken.Length == 0)
                        continue;

                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                    continue;
                }

                currentToken.Append(character);
            }

            if (currentToken.Length > 0)
                tokens.Add(currentToken.ToString());

            return tokens;
        }

        private static bool TryParseFloat(string token, out float value)
        {
            return float.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseVector(string token, int expectedSize, out float[] values)
        {
            values = null;
            string[] parts = token.Split(',');
            if (parts.Length != expectedSize)
                return false;

            values = new float[expectedSize];
            for (int i = 0; i < expectedSize; i++)
            {
                if (!TryParseFloat(parts[i], out values[i]))
                {
                    values = null;
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseColor(string token, out Color color)
        {
            color = Color.white;
            string[] parts = token.Split(',');
            if (parts.Length != 3 && parts.Length != 4)
                return false;

            float[] values = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!TryParseFloat(parts[i], out values[i]))
                    return false;
            }

            color = parts.Length == 3
                ? new Color(values[0], values[1], values[2], 1f)
                : new Color(values[0], values[1], values[2], values[3]);

            return true;
        }

        private static bool TryParseNamedColor(string token, out Color color)
        {
            color = default;
            PropertyInfo property = typeof(Color).GetProperty(
                token,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

            if (property == null || property.PropertyType != typeof(Color))
                return false;

            color = (Color)property.GetValue(null, null);
            return true;
        }

        private sealed class TargetRegistration
        {
            public List<string> CommandNames { get; } = new();
            public List<string> VariableNames { get; } = new();
        }

        private sealed class CommandEntry
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public object Owner { get; set; }
            public object Target { get; set; }
            public MethodInfo Method { get; set; }
            public ParameterInfo[] Parameters { get; set; }
        }

        private sealed class VariableEntry
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public object Owner { get; set; }
            public object Target { get; set; }
            public FieldInfo Field { get; set; }
            public PropertyInfo Property { get; set; }
            public Type ValueType { get; set; }
            public bool CanRead { get; set; }
            public bool CanWrite { get; set; }

            public object GetValue()
            {
                if (Field != null)
                    return Field.GetValue(Target);

                return Property.GetValue(Target, null);
            }

            public void SetValue(object value)
            {
                if (Field != null)
                {
                    Field.SetValue(Target, value);
                    return;
                }

                Property.SetValue(Target, value, null);
            }
        }
    }
}
