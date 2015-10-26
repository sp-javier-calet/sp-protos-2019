using UnityEngine;
using UnityEngine.UI;

using SocialPoint.GameLoading;
using System.Collections.Generic;

public class DebugProgressBarTest : MonoBehaviour
{

    public GameObject ProgressContainer;
    public GameLoadingBarController LoadingBar;

    private List<LoadingOperation> _operations = new List<LoadingOperation>();

    void Start()
    {
        _operations = new List<LoadingOperation>();
        var op0 = new LoadingOperation(10);
        _operations.Add(op0);

        Invoke("finish", 5);

        var op1 = new LoadingOperation(2);
        _operations.Add(op1);

        var op2 = new LoadingOperation(5);
        _operations.Add(op2);
        Invoke("finish2", 3);
    }

    void Update()
    {
        float progress = 0;
        _operations.ForEach(p => {
            progress += p.Progress;
        });
        float percent = (progress / _operations.Count);
        LoadingBar.Percent = percent;

        if(System.Math.Abs(percent - 1) < 0.01f)
        {
            UnityEngine.Debug.Break();
            Application.LoadLevel("DebugProgressBarTest");
        }
    }

    void finish()
    {
        _operations[0].Update(1);
    }

    void finish2()
    {
        _operations[2].Update(1);
    }
}
