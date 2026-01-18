using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MonacoRoslynCompletionProvider
{
    public static class MetadataReferenceProvider
    {
        public static List<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            // Helper to add reference safely
            void Add(string assemblyName)
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load metadata reference for {assemblyName}: {ex.Message}");
                }
            }

            void AddType(Type type)
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load metadata reference for type {type.FullName}: {ex.Message}");
                }
            }

            AddType(typeof(Console));
            Add("System.Runtime");
            AddType(typeof(List<>));
            AddType(typeof(int));
            Add("netstandard");
            AddType(typeof(DescriptionAttribute));
            AddType(typeof(object));
            AddType(typeof(Dictionary<,>));
            AddType(typeof(Enumerable));
            AddType(typeof(DataSet));
            AddType(typeof(XmlDocument));
            AddType(typeof(INotifyPropertyChanged));
            AddType(typeof(System.Linq.Expressions.Expression));

            return references;
        }
    }
}
