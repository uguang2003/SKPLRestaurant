using UnityEngine;

using Whilefun.FPEKit;

public class DemoTapeDeck : FPEGenericSaveableGameObject
{

    private ParticleSystem mySmoke;
    private bool destroyed = false;

    void Awake()
    {

        mySmoke = gameObject.GetComponentInChildren<ParticleSystem>();

        if (!mySmoke)
        {
            Debug.LogError("DemoTapeDeck:: Cannot find ParticleSystem 'mySmoke'");
        }

        mySmoke.Stop();

    }

    public void destroyTapeDeck()
    {

        if (!destroyed)
        {
            gameObject.GetComponent<AudioSource>().Play();
            destroyed = true;
            mySmoke.Play();
        }

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, 0, 0f, destroyed);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {

        destroyed = data.SavedBool;

        if (destroyed)
        {
            mySmoke.Play();
        }
        else
        {
            mySmoke.Stop();
        }

    }

}
