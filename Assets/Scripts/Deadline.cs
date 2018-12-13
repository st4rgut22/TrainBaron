using System.Collections;
using System.Collections.Generic;

public class Deadline {
    int date; //deadline
    int duration;
    public Deadline(int date, int duration){
        this.date = date;
        this.duration = duration;
    }

    public int getDate()
    {
        return date;
    }

    public void setDate()
    {
        date+=duration;
    }

    public int getDuration()
    {
        return duration;
    }
}
