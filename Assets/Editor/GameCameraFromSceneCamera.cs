using UnityEngine;

[ExecuteInEditMode]
public class GameCameraFromSceneCamera : MonoBehaviour
{
#if UNITY_EDITOR
    Camera _mainCam;

    void OnEnable() => UnityEditor.EditorApplication.update += UpdateCameras;
    void OnDisable() => UnityEditor.EditorApplication.update -= UpdateCameras;

    void UpdateCameras()
    {
        if (UnityEditor.SceneView.sceneViews.Count == 0) return;

        Camera sceneViewCam = ((UnityEditor.SceneView)UnityEditor.SceneView.sceneViews[0]).camera;
        if (!sceneViewCam) return;

        _mainCam ??= Camera.main;
        if (!_mainCam) return;

        _mainCam.transform.SetPositionAndRotation(sceneViewCam.transform.position, sceneViewCam.transform.rotation);
    }
#endif
}
