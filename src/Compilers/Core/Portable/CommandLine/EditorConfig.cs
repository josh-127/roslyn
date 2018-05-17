﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Represents a single EditorConfig file, see http://editorconfig.org for details about the format.
    /// </summary>
    internal sealed class EditorConfig
    {
        // Matches EditorConfig section header such as "[*.{js,py}]", see http://editorconfig.org for details
        private static readonly Regex s_sectionMatcher = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$", RegexOptions.Compiled);
        // Matches EditorConfig property such as "indent_style = space", see http://editorconfig.org for details
        private static readonly Regex s_propertyMatcher = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$", RegexOptions.Compiled);

        /// <summary>
        /// A set of keys that are reserved for special interpretation for the editorconfig specification.
        /// All values corresponding to reserved keys in a (key,value) property pair are always lowercased
        /// during parsing.
        /// </summary>
        /// <remarks>
        /// This list was retrieved from https://github.com/editorconfig/editorconfig/wiki/EditorConfig-Properties
        /// at 2018-04-21 19:37:05Z. New keys may be added to this list in newer versions, but old ones will
        /// not be removed.
        /// </remarks>
        public static ImmutableHashSet<string> ReservedKeys { get; }
            = ImmutableHashSet.CreateRange(CaseInsensitiveComparison.Comparer, new[] {
                "root",
                "indent_style",
                "indent_size",
                "tab_width",
                "end_of_line",
                "charset",
                "trim_trailing_whitespace",
                "insert_final_newline",
            });

        /// <summary>
        /// A set of values that are reserved for special use for the editorconfig specification
        /// and will always be lower-cased by the parser.
        /// </summary>
        public static ImmutableHashSet<string> ReservedValues { get; }
            = ImmutableHashSet.CreateRange(CaseInsensitiveComparison.Comparer, new[] { "unset" });

        public Section GlobalSection { get; }

        public string Directory { get; }

        public ImmutableArray<Section> NamedSections { get; }

        private EditorConfig(Section globalSection, ImmutableArray<Section> namedSections, string directory)
        {
            GlobalSection = globalSection;
            NamedSections = namedSections;
            Directory = directory;
        }

        /// <summary>
        /// Gets whether this editorconfig is a the topmost editorconfig.
        /// </summary>
        public bool IsRoot
            => GlobalSection.Properties.TryGetValue("root", out string val) && val == "true";

        /// <summary>
        /// Parses an editor config file text located within the given parent directory. No parsing
        /// errors are reported. If any line contains a parse error, it is dropped.
        /// </summary>
        public static EditorConfig Parse(string text, string parentDirectory)
        {
            Section globalSection = null;
            var namedSectionBuilder = ImmutableArray.CreateBuilder<Section>();

            // N.B. The editorconfig documentation is quite loose on property interpretation.
            // Specifically, it says:
            //      Currently all properties and values are case-insensitive.
            //      They are lowercased when parsed.
            // To accommodate this, we use a lower case Unicode mapping when adding to the
            // dictionary, but we also use a case-insensitive key comparer when doing lookups
            var activeSectionProperties = ImmutableDictionary.CreateBuilder<string, string>(
                CaseInsensitiveComparison.Comparer);
            string activeSectionName = "";

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (IsComment(line))
                    {
                        continue;
                    }

                    var sectionMatches = s_sectionMatcher.Matches(line);
                    if (sectionMatches.Count > 0 && sectionMatches[0].Groups.Count > 0)
                    {
                        // Close out the previous section
                        var section = new Section(activeSectionName, activeSectionProperties.ToImmutable());
                        if (activeSectionName == "")
                        {
                            // This is the global section
                            globalSection = section;
                        }
                        else
                        {
                            namedSectionBuilder.Add(section);
                        }

                        var sectionName = sectionMatches[0].Groups[1].Value;
                        Debug.Assert(!string.IsNullOrEmpty(sectionName));

                        activeSectionName = sectionName;
                        activeSectionProperties = ImmutableDictionary.CreateBuilder<string, string>(
                            CaseInsensitiveComparison.Comparer);
                        continue;
                    }

                    var propMatches = s_propertyMatcher.Matches(line);
                    if (propMatches.Count > 0 && propMatches[0].Groups.Count > 1)
                    {
                        var key = propMatches[0].Groups[1].Value;
                        var value = propMatches[0].Groups[2].Value;

                        Debug.Assert(!string.IsNullOrEmpty(key));
                        Debug.Assert(key == key.Trim());
                        Debug.Assert(value == value?.Trim());

                        key = CaseInsensitiveComparison.ToLower(key);
                        if (ReservedKeys.Contains(key) || ReservedValues.Contains(value))
                        {
                            value = CaseInsensitiveComparison.ToLower(value);
                        }

                        activeSectionProperties[key] = value ?? "";
                        continue;
                    }
                }
            }

            // Close out the last section
            var lastSection = new Section(activeSectionName, activeSectionProperties.ToImmutable());
            if (activeSectionName == "")
            {
                // This is the global section
                globalSection = lastSection;
            }
            else
            {
                namedSectionBuilder.Add(lastSection);
            }

            return new EditorConfig(globalSection, namedSectionBuilder.ToImmutable(), parentDirectory);
        }

        private static bool IsComment(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (!char.IsWhiteSpace(c))
                {
                    return c == '#' || c == ';';
                }
            }

            return false;
        }

        /// <summary>
        /// Represents a named section of the editorconfig file, which consists of a name followed by a set
        /// of key-value pairs.
        /// </summary>
        internal sealed class Section
        {
            public Section(string name, ImmutableDictionary<string, string> properties)
            {
                Name = name;
                Properties = properties;
            }

            /// <summary>
            /// The name as present directly in the section specification of the editorconfig file.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Keys and values for this section. All keys are lower-cased according to the
            /// EditorConfig specification and keys are compared case-insensitively. Values are
            /// lower-cased if the value appears in <see cref="EditorConfig.ReservedValues" />
            /// or if the corresponding key is in <see cref="EditorConfig.ReservedKeys" />. Otherwise,
            /// the values are the literal values present in the source.
            /// </summary>
            public ImmutableDictionary<string, string> Properties { get; }
        }
    }
}