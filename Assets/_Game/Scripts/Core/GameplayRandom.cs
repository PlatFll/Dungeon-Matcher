using UnityEngine;

public static class GameplayRandom
{
    public static int Range(int minimum,int maximum)
    {
        var random=RunSession.Current?.Continuation?.Random;
        return random!=null?random.Next(minimum,maximum):UnityEngine.Random.Range(minimum,maximum);
    }
    public static float Range(float minimum,float maximum)
    {
        var random=RunSession.Current?.Continuation?.Random;
        return random!=null?minimum+(maximum-minimum)*(float)random.NextDouble():UnityEngine.Random.Range(minimum,maximum);
    }
}
