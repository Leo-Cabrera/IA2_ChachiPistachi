
using UnityEngine;

public class TIMBURR : GAgent
{
    protected override void Start()
    {
        base.Start();

        // MODIFICADO: objetivo de acercarse al enemigo.
        // Se mantiene disponible para poder volver a intentarlo
        // en los siguientes turnos si todavía no está cerca.
        SubGoal acercarse = new SubGoal("BeNextTo", 1, false);
        goals.Add(acercarse, 3);

        // NUEVO: alternativa de quedarse quieto.
        // No elimina el objetivo para que pueda volver a elegirse
        // en turnos posteriores.
        SubGoal quedarseQuieto = new SubGoal("StayStill", 1, false);
        goals.Add(quedarseQuieto, 1);
    }
}
