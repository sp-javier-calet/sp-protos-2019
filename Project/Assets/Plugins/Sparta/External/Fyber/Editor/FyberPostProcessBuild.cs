using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.XCodeEditor.FyberPlugin;
using FyberEditor;
using System.Text;
using System.IO;
using FyberPlugin.Editor;

public class FyberPostProcessBuild
{

	[PostProcessBuild(500)]
	public static void OnPostProcessBuild( BuildTarget target, string path )
	{

#if UNITY_5_3_OR_NEWER
		if (target == BuildTarget.iOS)
#else
		if (target == BuildTarget.iPhone)
#endif
		{
			XCProject project = new XCProject(path);
			
			// Find and run through all projmods files to patch the project
			string projModPath = System.IO.Path.Combine(Application.dataPath, "Plugins/Sparta/External/Fyber/iOS");
			string[] files = System.IO.Directory.GetFiles(projModPath, "*.projmods", System.IO.SearchOption.AllDirectories);
			foreach( var file in files ) 
			{
				project.ApplyMod(Application.dataPath, file);

				if (file.Contains("Chartboost"))
				{
					string unityVersionPlist = "<plist><key>name</key><string>Chartboost</string><key>settings</key><dict><key>FYBUnityVersion</key><string>" + Application.unityVersion +"</string></dict></plist>";
            		PlistUpdater.UpdatePlist(project.projectRootPath, unityVersionPlist);
				}
			}
			project.Save();
			
		}
	}

	[PostProcessBuild(600)]
	public static void OnPostProcessBuildOrientationFix(BuildTarget target, string pathToBuildProject)
	{
		if (PlayerPrefs.GetInt ("FYBPostProcessBuild") == 0)
			return;
		
		StringBuilder newFile = new StringBuilder();
		
		string[] file = File.ReadAllLines(pathToBuildProject + "/Classes/UnityAppController.mm");
		
		for (int idx = 0; idx < file.Length; idx++)
		{
			string line = file[idx];
			if (line.Contains("- (NSUInteger)application:(UIApplication*)application supportedInterfaceOrientationsForWindow:(UIWindow*)window"))
			{
				// Calculate the length of the method
				int subIdx = 0;
				for (subIdx = idx; subIdx < file.Length; subIdx++)
				{
					string subLine = file[subIdx];
					if (subLine.Contains("}"))
					{
						break;
					}
				}
				
				// Replace methods content
				newFile.Append(line + "\r\n");
				newFile.Append("{" + "\r\n");
				newFile.Append("\t" + PlayerPrefs.GetString ("FYBOrientationReturnValueKey") + "\r\n");
				newFile.Append("}" + "\r\n");
				
				// Move to the next method
				idx += (subIdx - idx);
				
				continue;
			}
			
			newFile.Append(line + "\r\n");
		}
		
		File.WriteAllText(pathToBuildProject + "/Classes/UnityAppController.mm", newFile.ToString());
	}
}

