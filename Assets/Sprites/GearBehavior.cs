using System.Collections.Generic;
using UnityEngine;
using System;

public class Gear
{
	public Vector2 _pos { get; set; }
	private double _collide_radius;
	private double _clutch_radius;
    public bool rotate;

	public Gear(){}

	public Gear(Vector2 pos, double radius, double clutch_radius)
    {
		_pos = pos;
		_collide_radius = radius;
		_clutch_radius = clutch_radius;
	}

	public double collide_radius
    {
		get{ return _collide_radius; }
	}

	public double clutch_radius
    {
		get{ return _clutch_radius; }
	}

	public bool collide(Gear other)
    {
		return (_pos - other._pos).magnitude < Math.Max(collide_radius + other.clutch_radius, clutch_radius + other.collide_radius);
	}

	public bool clutch(Gear other)
    {
		return (_pos - other._pos).magnitude < clutch_radius + other.clutch_radius;
	}
}

class SpriteGear: Gear
{
	public SpriteRenderer spriteR;

	public SpriteGear(SpriteRenderer s):
		base
        (
			s.transform.position,
			get_sprite_radius() * s.transform.localScale.x,
			get_sprite_teeth_radius() * s.transform.localScale.x
		)
	{
		spriteR = s;
	}

	static double get_sprite_radius()
    {
		return 1.13;
	}

	static double get_sprite_teeth_radius()
    {
		return 1.24;
	}
}

class Graph
{
	public List<Node> _nodes = new List<Node>();

	public Graph(){}

    // check if this gear could be added	
    public KeyValuePair<bool, Node> can_insertfull(Gear gear)
    {
        sbyte i;
        for (i = (sbyte)(_nodes.Count-1); i != -1 && !_nodes[i].gear.collide(gear); i--) { }
        for (sbyte k = (sbyte)(i-1); k >= 0; k--) if (_nodes[k].gear.collide(gear)) return new KeyValuePair<bool, Node>(false, null);
        return new KeyValuePair<bool, Node>(true, i == -1 ? null: _nodes[i]);
    }

    public void peres(Gear gear, Gear selected)
    {
        if (selected.rotate == gear.rotate) selected.rotate = !selected.rotate;
        double tg_angle = (gear._pos.y - selected._pos.y) / (gear._pos.x - selected._pos.x), angle = selected.rotate ? 0 : Math.PI / 24;
        for (double delta = Math.PI / 12; Math.Tan(angle) < tg_angle; angle += delta) { }
        tg_angle = selected._pos.x;
        selected._pos = new Vector2(selected._pos.x, (float)((gear._pos.x - tg_angle) * Math.Tan(angle)));
        double k = (gear.collide_radius + selected.clutch_radius) / (selected._pos - gear._pos).magnitude;
        selected._pos = new Vector2((float)(gear._pos.x + k * (selected._pos.x - gear._pos.x)),
                                    (float)(gear._pos.y + k * (selected._pos.y - gear._pos.y)));
        if (can_insert(selected)) return;
        selected._pos = new Vector2((float)tg_angle, (float)((gear._pos.x - tg_angle) * Math.Tan(angle - Math.PI / 12)));
        k = (gear.collide_radius + selected.clutch_radius) / (selected._pos - gear._pos).magnitude;
        selected._pos = new Vector2((float)(gear._pos.x + k * (tg_angle - gear._pos.x)),
                                    (float)(gear._pos.y + k * (selected._pos.y - gear._pos.y)));
    }

    public bool can_insert(Gear gear)
    {
        return _nodes.TrueForAll(n => !n.gear.collide(gear));
    }

    public bool rotation (Node node)
    {
        ((SpriteGear)node.gear).spriteR.transform.rotation = new Quaternion(0, 0, node.gear.rotate ? 7.5f : 0, 0);
        return rotate(node, node);
    }

    public bool rotate (Node node, Node start)
    {
        foreach (Node n in node.neighbors)
        {
            ((SpriteGear)n.gear).spriteR.transform.rotation = new Quaternion(0, 0, n.gear.rotate ? 7.5f : 0, 0);
        }
        var notrotate = node.neighbors.FindAll(n => n.gear.rotate == node.gear.rotate);
        if (notrotate.Count == 0) return true;
        else if (!notrotate.Contains(start))
        {
            bool f;
            var r = new Quaternion(0, 0, (f = !notrotate[0].gear.rotate) ? 7.5f : 0, 0);
            foreach (Node n in notrotate)
            {
                ((SpriteGear)n.gear).spriteR.transform.rotation = r;
                n.gear.rotate = f;
            }
            return notrotate.TrueForAll(n => rotate(n, start));
        }
        else return false;
    }

    public void delete (Node node)
    {
        foreach (Node n in _nodes.FindAll(n => !n.isMain))
        {
            n.angular_velocity = 0.0;
        }
        _nodes.Remove(node);
        foreach (Node n in _nodes.FindAll(n => n.neighbors.Contains(node)))
        {
            n.neighbors.Remove(node);
        }
        node.neighbors.Clear();
    }

	// insert gear into graph
	public void insert (Node node)
    {
		foreach (Node n in _nodes.FindAll(n => n.gear.clutch(node.gear)))
        {
			node.neighbors.Add(n);
			n.neighbors.Add (node);
		}
		_nodes.Add (node);
	}

	// find gear node containing given pos; if no gear there returns "null"
	public Node find (Vector2 pos)
    {
        return _nodes.Find(n => (n.gear._pos - pos).magnitude < n.gear.collide_radius);
	}

	public bool calculate_velocity (Node start)
    {
		foreach (Node neighbor in start.neighbors)
        {
			double neig_angular = -start.angular_velocity * start.gear.collide_radius / neighbor.gear.collide_radius;
			if (neighbor.angular_velocity == 0.0)
            {
				neighbor.angular_velocity = neig_angular;
				if (!calculate_velocity (neighbor)) return false;
			}
            else if (Math.Abs(neighbor.angular_velocity - neig_angular)>0.000000000001) return false;
		}
		return true;
	}
}

public class GearBehavior: MonoBehaviour
{
	private Graph _graph;
    private SpriteGear _selectedSG;
    private Node _selectedN;
    private bool isMoving;
    private bool isRotate = true;
    void Start()
    {
		build_graph ();
	}
    /*if (Input.touchCount == 2)
        {
            /*var mainCam = Camera.main;
            var ts0p = mainCam.ScreenToWorldPoint(Input.touches[0].position);
            var ts1p = mainCam.ScreenToWorldPoint(Input.touches[1].position);
            var ts0pold = ts0p - mainCam.ScreenToWorldPoint(Input.touches[0].deltaPosition);
            var ts1pold = ts1p - mainCam.ScreenToWorldPoint(Input.touches[1].deltaPosition);
            mainCam.orthographicSize += (float)(Math.Pow(ts0pold.x - ts1pold.x, 2) + Math.Pow(ts0pold.y - ts1pold.y, 2)
                - Math.Pow(ts0p.x - ts1p.x, 2) - Math.Pow(ts0p.y - ts1p.y, 2)) * Time.deltaTime;
        }
        else if (Camera.main.orthographicSize > 1 || Input.GetAxis("Mouse ScrollWheel") < 0) Camera.main.orthographicSize -= 4*Input.GetAxis("Mouse ScrollWheel");
    }*/

    void Moving()
    {
        if (isMoving)
        {
            if (Input.GetMouseButtonUp(0))
            {
                //_selectedSG.spriteR.sprite = ;
                _graph.delete(_selectedN);
                _selectedSG._pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var pair = _graph.can_insertfull(_selectedSG);
                if (!pair.Key)
                {
                    _selectedSG._pos = _selectedSG.spriteR.transform.position;
                    _graph.insert(_selectedN);
                }
                else if (pair.Value == null)
                {
                    _graph.insert(_selectedN);
                    if (_graph.rotation(_selectedN)) _selectedSG.spriteR.transform.position = _selectedSG._pos;
                    else
                    {
                        _graph.delete(_selectedN);
                        _selectedSG._pos = _selectedSG.spriteR.transform.position;
                        _graph.insert(_selectedN);
                    }
                }
                else
                {
                    _graph.peres(pair.Value.gear, _selectedSG);
                    if (_graph.can_insert(_selectedSG))
                    {
                        _graph.insert(_selectedN);
                        if (_graph.rotation(_selectedN)) _selectedSG.spriteR.transform.position = _selectedSG._pos;
                        else
                        {
                            _graph.delete(_selectedN);
                            _selectedSG._pos = _selectedSG.spriteR.transform.position;
                            _graph.insert(_selectedN);
                        }
                    }
                    else
                    {
                        _selectedSG._pos = _selectedSG.spriteR.transform.position;
                        _graph.insert(_selectedN);
                    }
                }
                isRotate = _graph._nodes.TrueForAll(n => n.isMain ? _graph.calculate_velocity(n) : true);
                isMoving = false;
            }
            else if (Input.GetMouseButtonDown(1)) { } //_selectedSG.spriteR.sprite = ;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _selectedN = _graph.find(Camera.main.ScreenToWorldPoint(Input.mousePosition)) as Node;
            if (_selectedN != null && !_selectedN.isNotMoving)
            {
                _selectedSG = (SpriteGear)_selectedN.gear;
                //(_selectedSG = ((SpriteGear)_selectedN.gear)).spriteR.sprite = ;
                isMoving = true;
                isRotate = false;
            }
        }
    }
    
	void Update()
    {
        Moving();
        if (isRotate)
        foreach (Node n in _graph._nodes)
        {
			Transform t = ((SpriteGear)n.gear).spriteR.transform;
			t.Rotate(Time.deltaTime * Vector3.forward * (float)n.angular_velocity);
		}
	}

	void build_graph()
    {
		_graph = new Graph();
        var sr = gameObject.GetComponentsInChildren<SpriteRenderer>();
        byte i = 0;
		foreach (Node n in gameObject.GetComponentsInChildren<Node>())
        {
            n.neighbors = new List<Node>();
            n.gear = new SpriteGear(sr[i++]);
            _graph.insert(n);
        }
    }
}