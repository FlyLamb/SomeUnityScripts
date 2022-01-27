
/*
    A build pipeline script for shell. Automatically builds and creates a github release.
    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)
    If for some weird reason you decide to use this script in your repo, feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.
    How to setup:
    Have a bash/zsh/fish/sh shell
    Install dotnet console thingy
    Install github console
    Install git
*/

using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;
using System.Dynamic;

public class BajtixBuildPipeline {

    [MenuItem("Build Pipeline/Game Build")]
    public static void GameBuild() {
        var previousVersion = PlayerSettings.bundleVersion;
        NewVersion(0, 0, 1);

        if (EditorUtility.DisplayDialog("Version change", $"Are you sure you want to change to version {PlayerSettings.bundleVersion}", "Yup", "Oof, not really")) {
            BuildProject();
        } else {
            PlayerSettings.bundleVersion = previousVersion;
        }
    }

    [MenuItem("Build Pipeline/Game Version")]
    public static void GameSub() {
        var previousVersion = PlayerSettings.bundleVersion;
        NewVersion(0, 1, 0);

        if (EditorUtility.DisplayDialog("Version change", $"Are you sure you want to change to version {PlayerSettings.bundleVersion}", "Yup", "Oof, not really")) {
            BuildProject();
        } else {
            PlayerSettings.bundleVersion = previousVersion;
        }
    }

    [MenuItem("Build Pipeline/Game Major Release")]
    public static void GameMajor() {
        var previousVersion = PlayerSettings.bundleVersion;
        NewVersion(1, 0, 0);

        if (EditorUtility.DisplayDialog("Version change", $"Are you sure you want to change to version {PlayerSettings.bundleVersion}?", "Yup", "Oof, not really")) {
            BuildProject();
        } else {
            PlayerSettings.bundleVersion = previousVersion;
        }
    }

    [MenuItem("Build Pipeline/Rebuild Last Version")]
    public static void GameRebuild() {
        var previousVersion = PlayerSettings.bundleVersion;
        if (EditorUtility.DisplayDialog("Rebuild?", $"Want to rebuild {PlayerSettings.bundleVersion}?", "Yup", "Oof, not really")) {
            BuildProject();
        } else {
            PlayerSettings.bundleVersion = previousVersion;
        }
    }

    public static void BuildProject() {
        if (BajtixBuildWindow.gitIntegration) GitCommit();
        if (BajtixBuildWindow.gitIntegration) GitTag();
        BajtixBuildWindow.SaveConfig();

        string linDir = $"Build/{PlayerSettings.bundleVersion}/lin/";
        string winDir = $"Build/{PlayerSettings.bundleVersion}/win/";
        string macDir = $"Build/{PlayerSettings.bundleVersion}/mac/";
        DateTime start = DateTime.Now;
        Directory.CreateDirectory(linDir);
        Directory.CreateDirectory(winDir);
        Directory.CreateDirectory(macDir);
        string linPath = linDir + PlayerSettings.productName;
        string winPath = winDir + PlayerSettings.productName + ".exe";
        string macPath = macDir + PlayerSettings.productName;

        if (BajtixBuildWindow.buildPipeLinux && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64)) {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, linPath, BuildTarget.StandaloneLinux64, BuildOptions.None);
            if (BajtixBuildWindow.senterIntegration) File.WriteAllText(linDir + "package.ini", "start=" + PlayerSettings.productName);
            Debug.Log("Finished LINUX");
        }
        if (BajtixBuildWindow.buildPipeWindows && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64)) {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, winPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (BajtixBuildWindow.senterIntegration) File.WriteAllText(winDir + "package.ini", "start=" + PlayerSettings.productName + ".exe");
            Debug.Log("Finished WINDOWS");
        }
        if (BajtixBuildWindow.buildPipeMac && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX)) {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, winPath, BuildTarget.StandaloneOSX, BuildOptions.None);
            if (BajtixBuildWindow.senterIntegration) File.WriteAllText(macDir + "package.ini", "start=" + PlayerSettings.productName);
            Debug.Log("Finished MAC");
        }
        DateTime end = DateTime.Now;

        var duration = (end - start).Seconds;
        BajtixBuildWindow.lastBuildTime = duration;
        BajtixBuildWindow.lastBuildDate = end;

        Debug.Log($"Builds finished ({duration}s)");

        if (BajtixBuildWindow.senterIntegration) {
            WriteManifest($"Build/{PlayerSettings.bundleVersion}/version.json", PlayerSettings.bundleVersion, BajtixBuildWindow.versionChangelog, BajtixBuildWindow.source, end);
        }
    }

    public static void NewVersion(int m, int s, int b) {
        try {
            string v = Application.version;
            var vs = v.Split('.');

            var major = vs[0];
            var sub = vs[1];
            var build = vs[2];

            build = (int.Parse(build) + b).ToString();
            if (s != 0) {
                build = b.ToString();
                sub = (int.Parse(sub) + s).ToString();
            }
            if (m != 0) {
                build = b.ToString();
                sub = s.ToString();
                major = (int.Parse(major) + m).ToString();
            }


            PlayerSettings.bundleVersion = $"{major}.{sub}.{build}";

        } catch {
            PlayerSettings.bundleVersion = "0.1.0";
        }
    }

    public static void GitCommit() {
        string command = $"add .";
        Process.Start("git", command);
        command = $"commit -m \"Build Version {PlayerSettings.bundleVersion}\"";

        var pci = new ProcessStartInfo("git", command) { RedirectStandardOutput = true, UseShellExecute = false };
        var pc = Process.Start(pci);

        var sr = pc.StandardOutput;
        Debug.Log(sr.ReadToEnd());

        pc.WaitForExit();
    }

    public static void GitTag() {
        string command = $"tag -a {PlayerSettings.bundleVersion} -m \"Version {PlayerSettings.bundleVersion}\" -f";
        var pci = new ProcessStartInfo("git", command) { RedirectStandardOutput = true, UseShellExecute = false };
        var pc = Process.Start(pci);

        var sr = pc.StandardOutput;
        Debug.Log(sr.ReadToEnd());

        pc.WaitForExit(); // ? idk why

        pc.WaitForExit();

    }

    public static string[] GitGetTags() {
        string command = $"tag";
        var pci = new ProcessStartInfo("git", command) { RedirectStandardOutput = true, UseShellExecute = false };
        var pc = Process.Start(pci);

        var sr = pc.StandardOutput;
        string output = sr.ReadToEnd();

        pc.WaitForExit();

        pc.WaitForExit();

        return output.Split(Environment.NewLine);
    }

    public static string[] GitGetCommits(string fromTag) {
        string command = $"log --pretty=\"format:%s\" HEAD...{fromTag}";
        var pci = new ProcessStartInfo("git", command) { RedirectStandardOutput = true, UseShellExecute = false };
        var pc = Process.Start(pci);

        var sr = pc.StandardOutput;
        string output = sr.ReadToEnd();

        pc.WaitForExit();

        pc.WaitForExit();

        return output.Split(Environment.NewLine);

    }

    public static void WriteManifest(string path, string version, string changelog, string source, DateTime date) {
        dynamic dataObject = new ExpandoObject();
        dataObject.version = version;
        dataObject.changelog = changelog;
        dataObject.source = source;
        dataObject.dateReleaseDate = date;
        string content = JsonConvert.SerializeObject(dataObject);
        File.WriteAllText(path, content);
    }
}


public class BajtixBuildWindow : EditorWindow {
    [MenuItem("Build Pipeline/BUILD MANAGER", false, 0)]
    public static void CreateWindow() {
        EditorWindow.CreateWindow<BajtixBuildWindow>().Show();
        LoadConfig();
    }

    public static bool buildPipeLinux, buildPipeWindows, buildPipeMac;
    public static bool gitIntegration, senterIntegration;
    public static int lastBuildTime = 0;
    public static DateTime lastBuildDate = DateTime.MinValue;
    public static string versionChangelog = "", source = "";

    private Vector2 s_pos;

    private string lastGit;

    private static bool settingsLoaded = false;



    public static void SaveConfig() {
        var data = (buildPipeLinux, buildPipeWindows, buildPipeMac, gitIntegration, lastBuildDate, lastBuildTime, versionChangelog, source, senterIntegration);
        string content = JsonConvert.SerializeObject(data);
        File.WriteAllText("bajabuildpipeline", content);
    }

    public static void LoadConfig() {
        if (!File.Exists("bajabuildpipeline")) return;
        string content = File.ReadAllText("bajabuildpipeline");
        var data = JsonConvert.DeserializeObject<dynamic>(content);
        buildPipeLinux = data.Item1;
        buildPipeWindows = data.Item2;
        buildPipeMac = data.Item3;
        gitIntegration = data.Item4;
        lastBuildDate = data.Item5;
        lastBuildTime = data.Item6;
        source = data.Item7;
        senterIntegration = data.Rest.Item2;

        settingsLoaded = true;
    }

    private void GetLastGitVersion() {
        var tags = BajtixBuildPipeline.GitGetTags();
        if (tags.Length > 1)
            lastGit = tags[^2];
    }

    public void OnGUI() {
        if (!settingsLoaded) LoadConfig();

        this.minSize = new Vector2(200, 300);


        if (position.width < minSize.x) {
            GUILayout.Label("window \nis \ntoo \nsmall");
            return;
        }


        GUILayout.Label("Build Pipeline Manager", new GUIStyle(EditorStyles.boldLabel) { fontSize = 28 });



        if (string.IsNullOrWhiteSpace(lastGit) && gitIntegration) {
            GetLastGitVersion();
        }

        if (BuildPipeline.isBuildingPlayer) {
            GUILayout.Label($"A build is in progress for {PlayerSettings.bundleVersion}");
            return;
        }

        GUILayout.Label("Build platforms", EditorStyles.boldLabel);

        if (position.width > 520)
            GUILayout.BeginHorizontal();

        GUI.enabled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        buildPipeLinux = EditorGUILayout.Toggle("Linux", buildPipeLinux);

        GUI.enabled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        buildPipeWindows = EditorGUILayout.Toggle("Windows", buildPipeWindows);

        GUI.enabled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
        buildPipeMac = EditorGUILayout.Toggle("Mac", buildPipeMac);
        GUI.enabled = true;

        if (position.width > 520)
            GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Version settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Current Version", PlayerSettings.bundleVersion);
        if (gitIntegration)
            EditorGUILayout.LabelField($"Last git Version", lastGit);


        GUILayout.Label("Changelog");
        s_pos = GUILayout.BeginScrollView(s_pos);
        versionChangelog = EditorGUILayout.TextArea(versionChangelog);

        if (gitIntegration) {
            if (GUILayout.Button("Auto from git")) {
                foreach (var s in BajtixBuildPipeline.GitGetCommits(lastGit)) {
                    if (s.Contains("Build Version") || string.IsNullOrWhiteSpace(s)) continue;
                    versionChangelog += $"+ {s} \n";
                }
            }
        }
        GUILayout.EndScrollView();


        GUILayout.FlexibleSpace();
        gitIntegration = EditorGUILayout.Toggle(new GUIContent("Enable git integration", "Enable integration with git source control"), gitIntegration);
        senterIntegration = EditorGUILayout.Toggle(new GUIContent("Enable outer integration", "Allows for further integration with some shell scripts"), senterIntegration);
        if (GUILayout.Button("New Game Build", new GUIStyle(EditorStyles.miniButton) { fontSize = 20, fixedHeight = 40 })) BajtixBuildPipeline.GameBuild();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New Version")) BajtixBuildPipeline.GameSub();
        if (GUILayout.Button("New Major Release")) BajtixBuildPipeline.GameMajor();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Rebuild Last")) BajtixBuildPipeline.GameRebuild();

        GUILayout.BeginHorizontal();
        if (lastBuildDate != DateTime.MinValue) GUILayout.Label($"Last build: {lastBuildDate:g}");
        if (lastBuildTime != 0) GUILayout.Label($"Last build time: {lastBuildTime}s");
        GUILayout.EndHorizontal();

    }
}