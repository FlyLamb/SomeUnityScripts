/*
    A text utility for TMPro in unity.
    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)
    If for some weird reason you decide to use this script in your repo, feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.
*/

using System.Text.RegularExpressions;

public static class Tutil {
    // you might want to create variations of this for common objects that simplify the naming
    /// Replace templates in the text with data from the object
    public static string Apply(this string tt, object obj) {
        var matches = Regex.Matches(tt, @"\%\w+");
        foreach (Match match in matches) {
            var key = match.Value.Substring(1);
            string val;
            try {
                val = obj.GetType().GetField(key, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj).ToString();
            } catch {
                key = $"<{key}>i__Field";
                try {
                    val = obj.GetType().GetField(key, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj).ToString();
                } catch {
                    val = "";
                    DebugLog($"Could not find field for {match.Value}.");
                    var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    DebugLog("Available fields: " + fields.Length);
                    foreach (var f in fields) {
                        DebugLog($"{f.Name} = {f.GetValue(obj)}");
                    }
                }
            }
            tt = tt.Replace(match.Value, val);
        }
        return tt;
    }

    private static void DebugLog(string msg) {
#if UNITY_5_3_OR_NEWER
        UnityEngine.Debug.Log(msg);
        return;
#endif
        System.Console.WriteLine(msg);
    }

    public static string Color(this string msg, UnityEngine.Color color) {
        return $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(color)}>{msg}</color>";
    }

    public static string Size(this string msg, int size) {
        return $"<size={size}>{msg}</size>";
    }

    public static string Bold(this string msg) {
        return $"<b>{msg}</b>";
    }

    public static string Italic(this string msg) {
        return $"<i>{msg}</i>";
    }
}
