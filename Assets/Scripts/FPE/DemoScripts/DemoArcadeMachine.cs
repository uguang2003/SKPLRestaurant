using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Whilefun.FPEKit;

//
// DemoArcadeMachine
//
// This script demonstrates how you can tie a Dock type interaction to an 
// arbitrarily complex object. This arcade cabinet contains its own state 
// machine and mini game that is activated by the player docking onto it.
//
// Copyright 2021 While Fun Games
// http://whilefun.com
//
public class DemoArcadeMachine : FPEGenericSaveableGameObject {

    [Header("Scoreboard UI elements")]
    [SerializeField]
    private Text attractModeText = null;
    [SerializeField]
    private Text scoreText = null;
    [SerializeField]
    private Image livesLeftImage = null;
    [SerializeField]
    private Sprite[] livesLeftSprites = null;
    [SerializeField]
    private Text highScoreText = null;

    private enum eGameState
    {
        ATTRACTMODE = 0,
        WELCOME,
        GAMEPLAY,
        GAMEOVER,
        GOODBYE
    }
    private eGameState currentGameState = eGameState.ATTRACTMODE;

    [Header("Sound Effects")]
    //[SerializeField]
    //private AudioClip attractModeLoop = null;
    [SerializeField]
    private AudioClip welcomeToGame = null;
    //[SerializeField]
    //private AudioClip gameStart = null;
    [SerializeField]
    private AudioClip gameOverRegular = null;
    [SerializeField]
    private AudioClip gameOverNewHighScore = null;
    [SerializeField]
    private AudioClip goodbye = null;
    [SerializeField]
    private AudioClip frobsUp = null;
    [SerializeField]
    private AudioClip frobsDown = null;
    [SerializeField]
    private AudioClip frobImpact = null;
    [SerializeField]
    private AudioClip frobMissed = null;

    private AudioSource myAudio = null;

    // Attract Mode
    private string[] attractModeMessages = {
        "",
        "* * * * * * *",
        "Step right up!",
        "Clob some frobs!",
        "Beat the high",
        "score!"
    };

    private int attractModeMessageIndex = 0;
    private float attractModeScrollRate = 1.5f;
    private float attractModeScrollCounter = 0.0f;

    // Welcome and Goodbye
    private float welcomeDuration = 2.5f;
    private float welcomeCounter = 0.0f;
    private float goodbyeDuration = 1.0f;
    private float goodbyeCounter = 0.0f;

    // High Score
    private int highScore = 5;
    private int currentScore = 0;

    // Frobs to Clob
    [Header("Frobs")]
    [SerializeField]
    private GameObject[] frobs = null;
    private Vector3[] frobUpPositions = null;
    private Vector3[] frobDownPositions = null;

    // Buttons and things
    private GameObject goButton = null;

    // Make first frob the middle one
    private int[,] currentFrobsToClob = new int[,]
    {
        {0,0,0,1,0},
        {0,1,0,0,0},
        {0,1,0,1,0},
        {0,0,0,0,1},
        {1,1,1,0,0},
        {1,0,1,1,0},
        {1,0,1,0,1},
        {1,1,0,0,0},
        {1,0,1,0,0},
        {1,0,0,1,0},
        {0,0,0,1,0},
        {0,0,0,1,0},
        {1,0,1,0,1},
        {1,0,0,0,1},
        {1,1,0,1,1},
        {1,1,1,0,1},
        {1,1,0,1,0},
        {0,1,1,0,0}
    };

    private int frobClobIndex = 0;
    private bool[] frobsClobbedThisRound = null;
    private int frobsMissed = 0;
    private int frobsMissedForGameOver = 5;
    private float gameOverDuration = 2.5f;
    private float gameOverCounter = 0.0f;

    private enum eFrobState
    {
        DOWN = 0,
        UP
    }
    private eFrobState currentFrobState = eFrobState.DOWN;

    private float frobPopupInterval = 0.5f;
    private float frobPopupCounter = 0.0f;
    private float frobHideTime = 1.0f;
    private float frobHideCounter = 0.0f;

    void Start()
    {

        myAudio = gameObject.GetComponent<AudioSource>();

        if (!myAudio)
        {
            Debug.LogError("DemoArcadeMachine:: Missing Audio Source!");
        }

        if (frobs.Length != 5)
        {
            Debug.LogError("DemoArcadeMachine:: Looks like the frobs were not assigned in the inspector, or you assigned too many (there should be 5)!");
        }

        frobUpPositions = new Vector3[frobs.Length];
        frobDownPositions = new Vector3[frobs.Length];
        frobsClobbedThisRound = new bool[frobs.Length];

        for (int f = 0; f < frobs.Length; f++)
        {

            frobUpPositions[f] = frobs[f].transform.localPosition;
            frobs[f].transform.Translate(new Vector3(0.0f, 0.0f, -0.1f));
            frobDownPositions[f] = frobs[f].transform.localPosition;

            frobs[f].gameObject.GetComponent<BoxCollider>().enabled = false;

        }

        goButton = transform.Find("GoButton").gameObject;
        if (!goButton)
        {
            Debug.LogError("DemoArcadeMachine:: Cannot find GoButton");
        }

        startAttractMode();

    }

    void Update()
    {

        if (currentGameState == eGameState.ATTRACTMODE)
        {

            attractModeScrollCounter -= Time.deltaTime;

            if (attractModeScrollCounter <= 0.0f)
            {

                attractModeScrollCounter = attractModeScrollRate;

                attractModeMessageIndex++;
                if (attractModeMessageIndex >= attractModeMessages.Length)
                {
                    attractModeMessageIndex = 0;
                    attractModeText.text = "high score: " + highScore;
                }
                else
                {
                    attractModeText.text = attractModeMessages[attractModeMessageIndex];
                }

            }

        }
        else if (currentGameState == eGameState.WELCOME)
        {

            welcomeCounter -= Time.deltaTime;

            if(welcomeCounter <= 0.0f)
            {
                currentGameState = eGameState.GAMEPLAY;
                frobPopupCounter = frobPopupInterval;
                currentFrobState = eFrobState.DOWN;

                attractModeText.text = "";
                updateScoreboard();
                highScoreText.text = "High Score: " + highScore;

            }

        }
        else if (currentGameState == eGameState.GAMEPLAY)
        {

            if(currentFrobState == eFrobState.DOWN)
            {

                frobPopupCounter -= Time.deltaTime;

                if (frobPopupCounter <= 0.0f)
                {

                    // Move frobs up //
                    for (int f = 0; f < frobs.Length; f++)
                    {

                        if (currentFrobsToClob[frobClobIndex, f] == 1)
                        {
                            frobs[f].transform.localPosition = frobUpPositions[f];
                            frobs[f].gameObject.GetComponent<BoxCollider>().enabled = true;
                        }

                        frobsClobbedThisRound[f] = false;
                        myAudio.PlayOneShot(frobsUp);

                    }


                    frobHideCounter = frobHideTime;
                    currentFrobState = eFrobState.UP;

                }

            }
            else if(currentFrobState == eFrobState.UP)
            {
                
                frobHideCounter -= Time.deltaTime;

                if (frobHideCounter <= 0.0f)
                {

                    int frobsClobbed = 0;
                    int frobsMissedThisRound = 0;

                    // Move frobs down //
                    for (int h = 0; h < frobs.Length; h++)
                    {

                        // If we were supposed to clob this frob, do book keeping for score, and move it back down for next round
                        if (currentFrobsToClob[frobClobIndex, h] == 1)
                        {

                            if (frobsClobbedThisRound[h])
                            {
                                frobsClobbed++;
                            }
                            else
                            {
                                frobsMissed++;
                                frobsMissedThisRound++;
                                frobs[h].transform.localPosition = frobDownPositions[h];
                                frobs[h].gameObject.GetComponent<BoxCollider>().enabled = false;
                            }

                        }

                    }

                    if(frobsMissedThisRound > 0)
                    {
                        myAudio.PlayOneShot(frobMissed);
                    }

                    myAudio.PlayOneShot(frobsDown);
                    updateScoreboard();

                    if(frobsMissed >= frobsMissedForGameOver)
                    {
                        gameOver();
                    }
                    else
                    {

                        // If player made it through the loop, reset and make it go faster.
                        frobClobIndex++;
                        if (frobClobIndex >= currentFrobsToClob.GetLength(0))
                        {

                            frobClobIndex = 0;
                            frobPopupInterval *= 0.8f;
                            Debug.Log("TODO: play a fun sound here");
                            addToScore(10);

                        }

                        frobPopupCounter = frobPopupInterval;
                        currentFrobState = eFrobState.DOWN;
                    }

                }

            }

        }
        else if (currentGameState == eGameState.GAMEOVER)
        {

            gameOverCounter -= Time.deltaTime;

            if (gameOverCounter <= 0.0f)
            {
                endGame();
            }

        }
        else if (currentGameState == eGameState.GOODBYE)
        {

            goodbyeCounter -= Time.deltaTime;

            if(goodbyeCounter <= 0.0f)
            {
                startAttractMode();
            }

        }

    }

    public void registerClobbedFrob(int index)
    {

        addToScore(1);
        frobs[index].transform.localPosition = frobDownPositions[index];
        frobsClobbedThisRound[index] = true;
        frobs[index].gameObject.GetComponent<BoxCollider>().enabled = false;
        myAudio.PlayOneShot(frobImpact);

    }

    private void addToScore(int amount)
    {
        currentScore += amount;
    }

    private void updateScoreboard()
    {
        scoreText.text = "Score: " + currentScore;
        int livesLeftIndex = Mathf.Max(0, (frobsMissedForGameOver - frobsMissed));
        livesLeftImage.enabled = true;
        livesLeftImage.overrideSprite = livesLeftSprites[livesLeftIndex];
    }

    public void startGame()
    {

        if (currentGameState == eGameState.ATTRACTMODE)
        {

            currentGameState = eGameState.WELCOME;
            attractModeText.text = "GET READY!";
            myAudio.PlayOneShot(welcomeToGame);
            welcomeCounter = welcomeDuration;
            currentScore = 0;
            frobsMissed = 0;
            frobClobIndex = 0;
            goButton.SetActive(false);

        }
        else
        {
            Debug.Log("Can't start game, not in Welcome mode");
        }

    }

    private void gameOver()
    {

        if(currentScore > highScore)
        {

            highScore = currentScore;
            myAudio.PlayOneShot(gameOverNewHighScore);
            attractModeText.text = "NEW HIGH SCORE!";

        }
        else
        {

            myAudio.PlayOneShot(gameOverRegular);
            attractModeText.text = "GAME OVER :(";
            livesLeftImage.enabled = false;

        }

        currentGameState = eGameState.GAMEOVER;
        gameOverCounter = gameOverDuration;

    }

    public void endGame(bool sayGoodbye = false)
    {

        currentGameState = eGameState.GOODBYE;

        if (sayGoodbye)
        {
            myAudio.PlayOneShot(goodbye);
        }

        goodbyeCounter = goodbyeDuration;

        for (int h = 0; h < frobs.Length; h++)
        {
            frobs[h].transform.localPosition = frobDownPositions[h];
        }

    }

    public void startAttractMode()
    {

        currentGameState = eGameState.ATTRACTMODE;
        attractModeText.text = attractModeMessages[1];
        attractModeScrollCounter = attractModeScrollRate;
        highScoreText.text = "High Score: " + highScore;
        livesLeftImage.enabled = false;
        scoreText.text = "";
        goButton.SetActive(true);

    }

    public override FPEGenericObjectSaveData getSaveGameData()
    {
        return new FPEGenericObjectSaveData(gameObject.name, highScore, 0f, false);
    }

    public override void restoreSaveGameData(FPEGenericObjectSaveData data)
    {
        highScore = data.SavedInt;
        highScoreText.text = "High Score: " + highScore;
    }

}
