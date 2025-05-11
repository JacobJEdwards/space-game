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

    public static void HideLockMouse(bool on)
    {
        if (on)
        {
            if (Cursor.visible) Cursor.visible = false;
            if (Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            if (Cursor.visible == false) Cursor.visible = true;
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
        }
    }
}