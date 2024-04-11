// using System.Collections.Generic;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.EventSystems;

// public class RoadGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
// {
//     public Road Road { get; set; }

//     public List<float3> LeftMesh { get; set; }
//     public List<float3> RightMesh { get; set; }
//     public Mesh OriginalMesh { get; set; }
//     public bool MouseOver { get; set; }

//     public RoadGameObject()
//     {
//         MouseOver = false;
//     }

//     public void OnPointerEnter(PointerEventData eventData)
//     {
//         MouseOver = true;
//     }

//     public void OnPointerExit(PointerEventData eventData)
//     {
//         MouseOver = false;
//     }
// }