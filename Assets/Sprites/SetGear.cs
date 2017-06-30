using UnityEngine;

public class SetGear : MonoBehaviour
{
	public Transform gear; //Сюда можно скинуть положение любого объекта
	private bool isRotate = false;
	void Start () //Конструктор
    {
	
	}
	void Update () //Срабатывает каждый кадр
    {
		if (Input.GetMouseButtonDown (0)) //Если отпущена левая кнопка мыши
        {
			RaycastHit hit; //Объект для хранения информации о объекте, в который попал луч.
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100)) //Если луч попал в какой-нибудь объект
                if (hit.collider.tag == "point") gear.position = hit.collider.transform.position; //Если имя коллайдера point, то поставить gear на место point
		}
		if (isRotate) gear.Rotate (Vector3.forward * (Time.deltaTime*12)); //Повернуть вокруг (0,0,1) на deltaTime*12
	}
	void OnGUI()
    {
		if (GUI.Button (new Rect (10, 10, 50, 30), "Click")) isRotate = true; //Если нажали на кнопку (форма Rect с названием Click)		
	}
}
