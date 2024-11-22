using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Whilefun.FPEKit
{

    // This script allows you to play a preview of the FPESimpleSoundbank from the Inspector, in order to adjust the sounds
    [CustomEditor(typeof(FPESoundBank), true)]
    public class FPEAudioEventEditor : Editor
    {

        [SerializeField]
        private AudioSource _previewer;

        public void OnEnable()
        {
            _previewer = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        public void OnDisable()
        {
            DestroyImmediate(_previewer.gameObject);
        }

        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview"))
            {
                ((FPESoundBank)target).Play(_previewer);
            }
            EditorGUI.EndDisabledGroup();

        }

    }

}