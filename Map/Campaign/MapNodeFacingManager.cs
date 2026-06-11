using System.Collections.Generic;
using UnityEngine;

namespace TJ.Map
{
    public class MapNodeFacingManager : MonoBehaviour
    {
        public static MapNodeFacingManager Instance { get; private set; }

        private struct FacingEntry
        {
            public Transform transform;
            public Renderer renderer;
            public bool lockY;
        }

        private Camera _camera;
        private readonly List<FacingEntry> _entries = new();
        private Vector3 _lastCameraPos;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetCamera(Camera cam)
        {
            _camera = cam;
            _lastCameraPos = cam.transform.position - Vector3.one; // force initial update
        }

        public void Register(Transform t, bool lockY = true)
        {
            if (t != null)
                _entries.Add(new FacingEntry { transform = t, renderer = null, lockY = lockY });
        }

        public void Register(Transform t, Renderer r, bool lockY = true)
        {
            if (t != null && r != null)
                _entries.Add(new FacingEntry { transform = t, renderer = r, lockY = lockY });
        }

        public void Unregister(Transform t) => _entries.RemoveAll(e => e.transform == t);

        private void LateUpdate()
        {
            if (_camera == null) return;

            Vector3 camPos = _camera.transform.position;
            if (camPos == _lastCameraPos) return;
            _lastCameraPos = camPos;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.transform == null) { _entries.RemoveAt(i); continue; }
                if (entry.renderer != null && !entry.renderer.isVisible) continue;
                FaceCamera(entry.transform, camPos, entry.lockY);
            }
        }

        private static void FaceCamera(Transform t, Vector3 camPos, bool lockY)
        {
            Vector3 dir = t.position - camPos;
            if (lockY) dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                t.rotation = Quaternion.LookRotation(dir);
        }
    }
}
