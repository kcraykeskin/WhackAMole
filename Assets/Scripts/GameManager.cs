using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Mole> moles;

    [Header("UI Objects")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject outOfTimeText;
    [SerializeField] private GameObject bombText;
    [SerializeField] private GameObject logo;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    private float startingTime = 30f;
    private float timeRemaining;

    private HashSet<Mole> currentMoles = new HashSet<Mole>();
    private int score;
    private bool playing= false;

    public void StartGame()
    {
        //clear the play button 
        playButton.SetActive(false);
        outOfTimeText.SetActive(false);
        bombText.SetActive(false);
        logo.SetActive(false);
        gameUI.SetActive(true);


        //hide each mole
        for(int i = 0; i < moles.Count; i++)
        {
            moles[i].Hide();
            moles[i].SetIndex(i);
        }

        currentMoles.Clear();
        timeRemaining = startingTime;
        score = 0;
        scoreText.text = "0";
        playing = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        logo.SetActive(true);
        gameUI.SetActive(false);
        for (int i = 0; i < moles.Count; i++)
        {
            moles[i].Hide();
        }

    }

    // Update is called once per frame
    void Update()
    {
        //while the game keeps running decrease the remaining time
        if (playing) { 
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0) {
                timeRemaining = 0;
                GameOver(0);
            }
            timeText.text = $"{(int)timeRemaining / 60}:{(int)timeRemaining % 60:D2}";
            //check if there are more moles needed
            if(currentMoles.Count <= (score/10)) {
                //then pick a random number 
                int index = Random.Range(0, moles.Count);
                //if that pit is empty than put a mole in it 
                if (!currentMoles.Contains(moles[index]))
                {
                    currentMoles.Add(moles[index]);
                    moles[index].Activate(score / 10);
                }
            }
        }
    }

    public void GameOver(int type)
    {
        if(type == 0)
        {
            outOfTimeText.SetActive(true);
        }
        else
        {
            bombText.SetActive(true);
        }
        foreach(Mole mole in moles)
        {
            mole.StopGame();    
        }
        //game is over so update the button and playing state variable
        playing = false;
        playButton.SetActive(true);
    }
    public void AddScore(int moleIndex)
    {
        //increse the score and give a bit of extra time then remove the mole
        score++;
        scoreText.text = $"{score}";
        timeRemaining++;
        currentMoles.Remove(moles[moleIndex]);  
    }
    public void Missed(int moleIndex, bool isMole) {
        //if player cant press the mole on time give it a time penalty
        //check if the missed object is a mole not a bomb
        if (isMole)
        {
            timeRemaining -= 2;
        }
        currentMoles.Remove(moles[moleIndex]);
    }

}
