using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class InAppBrowserBridge : MonoBehaviour {

	[System.Serializable]
	public class BrowserLoadingCallback: UnityEvent<string> {
		
	}

	[System.Serializable]
	public class BrowserLoadingWithErrorCallback: UnityEvent<string, string> {
		
	}

	public BrowserLoadingCallback onJSCallback = new BrowserLoadingCallback();

	public BrowserLoadingCallback onBrowserFinishedLoading = new BrowserLoadingCallback();

	public BrowserLoadingWithErrorCallback onBrowserFinishedLoadingWithError = new BrowserLoadingWithErrorCallback();

	public UnityEvent onBrowserClosed = new UnityEvent();

    public UnityEvent onBrowserBack = new UnityEvent();

	public UnityEvent onAndroidBackButtonPressed = new UnityEvent();

	void OnBrowserJSCallback(string callback) {
		Debug.Log("InAppBrowser: JS Message: " + callback);
		onJSCallback.Invoke(callback);
	}

	void OnBrowserFinishedLoading(string url) {
		Debug.Log("InAppBrowser: FinishedLoading " + url);
		onBrowserFinishedLoading.Invoke(url);
	}

	void OnBrowserFinishedLoadingWithError(string urlAndError) {
		Debug.Log("InAppBrowser: FinishedLoading With Error " + urlAndError);
		string[] parts = urlAndError.Split(',');
		Debug.Log("URL:"+parts[0]);
		Debug.Log("ERROR:"+parts[1]);
		onBrowserFinishedLoadingWithError.Invoke(parts[0], parts[1]);
	}

	public void OnBrowserClosed(string message) {
		Debug.Log("InAppBrowser: Closed");
		onBrowserClosed.Invoke();
        Destroy(this.gameObject);
	}

    public void OnBrowserBack(string message)
    {
        Debug.Log("InAppBrowser: Back");
        InAppBrowser.ExecuteJS("window.history.back();");
        onBrowserBack.Invoke();
    }

	void OnAndroidBackButtonPressed() {
		Debug.Log("InAppBrowser: Android back button pressed");
		onAndroidBackButtonPressed.Invoke();
	}
}
