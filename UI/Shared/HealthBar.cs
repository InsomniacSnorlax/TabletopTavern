using UnityEngine;
using UnityEngine.UI;

namespace TJ
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider, moraleSlider, ammoSlider;
        public Slider HealthSlider => healthSlider;
        public Slider MoraleSlider => moraleSlider;
        public Slider AmmoSlider => ammoSlider;
        [SerializeField] private Image brokenImage;
        [SerializeField] private Image fill;
        public Image Fill => fill;
        [SerializeField] private Color playerColor, enemyColor;
        [SerializeField] private Transform squadTransform;
        public Transform SquadTransform => squadTransform;

        [Header("Status Effect Icons")]
        [SerializeField] private GameObject fireAtWillGO;
        [SerializeField] private GameObject chargeGO, terrifiedGO, exhaustedGO, weaponStrengthGO, armorSunderedGO, isTakingFlankingDamageGO, isFlankingGO, isOnFireGO, defensiveStanceGO, bracedGO, outOfAmmoGO;
        private bool _isExhausted = false;
        public bool IsExhausted => _isExhausted;
        private bool _weaponStrengthBonusActive = false;
        public bool WeaponStrengthBonusActive => _weaponStrengthBonusActive;
        private bool _armorSunderedActive = false;
        public bool ArmorSunderedActive => _armorSunderedActive;
        private bool _isTakingFlankingDamage = false;
        public bool IsTakingFlankingDamage => _isTakingFlankingDamage;
        private bool _isFlanking = false;
        public bool IsFlanking => _isFlanking;
        private bool _isTakingFireDamage = false;
        public bool IsTakingFireDamage => _isTakingFireDamage;
        private bool _isFireAtWill = false;
        public bool IsFireAtWill => _isFireAtWill;
        private bool _isDefensiveStance = false;
        public bool IsDefensiveStance => _isDefensiveStance;
        private bool _isBraced = false;
        public bool IsBraced => _isBraced;
        private bool _isOutOfAmmo = false;
        public bool IsOutOfAmmo => _isOutOfAmmo;
        
        public void SetUp(Team _team, Transform _squadTransform, int ammunition, bool isGate)
        {
            squadTransform = _squadTransform;
            fill.color = _team == Team.Player ? playerColor : enemyColor;
            DisableAllStatusIcons();
            
            if (ammunition > 0)
            {
                ammoSlider.gameObject.SetActive(true);
                ammoSlider.maxValue = ammunition;
                ammoSlider.value = ammunition;
            }
            else
            {
                ammoSlider.gameObject.SetActive(false);
            }
            
            if(isGate)
                moraleSlider.gameObject.SetActive(false);
        }
        private void DisableAllStatusIcons()
        {
            chargeGO.SetActive(false);
            terrifiedGO.SetActive(false);
            exhaustedGO.SetActive(false);
            weaponStrengthGO.SetActive(false);
            armorSunderedGO.SetActive(false);
            isTakingFlankingDamageGO.SetActive(false);
            isFlankingGO.SetActive(false);
            isOnFireGO.SetActive(false);
            fireAtWillGO.SetActive(false);
            bracedGO.SetActive(false);
            defensiveStanceGO.SetActive(false);
            if (outOfAmmoGO != null) outOfAmmoGO.SetActive(false);
        }
        public void OnBroken()
        {
            if (healthSlider == null || healthSlider.gameObject == null) return;

            brokenImage.enabled = true;
            healthSlider.gameObject.SetActive(false);
            moraleSlider.gameObject.SetActive(false);
            ammoSlider.gameObject.SetActive(false);

            DisableAllStatusIcons();
        }
        public void SetCharge(bool isCharge)
        {
            if(IsExhausted) return;
            chargeGO.SetActive(isCharge);
        }
        public void SetTerrified(bool isTerrified)
        {
            terrifiedGO.SetActive(isTerrified);
        }
        public void SetExhausted(bool isExhausted)
        {
            _isExhausted = isExhausted;
            exhaustedGO.SetActive(isExhausted);
            chargeGO.SetActive(false);
        }
        public void SetWeaponStrengthActive(bool isActive)
        {
            _weaponStrengthBonusActive = isActive;
            if(weaponStrengthGO != null)
            weaponStrengthGO.SetActive(isActive);
        }
        public void SetArmorSunderedActive(bool isActive)
        {
            _armorSunderedActive = isActive;
            if(armorSunderedGO != null)
            armorSunderedGO.SetActive(isActive);
        }
        public void SetTakingFlankingDamageActive(bool isActive)
        {
            _isTakingFlankingDamage = isActive;
            if (isTakingFlankingDamageGO != null)
            isTakingFlankingDamageGO.SetActive(isActive);
        }
        public void SetFlankingActive(bool isActive)
        {
            _isFlanking = isActive;
            if (isFlankingGO != null)
            isFlankingGO.SetActive(isActive);
        }
        public void SetTakingFireDamageActive(bool isActive)
        {
            _isTakingFireDamage = isActive;
            if (isOnFireGO != null)
            isOnFireGO.SetActive(isActive);
        }
        public void SetFireAtWill(bool isFireAtWill)
        {
            _isFireAtWill = isFireAtWill;
            if (fireAtWillGO != null)
            fireAtWillGO.SetActive(isFireAtWill);
        }
        public void SetDefensiveStanceActive(bool isActive)
        {
            _isDefensiveStance = isActive;
            if (defensiveStanceGO != null)
            defensiveStanceGO.SetActive(isActive);
        }
        public void SetBracedActive(bool isActive)
        {
            _isBraced = isActive;
            if (bracedGO != null)
            bracedGO.SetActive(isActive);
        }
        public void SetOutOfAmmo(bool isOutOfAmmo)
        {
            _isOutOfAmmo = isOutOfAmmo;
            if (outOfAmmoGO != null)
            outOfAmmoGO.SetActive(isOutOfAmmo);
        }
    }
}