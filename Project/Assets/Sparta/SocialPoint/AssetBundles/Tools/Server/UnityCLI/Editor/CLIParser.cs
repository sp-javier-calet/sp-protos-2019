using System.Collections;

public class CLIParser
{
	public static Hashtable parse (string[] args)
	{
		Hashtable htArgs = new Hashtable ();
		int index;

		foreach(string arg in args)
		{
			index = arg.IndexOf("=");
			if(index > 0)
			{
				htArgs.Add(arg.Substring(0, index), arg.Substring(index+1));
			}
		}

		return htArgs;
	}
}

