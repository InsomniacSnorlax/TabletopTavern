using UnityEngine;
using QuickOutline;
using System.Collections.Generic;
using TJ;
using System.Collections;

namespace TJ.IrregularGrid
{
    public class BiomeCollider : MonoBehaviour
    {
        public List<Outline> outlines = new();
        public GridBiome biome;
        private readonly string swampOutlineColor = "#00E54D";
        private readonly string forestOutlineColor = "#b96f25ff";
        private readonly float OutlineWidth = 5f;
        private readonly float EmissionStrength = 10f;
        Coroutine outlineCoroutine;
        public void SetUp(GridBiome _biome)
        {
            biome = _biome;
            foreach (Outline outline in outlines)
            {
                outline.OutlineColor = 
                    biome == GridBiome.Swamp ? ColorData.HexToRgba(swampOutlineColor) :
                    biome == GridBiome.Forest ? ColorData.HexToRgba(forestOutlineColor) :
                    Color.white;
                    
                outline.OutlineWidth = 0;
                outline.EmissionStrength = 0;
            }
        }
        public void StartOutlineGlow()
        {
            if(outlineCoroutine != null) StopCoroutine(outlineCoroutine);

            outlineCoroutine = StartCoroutine(OutlineGlow());
        }
        public void StopOutlineGlow()
        {
            if(outlineCoroutine != null) StopCoroutine(outlineCoroutine);
            foreach (Outline outline in outlines)
            {
                outline.OutlineWidth = 0;
                outline.EmissionStrength = 0;
            }
        }
        public IEnumerator OutlineGlow()
        {
            // Turn on in 0.5 seconds
            float elapsedTime = 0f;
            float turnOnDuration = 0.5f;

            while (elapsedTime < turnOnDuration)
            {
                float t = elapsedTime / turnOnDuration;

                foreach (Outline outline in outlines)
                {
                    outline.OutlineWidth = Mathf.Lerp(0, OutlineWidth, t);
                    outline.EmissionStrength = Mathf.Lerp(0, EmissionStrength, t);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final values are set
            foreach (Outline outline in outlines)
            {
                outline.OutlineWidth = OutlineWidth;
                outline.EmissionStrength = EmissionStrength;
            }

            // Pause for 1 second
            yield return new WaitForSeconds(3f);

            // Turn off in 0.5 seconds
            elapsedTime = 0f;
            float turnOffDuration = 0.5f;

            while (elapsedTime < turnOffDuration)
            {
                float t = elapsedTime / turnOffDuration;

                foreach (Outline outline in outlines)
                {
                    outline.OutlineWidth = Mathf.Lerp(OutlineWidth, 0, t);
                    outline.EmissionStrength = Mathf.Lerp(EmissionStrength, 0, t);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final values are set
            foreach (Outline outline in outlines)
            {
                outline.OutlineWidth = 0;
                outline.EmissionStrength = 0;
            }
        }
    }
}