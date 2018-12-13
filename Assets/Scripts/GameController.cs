using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour {
    public int[] railroadIndex; //indices while cycling through multiple railroads on a tile
    public List<List<GameObject>> railroad; //includes the train too
    public List<GameObject> gameBoard;
    public List<GameObject> trainBoard;
    List<GameObject> deadlineText;
    public Sprite greenGrass;
    public Sprite greenerGrass;
    public Sprite destSprite;
    int screenWidth;
    public int startIndex;
    int screenHeight;
    public float tileWidth;
    public float tileHeight;
    public Dictionary<string, int[]> curveDict;
    public int[,] orientation;
    public static GameController instance = null;
    private GameObject placeObject=null;
    public GameObject blue, red, yellow;
    public GameObject car, train, curve1, curve2, curve3, curve4, hor, vert, deadline;
    [HideInInspector] public int money;
    public GameObject levelCanvas;
    GameObject loseCanvas;
    public GameObject moneyText;
    int prevDepart=-1; //change sprite back
    Vector2 prevWorldPosition;
    Vector2 worldPosition; //from the Update()
    Vector2 adjustText;
    int industryIndex=-1; //from the Update()
    public static int level = 1;
    public int goal;
    public AudioClip trainSound;
    public AudioClip loseSound;
    public AudioClip trackSound;
    public AudioClip winSound;
    public bool screenLoad = true;
    public static bool lost = false;
    public static string msg = "";
    // Use this for initialization

    private void Awake()
    {
        goal = level * 1000;
        money = 900; //900
        //singleton design pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        railroad = new List<List<GameObject>>(45);
        gameBoard = new List<GameObject>(45);
        deadlineText = new List<GameObject>();
        railroadIndex = new int[45];
        for (int i = 0; i < 45; i++) railroad.Add(new List<GameObject>());
        for (int i = 0; i < 45; i++) gameBoard.Add(null);
        for (int i = 0; i < 45; i++) trainBoard.Add(null);
        trainBoard[0] = GameObject.FindWithTag("train");
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        tileWidth = screenWidth / 9f;
        tileHeight = screenHeight / 5f;
        adjustText = new Vector2(0, tileHeight / 2);
        //curve map. -2 is left, 2 is right, -1 is down, 1 is up
        curveDict = new Dictionary<string, int[]> {
            { "curve1",new[] { 0, 0, 2, 1 }},
            { "curve2",new[] { -2, 0, 0, 1 } },
            { "curve3",new[] { -2, -1, 0, 0 } },
            { "curve4",new[] { 0, -1, 2, 0 } },
            { "Hor",new[] { -2, 0, 2, 0 } },
            { "Vert",new[] { 0, -1, 0, 1 } }
        };
        if (level>1){
            //proceed to next level 
            IntroController.instance.playGame = true; //next scene allows us to immediately play instead of stopping at main page
            IntroController.instance.disableAllComponent(GameObject.FindWithTag("main"));
            //win sound here
            SoundManager.instance.PlaySingle(winSound);
        }
    }

    void Start () {
        Debug.Log("starting");
        loseCanvas = GameObject.FindWithTag("losing");
        //i think level canvas and money text are loaded later
    }

    public void lostGame(){ //
        SoundManager.instance.PlaySingle(loseSound);
        StartCoroutine(loseScreen(msg));
    }

    void playGame() //called from intro controller
    {
        loseScreenOff();
        StartCoroutine(levelScreen()); //show
    }

    //also call thsi function from time amangement when it is day 11
    IEnumerator levelScreen(){
        screenLoad = true; //tell industrycontroller & timemangement to wait
        levelCanvas.transform.GetChild(1).GetComponent<Text>().text = "Level " + level + "\n Goal: $" + goal;
        //levelCanvas.transform.GetChild(0).GetComponent<Image>().enabled = true;
        //levelCanvas.transform.GetChild(1).GetComponent<Text>().enabled = true;
        yield return new WaitForSeconds(3);
        screenLoad = false; 
        levelCanvas.transform.GetChild(0).GetComponent<Image>().enabled = false;
        levelCanvas.transform.GetChild(1).GetComponent<Text>().enabled = false;
    }

    IEnumerator loseScreen(string text)
    {
        loseCanvas.transform.GetChild(0).GetComponent<Image>().enabled = true;
        loseCanvas.transform.GetChild(1).GetComponent<Text>().enabled = true;
        loseCanvas.transform.GetChild(2).GetComponent<Text>().enabled = true;
        screenLoad = true; //tell industrycontroller & timemangement to wait
        loseCanvas.transform.GetChild(2).GetComponent<Text>().text = text;

        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("SampleScene");
    }

    void loseScreenOff(){
        loseCanvas.transform.GetChild(0).GetComponent<Image>().enabled = false;
        loseCanvas.transform.GetChild(1).GetComponent<Text>().enabled = false;
        loseCanvas.transform.GetChild(2).GetComponent<Text>().enabled = false;
    }

    public void loadNewLevel(){
        if (money > goal) {
            level++;
            SceneManager.LoadScene("SampleScene");
        }
        else {
            //take you back to the main screen because level equals 1 
            level = 1;
            lost = true;
            if (money < 0) {
                msg = "You Ran out of Money";
                lostGame();
            }
            else {
                msg = "You Ran out of Time";
                lostGame();
            }
        }
    }
	
    Vector2 getScreenCoords(int index){
        return new Vector2(index % 9 * tileWidth + tileWidth / 2, index / 9 * tileHeight + tileHeight / 2);
    }

    public Vector2 getCenterTile(int index){
        Vector2 tileCenter = getScreenCoords(index);
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(tileCenter);
        return worldPosition;
    }

	// Update is called once per frame
	void Update () {
        //check whether the intro is over 
        if (IntroController.instance.playGame){
            playGame();
            IntroController.instance.playGame = false; //so we dont repeatedly call playgame()
        }
        moneyText.GetComponent<Text>().text = "$" + money.ToString();
        if (Input.GetMouseButtonDown(0) && !MessageController.instance.routeOn)
        {
            //if click menu item
            int index = getTile(Input.mousePosition); //dont conver to world points, screen points good 
            GameObject firstGO = gameBoard[index]; //check if null 
            string nameGO = "";
            if (firstGO!=null) nameGO = firstGO.tag; //if something exists on tile, then we can check what it is 
            worldPosition = getCenterTile(index);
            if (MessageController.instance.scheduleOn){
                if (index<36)
                {
                    //check if its an industry
                    if (gameBoard[index]!=null && gameBoard[index].tag=="industry")
                    {
                        industryIndex = index;
                        if (prevDepart!=-1) { //change last tile back to original color
                            Background.instance.changeBoardTileSprite(prevWorldPosition,greenGrass);
                            markDestinations(prevDepart, greenGrass);
                            destroyDeadlineText();
                        }
                        addDeadlineText(index); //instantiate new deadline objects 
                        Background.instance.changeBoardTileSprite(worldPosition,greenerGrass);
                        markDestinations(index, destSprite); //loop through allFactories, mark all indices greenergrass
                        //change the color of all the destination tiles 
                        prevDepart = index;
                        prevWorldPosition = worldPosition;
                    }
                }
                return;
            }
            if (index > 35) //its a menu item 
            {
                Debug.Log("you clicked index " + index);
                if (index == 36) placeObject = train;
                //if (index == 37) placeObject = car;
                if (index == 37) placeObject = hor;
                if (index == 38) placeObject = vert;
                if (index == 39) placeObject = curve2;
                if (index == 40) placeObject = curve1;
                if (index == 41) placeObject = curve4;
                if (index == 42) placeObject = curve3;
                if (index == 43 && !MessageController.instance.scheduleOn && !MessageController.instance.routeOn && IndustryController.instance.depart!=-1) {
                    //you clicked clock, opens a list of routes
                    //MessageController.instance.scheduleOn = true;
                    MessageController.instance.showSchedule(true);
                }
            }
            else if (placeObject!=null){
                string newObject = placeObject.tag;
                //if its a railroad we instantiate and/or add it to list of lists
                if (newObject == "curve1" || newObject == "curve2" || newObject == "curve3" || newObject == "curve4" || newObject=="Hor" || newObject=="Vert")
                {
                    //add railroad if nothings on the tile or a railroad is on the tile
                    if (index!=0 && (nameGO=="" || nameGO == "curve1" || nameGO == "curve2" || nameGO == "curve3" || nameGO == "curve4" || nameGO == "Hor" || nameGO == "Vert"))
                    {
                        if (money>=10) 
                        {
                            SoundManager.instance.PlaySingle(trackSound);
                            money -= 10; //each track costs $10
                            railroad[index].Add(placeObject);
                            placeGameObject(placeObject, index, worldPosition);
                            //scaleTrack(newObject, placeObject);
                            //change color
                            if (railroad[index].Count > 1)
                            {
                                Background.instance.changeBoardTileSprite(worldPosition, greenerGrass);
                            }
                        }
                    }
                }
                else { //not a railroad, instantiate only if there's nothing on the tile
                    Debug.Log("train at starting tile " + trainBoard[0]);
                    if (gameBoard[index]==null && newObject!="train"){
                        placeGameObject(placeObject, index, worldPosition);
                    }
                    else if (newObject=="train" && trainBoard[0]==null){
                        Debug.Log("instantiating train");
                        if (money>=100){
                            money -= 100;
                            Vector2 startPosition = new Vector2(-4.12f * Background.instance.scaleFactor, -2.04f * Background.instance.scaleFactor); //hardcoding may cause problem
                            GameObject trainTile = Instantiate(placeObject, startPosition, Quaternion.identity);
                            Background.instance.scaleTile(trainTile,.3f,.3f);
                            SoundManager.instance.PlaySingle(trainSound);
                            trainBoard[0] = trainTile;
                        }
                    }
                }
                placeObject = null;
            }
            //click on a train (starting index) or industry
            else if (trainBoard[index]!=null) //nameGO=="train" || (
            {
                //clicked on a train
                TrainController tc = trainBoard[index].GetComponent<TrainController>(); //index is the train's starting location
                if (!tc.coroutineActive){
                    tc.openPath = true;
                    startIndex = index;
                    tc.StartCoroutine(tc.startTrain()); //start the train ALL ABOARD
                }
            }
            //cycles through railroads
            else if (nameGO == "curve1" || nameGO == "curve2" || nameGO == "curve3" || nameGO == "curve4" || nameGO == "Hor" || nameGO == "Vert")
            {
                //cycle 
                if (railroad[index].Count > 1)
                {
                    Destroy(gameBoard[index]);
                    railroadIndex[index] += 1;
                    int nextRailroad = railroadIndex[index];
                    int trackCount = railroad[index].Count;
                    int track = nextRailroad % trackCount;
                    GameObject trackTile = railroad[index][track]; //railroad holds list of tracks for each tile 
                    GameObject trackTilePrefab = Instantiate(trackTile, worldPosition, Quaternion.identity);
                    scaleTrack(trackTile.tag,trackTilePrefab);
                    gameBoard[index] = trackTilePrefab;
                }
            }
        }
    }

    void scaleTrack(string trackName, GameObject GO)
    {
        if (trackName == "Hor") Background.instance.scaleTile(GO, .45f, .3f); //TEST
        else if (trackName=="Vert") Background.instance.scaleTile(GO, .33f, .4f); //TEST
        else if (trackName == "curve1" || trackName == "curve2" || trackName == "curve3" || trackName == "curve4") Background.instance.scaleTile(GO, .35f, .4f); //TEST
        else
        {
            Background.instance.scaleTile(GO, .3f, .3f);
        }
    }


    public void restoreGreenField(){
        if (industryIndex!=-1){
            Vector2 pos = getCenterTile(industryIndex);
            //Debug.DrawLine(Vector3.zero, pos, Color.white, 2.5f);
            Background.instance.changeBoardTileSprite(pos, greenGrass); //change from tile to green
            markDestinations(industryIndex, greenGrass); //change to tiles to green
            Debug.Log(industryIndex);
        }
    }

    void markDestinations(int fromIndex,Sprite tileSprite){ //change destination tiles  
        //mark orunmarks the destinations
        List<List<int>> routes = IndustryController.allFactories;
        List<int> destList = routes[fromIndex];
        for (int i = 0; i < destList.Count;i++) 
        {
            Vector2 destPos = getCenterTile(destList[i]); //problem centering it 
            Background.instance.changeBoardTileSprite(destPos, tileSprite);
        }
    }

    public void destroyDeadlineText(){
        for (int i = 0; i < deadlineText.Count;i++){
            Destroy(deadlineText[i]);
        }
        //delete everything
        deadlineText = new List<GameObject>();
    }

    void addDeadlineText(int index){ //call this when we mark destination 
        List<List<int>> routes = IndustryController.allFactories;
        List<int> destList = routes[index];
        for (int i = 0; i < destList.Count; i++)
        {
            int daysLeft = TimeManagement.instance.daysLeft(i, index);

            //GameObject newDeadline = Instantiate(deadline, GameObject.FindWithTag("deadline").transform, false);
            //position this how i don tknow 
            Vector3 worldPoint = gameBoard[destList[i]].transform.position;
            GameObject light;
            if (daysLeft == 1) { light = Instantiate(red, worldPoint, Quaternion.identity); }
            else if (daysLeft == 2) { light = Instantiate(yellow, worldPoint, Quaternion.identity); }
            else { light = Instantiate(blue, worldPoint, Quaternion.identity); }
            Background.instance.scaleTile(light, .2f, .2f);
            Vector3 viewPoint = Camera.main.WorldToViewportPoint(worldPoint);
            Debug.Log("viewport point " + viewPoint);
            GameObject newDeadline = Instantiate(deadline);
            newDeadline.transform.SetParent(GameObject.FindWithTag("deadline").transform);
            newDeadline.GetComponent<RectTransform>().anchorMax = viewPoint;
            newDeadline.GetComponent<RectTransform>().anchorMin = viewPoint;
            deadlineText.Add(newDeadline);
            deadlineText.Add(light);

            newDeadline.GetComponent<Text>().text = daysLeft + "";
        }
    }

    void placeGameObject(GameObject GO, int index, Vector2 worldPosition){
        if (gameBoard[index] == null)
        {
            GameObject gameTile = Instantiate(GO, worldPosition, Quaternion.identity);
            scaleTrack(gameTile.tag, gameTile);
            gameBoard[index] = gameTile;
        }
    }

    public int getTile(Vector2 pos){
        float x = pos.x;
        float y = pos.y;
        int row = getRow(y);
        int col = getCol(x);
        return row * 9 + col;
    }

    public int getRow(float y){
        return (int)(y / tileHeight);
    }

    public int getCol(float x)
    {
        return (int)(x / tileWidth);
    }
}
