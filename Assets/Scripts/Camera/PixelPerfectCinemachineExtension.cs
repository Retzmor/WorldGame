using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")] // Oculta del menú normal
public class PixelPerfectCinemachineExtension : CinemachineExtension
{
    public float pixelsPerUnit = 32f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize && pixelsPerUnit > 0f)
        {
            var pos = state.RawPosition;
            pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
            pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
            state.RawPosition = pos;
        }
    }
}
