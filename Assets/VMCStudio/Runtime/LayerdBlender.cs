using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VMCStudio
{
    public interface ILayerdValue
    {
        float value { get; set; }
        int layer { get; }
    }

    public static class LayerdBlendUtility
    {
        //                 System.Array.Resize (ref _layerTotalWeights, elements.Max (t => t.layer) + 1);

        static float[] __layerTotalWeights = new float[20];

        /// <summary>
        /// レイヤー毎にトータルのウェイトが１を超えないように調整
        /// </summary>
        /// <param name="entities"></param>
        public static void Blend<T> (ref T[] entities, int highPriorityIndex = -1)
            where T : ILayerdValue
        {
            System.Array.Clear (__layerTotalWeights, 0, __layerTotalWeights.Length);

            if (highPriorityIndex >= 0) {
                int i = highPriorityIndex;
                var layer = entities[i].layer;
                var weight = entities[i].value;

                weight = Mathf.Clamp (weight, 0, 1 - __layerTotalWeights[layer]);

                entities[i].value = weight;
                __layerTotalWeights[layer] += weight;
            }

            for (int i = 0; i < entities.Length; i++) {
                if (highPriorityIndex == i) continue;
                var layer = entities[i].layer;
                var weight = entities[i].value;

                weight = Mathf.Clamp (weight, 0, 1 - __layerTotalWeights[layer]);

                entities[i].value = weight;
                __layerTotalWeights[layer] += weight;
            }
        }
    }
}