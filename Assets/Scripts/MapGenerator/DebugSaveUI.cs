using UnityEngine;

public class DebugSaveUI : MonoBehaviour
{
    public WorldSaveSystem saveSystem;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));

        if (GUILayout.Button("Guardar partida"))
        {
            saveSystem.SaveWorld();
        }

        if (GUILayout.Button("Cargar partida"))
        {
            saveSystem.LoadWorld();
        }

        if (GUILayout.Button("Borrar partida"))
        {
            saveSystem.DeleteWorld();
        }

        GUILayout.EndArea();
    }

}
