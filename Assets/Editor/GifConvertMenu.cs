using System.IO;
using UnityEditor;
using UnityEngine;

public static class GifConvertMenu
{
    private const string MenuPath = "Assets/GIF/Convert to GifBytes (Sprite Importer)";
    private const string SourceExtension = ".gif";
    private const string TargetExtension = "gifbytes";

    [MenuItem(MenuPath, true)]
    private static bool ValidateConvert()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0) return false;

        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            if (path.EndsWith(SourceExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    [MenuItem(MenuPath)]
    private static void ConvertSelected()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0) return;

        var projectRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(projectRoot)) return;

        var converted = 0;
        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            if (!path.EndsWith(SourceExtension, System.StringComparison.OrdinalIgnoreCase)) continue;

            var destPath = Path.ChangeExtension(path, TargetExtension);
            var absoluteSource = Path.Combine(projectRoot, path);
            var absoluteDest = Path.Combine(projectRoot, destPath);

            try
            {
                File.Copy(absoluteSource, absoluteDest, true);
                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
                converted++;
            }
            catch (IOException ex)
            {
                Debug.LogError($"GIF convert failed: {path}\n{ex.Message}");
            }
        }

        if (converted > 0)
        {
            AssetDatabase.Refresh();
        }
    }
}

