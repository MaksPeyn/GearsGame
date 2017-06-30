using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Gear gear;
    public List<Node> neighbors { get; set; }
    public bool isNotMoving;
    public bool isMain;
    public double angular_velocity = 0.0;
    public Node (Gear Gear)
    {
        gear = Gear;
    }
    public void Start(){}
}