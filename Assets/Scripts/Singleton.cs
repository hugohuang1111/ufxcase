using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T gInstance = null;

    public static T Instance
    {
        get { return gInstance; }
    }

    protected virtual void Awake()
    {
        gInstance = this as T;
    }
}