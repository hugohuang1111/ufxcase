using System;
using UnityEngine;

public class AnimatorEvent : MonoBehaviour
{
    //private Animator mAnimator = null;
    //private RuntimeAnimatorController mRuntimeAnimatorController = null;
    private AnimationClip[] mAnimatorClips = null;

    void Start()
    {
        DynamicBindEvent();
    }

    void OnWindows0_20_Start()
    {
        Debug.LogFormat("{0} OnWindows0_20_Start", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"));
    }
    void OnWindows0_20_End()
    {
        Debug.LogFormat("{0} OnWindows0_20_End", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"));
    }

    void OnWindows20_100_Start()
    {
        Debug.LogFormat("{0} OnWindows20_100_Start", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"));
    }
    void OnWindows20_100_End()
    {
        Debug.LogFormat("{0} OnWindows20_100_End", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"));
    }


    void DynamicBindEvent()
    {
        if (null != mAnimatorClips)
        {
            return;
        }

        Animator animatorComp = GetComponent<Animator>();
        if (null == animatorComp)
        {
            Debug.Log("AnimatorEvent, can't find Animator");
            return;
        }
        RuntimeAnimatorController runtimeAnimatorController = animatorComp.runtimeAnimatorController;
        mAnimatorClips = runtimeAnimatorController.animationClips;

        for (int i = 0; i < mAnimatorClips.Length; i++)
        {
            var animatorClip = mAnimatorClips[i];
            if (animatorClip.events.Length == 0)
                switch (animatorClip.name)
                {
                    case "Windows0_20":
                        {
                            AnimationEvent Windows0_20_Start = new AnimationEvent();
                            AnimationEvent Windows0_20_End = new AnimationEvent();

                            Windows0_20_Start.functionName = "OnWindows0_20_Start";
                            Windows0_20_End.functionName = "OnWindows0_20_End";

                            Windows0_20_Start.time = 0;
                            Windows0_20_End.time = animatorClip.length;

                            animatorClip.AddEvent(Windows0_20_Start);
                            animatorClip.AddEvent(Windows0_20_End);
                        }
                        break;
                    case "Windows20_100":
                        {
                            AnimationEvent Windows20_100_Start = new AnimationEvent();
                            AnimationEvent Windows20_100_End = new AnimationEvent();

                            Windows20_100_Start.functionName = "OnWindows20_100_Start";
                            Windows20_100_End.functionName = "OnWindows20_100_End";

                            Windows20_100_Start.time = 0.001f;
                            Windows20_100_End.time = animatorClip.length;

                            animatorClip.AddEvent(Windows20_100_Start);
                            animatorClip.AddEvent(Windows20_100_End);
                        }
                        break;
                }
        }

        animatorComp.Rebind();
    }

    void UnSubscription()
    {
        for (int i = 0; i < mAnimatorClips.Length; i++)
        {
            if (mAnimatorClips[i].events.Length > 0)
            {
                mAnimatorClips[i].events = default(AnimationEvent[]);
            }
        }
    }

}
