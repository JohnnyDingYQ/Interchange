// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using QuikGraph;
// using UnityEngine.Splines;
// using Unity.Mathematics;


// public class Car : MonoBehaviour
// {
//     public int Origin {get; set;}
//     public int Destination {get; set;}
//     private int carID;
//     private float timeElapsed;
//     // Start is called before the first frame update
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     public void setID(int id)
//     {
//         carID = id;
//     }
//     public void Cruise(IEnumerable<TaggedEdge<int, Spline>> edges)
//     {
//         StartCoroutine(CruiseStepper(edges));
//     }

//     IEnumerator CruiseStepper(IEnumerable<TaggedEdge<int, Spline>> edges)
//     {
//         foreach (TaggedEdge<int, Spline> edge in edges)
//         {
//             // while (!transform.position.Equals(tile.transform.position))
//             // {
//             //     transform.position = Vector3.MoveTowards(
//             //     transform.position,
//             //     tile.transform.position,
//             //     5 * Time.deltaTime
//             // );
//             timeElapsed = 0;
//             Spline spline = edge.Tag;
//             float speed = 0.05f;
//             float length = spline.GetLength();
//             float time = length/speed /165.0f;
//             while (timeElapsed < time)
//             {
//                 spline.Evaluate(timeElapsed/time, out float3 position, out float3 tangent, out float3 upVector);
//                 transform.position = position;
//                 timeElapsed += Time.deltaTime;
//                 yield return null;
//             }
//         }
//     }
// }
