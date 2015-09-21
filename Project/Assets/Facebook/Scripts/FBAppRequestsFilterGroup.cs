using System;
using System.Collections;
using System.Collections.Generic;

public sealed class FBAppRequestFilterGroup: Dictionary<string, object>
{
  public FBAppRequestFilterGroup(string name, List<string> user_ids)
  {
    this["name"] = name;
    this["user_ids"] = user_ids;
  }
}
