using UnityEngine;

public class TIMBURR : GAgent
{
   
    protected override void Start()
    {
        base.Start();
        SubGoal s1 = new SubGoal("MoreCoins", 1, true);
        goals.Add(s1, 3);
    }

    
}
