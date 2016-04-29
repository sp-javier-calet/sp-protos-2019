CalcWindow.cs

\code{.csharp}
using UnityEngine;
using UnityEditor;
using SocialPoint.Tool.Shared.TLGUI;

public class CalcWindow : TLWindow {

	static CalcWindow	instance;
	public static CalcWindow Instance
	{
		get
		{
			if (instance == null) {
				instance = (CalcWindow)(ScriptableObject.CreateInstance (typeof(CalcWindow)));
				instance.Init ();
			}
			return instance;
		}
	}

	// The views that will be added to this window
	public CalcView		calcView 	{ get; private set; }
	public AboutView	aboutView 	{ get; private set; }

	void Init()
	{
		title = "Calculator Sample";

		// CalcView doesn't need a model
		calcView = new CalcView (this);
		calcView.SetController (new CalcController(calcView));

		// AboutView doesn't need model nor controller, it does not have any logic
		aboutView = new AboutView (this);

		// Add the views available
		AddView (calcView);
		AddView (aboutView);
	}


	[MenuItem("Window/Calculator Sample")]
	public static void OpenWindow()
	{
		CalcWindow window = CalcWindow.Instance;
		window.Show ();

		// Select the starting view
		window.LoadView (window.calcView);
	}
}
\endcode

CalcView.cs

\code{.csharp}
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

public class CalcView : TLView {

	static Dictionary<string, TLStyle>	stylesDict;
	public static Dictionary<string, TLStyle>	StylesDict
	{
		get
		{
			if (stylesDict == null)
				stylesDict = InitStylesDict ();
			return stylesDict;
		}
	}

	public TLWNumberField	nfTop;
	public TLWLabel			lbOperation;
	public TLWNumberField	nfBot;
	public TLWLabel			lbResult;
	public TLWComboBox		cbOpSelector;
	public TLWButton		btnPerformOp;
	public TLWButton		btnAbout;

	public CalcView( TLWindow window ) : base(window, null)
	{
		position = new Rect (100, 100, 300, 300);
		minSize = new Vector2 (250, 250);
		maxSize = new Vector2 (300, 300);

		// Header
		TLWVeticalLayout vlLayout_n0 = new TLWVeticalLayout (this,
		                                                     "vlLayout_n0",
		                                                     StylesDict["layout_margins_stl"],
		                                                     TLLayoutOptions.expandall);
		vlLayout_n0.AddWidget (new TLWLabel (this, "lbTitle", "Calculator", StylesDict["title_stl"]));
		vlLayout_n0.AddWidget (new TLWSpacer (this, "spVertical_n0_n0", 30));
		//

		TLWHorizontalLayout hlLayout_n0_n0 = new TLWHorizontalLayout (this,
		                                                              "hlLayout_n0_n0",
		                                                              new GUILayoutOption[] { GUILayout.MaxHeight (100) });

		// Left column
		TLWVeticalLayout vlLayout_n0_n0_n0 = new TLWVeticalLayout (this,
		                                                           "vlLayout_n0_n0_n0",
		                                                           new GUILayoutOption[] { GUILayout.MaxWidth(125) });

		nfTop = new TLWNumberField (this, "nfTop", "0", 5, new GUILayoutOption[] {GUILayout.Width(100), GUILayout.ExpandHeight(false)});
		lbOperation = new TLWLabel (this, "lbOperation", "");
		nfBot = new TLWNumberField (this, "nfBot", "0", 5, new GUILayoutOption[] {GUILayout.Width(100), GUILayout.ExpandHeight(false)});
		lbResult = new TLWLabel (this, "lbOperation", "");

		vlLayout_n0_n0_n0.AddWidget (nfTop);
		vlLayout_n0_n0_n0.AddWidget (lbOperation);
		vlLayout_n0_n0_n0.AddWidget (nfBot);
		vlLayout_n0_n0_n0.AddWidget (new TLWSplitter (this, "slOperation", 2));
		vlLayout_n0_n0_n0.AddWidget (new TLWSpacer (this, "spVertical_n0_n0_n0_n0", true));
		vlLayout_n0_n0_n0.AddWidget (lbResult);
		//

		// Right column
		TLWVeticalLayout vlLayout_n0_n0_n1 = new TLWVeticalLayout (this,
		                                                           "vlLayout_n0_n0_n1",
		                                                           new GUILayoutOption[] { GUILayout.MaxWidth(120) });

		cbOpSelector = new TLWComboBox (this, "cbOpSelector");
		btnPerformOp = new TLWButton (this, "btnPerformOp", "Calc");

		vlLayout_n0_n0_n1.AddWidget (cbOpSelector);
		vlLayout_n0_n0_n1.AddWidget (new TLWSpacer (this, "spVertical_n0_n0_n1_n0", true));
		vlLayout_n0_n0_n1.AddWidget (btnPerformOp);
		//

		// About icon button
		TLWHorizontalLayout hlLayout_n0_n1 = new TLWHorizontalLayout (this,
		                                                              "hlLayout_n0_n1");

		btnAbout = new TLWButton (this, "btnAbout", TLIcons.gearImg);

		hlLayout_n0_n1.AddWidget (new TLWSpacer (this, "spHorizontal_n0_n1_n0", true));
		hlLayout_n0_n1.AddWidget (btnAbout);
		//

		hlLayout_n0_n0.AddWidget (vlLayout_n0_n0_n0);
		hlLayout_n0_n0.AddWidget (new TLWSpacer (this, "spHorizontal_n0_n0_n0", true));
		hlLayout_n0_n0.AddWidget (vlLayout_n0_n0_n1);
		vlLayout_n0.AddWidget (hlLayout_n0_n0);
		vlLayout_n0.AddWidget (new TLWSpacer (this, "spVertical_n0_n1", true));
		vlLayout_n0.AddWidget (hlLayout_n0_n1);
		this.AddWidget (vlLayout_n0);
	}

	static Dictionary<string, TLStyle> InitStylesDict( )
	{
		Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle> ();

		TLStyle layout_margins_stl = new TLStyle();
		layout_margins_stl.margin = new RectOffset (10, 10, 10, 10);
		dict.Add ("layout_margins_stl", layout_margins_stl);

		TLStyle title_stl = new TLStyle("Label");
		title_stl.alignment = TextAnchor.MiddleLeft;
		title_stl.fontSize = 16;
		title_stl.fontStyle = FontStyle.Bold;
		dict.Add ("title_stl", title_stl);

		return dict;
	}

}
\endcode

CalcController.cs

\code{.csharp}
using SocialPoint.Tool.Shared.TLGUI;

public class CalcController : TLController {

	public CalcView View 
	{ 
		get 
		{ 
			return (CalcView)_view;
		}
	}

	public CalcController( CalcView view ) : base(view, null)
	{
		Init ();
	}

	void Init( )
	{
		// Init widget cbOpSelector with the values
		View.cbOpSelector.Add(new string[] {"ADD", "SUB", "MULT"});

		// Connect the widget events to the actions
		View.cbOpSelector.selectionChange.Connect (OnOpChanged);
		View.btnPerformOp.onClickEvent.Connect (OnCalcClicked);
		View.btnAbout.onClickEvent.Connect (OnAboutClicked);

		// HACK: TLWFloater widgets must control widgets that are drawn under them
		View.cbOpSelector.expanded.Connect (OnCbExpand);
		View.cbOpSelector.contracted.Connect (OnCbContract);
	}

	void OnOpChanged( )
	{
		string selected = View.cbOpSelector.Selected;

		switch (selected)
		{
		case "ADD":
			View.lbOperation.text = "+";
			break;
		case "SUB":
			View.lbOperation.text = "-";
			break;
		case "MULT":
			View.lbOperation.text = "x";
			break;
		default:
			View.lbOperation.text = "";
			break;
		}
	}

	void OnCalcClicked( )
	{
		string selected = View.cbOpSelector.Selected;
		
		switch (selected)
		{
		case "ADD":
			View.lbResult.text = (int.Parse(View.nfTop.number) + int.Parse(View.nfBot.number)).ToString ();
			break;
		case "SUB":
			View.lbResult.text = (int.Parse(View.nfTop.number) - int.Parse(View.nfBot.number)).ToString ();
			break;
		case "MULT":
			View.lbResult.text = (int.Parse(View.nfTop.number) * int.Parse(View.nfBot.number)).ToString ();
			break;
		default:
			View.lbResult.text = "";
			break;
		}
	}

	void OnCbExpand( )
	{
		View.btnPerformOp.SetDisabled (true);
	}

	void OnCbContract( )
	{
		View.btnPerformOp.SetDisabled (false);
	}

	void OnAboutClicked( )
	{
		View.window.ChangeView ((View.window as CalcWindow).aboutView);
	}
}
\endcode

AboutView.cs

\code{.csharp}
using SocialPoint.Tool.Shared.TLGUI;

public class AboutView : TLView {

	public AboutView( TLWindow window ) : base(window, null)
	{
		AddWidget (new TLWLabel (this, "lbDescription", "This is a sample usage of the MVC EditorGUI."));
	}
}
\endcode