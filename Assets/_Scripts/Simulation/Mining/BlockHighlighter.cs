using UnityEngine;

public class BlockHighlighter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject highlightCube;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Default");
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);
            highlightCube.transform.position = hitPosition;
            highlightCube.SetActive(true);
        }
        else
        {
            highlightCube.SetActive(false);
        }
    }
}