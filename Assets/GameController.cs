using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed  class GameController
{
    private static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameController();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
                instance.checkpoints = instance.checkpoints.OrderBy(x => x.name).ToList();
            }
            return instance;
        }

    }
    private List<GameObject> checkpoints = new List<GameObject>();
    public List<GameObject> Checkpoints { get { return checkpoints; } }
    // Start is called before the first frame update

}
