using System;
using System.IO;
using System.Linq;
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
                UnityEngine.Debug.LogError($"[NvimUnity] Template not found at {templateFullPath}");
                return;
            }

            string outputPath = Path.Combine(ProjectRoot, "Assembly-CSharp.csproj");

            string templateContent = File.ReadAllText(templateFullPath);

            string analyzersGroup = GenerateAnalyzersItemGroup();
            string generateProject = GenerateGeneratorProjectGroup();
            string compileIncludes = GenerateCompileIncludes();
            string referenceIncludes = GenerateReferenceIncludes();

            string finalContent = templateContent
                .Replace("{{ANALYZERS}}",analyzersGroup)
                .Replace("{{GENERATE_PROJECT_GROUP}}", generateProject) 
                .Replace("{{COMPILE_INCLUDES}}", compileIncludes)
                .Replace("{{REFERENCES}}", referenceIncludes)
                .Replace("\r\n", "\n"); // força LF

            File.WriteAllText(outputPath, finalContent, new UTF8Encoding(false));
        }

        public static string GenerateAnalyzersItemGroup()
        {
            var unityPath = EditorApplication.applicationPath; // Ex: C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe
            var editorDir = Path.GetDirectoryName(unityPath);  // ...\Editor
            var dataDir = Path.Combine(editorDir, "Data");
            var toolsDir = Path.Combine(dataDir, "Tools", "Unity.SourceGenerators");

            // Lista dos DLLs de analyzer que queremos incluir
            var analyzers = new[]
            {
                "Unity.SourceGenerators.dll",
                "Unity.Properties.SourceGenerator.dll",
                "Unity.UIToolkit.SourceGenerator.dll"
            };

            var sb = new StringBuilder();

            foreach (var analyzer in analyzers)
            {
                var fullPath = Path.Combine(toolsDir, analyzer);
                if (File.Exists(fullPath))
                {
                    sb.AppendLine($"    <Analyzer Include=\"{fullPath}\" />");
                }
                else
                {
                    Debug.LogWarning($"Analyzer not found: {fullPath}");
                }
            }
      
            return sb.ToString().Replace("\r\n", "\n").Replace("\r", "").TrimEnd('\n');                }

        private static string GenerateGeneratorProjectGroup()
        {
            var sb = new StringBuilder();

            string unityVersion = Application.unityVersion;
            string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            int buildTargetId = (int)EditorUserBuildSettings.activeBuildTarget;

            // Detectar se é um projeto de editor (presença de pasta Assets/Editor)
            string projectType = Directory.Exists("Assets/Editor") ? "Editor:2" : "Game:1";

            // Obter a versão do gerador dinamicamente (use AssemblyInfo.cs para definir [assembly: AssemblyVersion("x.x.x")])
            string generatorVersion = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";

            if (string.IsNullOrEmpty(generatorVersion) || generatorVersion == "0.0.0.0")
            {
                generatorVersion = "2.0.22"; // fallback
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

        private static string GenerateCompileIncludes()
        {
            var sb = new StringBuilder();
            var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                // Remove o caminho absoluto até "Assets/" e normaliza para usar "/" como separador
                string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
                sb.AppendLine($"    <Compile Include=\"{Utils.NormalizePath(relativePath)}\" />");
            }
            return sb.ToString().Replace("\r\n", "\n").Replace("\r", "").TrimEnd('\n');         }

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

            // Agora adiciona manualmente os .dll de Library/ScriptAssemblies
            string assembliesDir = Path.Combine(Directory.GetCurrentDirectory(), "Library", "ScriptAssemblies");
            if (Directory.Exists(assembliesDir))
            {
                foreach (var dll in Directory.GetFiles(assembliesDir, "*.dll"))
                {
                    string name = Path.GetFileNameWithoutExtension(dll);
                    if (!added.Contains(name)) // Evita duplicar
                    {
                        string hintPath = Utils.NormalizePath(dll);
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

