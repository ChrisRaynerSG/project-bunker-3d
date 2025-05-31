using UnityEngine;

public class BlockHighlighter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject highlightCube;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
            highlightCube.transform.position = hitPosition;
            highlightCube.SetActive(true);
        }
        else
        {
            highlightCube.SetActive(false);
        }
    }
}