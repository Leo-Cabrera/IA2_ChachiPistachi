
using UnityEngine;

public class FENNEKIN : GAgent
{
    protected override void Start()
    {
        base.Start();


        SubGoal quedarseQuieto = new SubGoal("StayStill", 1, false);
        goals.Add(quedarseQuieto, 1);
    }
}
