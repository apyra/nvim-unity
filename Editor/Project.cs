using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;

namespace NvimUnity
{
    public static class Project
    {
        private static string ProjectRoot => NeovimEditor.RootFolder;
        private static string TemplatesPath => Utils.NormalizePath(Path.GetFullPath("Packages/com.apyra.nvim-unity/Editor/Templates"));

        private static string csprojPath;
        private static HashSet<string> toCompile = new HashSet<string>();

        static Project()
        {
            csprojPath = Path.Combine(ProjectRoot, "Assembly-CSharp.csproj");
            if (Exists())
                GetCompileIncludes();
        }

        public static bool Exists()
        {
            return File.Exists(csprojPath);
        }


        public static void GenerateAll()
        {
            GenerateSolution();
            GenerateProject();
        }

        public static void GenerateSolution()
        {
            string slnTemplatePath = Path.Combine(TemplatesPath, "template.sln");
            string slnOutputPath = Path.Combine(ProjectRoot, $"{Path.GetFileName(ProjectRoot)}.sln");

            if (!File.Exists(slnTemplatePath))
            {
                Debug.LogError("[NvimUnity] Missing template.sln");
                return;
            }

            string slnContent = File.ReadAllText(slnTemplatePath);
            slnContent = slnContent.Replace("{{ProjectName}}", "Assembly-CSharp");

            File.WriteAllText(slnOutputPath, slnContent);
        }

        public static void GenerateProject()
        {
            string templateFullPath = Path.Combine(TemplatesPath, "template.csproj");

            if (!File.Exists(templateFullPath))
            {
                Debug.LogError($"[NvimUnity] Template not found at {templateFullPath}");
                return;
            }

            string templateContent = File.ReadAllText(templateFullPath);

            string analyzersGroup = GenerateAnalyzersItemGroup();
            string generateProject = GenerateGeneratorProjectGroup();
            string referenceIncludes = GenerateReferenceIncludes();

            string finalContent = templateContent
                .Replace("{{ANALYZERS}}", analyzersGroup)
                .Replace("{{GENERATE_PROJECT_GROUP}}", generateProject)
                .Replace("{{REFERENCES}}", referenceIncludes)
                .Replace("\r\n", "\n"); // Forces LF

            File.WriteAllText(csprojPath, finalContent, new UTF8Encoding(false));

            GenerateCompileIncludes();
        }

        public static bool HasFilesBeenDeletedOrMoved()
        {
            return toCompile.Any(file => !AssetDatabase.AssetPathExists(file));
        }

        public static bool NeedRegenerateCompileIncludes(List<string> files)
        {
            return files.Any(file => !toCompile.Contains(file));
        }

        public static void GetCompileIncludes()
        {
            var xml = XDocument.Load(csprojPath);
            var ns = xml.Root.Name.Namespace;

            toCompile.Clear(); // Clear current cache

            foreach (var compile in xml.Descendants(ns + "Compile"))
            {
                var includeAttr = compile.Attribute("Include");
                if (includeAttr != null)
                {
                    toCompile.Add(includeAttr.Value);
                }
            }
        }

        public static void GenerateCompileIncludes()
        {
            toCompile.Clear();

            var xml = XDocument.Load(csprojPath);
            var ns = xml.Root.Name.Namespace;

            string rawXml = File.ReadAllText(csprojPath);
            bool hasPlaceholder = rawXml.Contains("<!-- {{COMPILE_INCLUDES}} -->");

            // Generate relative paths of all .cs files inside Assets/
            var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            var compileElements = files.Select(file =>
            {
                string relativePath = Utils.NormalizePath("Assets" + file.Substring(Application.dataPath.Length));
                toCompile.Add(relativePath);
                return new XElement(ns + "Compile", new XAttribute("Include", relativePath));
            }).ToList();

            XElement itemGroup = null;

            if (hasPlaceholder)
            {
                itemGroup = xml.Root.Elements(ns + "ItemGroup")
                    .FirstOrDefault(g => g.Nodes().OfType<XComment>().Any(c => c.Value.Contains("{{COMPILE_INCLUDES}}")));

                if (itemGroup != null)
                {
                    // Remove all existing <Compile> tags
                    itemGroup.Elements(ns + "Compile").Remove();

                    // Add new ones
                    foreach (var compile in compileElements)
                        itemGroup.Add(compile);
                }
            }
            else
            {
                // Create new ItemGroup with the placeholder comment and <Compile> tags
                itemGroup = new XElement(ns + "ItemGroup",
                    new XComment(" {{COMPILE_INCLUDES}} ")
                );

                foreach (var compile in compileElements)
                    itemGroup.Add(compile);

                // Insert as the 10th child of <Project>, or at the end if there are less than 10
                var projectChildren = xml.Root.Elements().ToList();
                if (projectChildren.Count >= 10)
                    projectChildren[9].AddBeforeSelf(itemGroup);
                else
                    xml.Root.Add(itemGroup);
            }

            xml.Save(csprojPath);
        }

        private static string GenerateAnalyzersItemGroup()
        {
            var unityPath = EditorApplication.applicationPath; // Ex: C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe
            var editorDir = Path.GetDirectoryName(unityPath);  // ...\Editor
            var dataDir = Path.Combine(editorDir, "Data");
            var toolsDir = Path.Combine(dataDir, "Tools", "Unity.SourceGenerators");

            var sb = new StringBuilder();

            if (Directory.Exists(toolsDir))
            {
                var dlls = Directory.GetFiles(toolsDir, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (var dll in dlls)
                {
                    sb.AppendLine($"    <Analyzer Include=\"{dll}\" />");
                }
            }
            return sb.ToString().Replace("\r\n", "\n").Replace("\r", "").TrimEnd('\n');
        }

        private static string GenerateGeneratorProjectGroup()
        {
            var sb = new StringBuilder();

            string unityVersion = Application.unityVersion;
            string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            int buildTargetId = (int)EditorUserBuildSettings.activeBuildTarget;

            // Detect if it's an editor project (presence of Assets/Editor folder)
            string projectType = Directory.Exists("Assets/Editor") ? "Editor:2" : "Game:1";

            // Obtain the generator version dynamically (use AssemblyInfo.cs to set [assembly: AssemblyVersion("x.x.x")])
            string generatorVersion = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";

            if (string.IsNullOrEmpty(generatorVersion) || generatorVersion == "0.0.0.0")
            {
                generatorVersion = "2.0.22"; // Fallback
            }

            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <UnityProjectGenerator>Package</UnityProjectGenerator>");
            sb.AppendLine($"    <UnityProjectGeneratorVersion>{generatorVersion}</UnityProjectGeneratorVersion>");
            sb.AppendLine("    <UnityProjectGeneratorStyle>SDK</UnityProjectGeneratorStyle>");
            sb.AppendLine($"    <UnityProjectType>{projectType}</UnityProjectType>");
            sb.AppendLine($"    <UnityBuildTarget>{buildTarget}:{buildTargetId}</UnityBuildTarget>");
            sb.AppendLine($"    <UnityVersion>{unityVersion}</UnityVersion>");
            sb.AppendLine("  </PropertyGroup>");

            return sb.ToString().Replace("\r\n", "\n").Replace("\r", "").TrimEnd('\n');
        }

        private static string GenerateReferenceIncludes()
        {
            var sb = new StringBuilder();

            var assemblies = CompilationPipeline.GetAssemblies();
            var asm = assemblies.FirstOrDefault(a => a.name == "Assembly-CSharp");

            HashSet<string> added = new();

            if (asm != null)
            {
                foreach (var reference in asm.compiledAssemblyReferences)
                {
                    if (File.Exists(reference))
                    {
                        string name = Path.GetFileNameWithoutExtension(reference);
                        string hintPath = Utils.NormalizePath(reference);

                        sb.AppendLine($@"    <Reference Include=""{name}"">");
                        sb.AppendLine($@"      <HintPath>{hintPath}</HintPath>");
                        sb.AppendLine($@"      <Private>False</Private>");
                        sb.AppendLine($@"    </Reference>");

                        added.Add(name);
                    }
                }
            }

            // Add manually the .dll from Library/ScriptAssemblies
            string assembliesDir = Path.Combine(Directory.GetCurrentDirectory(), "Library", "ScriptAssemblies");
            if (Directory.Exists(assembliesDir))
            {
                foreach (var dll in Directory.GetFiles(assembliesDir, "*.dll"))
                {
                    string name = Path.GetFileNameWithoutExtension(dll);
                    if (!added.Contains(name)) // Avoids duplicates
                    {
                        string normalizedPath = Utils.NormalizePath(dll);

                        // Ensures HintPath starts with "Library\..."
                        string hintPath;
                        var index = normalizedPath.IndexOf("Library" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                            hintPath = normalizedPath.Substring(index);
                        else
                            hintPath = normalizedPath; // Fallback in case of unexpected behavior

                        sb.AppendLine($@"    <Reference Include=""{name}"">");
                        sb.AppendLine($@"      <HintPath>{hintPath}</HintPath>");
                        sb.AppendLine($@"      <Private>False</Private>");
                        sb.AppendLine($@"    </Reference>");
                    }
                }
            }

            return sb.ToString().Replace("\r\n", "\n").TrimEnd('\n');
        }
    }
}

