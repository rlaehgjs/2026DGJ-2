using System;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 1;

    public int Version = CurrentVersion;
    public GameProgressState ProgressState = GameProgressState.NotStarted;
    public int CheckpointIndex;
}
