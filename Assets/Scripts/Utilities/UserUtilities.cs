using System;
using UnityEngine;

public static class UserUtilities
{
    private const int MinRGBValue = 1;
    private const int MaxRGBValue = 255;

    public static void CatchNullArgumentException(object obj, string nameOfObject)
    {
        if (obj == null)
            throw new ArgumentNullException(nameOfObject);
    }

    public static void CatchNullArgumentException(params (object value, string name)[] objects)
    {
        foreach (var (value, name) in objects)
            CatchNullArgumentException(value, name);
    }

    public static float GenerateRandomFloat(float MinValueIncluse = 0f, float MaxValueInclusive = 1f)
    {
        return UnityEngine.Random.Range(MinValueIncluse, MaxValueInclusive);
    }

    public static int GenerateRandomInt(int minInclusive, int maxInclusive)
    {
        return UnityEngine.Random.Range(minInclusive, maxInclusive + 1);
    }

    public static Color GenerateRandomRGBColor()
    {
        float red = Convert.ToSingle(GenerateRandomInt(MinRGBValue, MaxRGBValue)) / MaxRGBValue;
        float green = Convert.ToSingle(GenerateRandomInt(MinRGBValue, MaxRGBValue)) / MaxRGBValue;
        float blue = Convert.ToSingle(GenerateRandomInt(MinRGBValue, MaxRGBValue)) / MaxRGBValue;

        return new Color(red, green, blue);
    }
}
