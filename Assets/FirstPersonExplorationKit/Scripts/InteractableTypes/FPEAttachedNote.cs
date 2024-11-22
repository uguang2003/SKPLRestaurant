using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEAttachedNote
    // This script allows you to attach notes to any other Interactable type
    // object. It is useful for adding text content to the player's inventory 
    // screen for later reference. For example, in a tutorial you might attach 
    // a note to the first object of a given type to explain to the player what 
    // it is and how it works. Later, the player can recall that information by
    // reading the note in their inventory screen.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEAttachedNote : MonoBehaviour
    {

        [SerializeField]
        private string noteTitle = "Note Title Here";
        public string NoteTitle {
            get { return noteTitle; }
        }

        [SerializeField]
        [TextArea]
        private string noteBody = "Note Body Here";
        public string NoteBody {
            get { return noteBody; }
        }

        private bool collected = false;
        public bool Collected {
            get { return collected; }
        }

        public FPENoteEntry collectNote()
        {

            FPENoteEntry note = null;

            if (!collected)
            {

                collected = true;
                note = new FPENoteEntry(noteTitle, noteBody);

            }

            return note;

        }

        public FPEAttachedNoteSaveData getSaveGameData()
        {
            return new FPEAttachedNoteSaveData(gameObject.name, collected);
        }

        public void restoreSaveGameData(FPEAttachedNoteSaveData data)
        {
            collected = data.Collected;
        }

    }

}