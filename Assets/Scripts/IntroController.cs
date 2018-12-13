using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroController : MonoBehaviour {
    public static IntroController instance;
    public GameObject mainScreen;
    public GameObject tutorialScreens;
    GameObject[] tutorialImages = new GameObject[4];
    Button playBtn;
    Button tutorialBtn;
    Button nextBtn; //hides the current screen, displays the next one
    public bool playGame = false; //flag to pause other scripts
    public bool startTut = false; //flag to pause other scripts
    int screenNumber = 3;

    private void Awake()
    {
        //singleton pattern
        if (instance == null) instance = this;
        else if (instance!=this){
            Destroy(gameObject);
        }
        for (int i = 0; i < tutorialImages.Length;i++){
            tutorialImages[i] = tutorialScreens.transform.GetChild(i).gameObject; //each tutorial image and all its associated components
            disableAllComponent(tutorialImages[i]);
        }
        playBtn = mainScreen.transform.GetChild(3).GetComponent<Button>();
        tutorialBtn = mainScreen.transform.GetChild(2).GetComponent<Button>();
        playBtn.onClick.AddListener(() => startGame());
        tutorialBtn.onClick.AddListener(() => startTutorial());
    }

    public void disableAllComponent(GameObject go){
        foreach (Behaviour childCompnent in go.GetComponentsInChildren<Behaviour>())
            childCompnent.enabled = false;
    }

    public void enableAllComponent(GameObject go)
    {
        foreach (Behaviour childCompnent in go.GetComponentsInChildren<Behaviour>())
            childCompnent.enabled = true;
    }

    public void startGame(){
        disableAllComponent(mainScreen);
        playGame = true;
    }

    public void startTutorial(){
        startTut = true;
    }

    public void nextScreen()
    {
        disableAllComponent(tutorialImages[screenNumber+1]);
        startTut = true; //allows us to gets references to elements on the new screen
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (startTut){
            enableAllComponent(tutorialImages[screenNumber]);
            nextBtn = tutorialImages[screenNumber].transform.GetChild(2).GetComponent<Button>();
            nextBtn.onClick.AddListener(() => nextScreen());
            startTut = false;
            if (screenNumber>0){
                screenNumber--;
            }
            else {
                disableAllComponent(tutorialImages[screenNumber]); //we see the intro page again
                tutorialBtn.onClick.AddListener(() => startTutorial()); //why this disappears I don't know
            }
            //enable the first image  
        }
	}
}
