using System;
using System.IO;

public static class BMUtils
{
    static public DateTime Now()
    {
        var d = DateTime.Now;
        return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0);
    }

    static public DateTime GetLastWriteTime(string path)
    {
        var d = System.IO.File.GetLastWriteTime(path);
        return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0);
    }
}
