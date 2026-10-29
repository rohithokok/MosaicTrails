using UnityEngine;

public static class ProgressStore
{
    private const string UnlockedLevelKey = "MosaicTrails.UnlockedLevel.v1";

    public static int HighestUnlockedLevel(int levelCount)
    {
        if (levelCount <= 0)
            return 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedLevelKey, 0), 0, levelCount - 1);
    }

    public static void UnlockNext(int completedLevel, int levelCount)
    {
        if (levelCount <= 0)
            return;

        int newHighest = Mathf.Min(completedLevel + 1, levelCount - 1);
        int current = HighestUnlockedLevel(levelCount);
        if (newHighest > current)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, newHighest);
            PlayerPrefs.Save();
        }
    }
}
