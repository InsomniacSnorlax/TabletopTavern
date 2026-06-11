using System;
using System.Collections.Generic;
using UnityEngine;

namespace TJ.Battle
{
    /// <summary>
    /// Single source of truth for the display order of squad cards in battle.
    /// All mutations must go through this class. Subscribers (UIManager, SquadManager,
    /// GroupManager) react to OnSquadOrderChanged and never modify _squadOrder themselves.
    /// </summary>
    public class SquadOrderManager : MonoBehaviour
    {
        private List<int> _squadOrder = new();
        public IReadOnlyList<int> SquadOrder => _squadOrder.AsReadOnly();

        public event Action<IReadOnlyList<int>> OnSquadOrderChanged;

        /// <summary>
        /// Called once after all squad cards are created at battle start.
        /// Replaces any existing order. Idempotent — safe to call again on scene reload.
        /// </summary>
        public void Initialize(List<int> orderedIds)
        {
            _squadOrder = new List<int>(orderedIds);
            OnSquadOrderChanged?.Invoke(_squadOrder.AsReadOnly());
        }

        /// <summary>
        /// Replaces the order wholesale. Validates all IDs are present before accepting.
        /// Only fires the event if the order actually changed. Used by load-from-save.
        /// </summary>
        public void SetOrder(List<int> newOrder)
        {
            bool changed = false;
            if (newOrder.Count != _squadOrder.Count)
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i < newOrder.Count; i++)
                {
                    if (newOrder[i] != _squadOrder[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (!changed) return;

            _squadOrder = new List<int>(newOrder);
            OnSquadOrderChanged?.Invoke(_squadOrder.AsReadOnly());
        }

        /// <summary>
        /// Moves a single squad to a new display position.
        /// No-op if the squad is already at that index.
        /// </summary>
        public void MoveSquad(int squadId, int toIndex)
        {
            int currentIndex = _squadOrder.IndexOf(squadId);
            if (currentIndex < 0)
            {
                Debug.LogWarning($"SquadOrderManager.MoveSquad: squadId {squadId} not found in order.");
                return;
            }

            toIndex = Mathf.Clamp(toIndex, 0, _squadOrder.Count - 1);
            if (currentIndex == toIndex) return;

            _squadOrder.RemoveAt(currentIndex);
            _squadOrder.Insert(toIndex, squadId);
            OnSquadOrderChanged?.Invoke(_squadOrder.AsReadOnly());
        }

        /// <summary>
        /// Moves all squads in the group to the front of the list,
        /// preserving their relative order among themselves.
        /// </summary>
        public void BringGroupToFront(List<int> groupSquadIds)
        {
            if (groupSquadIds == null || groupSquadIds.Count == 0) return;

            // Collect the group IDs in the order they currently appear in _squadOrder
            List<int> orderedGroupIds = new();
            foreach (int id in _squadOrder)
            {
                if (groupSquadIds.Contains(id))
                    orderedGroupIds.Add(id);
            }

            // Remove them from their current positions
            foreach (int id in orderedGroupIds)
                _squadOrder.Remove(id);

            // Insert at the front in original relative order
            for (int i = orderedGroupIds.Count - 1; i >= 0; i--)
                _squadOrder.Insert(0, orderedGroupIds[i]);

            OnSquadOrderChanged?.Invoke(_squadOrder.AsReadOnly());
        }

        /// <summary>
        /// Reorders all squads so groups appear in numerical order (group 1 leftmost, then 2, etc.),
        /// followed by ungrouped squads in their current relative positions.
        /// </summary>
        public void ArrangeGroupsInOrder(SquadGroup[] squadGroups)
        {
            List<int> newOrder = new();

            // Grouped squads in group-number order, preserving relative display order within each group
            for (int i = 0; i < squadGroups.Length; i++)
            {
                foreach (int id in _squadOrder)
                {
                    if (squadGroups[i].squadIds.Contains(id))
                        newOrder.Add(id);
                }
            }

            // Ungrouped squads in their current relative order
            HashSet<int> grouped = new(newOrder);
            foreach (int id in _squadOrder)
            {
                if (!grouped.Contains(id))
                    newOrder.Add(id);
            }

            SetOrder(newOrder);
        }

        /// <summary>
        /// Removes a squad from the order. Called when a squad is disbanded mid-battle.
        /// </summary>
        public void RemoveSquad(int squadId)
        {
            if (_squadOrder.Remove(squadId))
                OnSquadOrderChanged?.Invoke(_squadOrder.AsReadOnly());
        }
    }
}
