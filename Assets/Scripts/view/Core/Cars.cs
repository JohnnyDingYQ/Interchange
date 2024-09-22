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
            foreach (Car car in Game.Cars.Values)
            {
                car.Move(Time.deltaTime);
                if (car.Status == CarStatus.Finished)
                {
                    Debug.Log(toRemove.Count);
                    Assert.IsTrue(!toRemove.Contains(car));
                    toRemove.Add(car);
                }
            }
            List<Road> roadsInSight = roads.GetRoadsInSight();
            GetVertexInSight(roadsInSight);
            int numCarsInSight = CountCarInSight();
            if (numCarsInSight < verticesInSight.Count() && verticesInSight.Count() > 1)
            {
                int numCarToSpawn = verticesInSight.Count() - numCarsInSight;
                for (int i = 0; i < numCarToSpawn; i++)
                {
                    Vertex selectedV = verticesInSight[MyNumerics.GetRandomIndex(verticesInSight.Count)];
                    Vertex otherV = verticesInSight[MyNumerics.GetRandomIndex(verticesInSight.Count)];
                    IEnumerable<Edge> edges = Graph.AStar(selectedV, otherV);
                    if (edges == null)
                        continue;
                    Path path = new(edges);
                    Assert.AreNotEqual(selectedV.Id, otherV.Id);
                    Game.RegisterCar(new(path));
                }
            }
            verticesInSight.Clear();
        }
        else
        {
            toRemove.AddRange(Game.Cars.Values);
            foreach (Edge edge in Game.Edges.Values)
                edge.Cars.Clear();
        }

        for (int i = 0; i < toRemove.Count; i++)
            Game.RemoveCar(toRemove[i]);
        toRemove.Clear();

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