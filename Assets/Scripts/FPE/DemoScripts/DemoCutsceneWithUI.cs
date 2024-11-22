using UnityEngine;
using UnityEngine.EventSystems;
using Whilefun.FPEKit;

public class DemoCutsceneWithUI : FPEGenericSaveableGameObject {

    private GameObject cutsceneCanvas = null;
    private GameObject myButton;
    private bool cutsceneHasPlayed = false;

    private void Awake()
    {

        cutsceneCanvas = transform.parent.Find("CutsceneCanvas").gameObject;
        myButton = cutsceneCanvas.transform.Find("Background/OKButton").gameObject;
        cutsceneCanvas.SetActive(false);

    }

    private void Start()
    {
        gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {

        // We check timeSinceLevelLoad to handle edge case where player quick saves, enters trigger, dismissed cutscene, then immediately quick loads.
        if (other.CompareTag("Player") && Time.timeSinceLevelLoad > 1.0f)
        {
            startCutscene();
        }

    }

    public void startCutscene()
    {

        if (cutsceneHasPlayed == false)
        {

            cutsceneHasPlayed = true;
            FPEInteractionManagerScript.Instance.BeginCutscene(true);
            cutsceneCanvas.SetActive(true);
            FPEEventSystem.Instance.gameObject.GetComponent<EventSystem>().SetSelectedGameObject(myButton);

        }

    }

    public void stopCutscene()
    {
        FPEInteractionManagerScript.Instance.EndCutscene(true);
        cutsceneCanvas.SetActive(false);
    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, 0, 0f, cutsceneHasPlayed);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {
        cutsceneHasPlayed = data.SavedBool;
    }

}
