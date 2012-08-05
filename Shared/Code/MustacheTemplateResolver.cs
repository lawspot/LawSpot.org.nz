using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lawspot.Shared
{
    public interface IMustacheDataModel
    {
        /// <summary>
        /// Gets the full name of the type that the properties belong to.
        /// </summary>
        /// <returns> The full name of the type that the properties belong to. </returns>
        string GetTypeName();

        /// <summary>
        /// Gets the value of the property, if that property exists.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <param name="value"> Set to the value of the property once the method returns. </param>
        /// <returns> <c>true</c> if the property exists; <c>false</c> otherwise. </returns>
        bool TryGetValue(string name, out object value);
    }

    public static class MustacheTemplateResolver
    {
        /// <summary>
        /// Transforms a mustache template into HTML.
        /// </summary>
        /// <param name="template"> The mustache template. </param>
        /// <param name="model"> The model values to plug in. </param>
        /// <param name="result"> A string builder that will receive the transformed HTML. </param>
        public static void Resolve(string template, object model, StringBuilder result)
        {
            string startDelimiter = "{{";
            string endDelimiter = "}}";
            Resolve(template, model, result, ref startDelimiter, ref endDelimiter);
        }

        /// <summary>
        /// Transforms a mustache template into HTML.
        /// </summary>
        /// <param name="template"> The mustache template. </param>
        /// <param name="model"> The model values to plug in. </param>
        /// <param name="result"> A string builder that will receive the transformed HTML. </param>
        /// <param name="startDelimiter"> </param>
        /// <param name="endDelimiter"> </param>
        private static void Resolve(string template, object model, StringBuilder result, ref string startDelimiter, ref string endDelimiter)
        {
            int start = 0;

            while (true)
            {
                // Find the start of the next tag.
                int startTagIndex = template.IndexOf(startDelimiter, start);
                if (startTagIndex == -1)
                    startTagIndex = template.Length;

                // Copy all the content between the end of the last tag and the start of the next tag to the output.
                string content = template.Substring(start, startTagIndex - start);
                result.Append(content);

                // Exit if there are no more tags.
                if (startTagIndex == template.Length)
                    break;
                startTagIndex += startDelimiter.Length;

                // Find the end of the tag.
                int endTagIndex = template.IndexOf(endDelimiter, startTagIndex);
                if (endTagIndex == -1)
                    throw new FormatException("Unterminated start tag.");

                // Extract the tag contents.
                string tagContents = template.Substring(startTagIndex, endTagIndex - startTagIndex);
                if (tagContents == "")
                    throw new FormatException("Empty tag.");

                // Count the number of newlines (should be none).
                foreach (char c in tagContents)
                    if (c == '\n')
                        throw new FormatException("Newline in tag");

                // By default, start searching on the next loop around after the tag.
                start = endTagIndex + endDelimiter.Length;

                switch (tagContents[0])
                {
                    case '!':       // Comment.
                        break;

                    case '{':       // Unescaped.
                        if (template.Length == start || template[start] != '}')
                            throw new FormatException("Invalid tag syntax.");
                        tagContents = tagContents.Substring(1).Trim();
                        start++;

                        // Convert the expression to a string and display unescaped.
                        result.Append(ConvertToString(ResolveValue(tagContents, model)));
                        break;

                    case '#':       // Section.
                    case '^':       // Inverted section.
                        bool inverted = tagContents[0] == '^';
                        tagContents = tagContents.Substring(1);

                        // Find the end tag.
                        string endSectionTag = string.Format("{0}/{1}{2}", startDelimiter, tagContents, endDelimiter);
                        int endSectionIndex = template.IndexOf(endSectionTag, start);
                        if (endSectionIndex == -1)
                            throw new FormatException(string.Format("Could not find end section {0}", endSectionTag));

                        // Get the text in between.
                        string sectionContents = template.Substring(start, endSectionIndex - start);

                        // Resolve the expression.
                        object expressionValue = ResolveValue(tagContents, model);

                        // Determine the context of the section.
                        object context = IsPrimitive(expressionValue) ? model : expressionValue;

                        if (inverted == false)
                        {
                            if (expressionValue is IEnumerable && (expressionValue is string) == false)
                            {
                                // Repeat the section for each item in the list.
                                foreach (var child in ((IEnumerable)expressionValue))
                                    Resolve(sectionContents, child, result, ref startDelimiter, ref endDelimiter);
                            }
                            else if (ConvertToBoolean(expressionValue))
                            {
                                // The expression evaluated to true - show the section.
                                Resolve(sectionContents, context, result, ref startDelimiter, ref endDelimiter);
                            }
                        }
                        else
                        {
                            if (expressionValue is IEnumerable)
                            {
                                // Detect if there are items in the list.
                                var enumerator = ((IEnumerable)expressionValue).GetEnumerator();
                                bool hasElements = enumerator.MoveNext();
                                if (enumerator is IDisposable)
                                    ((IDisposable)enumerator).Dispose();

                                // Show the section if there are no items in the list.
                                if (!hasElements)
                                    Resolve(sectionContents, context, result, ref startDelimiter, ref endDelimiter);
                            }
                            else if (ConvertToBoolean(expressionValue) == false)
                            {
                                // The expression evaluated to false - show the section.
                                Resolve(sectionContents, context, result, ref startDelimiter, ref endDelimiter);
                            }
                        }

                        // start searching on the next loop around after the end section tag.
                        start = endSectionIndex + endSectionTag.Length;
                        break;

                    case '/':       // End of section.
                        throw new FormatException("Unexpected end tag.");

                    case '=':       // Set delimiters.
                        if (tagContents.EndsWith("=") == false)
                            throw new FormatException("Invalid tag syntax.");
                        tagContents = tagContents.Substring(1, tagContents.Length - 2);
                        string[] delimiters = tagContents.Split(' ');
                        if (delimiters.Length != 2)
                            throw new FormatException("Expected two delimiters.");
                        startDelimiter = delimiters[0];
                        endDelimiter = delimiters[1];
                        break;

                    default:
                        // Hack: {{mustache}} appears in the mustache javascript file.
                        if (tagContents == "mustache" || tagContents == "\",\"")
                        {
                            result.AppendFormat("{0}{1}{2}", startDelimiter, tagContents, endDelimiter);
                            break;
                        }

                        tagContents = tagContents.Trim();

                        // Convert the expression to a string and display unescaped.
                        result.Append(System.Net.WebUtility.HtmlEncode(ConvertToString(ResolveValue(tagContents, model))));
                        break;
                }
            }
        }

        private class ObjectWrapper : IMustacheDataModel
        {
            private object instance;
            private Type type;

            public ObjectWrapper(object instance)
            {
                if (instance == null)
                    throw new ArgumentNullException("instance");
                this.instance = instance;
                this.type = instance.GetType();
            }

            /// <summary>
            /// Gets the full name of the type that the properties belong to.
            /// </summary>
            /// <returns> The full name of the type that the properties belong to. </returns>
            public string GetTypeName()
            {
                return this.type.FullName;
            }

            /// <summary>
            /// Gets the value of the property, if that property exists.
            /// </summary>
            /// <param name="name"> The name of the property. </param>
            /// <param name="value"> Set to the value of the property once the method returns. </param>
            /// <returns> <c>true</c> if the property exists; <c>false</c> otherwise. </returns>
            public bool TryGetValue(string name, out object value)
            {
                value = null;
                var property = this.type.GetProperty(name);
                if (property == null)
                    return false;
                value = property.GetValue(this.instance, null);
                return true;
            }
        }

        /// <summary>
        /// Resolves an expression.
        /// </summary>
        /// <param name="expression"> The mustache expression. </param>
        /// <param name="model"> The model values to plug in. </param>
        private static object ResolveValue(string expression, object model)
        {
            // Dot is the current value.
            if (expression == ".")
                return model;

            // Dot separates child identifiers.
            object result = model;
            StringBuilder parentIdentifiers = new StringBuilder();
            foreach (var identifier in expression.Split('.'))
            {
                if (identifier == string.Empty)
                    throw new FormatException(string.Format("Invalid expression '{0}'.", expression));
                if (result != null)
                {
                    var dataModel = result is IMustacheDataModel ? (IMustacheDataModel)result : new ObjectWrapper(result);
                    bool exists = dataModel.TryGetValue(identifier, out result);
                    if (exists == false)
                        throw new FormatException(string.Format("Could not evaluate '{0}' because '{1}' does not exist in {2}.",
                            expression, identifier, dataModel.GetTypeName()));
                }
                if (parentIdentifiers.Length > 0)
                    parentIdentifiers.Append('.');
                parentIdentifiers.Append(identifier);
            }
            return result;
        }

        /// <summary>
        /// Converts the given value to true or false using javascript truthy rules.
        /// </summary>
        /// <param name="expressionValue"> The value to convert. </param>
        /// <returns> <c>true</c> if the value is truthy; <c>false</c> otherwise. </returns>
        private static bool ConvertToBoolean(object expressionValue)
        {
            if (expressionValue == null)
                return false;
            switch (Type.GetTypeCode(expressionValue.GetType()))
            {
                case TypeCode.Boolean:
                    return (bool)expressionValue;
                case TypeCode.Byte:
                    return ((byte)expressionValue) != 0;
                case TypeCode.Char:
                    return ((char)expressionValue) != '\0';
                case TypeCode.DateTime:
                    return ((DateTime)expressionValue) != DateTime.MinValue;
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Decimal:
                    return ((decimal)expressionValue) != 0.0m;
                case TypeCode.Double:
                    return ((double)expressionValue) != 0.0;
                case TypeCode.Empty:
                    return false;
                case TypeCode.Int16:
                    return ((short)expressionValue) != 0;
                case TypeCode.Int32:
                    return ((int)expressionValue) != 0;
                case TypeCode.Int64:
                    return ((long)expressionValue) != 0;
                case TypeCode.Object:
                    return expressionValue != null;
                case TypeCode.SByte:
                    return ((sbyte)expressionValue) != 0;
                case TypeCode.Single:
                    return ((float)expressionValue) != 0;
                case TypeCode.String:
                    return !string.IsNullOrEmpty((string)expressionValue);
                case TypeCode.UInt16:
                    return ((ushort)expressionValue) != 0;
                case TypeCode.UInt32:
                    return ((uint)expressionValue) != 0;
                case TypeCode.UInt64:
                    return ((ulong)expressionValue) != 0;
            }
            throw new InvalidOperationException(string.Format("Unsupported type {0}", expressionValue.GetType()));
        }

        /// <summary>
        /// Converts the given value to a string.
        /// </summary>
        /// <param name="expressionValue"> The value to convert. </param>
        /// <returns> The textual representation of the value.  Null will be converted to the empty string. </returns>
        private static string ConvertToString(object expressionValue)
        {
            if (expressionValue == null)
                return string.Empty;
            return expressionValue.ToString();
        }

        /// <summary>
        /// Determines if the given value is a primitive.
        /// </summary>
        /// <param name="expressionValue"> The value to check. </param>
        /// <returns> <c>true</c> if the value is a primitive; <c>false</c> otherwise. </returns>
        private static bool IsPrimitive(object expressionValue)
        {
            if (expressionValue == null)
                return true;
            return Type.GetTypeCode(expressionValue.GetType()) != TypeCode.Object;
        }
    }
}

