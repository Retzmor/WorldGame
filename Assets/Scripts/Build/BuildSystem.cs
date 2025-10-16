using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class BuildSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;                     // cámara ortográfica principal
    public Grid grid;                      // el Grid que contiene los Tilemaps
    public Tilemap groundTilemap;          // tilemap "tierra"
    public Tilemap waterTilemap;           // tilemap "agua"
    public Transform player;               // referencia al jugador (posición para rango)
    public Buildable currentBuildable;     // objeto seleccionado para construir

    [Header("Build Settings")]
    public int buildRange = 4;             // rango máximo en tiles (Manhattan)

    private GameObject ghost;
    private SpriteRenderer[] ghostRenderers;
    public bool buildMode = false;        // ← NUEVO: modo construcción activo/inactivo

    void Update()
    {
        // --- toggle de construcción con click derecho
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            buildMode = !buildMode;

            if (!buildMode) CancelBuild(); // apagar ghost si salgo del modo
        }

        if (!buildMode || currentBuildable == null) return;
        if (cam == null) cam = Camera.main;
        if (grid == null) return;

        // --- obtener mouse y convertir a world
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z)
        );
        mouseWorld.z = 0f;

        // --- celda bajo el mouse
        Vector3Int anchorCell = grid.WorldToCell(mouseWorld);

        // --- calcular footprint (desde la celda ancla)
        Vector2Int size = currentBuildable.size;
        Vector3Int startCell = new Vector3Int(
            anchorCell.x - size.x / 2,
            anchorCell.y - size.y / 2,
            anchorCell.z
        );

        // --- crear ghost si no existe
        if (ghost == null)
        {
            ghost = Instantiate(currentBuildable.prefab);
            SetGhostMode(ghost);
            ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>();
        }

        // --- centro del footprint
        Vector3 footprintCenter = Vector3.zero;
        int count = 0;
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int c = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);
                footprintCenter += grid.GetCellCenterWorld(c);
                count++;
            }
        if (count > 0) footprintCenter /= count;
        footprintCenter.z = 0f;

        ghost.transform.position = footprintCenter;

        // --- validación de colocación
        bool canPlace = CanPlaceAt(startCell, size);

        // --- color del ghost
        if (ghostRenderers != null)
        {
            Color c = canPlace ? new Color(0f, 1f, 0f, 0.5f)
                               : new Color(1f, 0f, 0f, 0.5f);
            foreach (var sr in ghostRenderers)
                if (sr != null) sr.color = c;
        }

        // --- click izquierdo coloca
        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceAt(startCell, size);
        }

        // --- cancelar con ESC (opcional)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelBuild();
            buildMode = false;
        }
    }

    private void PlaceAt(Vector3Int startCell, Vector2Int size)
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int c = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);
                center += grid.GetCellCenterWorld(c);
                count++;
            }
        if (count > 0) center /= count;
        center.z = 0f;

        Instantiate(currentBuildable.prefab, center, Quaternion.identity);
    }

    private bool CanPlaceAt(Vector3Int startCell, Vector2Int size)
    {
        if (player != null)
        {
            Vector3 footprintCenterWorld = Vector3.zero;
            int count = 0;
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    var c = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);
                    footprintCenterWorld += grid.GetCellCenterWorld(c);
                    count++;
                }
                
            if (count > 0) footprintCenterWorld /= count;

            Vector3Int footprintCenterCell = grid.WorldToCell(footprintCenterWorld);
            Vector3Int playerCell = grid.WorldToCell(player.position);

            int manhattan = Mathf.Abs(playerCell.x - footprintCenterCell.x) +
                            Mathf.Abs(playerCell.y - footprintCenterCell.y);
            if (manhattan > buildRange) return false;
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCell = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);

                if (waterTilemap != null && waterTilemap.HasTile(checkCell)) return false;
                if (groundTilemap != null && !groundTilemap.HasTile(checkCell)) return false;

                Vector3 checkCenter = grid.GetCellCenterWorld(checkCell);
                Collider2D hit = Physics2D.OverlapPoint(checkCenter);
                if (hit != null) return false;
            }
        }

        return true;
    }

    private void SetGhostMode(GameObject obj)
    {
        if (obj == null) return;
        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }

    private void CancelBuild()
    {
       // currentBuildable = null;
        if (ghost != null) { Destroy(ghost); ghost = null; }
    }

    public void SelectBuildable(Buildable buildable)
    {
        currentBuildable = buildable;
        if (ghost != null) { Destroy(ghost); ghost = null; }
    }
}
