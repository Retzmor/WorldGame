using Pathfinding;
using UnityEngine;

public class InfiniteGridUpdater : MonoBehaviour
{
    public Transform player;
    public float moveThreshold = 10f;
    public int gridSize = 50; // nodos de ancho/alto
    public float nodeSize = 1f; // tamaño de cada nodo

    private GridGraph grid;

    void Start()
    {
        grid = AstarPath.active.data.gridGraph;

        // Alinear el centro al tilemap desde el inicio
        grid.center = SnapToGrid(player.position);
        grid.SetDimensions(gridSize, gridSize, nodeSize);

        AstarPath.active.Scan();
    }

    void Update()
    {
        if (Vector3.Distance(grid.center, player.position) > moveThreshold)
        {
            // mover el centro, pero alineado al tilemap
            grid.center = SnapToGrid(player.position);

            // reescanear la zona
            AstarPath.active.Scan();
        }
    }

    /// <summary>
    /// Redondea una posición al múltiplo más cercano de nodeSize
    /// para que quede alineada con el tilemap.
    /// </summary>
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / nodeSize) * nodeSize;
        float y = Mathf.Round(pos.y / nodeSize) * nodeSize;
        return new Vector3(x, y, 0);
    }
}
