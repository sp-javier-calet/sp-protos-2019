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
        op0.ProgressChangedEvent += (message) => {};

        Invoke("finish",5);

        var op1 = new LoadingOperation(2);
        _operations.Add(op1);
        op1.ProgressChangedEvent += (message) => {};

        var op2 = new LoadingOperation(5);
        _operations.Add(op2);
        Invoke("finish2",3);
        op2.ProgressChangedEvent += (message) => {};
    }

    void Update()
    {
        float progress = 0;
        _operations.ForEach(p => {
            p.Update(Time.deltaTime);
            progress += p.FakeProgress;
        });
        float percent = (progress / _operations.Count);
        LoadingBar.UpdateProgress(percent, "");

        if(System.Math.Abs(percent - 1) < 0.01f)
        {
            UnityEngine.Debug.Break();
            Application.LoadLevel("DebugProgressBarTest");
        }
    }

    void finish()
    {
        _operations[0].UpdateProgress(1,"");
    }

    void finish2()
    {
        _operations[2].UpdateProgress(1,"");
    }
}
