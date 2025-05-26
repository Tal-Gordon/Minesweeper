using UnityEngine;

public static class Utils
{
    public const int FLAG = -2;
    public const int MINE = -1;
    public const int EMPTY = 0;

    public static bool RandBool(float chance)
    {
        if (chance < 0 || chance > 1)
        {
            Debug.LogError($"Error: chance value is invalid: {chance}");
            return false;
        }

        return Random.Range(0f, 1f) < chance;
    }
}
