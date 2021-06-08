/*
    Dumb, but simple and quick script to get console arguments. Should work without the UnityEngine lib if you were to remove some lines 
    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)

    This script may not be the best, I might improve it in the future, but I just needed to get it working and I've decided to share it in case someone needs a quck solution.
    If for some weird reason you decide to use this script feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.

    How to use:
    When you launch the app from the console you can pass some arguments that can be read by scripts by referencing ConArg.Get() (and other get methods)
    The argument format is strict: VAR [space] VALUE
    Note that they are separated by spaces, so they cannot be used for variable names or values. (a little dumb, but its just a quick script, ok?)
    If you use dashes in the argument name (probably good practice), you have to use the exact same name while getting them, ex.
    --cool_arg 69
    ConArg.Get("--cool_arg") // returns 69
*/

using System.Collections.Generic;
using UnityEngine; // can be removed if you get rid of the Debug.Logs and Application.streamingAssetsPath
using System.IO;
using System;

public class ConArg {

    /* parsing order:
        1. defaults
        2. console.args file (default is from StreamingAssets/console.args)
        3. the actual args
    */ 

    private static string consoleArgsFile = Application.streamingAssetsPath + "/console.args";

    private static string[] defaults = new string[] {
        //put the defaults here
    };

    public static Dictionary<string, string> args; // maybe could be private? i dont know, maybe im gonna need to access it somewhere.

    
    ///<summary>Loads the arguments from the sources and parses them, adding them into the dictionary</summary>
    private static void LoadArgs() {
        args = new Dictionary<string, string>();
        
        Parse(defaults, "Hardcoded defaults", false);

        if(File.Exists(Application.streamingAssetsPath + "/console.args")) {
            Parse(File.ReadAllText(consoleArgsFile).Replace('\n',' ').Split(' '), "console.args", false);
        }

        Parse(System.Environment.GetCommandLineArgs(), "Console Arguments", true);
    }

    ///<summary>Return the argument value, if it doesn't exist throws an exception</summary>
    public static string Get(string var) {
        if(args == null) LoadArgs();

        if(args.ContainsKey(var)) {
            return args[var];
        } else {
            throw new System.Exception($"Argument '{var}' does not exist.");
        }
    }

    ///<summary>Return the argument value, if it does not exist returns default</summary>
    public static string SGet(string var, string df) {
        if(args == null) LoadArgs();
        
        if(args.ContainsKey(var)) {
            return args[var];
        } else {
            return df;
        }
    }

    ///<summary>Parses the given arguments and adds them to the dictionay</summary>
    public static void Parse(string[] argsArray, string whatAreWeParsing = "Unknown args", bool doOffset = false) {
        
        List<string> vars = new List<string>();
        List<string> values = new List<string>();

        #if UNITY_STANDALONE || UNITY_EDITOR
        Debug.Log($"Parsing args: {whatAreWeParsing}"); //UNITY ONLY
        if(IsHeadlessMode()) 
        #endif
        Console.WriteLine($"Parsing args: {whatAreWeParsing}");

        int offset = doOffset ? 1 : 0; // offset by one for the commandline - the arg[0] is the exe name, which is unnecessary.

        try {
            for (int i = offset; i < argsArray.Length; i++) {
                if((i + offset) % 2 == 0) {
                    vars.Add(argsArray[i].Trim());
                } else {
                    values.Add(argsArray[i].Trim());
                }
            }
            
            for (int i = 0; i < vars.Count; i++) {
                if(args.ContainsKey(vars[i])) {
                    args[vars[i]] = values[i];
                } else {
                    args.Add(vars[i], values[i]);        
                }
                Debug.Log(vars[i] + " = " + args[vars[i]]);
            }
        } catch (Exception e) {
            #if UNITY_STANDALONE || UNITY_EDITOR
            Debug.LogError($"Console argument parsing went wrong while parsing '{whatAreWeParsing}'!");
            Debug.LogError(e.Message + "; " + e.StackTrace);
            if(IsHeadlessMode()) 
            #endif
            Console.WriteLine($"Console argument parsing went wrong while parsing '{whatAreWeParsing}'! EXCEPTION: {e.Message} {e.StackTrace}");
        }
    }

    #if UNITY_STANDALONE || UNITY_EDITOR
    // UNITY RELATED CODE
    public static bool IsHeadlessMode() {
        return UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
    }
    #endif
    

    #region Gets

    public static int GetInt(string var) {
        var w = Get(var);
        int result;
        if(int.TryParse(w, out result)) {
            return result; 
        }
        else throw new System.Exception($"Argument '{var}' type incorrect. Expected integer, got '{w}'");
    }

    public static float GetFloat(string var) {
        var w = Get(var);
        float result;
        if(float.TryParse(w, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result)) {
            return result; 
        }
        else throw new System.Exception($"Argument '{var}' type incorrect. Expected float, got '{w}'");
    }

    public static bool GetBool(string var) {
        var w = Get(var);
        bool result;
        if(bool.TryParse(w, out result)) {
            return result; 
        }
        else throw new System.Exception($"Argument '{var}' type incorrect. Expected boolean, got '{w}'");
    }

    #endregion
}
