namespace Whilefun.FPEKit
{

    //
    // FPENoteData
    // A basic data container for passing Note data back and 
    // forth between Game Systems and the UI for display to the player.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPENoteData
    {

        private string _noteTitle = "";
        public string NoteTitle {
            get { return _noteTitle; }
        }

        private string _noteBody = "";
        public string NoteBody {
            get { return _noteBody; }
        }

        public FPENoteData(string noteTitle, string noteBody)
        {
            _noteTitle = noteTitle;
            _noteBody = noteBody;
        }

    }

}