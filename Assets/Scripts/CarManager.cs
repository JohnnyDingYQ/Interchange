using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using UnityEngine.Splines;

public class CarManager : MonoBehaviour
{
    [SerializeField] private Car carPreFab;
    private Dictionary<int, Car> cars = new();
    private int id = 0;

    // Start is called before the first frame update
    void Start()
    {
        int origin = Grid.Height * Grid.Width/2;
        int destination = origin + Grid.Height - 1;
        Car car = Instantiate(carPreFab, Grid.GetPosByID(origin), Quaternion.identity);
        car.setID(id);
        car.Origin = origin;
        PathGraph.Graph.AddVertex(origin);
        PathGraph.Graph.AddVertex(destination);
        car.Destination = destination;
        cars[id] = car;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<int> toRemove = new();
            foreach (KeyValuePair<int, Car> entry in cars)
            {
                Car car = entry.Value;
                IEnumerable<TaggedEdge<int, Spline>> edges = PathGraph.GetPath(car.Origin, car.Destination);
                if (edges != null)
                {
                    car.Cruise(edges);
                    toRemove.Add(entry.Key);
                    Debug.Log("GOGOGO");
                }
            }
            foreach (int id in toRemove)
            {
                cars.Remove(id);
            }
        }

    }
}
