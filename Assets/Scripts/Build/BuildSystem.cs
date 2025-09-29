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

    void Update()
    {
        if (currentBuildable == null) return;
        if (cam == null) cam = Camera.main;
        if (grid == null) return;

        // --- obtener mouse (Input System) y convertir a world (plano z = 0)
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
        mouseWorld.z = 0f;

        // --- celda bajo el mouse (esta será la referencia para centrar la huella)
        Vector3Int anchorCell = grid.WorldToCell(mouseWorld);

        // --- calcular la lista de celdas que ocuparía la huella centrada en anchorCell
        Vector2Int size = currentBuildable.size;
        Vector3Int startCell = new Vector3Int(anchorCell.x - size.x / 2, anchorCell.y - size.y / 2, anchorCell.z);

        // if ghost missing -> crear (modo preview)
        if (ghost == null)
        {
            ghost = Instantiate(currentBuildable.prefab);
            SetGhostMode(ghost);
            ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>();
        }

        // --- calcular centro del footprint (promedio de los centros de cada celda)
        Vector3 footprintCenter = Vector3.zero;
        int count = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int c = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);
                footprintCenter += grid.GetCellCenterWorld(c);
                count++;
            }
        }
        if (count > 0) footprintCenter /= count;
        footprintCenter.z = 0f;

        // mover ghost al centro de la huella
        ghost.transform.position = footprintCenter;

        // comprobar si se puede colocar (agua, tierra, overlap, rango)
        bool canPlace = CanPlaceAt(startCell, size);

        // colorear ghost (verde/rojo)
        if (ghostRenderers != null)
        {
            Color c = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            foreach (var sr in ghostRenderers) if (sr != null) sr.color = c;
        }

        // colocar con click izquierdo
        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceAt(startCell, size);
        }

        // cancelar con tecla ESC (opcional)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelBuild();
        }
    }

    private void PlaceAt(Vector3Int startCell, Vector2Int size)
    {
        // instanciar en la misma posición del ghost (centro de footprint)
        // usar la misma lógica de centro para posicionamiento exacto
        Vector3 center = Vector3.zero;
        int count = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int c = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);
                center += grid.GetCellCenterWorld(c);
                count++;
            }
        }
        if (count > 0) center /= count;
        center.z = 0f;

        Instantiate(currentBuildable.prefab, center, Quaternion.identity);
    }

    /// <summary>
    /// Comprueba si la huella que empieza en startCell con tamaño size puede colocarse.
    /// </summary>
    private bool CanPlaceAt(Vector3Int startCell, Vector2Int size)
    {
        // 1) Rango Manhattan (usamos el centro de la huella como referencia)
        if (player != null)
        {
            // centro de la huella en celdas
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

            int manhattan = Mathf.Abs(playerCell.x - footprintCenterCell.x) + Mathf.Abs(playerCell.y - footprintCenterCell.y);
            if (manhattan > buildRange) return false;
        }

        // 2) Para cada celda dentro de la huella: comprobar agua, tierra y colisiones
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCell = new Vector3Int(startCell.x + x, startCell.y + y, startCell.z);

                // si alguna celda toca agua => no permitir
                if (waterTilemap != null && waterTilemap.HasTile(checkCell)) return false;

                // debe haber tierra en cada celda
                if (groundTilemap != null && !groundTilemap.HasTile(checkCell)) return false;

                // comprobación de colisiones físicas: OverlapPoint en el centro de esa celda
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
        // desactivar colisiones en el preview
        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // opcional: marcar layer para preview para evitar confusión
        // obj.layer = LayerMask.NameToLayer("IgnoreRaycast");
    }

    private void CancelBuild()
    {
        currentBuildable = null;
        if (ghost != null) { Destroy(ghost); ghost = null; }
    }

    public void SelectBuildable(Buildable buildable)
    {
        currentBuildable = buildable;
        if (ghost != null) { Destroy(ghost); ghost = null; }
    }
}
