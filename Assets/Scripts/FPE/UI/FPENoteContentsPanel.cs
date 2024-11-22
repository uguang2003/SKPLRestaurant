using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Whilefun.FPEKit
{

    //
    // FPENoteContentsPanel
    // A simple panel with no real interaction. Just a data display element.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPENoteContentsPanel : MonoBehaviour
    {

        private Text noteTitle = null;
        private Text noteBody = null;

        void Awake()
        {

            noteTitle = transform.Find("NoteTitle").GetComponent<Text>();
            noteBody = transform.Find("NoteBody").GetComponent<Text>();

            if (!noteTitle || !noteBody)
            {
                Debug.LogError("FPENoteContentsPanel:: NoteTitle (Text) or NoteBody (Text) are missing! Did you rename or remove them?");
            }

        }

        public void displayNoteContents(string title, string body)
        {
            noteTitle.text = title;
            noteBody.text = body;
        }

        public void clearNoteContents()
        {
            noteTitle.text = "";
            noteBody.text = "";
        }

    }

}