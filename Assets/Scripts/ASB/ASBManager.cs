using Fusion;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton network behavoir that manages the game and is managed by the shared mode master client.
/// </summary>
public class ASBManager : NetworkBehaviour, IStateAuthorityChanged
{
    [Tooltip("The current task stimuli.")]
    public List<GameObject> asbStimuliList;

    [Tooltip("Container for the stimulus element")]
    public GameObject stimuliContainerObj = null;

    [Tooltip("Stimuli obj position right")]
    public Transform stimuliObjRight;

    [Tooltip("Stimuli obj position left")]
    public Transform stimuliObjLeft;

    #region Networked Properties
    [Networked, Tooltip("Timer used for showing stimuli and transitioning between different states.")]
    public TickTimer stimTimer { get; set; }

    [Networked, Tooltip("Timer used for the whole game.")]
    public TickTimer gameTimer { get; set; }

    [Networked, Tooltip("The length of the timer, which corresponds to the duration of the game.")]
    public float gameTimerLength { get; set; }

    [Networked, Tooltip("The time between stimuli.")]
    public float stimTimerLength { get; set; }

    // TODO modify this with the cycling speed
    [Networked, Tooltip("The target speed.")]
    public float targetSpeed { get; set; }

    [Tooltip("The index of the current stimulus.  -1 means no stimulus is being currently shown.")]
    [Networked, OnChangedRender(nameof(UpdateCurrentStimulus))]
    public int CurrentStimulus { get; set; } = -1;

    [Tooltip("The number of stimuli shown so far.")]
    [Networked, OnChangedRender(nameof(UpdateStimuliShownText))]
    public int StimuliShown { get; set; } = 0;

    [Networked, Tooltip("The current stimuli position.")]
    public int currStimuliPos { get; set; }

    [Networked, Tooltip("Random index for Prefabs")]
    public int randomIndex { get; set; }

    [Tooltip("The current state of the game.")]
    [Networked, OnChangedRender(nameof(OnASBGameStateChanged))]
    public ASBStateGame GameState { get; set; } = ASBStateGame.Intro;

    [Networked, Tooltip("Type of the Prefab")]
    public string currTargetType { get; set; }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealFeedbackRPC()
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        Debug.Log("Received DealFeedbackRPC on StateAuthority, modifying Networked variable Game state to feedback");
        GameState = ASBStateGame.ShowFeedback;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void ActivateStimulusRPC()
    {
        Debug.LogError("Received ActivateStimulusRPC on StateAuthority, spawning network object");

        Transform targetParent;
        string posString;

        if (currStimuliPos == 0)
        {
            targetParent = stimuliObjRight;
            posString = "RIGHT";
        }
        else
        {
            targetParent = stimuliObjLeft;
            posString = "LEFT";
        }

        Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + "and target position is " + posString);
        
        var selectedTargetPrefab = targetPrefabs[randomIndex];

        if(selectedTargetPrefab != null)
         {
                NetworkObject currTarget = Runner.Spawn(selectedTargetPrefab, targetParent.position);
                currTarget.gameObject.SetActive(true);
                currTargetType = currTarget.gameObject.tag;
         }
        Debug.LogError("Current Target: " + currTargetType);
    }

    /// <summary>
    /// A randomized array of each question index.
    /// </summary>
    /// TODO use this to randomize the current stimuli
    [Networked, Capacity(2)]
    public NetworkArray<int> randomizedStimuliList => default;

    #endregion

    #region UI ELEMENTS

    /// <summary>
    /// Question, answer, and answer highlights
    /// </summary>
    public TextMeshProUGUI stimulus;
    public TextMeshProUGUI gameTimerText, stimTimerText;
    public Image[] answerHighlights;

    /// <summary>
    /// Text message shown when the game changes state.
    /// </summary>
    public TextMeshProUGUI asbMessage;

    public TextMeshProUGUI stimuliShownText;

    [Tooltip("Button displayed to leave the game after a round ends.")]
    public GameObject leaveGameBtn;

    [Tooltip("Button displayed, only to the master client, to start a new game.")]
    public GameObject startNewGameBtn;


    public NetworkObject[] targetPrefabs;

    #endregion

    [Header("Game Rules")]
    [Tooltip("The maximum number of stimuli.")]
    [Min(1)]
    [SerializeField] public float maxStimuli = 5; // this could be remove in the final version because the game stops when the duration expires

    [Tooltip("The amount of time the stimulus will be shown.")]
    [SerializeField] public float stimulusLength = 15;

    [Tooltip("The minimum number of points earned for getting a question correct")]
    [SerializeField] public int pointsPerStimulus;

    #region SFX
    [Header("SFX Audio Sources")]
    [SerializeField, Tooltip("AudioSource played when the local player gets an answer correct.")]
    private AudioSource _correctSFX;

    [SerializeField, Tooltip("AudioSource played when the local player gets an answer incorrect.")]
    private AudioSource _incorrectSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _confirmSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _errorSFX;
    #endregion

    /// <summary>
    /// Has a asb manager been made; set to true on spawn and false on despawn
    /// </summary>
    public static bool ASBManagerPresent { get; private set; } = false;

    /// <summary>
    /// The different states of the ABS game.  Made as a byte since there are not that many.
    /// </summary>
    public enum ASBStateGame : byte
    {
        Intro = 0,
        ShowStimulus = 1,
        ShowFeedback = 2,
        GameOver = 3,
        NewRound = 4,
    }

    public override void Spawned()
    {
        // Disallows players from joining once the game is started.
        if (Runner.IsSharedModeMasterClient)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
        }

        //// If we have state authority, we set an intro time and randomized the question list.
        if (HasStateAuthority)
        {
            // Sets an initial intro timer
            // The initial timer for the game is only 3 seconds.
            stimTimerLength = 3;
            stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);

            gameTimer = TickTimer.CreateFromSeconds(Runner, gameTimerLength);

            ShuffleStimuli();
        }

        ASBManagerPresent = true;

        FusionConnector.Instance?.SetPregameMessage(string.Empty);

        OnASBGameStateChanged();
        UpdateCurrentStimulus();
        UpdateStimuliShownText();
    }

    /// <summary>
    /// Shuffles the questions around.
    /// </summary>
    private void ShuffleStimuli()
    {
        Debug.Log("SHUFFLING STIMULI!");

        // Creates a temp list, adding every index avaiable
        List<int> stimuliAvailable = new List<int>();
        for (int i = 0; i < asbStimuliList.Count; i++)
            stimuliAvailable.Add(i);

        // Sets the fifty questions
        for (int i = 0; i < randomizedStimuliList.Length; i++)
        {
            int c = stimuliAvailable[Random.Range(0, stimuliAvailable.Count)];
            stimuliAvailable.Remove(c);
            randomizedStimuliList.Set(i, c);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ASBManagerPresent = false;
    }

    /// <summary>
    /// Note, was a bit confused because only the shared mode master client seems to run this.
    /// Game worked fine, but unsure if every client should be running this in their simulation or not.
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        // We check to see if any player has chosen answer, and if so, go to the show feedback state. // ACTIVE check if we are colliding a target

        if (!gameTimer.Expired(Runner))
        {
            if (stimTimer.Expired(Runner)) // if when the timer expires we are in the show stimulus state, it means that no answer has been chosen
            {

                if (StimuliShown < maxStimuli)
                {
                    currStimuliPos = Random.Range(0, 2);
                    randomIndex = Random.Range(0, targetPrefabs.Length);


                    ASBPlayer.LocalPlayer.ChosenAnswer = -1;
                    CurrentStimulus++;

                    stimTimerLength = stimulusLength;
                    stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);
                    GameState = ASBStateGame.ShowStimulus;
                    StimuliShown++;
                }
                else
                {
                    stimTimer = TickTimer.None;
                    gameTimer = TickTimer.None;
                    GameState = ASBStateGame.GameOver;
                }
            }

        }
        else // game over after the final duration has been reached
        {
            stimTimer = TickTimer.None;
            gameTimer = TickTimer.None;
            GameState = ASBStateGame.GameOver;
        }

    }

    /// <summary>
    /// Called when a player picks an answer.
    /// </summary>
    /// <param name="index"></param>
    public void PickAnswer(int index)
    {

        // If we are in the question state and the local player has not picked an answer...
        if (GameState == ASBStateGame.ShowStimulus)
        {
            // TODO modify here by checking whether the answer is correct or not
            // For now, if Chosen Answer is less than 0, this means they haven't picked an answer.
            // We don't allow players to pick new answers at this time.
            if (ASBPlayer.LocalPlayer.ChosenAnswer < 0)
            {
                _confirmSFX.Play();

                ASBPlayer.LocalPlayer.ChosenAnswer = index;
                //Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and my answer is " + ASBPlayer.LocalPlayer.ChosenAnswer.ToString());

                GameState = ASBStateGame.ShowFeedback;
            }
            //else
            //{
            //    _errorSFX.Play();
            //}
        }

    }

    /// <summary>
    /// Update function that updates the timer visual.
    /// </summary>
    public void Update()
    {
        //Debug.LogError("current state game " + GameState.ToString());
        //Debug.LogError("Update: I am " + ASBPlayer.LocalPlayer.PlayerName + " and has authority is " + HasStateAuthority.ToString());
        // Updates the timer visual
        float? stimRemainingTime = stimTimer.RemainingTime(Runner);
        if (stimRemainingTime.HasValue)
        {
            stimTimerText.text = Mathf.Round(stimRemainingTime.Value).ToString();
        }
        else
        {
            stimTimerText.text = "0";
        }

        float? gameRemainingTime = gameTimer.RemainingTime(Runner);

        if (gameRemainingTime.HasValue)
        {
            gameTimerText.text = Mathf.Round(gameRemainingTime.Value).ToString();
        }
        else
        {
            gameTimerText.text = "0";
        }
    }

    private void OnASBGameStateChanged()
    {
        // If showin an answer, we show which players got the question correct and increase their score.
        if (GameState == ASBStateGame.Intro || GameState == ASBStateGame.NewRound)
        {
            // Resets the score for the player
            ASBPlayer.LocalPlayer.Score = 0;

            //ASBPlayer.LocalPlayer.Expression = ASBPlayer.AvatarExpressions.Neutral;

            asbMessage.text = GameState == ASBStateGame.Intro ? "Select The Correct Answer\nStarting Game Soon" : "New Game Starting Soon!";

            targetSpeed = 20;
            stimuliContainerObj.GetComponent<StimuliMovement>().Move(targetSpeed);

            //endGameObject.Hide();
        }
        else if (GameState == ASBStateGame.ShowFeedback)
        {
            OnGameStateShowFeedback();

            //endGameObject.Hide();
        }
        else if (GameState == ASBStateGame.GameOver)
        {
            OnGameStateGameOver();
        }
        else if (GameState == ASBStateGame.ShowStimulus)
        {

            asbMessage.text = string.Empty;
            // TODO delete this if not appropriate
            //endGameObject.Hide();
        }

        leaveGameBtn.SetActive(GameState == ASBStateGame.GameOver);
        startNewGameBtn.SetActive(GameState == ASBStateGame.GameOver && Runner.IsSharedModeMasterClient == true);
    }

    private void OnGameStateGameOver()
    {
        Debug.Log("Game over");
    }

    private void OnGameStateShowFeedback()
    {
        asbMessage.text = string.Empty;

        // If the player picks the correct answer, their score increases
        if (ASBPlayer.LocalPlayer.ChosenAnswer == 0)
        {
            //ASBPlayer.LocalPlayer.Expression = TriviaPlayer.AvatarExpressions.Happy_CorrectAnswer;
            
            // Insert an if for park or city
            int scoreValue = 0;
            if (currTargetType == "Park")
            {
                scoreValue -= 1 ;
            }
            else if (currTargetType == "City")
            {
                scoreValue += 1;
            }

            // Gets the score pop up and toggles it.
            var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
            scorePopUp.Score = scoreValue;
            scorePopUp.Toggle = !scorePopUp.Toggle;
            ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;

            ASBPlayer.LocalPlayer.Score += scoreValue;

            _correctSFX.Play();
        }
        else
        {
            // Gets score value and toggles it
            var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
            scorePopUp.Score = 0;
            scorePopUp.Toggle = !scorePopUp.Toggle;
            ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;
            //TODO Modify here for feedback score
            //ASBPlayer.LocalPlayer.Expression = ASBPlayer.AvatarExpressions.Angry_WrongAnswer;

            _incorrectSFX.Play();
        }
    }

    private void UpdateCurrentStimulus()
    {
        // If we are asking a question, we set the answers.
        if (CurrentStimulus >= 0)
        {
            //stimulusObj.SetActive(true);


            //stimulus.text = asbStimuliList[stimulusIndex].name;
            if (HasStateAuthority)
            {
                Debug.LogError("ActivateStimulus: I am " + ASBPlayer.LocalPlayer.PlayerName + " and has authority is " + HasStateAuthority.ToString());
                ActivateStimulusRPC();
            }

            // Clears the trivia message
            asbMessage.text = string.Empty;

            // Deisgnate that the local player has not chosen an answer yet.
            ASBPlayer.LocalPlayer.ChosenAnswer = -1;

            //Change the game state
            if (HasStateAuthority)
            {
                GameState = ASBStateGame.ShowStimulus;
            }
        }

        // We hide the question element in case a player late joins at the end of the game.
        if (GameState != ASBStateGame.ShowFeedback && GameState != ASBStateGame.ShowStimulus)
        {
            //stimulusObj.SetActive(false);
            foreach (GameObject stim in asbStimuliList)
            {
                stim.SetActive(false);
            }
        }
    }

    private void UpdateStimuliShownText()
    {
        if (StimuliShown == 0)
            stimuliShownText.text = "";
        else
            stimuliShownText.text = "Stimuli shown: " + StimuliShown;
    }

    public async void LeaveGame()
    {
        await Runner.Shutdown(true, ShutdownReason.Ok);

        FusionConnector fc = GameObject.FindObjectOfType<FusionConnector>();
        if (fc)
        {
            fc.mainMenuObject.SetActive(true);
            fc.mainGameObject.SetActive(false);
        }
    }

    public void StartNewGame()
    {
        if (HasStateAuthority == false)
            return;

        GameState = ASBStateGame.NewRound;

        StimuliShown = 0;

        // Sets an initial intro timer
        stimTimerLength = 3f;
        stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);

        gameTimer = TickTimer.CreateFromSeconds(Runner, gameTimerLength);

        // Player movement isMoving true or false
    }

    public void StateAuthorityChanged()
    {
        if (GameState == ASBStateGame.GameOver)
        {
            startNewGameBtn.SetActive(Runner.IsSharedModeMasterClient);
        }
    }

}