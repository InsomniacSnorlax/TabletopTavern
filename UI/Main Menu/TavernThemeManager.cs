using UnityEngine;
using Memori.Utilities;
using Memori.SaveData;

namespace TJ.MainMenu
{
    public class TavernThemeManager : Singleton<TavernThemeManager>
    {
        [SerializeField] private TavernThemeData[] _allThemes;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private Material _flagMaterial;
        [SerializeField] private Material _tableClothMaterial1, _tableClothMaterial2;
        [SerializeField] private MeshRenderer[] _flags;
        Material _flagMaterialInstance;
        GameObject _activeThemeInstance;
        TavernThemeData themeToLoad;
        int _loadGeneration;

        private async void Start()
        {
            _flagMaterialInstance = Instantiate(_flagMaterial);
            CheckAndUnlockCompletedThemes();
            themeToLoad = GetThemeForCurrentSave();
// #if !UNITY_EDITOR
            await LoadTheme(themeToLoad);
// #endif
        }

        private void CheckAndUnlockCompletedThemes()
        {
            foreach (TavernThemeData theme in _allThemes)
            {
                if (theme.Race == Race.Special) continue;
                if (!SaveDataHandler.IsTavernThemeUnlocked(theme.Race) &&
                    SaveDataHandler.HasCompletedGodkingWithRace(theme.Race))
                {
                    SaveDataHandler.UnlockTavernTheme(theme.Race);
                }
            }
        }

        private TavernThemeData GetThemeForCurrentSave()
        {
            if (SaveDataHandler.TryGetActiveTavernTheme(out Race savedRace))
            {
                foreach (TavernThemeData theme in _allThemes)
                {
                    if (theme.Race == savedRace)
                        return theme;
                }
            }

            // Fall back to the Special (None) theme
            foreach (TavernThemeData theme in _allThemes)
            {
                if (theme.Race == Race.Special)
                    return theme;
            }

            return null;
        }

        public async void ApplyTheme(TavernThemeData _theme)
        {
            themeToLoad = _theme;
            await LoadTheme(themeToLoad);
        }

        public void UnloadTheme()
        {
            if (_activeThemeInstance != null)
            {
                Destroy(_activeThemeInstance);
                _activeThemeInstance = null;
            }
        }

        private async System.Threading.Tasks.Task LoadTheme(TavernThemeData _theme)
        {
            UnloadTheme();

            if (_theme == null || _theme.ThemeObjects == null || !_theme.ThemeObjects.RuntimeKeyIsValid())
                return;

            int generation = ++_loadGeneration;
            GameObject prefab = await AddressablesManager.Instance.LoadAsync<GameObject>(_theme.ThemeObjects);
            if (prefab == null || generation != _loadGeneration) return;

            _activeThemeInstance = Instantiate(prefab, _spawnParent);
            ColorFlags();
        }
        private void ColorFlags()
        {
            // Debug.Log($"[TavernThemeManager] Coloring flags for theme: {themeToLoad.ThemeName}");
            //get all flag components in the theme instance and set their colors based on the race data
            _flagMaterialInstance.SetColor("_PrimaryColor", themeToLoad.RaceData.PrimaryColor);
            _flagMaterialInstance.SetColor("_SecondaryColor", themeToLoad.RaceData.SecondaryColor);
            _flagMaterialInstance.SetColor("_OutlineColor", themeToLoad.RaceData.AccentColor);
            // _flagMaterialInstance.SetTexture("_IconSprite", null); 
            
            foreach (MeshRenderer flag in _flags)
            {
                flag.material = _flagMaterialInstance;
            }

            _tableClothMaterial1.color = themeToLoad.RaceData.PrimaryColor;
            _tableClothMaterial2.color = themeToLoad.RaceData.SecondaryColor;
        }
    }
}
