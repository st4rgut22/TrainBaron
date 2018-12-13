using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MovingObject {
    public int orientation = 1; //1 is up -1 is down 2 is right, -2 is left
    int inlet = 0;
    int outlet = 0;
    int tileIndex = 0;
    public bool openPath = true;
    int prevIndex;
    int departBldg;
    int destBldg;
    float tileHeight;
    float tileWidth;
    bool reachedIndustry = false;
    public bool coroutineActive = false; 
    // Use this for initialization

    private void Awake()
    {
        departBldg = -1;
        destBldg = -1;
        base.Awake();
        prevIndex = 0;
    }

    void Start () {
        //place train in game board
        //gc.gameBoard[0] = gameObject;
        gc.railroad[0].Add(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
        //if the train has been clicked
    }

    //TRAINWRECK
    public void OnCollisionEnter2D(Collision2D collision)
    {
        SoundManager.instance.playCrashSound();
        Destroy(gameObject);
    }

    public IEnumerator startTrain()
    {
        coroutineActive = true;
        while (openPath)
        {
            yield return null;
            if (move1 && move2)
            {
                Debug.Log("move complete check Path again");
                checkPath(orientation, ref tileIndex);
                if (reachedIndustry) 
                {
                    Debug.Log("reached industry");
                    break;
                }
            }
        }
        //end of route, reached an industry 
        gc.trainBoard[tileIndex]=gameObject; 
        gc.trainBoard[gc.startIndex] = null;
        coroutineActive = false;
        reachedIndustry = false;
    }

    bool crossPath(int inlet,string nameKey)
    {
        float xDir = 0;
        float yDir = 0;
        float rotation = 0;
        //straight
        if (inlet != 0)
        {
            //we are calculating change from center of tile !!! LOOK AT YOUR NOTES!
            //straight
            if (inlet == -outlet)
            {
                if (inlet == -1)
                {
                    xDir = 0;
                    yDir = .39f/2.2f;
                }
                else if (inlet == 1)
                {
                    xDir = 0;
                    yDir = -.39f/2.2f;
                }
                else if (inlet == 2)
                {
                    xDir = -.35f/2.2f;
                    yDir = 0f;
                }
                else if (inlet == -2)
                {
                    xDir = .35f/2.2f;
                    yDir = 0f;
                }
            }
            //curve
            else
            {
                //rotate tip: CLOCKWISE NEGATIVE, COUNTERLCOCKWISE POSITIVE
                if (inlet == -1)
                {
                    yDir = 0f;
                    if (outlet == 2) { rotation = -90; xDir = .35f/2.2f; }
                    if (outlet == -2) { rotation = 90f; xDir = -.35f/2.2f; }
                }
                else if (inlet == 1)
                {
                    yDir = -0f;
                    if (outlet == 2) { rotation = 90f; xDir = .35f/2.2f; }
                    if (outlet == -2) { rotation = -90f; xDir = -.35f/2.2f; }
                }
                else if (inlet == 2)
                {
                    xDir = -0f;
                    if (outlet == 1) { rotation = -90f; yDir = -.39f/2.2f; }
                    if (outlet == -1) { rotation = 90f; yDir = .39f/2.2f; }
                }
                else if (inlet == -2)
                {
                    xDir = 0f;
                    if (outlet == 1) { rotation = 90f; yDir = .39f/2.2f; }
                    if (outlet == -1) { rotation = -90f; yDir = -.39f/2.2f; }
                }
            }
            RaycastHit2D raycast;
            Vector2 centerTile = gc.getCenterTile(tileIndex);
            Vector2 destination = centerTile + new Vector2(xDir, yDir);
            if (!CanMove(destination, rotation, out raycast, nameKey))
            {
                Debug.Log("obstruction detected. Can't move");
            }
            else
            {
                Debug.Log("orientation equals " + outlet);
                orientation = outlet;
                return true;
            }
        }
        return false;
    }

    //check for valid path
    void checkPath(int dir,ref int tileIndex)
    {
        inlet = 0; //reset inlet for no path check
        Dictionary<string,int[]> directionDict = gc.curveDict; //curve directions
        List<GameObject>map = gc.gameBoard;
        int startTileIndex = tileIndex;
        if (dir == 1) tileIndex += 9;
        if (dir == -1) tileIndex -= 9;
        if (dir == 2) tileIndex++;
        if (dir == -2) tileIndex--;
        Debug.Log("next tile index  " + tileIndex);
        if (tileIndex>-1 && tileIndex<36){
            if (map[tileIndex]!=null){
                string nameKey = map[tileIndex].tag; 
                outlet = orientation;
                bool crossSuccess;
                if (nameKey=="industry"){
                    Debug.Log("reach industry");
                    if (departBldg == -1) {
                        departBldg = tileIndex;
                        crossSuccess = crossPath(-orientation, nameKey);
                        Debug.Log("depart success " + crossSuccess);
                        reachedIndustry = true;
                        return; //run loop again, traverse the industry tile, then stop OTHERWISE openPath = false and loop will stop 
                    }
                    else { 
                        destBldg = tileIndex;
                        //check if route completed
                        crossSuccess = crossPath(-orientation, nameKey);
                        Debug.Log("arrive success " + crossSuccess);
                        reachedIndustry = true;
                        if (crossSuccess) {
                            int index = IndustryController.instance.rewardCalculator(departBldg, destBldg); //reward player if she completed a route
                            if (index!=-1) {
                                Deadline deadline = TimeManagement.instance.deadlineCalendar[departBldg][index];
                                int daysLeft = deadline.getDate() - TimeManagement.instance.day;
                                if (daysLeft==1){
                                    TimeManagement.instance.almostLate--; //we completed one destination before time ran out huzzah
                                }
                                deadline.setDate();
                            }//reset the deadline, if we completed a route
                            if (gc.money > gc.goal) gc.loadNewLevel();
                        }
                        //reset route
                        departBldg = destBldg;
                        destBldg = -1;
                        return;
                    }
                }
                else if (nameKey=="train"){
                    //collision
                    Debug.Log("train");
                }
                else {
                    //railroad(s)
                    string railroadTag = map[tileIndex].tag;
                    int[] trackDir = directionDict[nameKey];
                    for (int i = 0; i < trackDir.Length; i++)
                    {
                        if (trackDir[i] != 0 && trackDir[i] == -orientation) inlet = trackDir[i];
                        else if (trackDir[i] != 0) outlet = trackDir[i];
                    }
                    if (crossPath(inlet, nameKey)) return;
                } 
            } 
        }
        Debug.Log("Cannot go to that tile. Attempted tile index is " + tileIndex);
        tileIndex = startTileIndex;
        openPath = false; 
    }
}
