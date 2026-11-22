
using UnityEngine;

public class TURTWIG : GAgent
{
    protected override void Start()
    {
        base.Start();


        SubGoal atacar = new SubGoal("Atacar", 1, false);
        goals.Add(atacar, 5);

        SubGoal curarse = new SubGoal("Curarse", 1, false);
        goals.Add(curarse, 4);

        SubGoal huir = new SubGoal("Huir", 1, false);
        goals.Add(huir, 4);

        SubGoal conseguirMonedas = new SubGoal("ConseguirMonedas", 1, false);
        goals.Add(conseguirMonedas, 2);

        SubGoal proteger = new SubGoal("Proteger", 1, false);
        goals.Add(proteger, 1);

        //SubGoal curar = new SubGoal("Curar", 1, false);
        //goals.Add(curar, 1);
    }
}
