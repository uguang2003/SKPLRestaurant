
namespace Whilefun.FPEKit
{

    //
    // FPEGenericSaveableInterface
    // This interface provides a mandatory API contract for all classes 
    // that should be generically saveable. Each such class must implement 
    // its own save and load logic, according to its requirements.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    interface FPEGenericSaveableInterface
    {

        FPEGenericObjectSaveData getSaveGameData();
        void restoreSaveGameData(FPEGenericObjectSaveData data);

    }

}
