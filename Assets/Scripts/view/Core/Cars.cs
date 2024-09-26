using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Assets.Scripts.Model.Roads;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;
using KaimiraGames;

public class Cars : MonoBehaviour
{
    [SerializeField] CarObject carPrefab;
    Dictionary<uint, CarObject> carMapping;
    ObjectPool<CarObject> carPool;
    [SerializeField] CameraSettings cameraSettings;
    [SerializeField] Roads roads;
    public static float TimeScale = 1;
    List<Vertex> verticesInSight;
    List<Car> toRemove;

    void Awake()
    {
        Game.CarAdded += CarAdded;
        Game.CarRemoved += RemoveCar;
        CreatePool();
        carMapping = new();
        verticesInSight = new();
        toRemove = new();
    }

    void Update()
    {
        if (Camera.main.transform.position.y < cameraSettings.ShowCarHeightBar)
        {
            MoveCars();
            List<Road> roadsInSight = roads.GetRoadsInSight();
            GetVertexInSight(roadsInSight);
            int numCarsInSight = CountCarInSight();
            if (numCarsInSight < verticesInSight.Count() && Game.Vertices.Count() > 1)
                SpawnCars(numCarsInSight);
            verticesInSight.Clear();
        }
        else
            RemoveAllCars();
        ApplyCarRemoval();

        void GetVertexInSight(List<Road> roadsInSight)
        {
            for (int i = 0; i < roadsInSight.Count; i++)
            {
                Road road = roadsInSight[i];
                for (int j = 0; j < road.LaneCount; j++)
                {
                    Lane lane = road.Lanes[j];
                    if (PosInSight(lane.StartVertex.Pos))
                        verticesInSight.Add(lane.StartVertex);
                    if (PosInSight(lane.EndVertex.Pos))
                        verticesInSight.Add(lane.EndVertex);
                }
            }
        }

        void MoveCars()
        {
            foreach (Car car in Game.Cars.Values)
            {
                if (Game.Edges.ContainsValue(car.CurrentEdge))
                {
                    car.Move(Time.deltaTime);
                    if (car.Status == CarStatus.Finished)
                        toRemove.Add(car);
                }
                else
                    toRemove.Add(car);
            }
        }

        void SpawnCars(int numCarsInSight)
        {
            WeightedList<Vertex> myWL = new();
            foreach (Vertex vertex in verticesInSight)
            {
                myWL.Add(vertex, (int)vertex.Lane.Length);
            }
            int numCarToSpawn = verticesInSight.Count() - numCarsInSight;
            for (int i = 0; i < numCarToSpawn; i++)
            {
                Vertex selectedV = myWL.Next();
                Vertex otherV = Game.Vertices.Values.ElementAt(MyNumerics.GetRandomIndex(Game.Vertices.Count));
                IEnumerable<Edge> edges = Graph.AStar(selectedV, otherV);
                if (edges == null)
                    continue;
                Path path = new(edges);
                Game.RegisterCar(new(path));
            }
        }

        void RemoveAllCars()
        {
            toRemove.AddRange(Game.Cars.Values);
            foreach (Edge edge in Game.Edges.Values)
                edge.Cars.Clear();
        }

        void ApplyCarRemoval()
        {
            for (int i = 0; i < toRemove.Count; i++)
                Game.RemoveCar(toRemove[i]);
            toRemove.Clear();
        }
    }

    bool PosInSight(float3 pos)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(pos);

        if (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z >= 0)
            return true;
        return false;
    }

    int CountCarInSight()
    {
        int count = 0;
        foreach (Car car in Game.Cars.Values)
            if (PosInSight(car.Pos))
                count++;
        return count;
    }



    void OnDestroy()
    {
        Game.CarAdded -= CarAdded;
        Game.CarRemoved -= RemoveCar;
    }

    void CreatePool()
    {
        carPool = new(
            () => Instantiate(carPrefab, transform),
            (o) => { o.gameObject.SetActive(true); o.gameObject.transform.position = new(0, -100, 0); },
            (o) => o.gameObject.SetActive(false),
            (o) => Destroy(o.gameObject),
            false,
            200,
            Constants.MaxCarCount
        );
    }

    void CarAdded(Car car)
    {
        CarObject carObject = carPool.Get();
        carObject.Car = car;
        carMapping[car.Id] = carObject;
        carObject.gameObject.layer = LayerMask.NameToLayer("Cars");
    }

    void RemoveCar(Car car)
    {
        carPool.Release(carMapping[car.Id]);
        carMapping.Remove(car.Id);
    }

    public void DestoryAll()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        carMapping.Clear();
        CreatePool();
    }
}