namespace Whilefun.FPEKit
{

    //
    // FPEAudioDiaryData
    // A basic data container for passing Audio Diary data back and 
    // forth between Game Systems and the UI for display to the player.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public class FPEAudioDiaryData
    {

        private string _diaryTitle = "";

        public string DiaryTitle {
            get { return _diaryTitle; }
        }

        
        public FPEAudioDiaryData(string diaryTitle)
        {
            _diaryTitle = diaryTitle;
        }

    }

}