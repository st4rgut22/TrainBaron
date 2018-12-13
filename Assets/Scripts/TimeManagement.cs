using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManagement : MonoBehaviour
{
    //max deadline is 5 days out 

    int daysPerLevel = 10;
    public int day = 0;
    public int almostLate = 0;
    float lengthOfDay = 20; //length of day in seconds
    public List<List<Deadline>> deadlineCalendar; //gamecontroller , add text 
    Text dayText;
    public static TimeManagement instance;
    public List<List<int>> schedule; //indexes are departures, values are days till deadline
    float displayStartTime;
    float displayEndTime;
    float coroutineStartTime;
    float ogWaitTime;
    public Sprite redClock;
    public AudioClip deadlineSound;
    public AudioClip missDeadlineSound;
    public Sprite whiteClock;
    bool flag1 = true;

    private void Awake()
    {
        deadlineCalendar = new List<List<Deadline>>(36); 
        for (int i = 0; i < 36; i++)
        {
            deadlineCalendar.Add(new List<Deadline>());
        }
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        dayText = GameObject.FindWithTag("day").transform.GetChild(0).GetComponent<Text>();
        dayText.enabled = false;
        for (int i = 0; i < 36; i++)
        {
            deadlineCalendar.Add(new List<Deadline>());
        }
    }
    // Use this for initialization
    void Start()
    {
        StartCoroutine(ticktock());
    }

    public int getRandomDeadline()
    {
        int length = Random.Range(2, 5); //inclusive
        int daysLeft = daysPerLevel - day;
        if (length > daysLeft)
        { //if our deadline is beyond the level ending choose a closer deadline
            length = Random.Range(1, daysLeft);
        }
        return length;
    }

    public void addDeadline(int fromIndustryTile, int toIndustryTile, int duration)
    {
        int date = duration + day;
        deadlineCalendar[fromIndustryTile].Add(new Deadline(date, duration));
    }

    public int daysLeft(int destIndex, int fromIndex)
    {
        List<Deadline> dest = deadlineCalendar[fromIndex];
        Deadline deadline = dest[destIndex];
        return deadline.getDate() - day; //if positive means we're still on time
    }

    public void deadlinePenalty()
    {
        almostLate = 0;
        bool allOnTime = true;
        for (int i = 0; i < 36; i++)
        {
            for (int j = 0; j < deadlineCalendar[i].Count; j++)
            {
                int stillTime = daysLeft(j, i);
                if (stillTime==1){
                    almostLate++;
                    if (allOnTime){
                        SoundManager.instance.PlaySingle(deadlineSound);
                        flag1 = true;
                        allOnTime = false;
                        Background.instance.changeBoardTileSprite(GameController.instance.getCenterTile(43), redClock);
                        //warning, red clock
                    }

                }
                if (stillTime <= 0)
                {
                    //missed the deadline, postpone delivery
                    SoundManager.instance.PlaySingle(missDeadlineSound);
                    IndustryController.instance.penalize(j, i);
                    deadlineCalendar[i][j].setDate();
                }
            }
        }
    }

    public void completeDeadlines()
    {

    }

    IEnumerator ticktock()
    {
        while (day <= daysPerLevel)
        {
            while (GameController.instance.screenLoad){
                yield return null;
            }
            day++;
            deadlinePenalty();
            dayText.text = "Day " + day;
            dayText.enabled = true;
            yield return new WaitForSeconds(3f); //seconds the day message appears on screen
            dayText.enabled = false;

            float timer = 0f;
            while (timer<lengthOfDay){
                while (MessageController.instance.scheduleOn || MessageController.instance.routeOn) { yield return null; }
                timer += Time.deltaTime;
                yield return null;
            }
        }
        //end of level, check if we passed or not
        GameController.instance.loadNewLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if (almostLate==0 && flag1) {
            flag1 = false;
            Background.instance.changeBoardTileSprite(GameController.instance.getCenterTile(43), whiteClock);
        }

    }
}
