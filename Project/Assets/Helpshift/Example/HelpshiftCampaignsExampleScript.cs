using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Helpshift.Campaigns;


#if UNITY_IOS || UNITY_ANDROID
using Helpshift;
using HSMiniJSON;
#endif

public class HelpshiftCampaignsExampleScript : MonoBehaviour {

#if UNITY_IOS || UNITY_ANDROID
    private HelpshiftSdk _support;
    private HelpshiftCampaigns _campaigns;
    // Use this for initialization

    void Start () {
        _support = HelpshiftSdk.getInstance();
        _campaigns = HelpshiftCampaigns.getInstance ();
        //_support.registerForPush("<gcm-key>");
        _support.install();
    }

    public void onAddPropertiesClick() {
        Dictionary<string,object> propertyDict = new Dictionary<string,object> ();
        propertyDict.Add ("DTestBoolKey", true);
        propertyDict.Add ("DTestIntKey", 10);
        propertyDict.Add ("DTestStringKey", "Helpshift");
        propertyDict.Add ("DTestDateKey", DateTime.Now);

        string[] result = _campaigns.AddProperties(propertyDict);
        foreach(var item in result)
            {
                Debug.Log ("Result is : " + item.ToString());
            }
    }

    public void onAddPropertyIntegerClick() {
        GameObject inputFieldGo = GameObject.FindGameObjectWithTag("property_int");
        InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();
        try
            {
                bool result = _campaigns.AddProperty("TestIntKey", Convert.ToInt32(inputFieldCo.text));
				Debug.Log ("Result is : " + result.ToString());
				Debug.Log ("Property added : " + inputFieldCo.text);
            }
        catch (FormatException e)
            {
                Debug.Log("Input string is not valid : " + e);
            }
    }

    public void onAddPropertyDateClick() {
        bool result = _campaigns.AddProperty("TestDateKey", DateTime.Now);
        Debug.Log ("Result is : " + result.ToString());
    }

    public void onAddPropertyBooleanClick() {
        GameObject inputFieldGo = GameObject.FindGameObjectWithTag("property_bool");
        InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();
        try
            {
                bool result = _campaigns.AddProperty("TestBoolKey",Convert.ToBoolean(inputFieldCo.text));
				Debug.Log ("Result is : " + result.ToString());
				Debug.Log ("Property added : " + inputFieldCo.text);
            }
        catch (FormatException e)
            {
                Debug.Log("Input string is not valid : " + e);
            }
    }

    public void onAddPropertyStringClick() {
        GameObject inputFieldGo = GameObject.FindGameObjectWithTag("property_string");
        InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();
        try
            {
                bool result = _campaigns.AddProperty("TestStringKey", Convert.ToString(inputFieldCo.text));
				Debug.Log ("Result is : " + result.ToString());
				Debug.Log ("Property added : " + inputFieldCo.text);
            }
        catch (FormatException e)
            {
                Debug.Log("Input string is not valid : " + e);
            }
    }

    public void onShowInboxClicked() {
        Dictionary<string,object> config = new Dictionary<string,object> ();
        _campaigns.ShowInbox(config);
    }

    public void Login() {
        _support.login ("user-1", "user.1", "user1@helpshift.com");
    }

    public void Logout() {
        _support.logout ();
    }

    public void onBackToSupportClick() {
        Application.LoadLevel(0);
    }
#endif
}
