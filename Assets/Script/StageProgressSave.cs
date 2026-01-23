using UnityEngine;

public static class StageProgressSave
{
    private const string StageKey = "StageProgress";
    private const int MinStage = (int)ScenesManagers.SceneType.Stage1;
    private const int MaxStage = (int)ScenesManagers.SceneType.Stage3;

    public static ScenesManagers.SceneType GetStartStage(ScenesManagers.SceneType fallback)
    {
        if (TryGetSavedStage(out var savedStage))
        {
            return savedStage;
        }

        return fallback;
    }

    public static bool TryGetSavedStage(out ScenesManagers.SceneType stage)
    {
        if (!PlayerPrefs.HasKey(StageKey))
        {
            stage = ScenesManagers.SceneType.Title;
            return false;
        }

        var stored = PlayerPrefs.GetInt(StageKey, MinStage);
        stored = Mathf.Clamp(stored, MinStage, MaxStage);
        stage = (ScenesManagers.SceneType)stored;
        return true;
    }

    public static void SaveClearedStage(ScenesManagers.SceneType clearedStage)
    {
        if (clearedStage < ScenesManagers.SceneType.Stage1)
        {
            return;
        }

        var nextStage = Mathf.Clamp((int)clearedStage, MinStage, MaxStage);

        PlayerPrefs.SetInt(StageKey, nextStage);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(StageKey);
        PlayerPrefs.Save();
    }
}

