/*
    A simple script to change the loading text to a random one - a fairly common thing a lot of games do
    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)
    If for some reason you decide to use this script feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.
    How to setup:
    Attach onto an object and select the text object. Don't forget to input the texts list
*/


using System.Collections.Generic;
using UnityEngine;

public class RandomLoader : MonoBehaviour {
    public List<string> texts;
    public TMPro.TextMeshProUGUI text;

    public string GetRandom() {
        return texts[Random.Range(0, texts.Count)];
    }

    private void OnEnable() {
        text.text = GetRandom();
    }
}
