using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Whilefun.FPEKit
{

    //
    // FPENoteEntry
    // This class holds note data and allows that data to be 
    // passed back and forth to various game systems without the
    // overhead of a monobehaviour.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    [Serializable]
    public class FPENoteEntry {

        private string noteTitle = "Note Title Here";
        public string NoteTitle {
            get { return noteTitle; }
        }

        private string noteBody = "Note Body Here";
        public string NoteBody {
            get { return noteBody; }
        }

        public FPENoteEntry(string title, string body)
        {

            noteTitle = title;
            noteBody = body;

        }

    }

}
