using UnityEngine;

public static class Utils
{
    public static T RandomElement<T>(T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static bool IsNotPlaying(int hash, Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash != hash;
    }

    public static bool IsNotPlaying(string name, Animator animator)
    {
        return !animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }
}