using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class HumanEmotions()
{
    #region props
    /// <summary>
    /// Humor atual.
    /// </summary>
    public MoodEnum Mood { get; private set; } = MoodEnum.Neutral;
    #endregion

    #region methods
    /// <summary>
    /// Altera o humor atual.
    /// </summary>
    public void ChangeMood(MoodEnum mood)
    {
        Mood = mood;
    }
    #endregion
}