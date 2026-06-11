using ProjectDawn.Navigation.Hybrid;
using TMPro;
using UnityEngine;
using ProjectDawn.Navigation;
using Unity.Mathematics;

namespace TJ
{
    public class SquadNavObject : MonoBehaviour
    {
        [SerializeField] private Transform goalPosition, centerPosition;
        [SerializeField] private quaternion squadRotation;
        [SerializeField] private TMP_Text goalPositionText, centerPositionText;
        private bool showDebug;
        [SerializeField] private GameObject debugStuff, _rotationIndicator;
        [SerializeField] private Material flankingMaterial, notFlankingMaterial;
        private float flankingAngleThresholdPlayer = -0.1f;
        private float flankingAngleThresholdEnemy = -0.7f;
        float flankingAngleThreshold;
        bool isPlayerSquad;

        public void SetUp(int _squadID)
        {
            gameObject.name = $"_squadID:{_squadID} SquadNavObject";
            isPlayerSquad = _squadID > 0;
            flankingAngleThreshold = isPlayerSquad ? flankingAngleThresholdPlayer : flankingAngleThresholdEnemy;
            Color textColor = isPlayerSquad ? Color.white : Color.red;
            goalPositionText.text = $"Goal Pos {_squadID}";
            centerPositionText.text = $"Center Pos {_squadID}";
            goalPositionText.color = textColor;
            centerPositionText.color = textColor;
            showDebug = PlayerPrefs.GetInt("DebugSquadNavObjects", 0) == 1;
            debugStuff.SetActive(showDebug);
            if (showDebug)
            {
                goalPosition.SetParent(null);
                MeshRenderer[] renderers = _rotationIndicator.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material = notFlankingMaterial;
                }
                _rotationIndicator.transform.localPosition += new Vector3(0, _squadID*0.2f, 0);
            }
        }

        public void UpdateNavObject(SquadMovementComponent _squadEntityMovementComponent)
        {
            transform.SetPositionAndRotation(_squadEntityMovementComponent.SquadCenter, _squadEntityMovementComponent.SquadRotation);

            // Debug forward vector of this squad's NavMeshAgent
            if (showDebug)
            {
                goalPosition.position = _squadEntityMovementComponent.GoalPosition;
                centerPosition.position = _squadEntityMovementComponent.SquadCenter;
            
                _rotationIndicator.transform.rotation = transform.rotation;
            }
        }

        public bool IsFlanking(SquadNavObject otherSquad)
        {
            Vector3 thisPos = centerPosition.position;
            Vector3 otherPos = otherSquad.centerPosition.position;
            Vector3 toThisSquad = (thisPos - otherPos).normalized;

            Vector3 targetForward = otherSquad.transform.rotation * Vector3.forward;

            // Dot product: 1 = directly in front, 0 = side, -1 = directly behind
            float facingDot = Vector3.Dot(targetForward, toThisSquad);

            bool isFlanking = facingDot < flankingAngleThreshold;
            // Debug.Log($"Facing dot product: {facingDot} (threshold: {flankingAngleThreshold}) isFlanking: {isFlanking}");

            if (showDebug)
            {
                MeshRenderer[] renderers = _rotationIndicator.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material = isFlanking ? flankingMaterial : notFlankingMaterial;
                }
            }

            return isFlanking;
        }
    }
}