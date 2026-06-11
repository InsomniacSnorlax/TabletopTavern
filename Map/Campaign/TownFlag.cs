using UnityEngine;

namespace TJ.Map
{
    public class TownFlag : MonoBehaviour
    {
        [SerializeField] private MeshRenderer flagMeshRenderer, minorHordeFlagMeshRenderer1, minorHordeFlagMeshRenderer2;

        [Header("Flag Materials")]
        [SerializeField] private Material raceFlagBaseMaterial;
        [SerializeField] private Sprite townSprite, hordeSprite, hordeMinorSprite;
        [SerializeField] private RaceData defaultRaceData;

        private Material _flagMaterial;
        private Material _minorHordeMaterial;

        public void SetUp(int _nodeIndex, int bookNumber)
        {
            Race townRace = CampaignSaveManager.GenerateTownRace(_nodeIndex, bookNumber);
            RaceData raceData = Application.isPlaying ? TabletopTavernData.Instance.GetRaceData(townRace) : defaultRaceData;
            if (_flagMaterial != null) Destroy(_flagMaterial);
            _flagMaterial = new (raceFlagBaseMaterial);
            _flagMaterial.SetTexture("_IconSprite", townSprite.texture);
            _flagMaterial.SetColor("_PrimaryColor", raceData.PrimaryColor);
            _flagMaterial.SetColor("_SecondaryColor", raceData.SecondaryColor);
            _flagMaterial.SetColor("_OutlineColor", raceData.AccentColor);
            flagMeshRenderer.material = _flagMaterial;
        }
        public void SetHordeFlag(Race race)
        {
            RaceData raceData = Application.isPlaying ? TabletopTavernData.Instance.GetRaceData(race) : defaultRaceData;
            if (_flagMaterial != null) Destroy(_flagMaterial);
            _flagMaterial = new (raceFlagBaseMaterial);
            _flagMaterial.SetTexture("_IconSprite", hordeSprite.texture);
            _flagMaterial.SetColor("_PrimaryColor", raceData.PrimaryColor);
            _flagMaterial.SetColor("_SecondaryColor", raceData.SecondaryColor);
            _flagMaterial.SetColor("_OutlineColor", raceData.AccentColor);
            flagMeshRenderer.material = _flagMaterial;

            if (_minorHordeMaterial != null) Destroy(_minorHordeMaterial);
            _minorHordeMaterial = new (raceFlagBaseMaterial);
            _minorHordeMaterial.SetTexture("_IconSprite", hordeMinorSprite.texture);
            _minorHordeMaterial.SetColor("_PrimaryColor", raceData.PrimaryColor);
            _minorHordeMaterial.SetColor("_SecondaryColor", raceData.SecondaryColor);
            _minorHordeMaterial.SetColor("_OutlineColor", raceData.AccentColor);
            minorHordeFlagMeshRenderer1.material = _minorHordeMaterial;
            minorHordeFlagMeshRenderer2.material = _minorHordeMaterial;
        }
        public void SetSkirmishFlag(Race race)
        {
            RaceData raceData = Application.isPlaying ? TabletopTavernData.Instance.GetRaceData(race) : defaultRaceData;
            if (_flagMaterial != null) Destroy(_flagMaterial);
            _flagMaterial = new (raceFlagBaseMaterial);
            _flagMaterial.SetColor("_PrimaryColor", raceData.PrimaryColor);
            _flagMaterial.SetColor("_SecondaryColor", raceData.SecondaryColor);
            _flagMaterial.SetColor("_OutlineColor", raceData.AccentColor);
            _flagMaterial.SetTexture("_IconSprite", hordeMinorSprite.texture);
            flagMeshRenderer.material = _flagMaterial;
        }
        private void OnDestroy()
        {
            if (_flagMaterial != null) Destroy(_flagMaterial);
            if (_minorHordeMaterial != null) Destroy(_minorHordeMaterial);
        }
    }
}