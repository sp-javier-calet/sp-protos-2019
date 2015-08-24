using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOperation
{
    public delegate void ProgressChanged(string message);

    public event ProgressChanged ProgressChangedEvent;

    public float Progress { private set; get; }

    public void UpdateProgress(float progress, string message = "")
    {
        Progress = progress;
        ProgressChangedEvent(message);
    }

    public void FinishProgress(string message = null)
    {
        UpdateProgress(1f, message);
    }

    //TODO: delete used as mock for develop
    public IEnumerator FakeLoadingProcess(float time)
    {
        var elapsed = 0f;
        while(elapsed < time)
        {
            elapsed += Time.deltaTime;
            if(elapsed < time)
                UpdateProgress((elapsed / time));
            yield return null;
        }
        FinishProgress("Faked loading finished");
    }
}

public class LoadingBarController : MonoBehaviour
{
	
    public Slider Slider;
    private List<LoadingOperation> LoadingOperations;

    public string[] funnyLogs;
    public bool displayFunnyLogs;

    public Text log;

    public void RegisterLoadingOperation(LoadingOperation operation)
    {
        if(LoadingOperations == null)
            LoadingOperations = new List<LoadingOperation>();

        LoadingOperations.Add(operation);
        operation.ProgressChangedEvent += OnProgressChanged;
    }

    public void RegisterLoadingOperation(List<LoadingOperation> operations)
    {
        operations.ForEach(RegisterLoadingOperation);
    }

    public void OnProgressChanged(string message)
    {
        if(message != string.Empty)
        {
            Debug.Log(message);
            if(log != null)
                log.text = message;
        }
        float progress = 0;
        LoadingOperations.ForEach(p => progress += p.Progress);
        var percent = (progress / LoadingOperations.Count);
        if(displayFunnyLogs && log != null)
        {
            log.text = funnyLogs[Mathf.Min((int)(funnyLogs.Length * percent), funnyLogs.Length - 1)];	
        }
        Slider.value = percent;
    }
}
