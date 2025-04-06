using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SyncHelper
{
    public static void RegenerateProjectFiles()
    {
        Debug.Log("[NvimUnity] Regenerating solution and C# project files...");

        // Chama API interna do Unity para regenerar arquivos
        UnityEditor.SyncVS.SyncSolution();

        // Aguarda um pouco pra garantir que os arquivos foram gerados
        System.Threading.Thread.Sleep(1000);

        // Filtra e remove csproj desnecessários
        CleanExtraCsprojFiles();

        Debug.Log("[NvimUnity] Project files regenerated.");
    }

    private static void CleanExtraCsprojFiles()
    {
        string root = Directory.GetCurrentDirectory();
        string[] allCsproj = Directory.GetFiles(root, "*.csproj", SearchOption.TopDirectoryOnly);

        var keep = new[]
        {
            "Assembly-CSharp.csproj",
            $"{new DirectoryInfo(root).Name}.csproj"
        };

        foreach (string file in allCsproj)
        {
            if (!keep.Any(k => Path.GetFileName(file).Equals(k)))
            {
                Debug.Log($"[NvimUnity] Deleting extra .csproj: {Path.GetFileName(file)}");
                File.Delete(file);
            }
        }
    }
}

