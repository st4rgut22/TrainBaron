using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndustryController : MonoBehaviour {

    GameObject[] industry; //building locations on board
    List<int> placedFactories; //random choose, list of available factories connectBuilding();
    public static List<List<int>> allFactories; //train will search this list for path confirmation
    static float cashPerMile = 100;
    public GameObject building;
    static GameController gc;
    public AudioClip cashSound;
    public int depart;
    bool ogRoute = true;
    int ogIndustryTile;
    int ogDepart;
    int days = -1;
    int bldgCount = 0;
    public static IndustryController instance;

    public void penalize(int to, int fromIndex)
    {
        int toIndex = allFactories[fromIndex][to];
        int duration = TimeManagement.instance.deadlineCalendar[fromIndex][to].getDuration();
        int penalty = (int)(rewardDistance(toIndex, fromIndex) * cashPerMile * 5 / duration);
        penalty /= 2;
        gc.money -= penalty;
        if (gc.money>=0)
        {
            Debug.Log("penalized" + penalty + " dollars");

        }
        else {
            //ran out of money 
            Debug.Log("penalized" + penalty + " dollars");
            GameController.instance.loadNewLevel();
        }
    }

    public float rewardDistance(int industry1,int industry2){
        Vector2 g1 = gc.getCenterTile(industry1);
        Vector2 g2 = gc.getCenterTile(industry2);
        float distance = Vector2.Distance(g1, g2);
        return distance * 5; //times 100 because the screen size is reduces
    }

    public int rewardCalculator(int industry1, int industry2)
    {
        List<int> destList = allFactories[industry1]; //industry1 is from  where
        for (int i = 0; i < destList.Count;i++){
            //route confirmation
            if (destList[i]==industry2){
                Debug.Log("reward");
                float distance = rewardDistance(industry1, industry2);
                Debug.Log("rewarded " + (cashPerMile * distance * 5 / TimeManagement.instance.deadlineCalendar[industry1][i].getDuration()));
                gc.money += (int)(cashPerMile * distance * 5/TimeManagement.instance.deadlineCalendar[industry1][i].getDuration()); //put this in a function later
                SoundManager.instance.PlaySingle(cashSound);
                return i;
            }
        }
        return -1;
    }

    void connectToBuilding(int depart,int currentTile){
        //routes contains lists of lists of destinations (eg where you're going to)
        allFactories[depart].Add(currentTile);
        placedFactories.Add(currentTile);
    }

    void addNewBuilding(int industryTile){
        placedFactories.Add(industryTile);
        Vector2 worldPos = gc.getCenterTile(industryTile);
        GameObject newBldg = Instantiate(building, worldPos, Quaternion.identity);
        Background.instance.scaleTile(newBldg,.35f,.35f); //TEST
        if (gc.gameBoard[industryTile] != null) Destroy(gc.gameBoard[industryTile]); //destroy railroad if it exists
        gc.gameBoard[industryTile] = newBldg;
    }

    int getRandomDeparture(){
        //chooses an industry to depart from
        if (placedFactories.Count == 0) return -1;
        int randomDepart = Random.Range(0, placedFactories.Count);
        int depart = placedFactories[randomDepart];
        return depart;
    }

    void showRoute(int industryTile, int depart)
    {
        ogIndustryTile = industryTile;
        ogDepart = depart;
        ogRoute = false;
        float distancePay = rewardDistance(ogIndustryTile, depart);
        days = TimeManagement.instance.getRandomDeadline();
        float timeRate = 5 / days; //between 5 and 1 
        int totalPay = (int)(distancePay * timeRate * cashPerMile);
        //add pay parameter below
        MessageController.instance.showRouteDialog(ogIndustryTile, depart, totalPay, days);
    }

    int randomizeTile(){
        int industryTile;
        do
        {
            industryTile = Random.Range(2, 34); //0 is where the train spawns,1 is also not acceptable.  dont put industries here
        }
        while (industryTile == 27 || industryTile == 8 || (gc.gameBoard[industryTile]!=null && gc.gameBoard[industryTile].tag == "industry") || possibleBlocking(industryTile));
        return industryTile;
    }

    IEnumerator placeBuilding(){
        while(true && TimeManagement.instance.day!=11){
            int industryTile = randomizeTile();
            //we can put an industry on a railroad, but it will have to replace the railroad 
            if (!MessageController.instance.scheduleOn && !gc.screenLoad && !MessageController.instance.routeOn)
            { //and flag is off
                //Debug.Log("placed indsutry at " + industryTile);
                depart = getRandomDeparture(); //industry departure tile
                //if first building dont show the dialog because we cant make a route yet
                if (depart==-1) { //first industry
                    addNewBuilding(industryTile); //industryTile
                    bldgCount++;
                    yield return new WaitForSeconds(3); //dont wait long to make 1st route
                    int secondTile = randomizeTile();  
                    showRoute(secondTile, industryTile);
                    bldgCount++;
                }
                else 
                {
                    //Debug.Log("wait 10 seconds");
                    yield return new WaitForSeconds(10); //wait 5 seconds 
                    if (!MessageController.instance.scheduleOn) showRoute(industryTile, depart);
                    else { yield return new WaitForSeconds(1); }
                }
                bldgCount++;
            }
            else {
                if (MessageController.instance.scheduleOn)
                {
                    //Debug.Log("waiting for scheduling task to be over");
                }
                //Debug.Log("something occupying index " + industryTile);
                //Debug.Log("wait 1 seconds");
                yield return new WaitForSeconds(1);
            }
        }
    }

    //check if this blocks a train
    private bool possibleBlocking(int bldgIndex){
        int left = bldgIndex - 1;
        int right = bldgIndex + 1;
        int up = bldgIndex + 9;
        int down = bldgIndex - 9;
        if (gc.trainBoard[bldgIndex] != null) { return true; } //a train at that location
        bool leftFace = (blockingTrain(left, 2) || blockingIndustry(left));
        bool rightFace = (blockingTrain(right, -2) || blockingIndustry(right));
        bool upFace = (blockingTrain(up, -1) || blockingIndustry(up));
        bool downFace = (blockingTrain(down, 1) || blockingIndustry(down));
        if (leftFace || rightFace || upFace || downFace) return true;
        else { return false; }
    }

    private bool blockingTrain(int surround,int faceIndustry){
        if (surround>=0 && surround<36){
            GameObject checkTrain = gc.trainBoard[surround];
            if (checkTrain == null) return false; //no train on this side, no need to check orientation of non-existing train
            int orient = checkTrain.GetComponent<TrainController>().orientation;
            if (orient == faceIndustry) return true;
            else { return false; }
        }
        return false;
    }

    //check gameboard for surrounding industries
    private bool blockingIndustry(int surround)
    {
        if (surround < 0) return false; //ok on the bottom border
        GameObject surroundTile = gc.gameBoard[surround];
        if (surroundTile!=null && surroundTile.tag=="industry"){
            return true;
        }
        return false;
    }

        private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        //initialize industry array
        placedFactories = new List<int>();
        allFactories = new List<List<int>>(36); //DOESNT WORK
        for (int i = 0; i < 36; i++)
        {
            allFactories.Add(new List<int>());
            //allFactories[i] = new List<int>(); //size initially 0 DOESNT WORK
        }
    }

    // Use this for initialization
    void Start () {
        gc = GameController.instance;
        StartCoroutine(placeBuilding());
	}
	
	// Update is called once per frame
	void Update () {

        if (MessageController.globalConfirm==1) //check whether we pressed yes or no
        {
            connectToBuilding(ogDepart, ogIndustryTile);
            addNewBuilding(ogIndustryTile);
            //update schedule as well 
            TimeManagement.instance.addDeadline(ogDepart, ogIndustryTile, days);
            MessageController.globalConfirm = -1; //-1 means no confirmation received , resetting
            ogRoute = true;
        }
        else if (MessageController.globalConfirm == 0)
        {
            MessageController.globalConfirm = -1;
            ogRoute = true;
        }
    }
}
