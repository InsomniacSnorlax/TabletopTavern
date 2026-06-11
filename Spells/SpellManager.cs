using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memori.Audio;
using Memori.Notifications;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TJ.Spells
{
public class SpellManager : MonoBehaviour
{
    // [SerializeField] private SpellDataOld[] spells;
    [SerializeField] private SpellData[] selectedSpells;
    public SpellData[] SpellIds => selectedSpells;
    [SerializeField] private List<GreyComanySpell> spellPrefabs;
    [SerializeField] private SpellCastButton spellUIPrefab;
    [SerializeField] private Transform spellUIButtonParent;
    private Dictionary<SpellName, SpellCastButton> spellButtonDictionary = new();
    private Dictionary<int, SpellName> spellNameToButtonIndex = new();
    private bool validSpellCastPoint;
    private Vector3 spellCursorOrigin;
    private SpellName selectedSpell;
    SpellData selectedSpellData;
    public Vector3 SpellCursorOrigin => spellCursorOrigin;
    public bool ValidSpellCastPoint => validSpellCastPoint;
    [SerializeField] private LayerMask validSpellCastLayerMask;
    int spellsCast = 0;
    public int SpellsCast => spellsCast; 
    bool mouseReleased = true;
    public bool MouseReleased => mouseReleased;
    public float SelectedSpellRadius => selectedSpellData.SpellRadius;

    private void Start()
    {

        #if UNITY_EDITOR
            // if(!SceneManager.GetSceneByBuildIndex(0).isLoaded) {
                selectedSpells = new SpellData[] {
                    SpellDataLibrary.LightningStrike,
                    SpellDataLibrary.NaturesWrath,
                };
                LoadSpells(selectedSpells);
            // }
        #endif
        BattleManager.Instance.OnCursorModeChanged += CursorModeChanged;
    }
    public void LoadSpells(SpellData[] _spells)
    {
        selectedSpells = _spells;
        spellButtonDictionary.Clear();
        spellNameToButtonIndex.Clear();
        for(int i = spellUIButtonParent.childCount - 1; i >= 0; i--) {
            Destroy(spellUIButtonParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < selectedSpells.Length; i++) {
            var spellUIButton = Instantiate(spellUIPrefab, spellUIButtonParent);
            spellUIButton.LoadSpellUI(selectedSpells[i], this, i+1);
            spellButtonDictionary.Add(selectedSpells[i].SpellName, spellUIButton);
            spellNameToButtonIndex.Add(i+1, selectedSpells[i].SpellName);
        }
    }
    public void SelectSpell(SpellName _spellName, bool hotkeyUseIndex = false)
    {
        DeselectSpell();

        //so that we can use the hotkey to select the spell
        // if(hotkeyUseIndex){
        //     if(!spellIdToButtonIndex.ContainsKey(_spellSelectedId)){
        //         // GameManager.Instance.IAudioRequester.PlayActionFailedSFX();
        //         return;
        //     }
        //     _spellSelectedId = spellIdToButtonIndex[_spellSelectedId];
        // }

        selectedSpell = _spellName;
        selectedSpellData = SpellDataLibrary.GetSpellData(selectedSpell);
        
        //check to see if locked
        // if(!spellButtonDictionary.ContainsKey(selectedSpellId)){
        //     // Runtime.Instance.NotificationManager.ErrorNotification("Spell is locked");
        //     return;
        // }

        //check to make sure spell is not on cooldown 
        if(spellButtonDictionary[selectedSpell].OnCooldown) {
            spellButtonDictionary[selectedSpell].FlashCooldownImage(Color.red);
            // Runtime.Instance.NotificationManager.ErrorNotification("Spell is on cooldown");

            return;
        }

        spellButtonDictionary[selectedSpell].SelectSpell();

        if(BattleManager.Instance.CursorMode != CursorMode.CastSpell){
            BattleManager.Instance.SetCursorMode(CursorMode.CastSpell);
        }
    }
    public void DeselectSpell()
    {
        if(spellButtonDictionary.ContainsKey(selectedSpell))
            spellButtonDictionary[selectedSpell].DeselectSpell();
    }
    public void AttemptCastSpell()
    {
        if(!validSpellCastPoint){
            NotificationManager.Instance.ErrorNotification("Invalid Spell Cast Point");
            return;
        }

        IAudioRequester.Instance.PlaySFX("cast-spell");
        CastSpell();
    }
    public IEnumerator GetMouseCursorPosition()
    {
        while(BattleManager.Instance.CursorMode == CursorMode.CastSpell)
        {
            if(Input.GetMouseButtonDown(1)){
                // Debug.Log($"Casting spell {selectedSpell} cancelled");
                BattleManager.Instance.SetCursorMode(CursorMode.Free);
                yield break;
            }

            if(Input.GetMouseButtonDown(0)){
                // Debug.Log($"Casting spell {selectedSpell}");
                AttemptCastSpell();
                // yield break;
            }
            
            Vector3 CastPoint = MouseWorldPosition.Instance.GetWorldPosition() + (Vector3.up*10f);

            if(selectedSpellData.SpellTargetingType == SpellTargetingType.Squad)
            {
                int hoveredSquadIndex = BattleManager.Instance.UIManager.HoveredSquadId;

                if(hoveredSquadIndex != 0) {
                    SquadEntity squadEntity = BattleManager.Instance.SquadManager.GetSquad(hoveredSquadIndex);
                    // bool validTarget = false;
                    // if (team.Player == selectedSpellData.Targetteam && hoveredSquadIndex > 0) {
                    //     validTarget = true;
                    // } else if (team.Enemy == selectedSpellData.Targetteam && hoveredSquadIndex < 0) {
                    //     validTarget = true;
                    // }

                    // if(validTarget) {
                    //     CastPoint = squadEntity.TrueSquadCenter;
                    //     validSpellCastPoint = true;
                    //     spellCursorOrigin = squadEntity.TrueSquadCenter;
                    // } else {
                        validSpellCastPoint = false;
                        spellCursorOrigin = MouseWorldPosition.Instance.GetWorldPosition();// + Vector3.up * 0.1f;
                    // }

                } else {
                    validSpellCastPoint = false;
                    spellCursorOrigin = MouseWorldPosition.Instance.GetWorldPosition();// + Vector3.up * 0.1f;
                }
            } else if (selectedSpellData.SpellTargetingType == SpellTargetingType.World) {

                if(Physics.Raycast(CastPoint, Vector3.down, 20, validSpellCastLayerMask)) {
                    validSpellCastPoint = true;
                } else {
                    validSpellCastPoint = false;
                }
                spellCursorOrigin = MouseWorldPosition.Instance.GetWorldPosition();// + Vector3.up * 0.1f;
            }

            yield return null;
        }
    }
    public async void CastSpell()
    {
        if(!spellButtonDictionary.ContainsKey(selectedSpell)){
            Debug.LogError($"tf not found: {selectedSpell}");
            return;
        }

        for (int i = 0; i < selectedSpells.Length; i++) {
            if (selectedSpells[i].SpellName == selectedSpell) {
                GreyComanySpell spell = Instantiate(GetSpellPrefab(selectedSpells[i].SpellName), spellCursorOrigin, Quaternion.identity);
                spell.Load(SpellDataLibrary.GetSpellData(selectedSpells[i].SpellName), MouseWorldPosition.Instance.GetWorldPosition());
                break;
            }
        }

        spellButtonDictionary[selectedSpell].ApplyCooldown();
        
        spellsCast++;

        mouseReleased = false;
        while(!mouseReleased){
            if(Input.GetMouseButtonUp(0)){
                mouseReleased = true;
                BattleManager.Instance.SetCursorMode(CursorMode.Free);
            }
            await Task.Yield();
        }
        // if(spellGO.ShakeOnSpellCast) Runtime.Instance.CameraManager.LittleCameraShake();
    }
    public GreyComanySpell GetSpellPrefab(SpellName _spellName)
    {
        for (int i = 0; i < spellPrefabs.Count; i++) {
            if (spellPrefabs[i].SpellName == _spellName) {
                return spellPrefabs[i];
            }
        }
        Debug.LogError($"Spell prefab not found: {_spellName}");
        return null;
    }
    public void TakeTimeOffCooldowns(float time)
    {
        for (int i = 0; i < selectedSpells.Length; i++) {

            if (spellButtonDictionary[selectedSpells[i].SpellName].OnCooldown) {
                spellButtonDictionary[selectedSpells[i].SpellName].ReduceCooldown(time);
            }
        }
    }
    public void CursorModeChanged(CursorMode _cursorMode)
    {
        if(_cursorMode == CursorMode.CastSpell) {
            StartCoroutine(GetMouseCursorPosition());
        } else {
            DeselectSpell();
        }
    }
    private void OnDestroy() 
    {
        if(BattleManager.Instance != null)
        {
            BattleManager.Instance.OnCursorModeChanged -= CursorModeChanged;
        }
    }
}
}
