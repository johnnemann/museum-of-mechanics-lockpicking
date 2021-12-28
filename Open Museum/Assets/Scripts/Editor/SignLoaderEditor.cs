using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SignLoader))]
public class SignLoaderEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        SignLoader signLoader = (SignLoader)target;
        if (GUILayout.Button("Load Lockpicking Signs"))
        {
            signLoader.LoadSignData();
            signLoader.AssignSignData();
        }

        if (GUILayout.Button("Load Info Signs"))
        {
            signLoader.LoadInfoData();
            signLoader.AssignInfoData();
        }
    }
}