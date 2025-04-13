using Mafi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomRecipes.ModuleTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ExportBinaries();
        }

        private static void ExportBinaries()
        {
            var mafiTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.dll").GetExportedTypes();
            //var mafiUnityTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Unity.dll").GetExportedTypes();
            var mafiCoreTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Core.dll").GetExportedTypes();
            var mafiBaseTypes = Assembly.LoadFrom($"{Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location)}/Mafi.Base.dll").GetExportedTypes();

            IEnumerable<Type> allTypes = mafiTypes.Concat(mafiCoreTypes).Concat(mafiBaseTypes);
            ILookup<string, Type> lookup = allTypes.ToLookup(t => t.Namespace);

            DirectoryInfo targetDirectory = new DirectoryInfo(@"..\..\..\Recipes");

            foreach (IGrouping<string, Type> item in lookup)
            {
                string directoryname = targetDirectory.FullName + @"\" + item.Key.Replace(".", @"\");
                string filename = targetDirectory.FullName + @"\" + item.Key.Replace(".", @"\") + @"\__init__.py";

                StringBuilder stringBuilder = new StringBuilder();

                string indent = "";
                foreach (Type type in item)
                {
                    if (type.IsNested)
                        continue;

                    WriteType(stringBuilder, indent, type);
                }

                Directory.CreateDirectory(directoryname);
                File.WriteAllText(filename, stringBuilder.ToString());
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void WriteType(StringBuilder stringBuilder, string indent, Type type)
        {
            if (type.IsArray) return;
            if (type.IsGenericType) return;

            stringBuilder.Append("\n");
            stringBuilder.Append(indent);
            stringBuilder.Append("class ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(":\n");

            HashSet<string> mayImport = new HashSet<string>();
            foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                WriteMember(stringBuilder, indent, property.Name, property.PropertyType, mayImport, () => property.GetValue(null));
            }

            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                WriteMember(stringBuilder, indent, field.Name, field.FieldType, mayImport, () => field.GetValue(null));
            }

            stringBuilder.Append($"{indent}    def __init__(self):\n");

            HashSet<string> names = new HashSet<string>();
            HashSet<string> mayImportSelf = new HashSet<string>();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                AppendInstanceProperty(indent, property.Name, property.PropertyType, stringBuilder, names, mayImport);
            }
            foreach (var property in type.GetInterfaces()
                .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)))
            {
                AppendInstanceProperty(indent, property.Name, property.PropertyType, stringBuilder, names, mayImport);
            }
            foreach (var property in type.GetInterfaces()
                .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)))
            {
                AppendInstanceProperty(indent, property.Name, property.PropertyType, stringBuilder, names, mayImport);
            }
            foreach (var property in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                AppendInstanceProperty(indent, property.Name, property.FieldType, stringBuilder, names, mayImport);
            }

            if (names.Count == 0)
                stringBuilder.Append($"{indent}        pass\n\n");

            foreach (var nested in type.GetNestedTypes())
            {
                WriteType(stringBuilder, indent + "    ", nested);
            }
        }

        private static void WriteMember(StringBuilder stringBuilder, string indent, string propertyName, Type propertyType, HashSet<string> imports, Func<object> valueGetter)
        {
            if (propertyType == typeof(bool))
                stringBuilder.Append($"{indent}    {propertyName} = False\n");
            else if (propertyType == typeof(string))
                stringBuilder.Append($"{indent}    {propertyName} = \"\"\n");
            else if (propertyType == typeof(int) || propertyType == typeof(long))
                stringBuilder.Append($"{indent}    {propertyName} = 0\n");
            else if (propertyType == typeof(float))
                stringBuilder.Append($"{indent}    {propertyName} = 0.0\n");
            else if (propertyType == typeof(Fix32))
                stringBuilder.Append(MayImport(imports, $"{indent}    from Mafi import Fix32\n") + $"{indent}    {propertyName} = Fix32()\n");
            else if (propertyType == typeof(Fix64))
                stringBuilder.Append(MayImport(imports, $"{indent}    from Mafi import Fix64\n") + $"{indent}    {propertyName} = Fix64()\n");
            else if (propertyType.Name.StartsWith("Option"))
                stringBuilder.Append(MayImport(imports, $"{indent}    from Mafi import Option\n") + $"{indent}    {propertyName} = Option()\n");
            else if (propertyType.Name.EndsWith("ID") && propertyType.IsNested)
            {
                string protoNamespace = propertyType.Namespace;
                string protoType = propertyType.FullName.Split('+')[0].Remove(0, (protoNamespace + ".").Length);
                if (!imports.Contains(propertyType.FullName))
                {
                    imports.Add(propertyType.FullName);
                    stringBuilder.Append($"{indent}    from {protoNamespace} import {protoType}\n");
                }
                string value = ((dynamic)valueGetter()).Value;
                stringBuilder.Append($"{indent}    {propertyName} = {protoType}.ID('{value}')\n");
            }
            else
                stringBuilder.Append($"{indent}    {propertyName} = None\n");
        }

        private static string MayImport(HashSet<string> imports, string text)
        {
            if (imports.Contains(text))
                return string.Empty;
            imports.Add(text);
            return text;
        }

        private static void AppendInstanceProperty(string indent, string propertyName, Type propertyType, StringBuilder stringBuilder, HashSet<string> names, HashSet<string> imports)
        {
            if (!names.Add(propertyName))
                return;

            if (propertyType == typeof(bool))
                stringBuilder.Append($"{indent}        self.{propertyName} = False\n");
            else if (propertyType == typeof(string))
                stringBuilder.Append($"{indent}        self.{propertyName} = \"\"\n");
            else if (propertyType == typeof(int) || propertyType == typeof(long))
                stringBuilder.Append($"{indent}        self.{propertyName} = 0\n");
            else if (propertyType == typeof(float))
                stringBuilder.Append($"{indent}        self.{propertyName} = 0.0\n");
            else if (propertyType == typeof(Fix32))
                stringBuilder.Append(MayImport(imports, $"{indent}        from Mafi import Fix32\n") + $"{indent}        self.{propertyName} = Fix32()\n");
            else if (propertyType == typeof(Fix64))
                stringBuilder.Append(MayImport(imports, $"{indent}        from Mafi import Fix64\n") + $"{indent}        self.{propertyName} = Fix64()\n");
            else if (propertyType.Name.StartsWith("Option"))
                stringBuilder.Append(MayImport(imports, $"{indent}        from Mafi import Option\n") + $"{indent}        self.{propertyName} = Option()\n");
            else if (propertyType.Name.EndsWith("ID") && propertyType.IsNested)
            {
                string protoNamespace = propertyType.Namespace;
                string protoType = propertyType.FullName.Split('+')[0].Remove(0, (protoNamespace + ".").Length);
                if (!imports.Contains(propertyType.FullName))
                {
                    imports.Add(propertyType.FullName);
                    stringBuilder.Append($"{indent}        from {protoNamespace} import {protoType}\n");
                }
                stringBuilder.Append($"{indent}        self.{propertyName} = {protoType}.ID()\n\n");
            }
            else
                stringBuilder.Append($"{indent}        self.{propertyName} = None\n");
        }
    }
}
