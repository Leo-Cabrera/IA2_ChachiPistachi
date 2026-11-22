
using UnityEngine;

public class TIMBURR : GAgent
{
    protected override void Start()
    {
        base.Start();

        SubGoal proteger = new SubGoal("Proteger", 1, false);
        goals.Add(proteger, 5);

        SubGoal atacar = new SubGoal("Atacar", 1, false);
        goals.Add(atacar, 5);

        SubGoal conseguirMonedas = new SubGoal("ConseguirMonedas", 1, false);
        goals.Add(conseguirMonedas, 3);

        SubGoal curarse = new SubGoal("Curarse", 1, false);
        goals.Add(curarse, 2);

        SubGoal huir = new SubGoal("Huir", 1, false);
        goals.Add(huir, 2);
        
    }
}
