using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#if UNITY_IOS || UNITY_ANDROID
using Helpshift;
using HSMiniJSON;
#endif

public class HelpshiftExampleScript : MonoBehaviour {

#if UNITY_IOS || UNITY_ANDROID
	private HelpshiftSdk _support;
	public void updateMetaData (string nothing) {
		Debug.Log("Update metadata ************************************************************");
		Dictionary<string, object> configMap = new Dictionary<string, object>();
		configMap.Add("user-level", "21");
		configMap.Add ("hs-tags", new string [] {"Tag-1"});
		_support.updateMetaData(configMap);
	}

	public void helpshiftSessionBegan (string message) {
		Debug.Log("Session Began ************************************************************");
	}

	public void helpshiftSessionEnded (string message) {
		Debug.Log("Session ended ************************************************************");
	}

	public void alertToRateAppAction (string result) {
		Debug.Log("User action on alert :" + result);
	}

	public void didReceiveNotificationCount(string count) {
		Debug.Log("Notification async count : " + count);
	}

	public void didReceiveInAppNotificationCount(string count) {
		Debug.Log("In-app Notification count : " + count);
	}

	/// <summary>
	/// Conversation delegates
	/// </summary>

	public void newConversationStarted (string message) {
		// your code here
	}

	public void userRepliedToConversation (string newMessage) {
		// your code here
	}

	public void userCompletedCustomerSatisfactionSurvey (string json) {
		Dictionary<string, object> csatInfo = (Dictionary<string, object>)Json.Deserialize(json);
        Debug.Log("Customer satisfaction information : " + csatInfo);
	}

	// Use this for initialization
	void Start () {
		_support = HelpshiftSdk.getInstance();
		_support.install();

        //_support.registerForPush("<gcmSenderId>");
	//_support.login(<your_user_identifier>, <user_name>, <user_email>);
	}

	public void onShowFAQsClick() {
  		Debug.Log("Show FAQs clicked !!");
        _support.showFAQs();
	}
	public void onCustomContactUsClick() {
		Dictionary<string, object>[] flows = getDynamicFlows();
		Dictionary<string, object> faqConfig = new Dictionary<string, object>();
		faqConfig.Add(HelpshiftSdk.HsCustomContactUsFlows, flows);
		_support.showFAQs (faqConfig);
	}

	protected Dictionary<string, object>[] getDynamicFlows() {
		Dictionary<string, object> conversationFlow = new Dictionary<string, object>();
		conversationFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeConversation);
		conversationFlow.Add(HelpshiftSdk.HsFlowTitle, "Converse");
		Dictionary<string, object> conversationConfig = new Dictionary<string, object>() {{"conversationPrefillText" , "This is from dynamic"}};
		conversationFlow.Add(HelpshiftSdk.HsFlowConfig, conversationConfig);

		Dictionary<string, object> faqsFlow = new Dictionary<string, object>();
		faqsFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeFaqs);
		faqsFlow.Add(HelpshiftSdk.HsFlowTitle, "FAQs");

		Dictionary<string, object> faqSectionFlow = new Dictionary<string, object>();
		faqSectionFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeFaqSection);
		faqSectionFlow.Add(HelpshiftSdk.HsFlowTitle, "FAQ section");
		faqSectionFlow.Add(HelpshiftSdk.HsFlowData, "1509");

		Dictionary<string, object> faqFlow = new Dictionary<string, object>();
		faqFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeSingleFaq);
		faqFlow.Add(HelpshiftSdk.HsFlowTitle, "FAQ");
		faqFlow.Add(HelpshiftSdk.HsFlowData, "2998");

		Dictionary<string, object> nestedFlow = new Dictionary<string, object>();
		nestedFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeNested);
		nestedFlow.Add(HelpshiftSdk.HsFlowTitle, "Next form");
		nestedFlow.Add(HelpshiftSdk.HsFlowData, new Dictionary<string, object>[] {conversationFlow, faqsFlow, faqSectionFlow, faqFlow});

		Dictionary<string, object>[] flows =  new Dictionary<string, object>[] {
			conversationFlow,
			faqsFlow,
			faqSectionFlow,
			faqFlow,
			nestedFlow
		};

		return flows;
	}

	public void onShowDynamicClick() {
		_support.showDynamicForm("This is a dynamic form", getDynamicFlows());
	}

	public void onShowConversationClick() {
		Debug.Log("Show Conversation clicked !!");
		_support.showConversation();
	}

	public void onShowFAQSectionClick () {
		GameObject inputFieldGo = GameObject.FindGameObjectWithTag("faq_section_id");
		InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();
		try
		{
			Convert.ToInt16(inputFieldCo.text);
			_support.showFAQSection(inputFieldCo.text);
		}
		catch (FormatException e)
		{
			Debug.Log("Input string is not a sequence of digits : " + e);
		}
	}

	public void onShowFAQClick () {
		GameObject inputFieldGo = GameObject.FindGameObjectWithTag("faq_id");
		InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();
		try
		{
			Convert.ToInt16(inputFieldCo.text);
			_support.showSingleFAQ(inputFieldCo.text);
		}
		catch (FormatException e)
		{
			Debug.Log("Input string is not a sequence of digits : " + e);
		}
	}

	public void onShowReviewReminderClick () {
#if UNITY_IOS
		_support.showAlertToRateAppWithURL("itms-apps://itunes.apple.com/app/id460171653");
#elif UNITY_ANDROID
		_support.showAlertToRateAppWithURL("market://details?id=com.RunnerGames.game.YooNinja_Lite");
#endif
	}

	public void onCampaignsTabClick () {
    	Application.LoadLevel(1);
	}
	#endif
}
