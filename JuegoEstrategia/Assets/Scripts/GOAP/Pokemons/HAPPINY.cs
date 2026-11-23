
using UnityEngine;

public class HAPPINY : GAgent
{
    protected override void Start()
    {
        base.Start();

        SubGoal curar = new SubGoal("Curar", 1, false);
        goals.Add(curar, 5);

        SubGoal huir = new SubGoal("Huir", 1, false);
        goals.Add(huir, 4);

        SubGoal conseguirMonedas = new SubGoal("ConseguirMonedas", 1, false);
        goals.Add(conseguirMonedas, 3);

        SubGoal curarse = new SubGoal("Curarse", 1, false);
        goals.Add(curarse, 2);

    }
}
