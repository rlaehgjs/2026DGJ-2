using UnityEditor;
using UnityEngine;

public static class SaveDebugMenu
{
    [MenuItem("Tools/Debug/Clear Game Save")]
    private static void ClearGameSaveFromMenu()
    {
        bool shouldClear = EditorUtility.DisplayDialog(
            "Clear Game Save",
            "게임 진행 저장 데이터를 삭제합니다. 설정값은 유지됩니다.",
            "삭제",
            "취소");

        if (!shouldClear)
        {
            return;
        }

        ClearGameSave();
        Debug.Log("[SaveDebugMenu] 게임 진행 저장 데이터를 삭제했습니다.");
    }

    private static void ClearGameSave()
    {
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }
}
