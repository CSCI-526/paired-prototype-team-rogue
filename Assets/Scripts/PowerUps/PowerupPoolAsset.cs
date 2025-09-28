using UnityEngine;
using System.Collections.Generic;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(menuName = "Powerups/Pool", fileName = "PowerupPool")]
public class PowerupPoolAsset : ScriptableObject
{
    [SerializeField] private List<PowerupData> items = new();   // 在 Inspector 里手动维护
    [Header("Rarity Weights")]
    [SerializeField] private float common = 100f;
    [SerializeField] private float uncommon = 60f;
    [SerializeField] private float rare = 25f;
    [SerializeField] private float epic = 10f;
    [SerializeField] private float legendary = 3f;

    [Header("Options")]
    [SerializeField] private bool allowDuplicates = true;

    public PowerupData[] Draw(int count)
    {
        var result = new PowerupData[count];
        if (items == null || items.Count == 0) return result;

        var src = allowDuplicates ? items : new List<PowerupData>(items);
        for (int i = 0; i < count; i++)
        {
            if (src.Count == 0) break;
            var pick = WeightedPick(src);
            result[i] = pick;
            if (!allowDuplicates) src.Remove(pick);
        }
        return result;
    }

    PowerupData WeightedPick(IList<PowerupData> src)
    {
        float total = 0f;
        for (int i = 0; i < src.Count; i++) total += GetWeight(src[i]);
        if (total <= 0f) return src[Random.Range(0, src.Count)];

        float r = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < src.Count; i++)
        {
            acc += GetWeight(src[i]);
            if (r <= acc) return src[i];
        }
        return src[src.Count - 1];
    }

    float GetWeight(PowerupData d)
    {
        if (d == null) return 0f;
        if (d.weightOverride >= 0f) return d.weightOverride; // 单独覆盖
        return d.rarity switch
        {
            Rarity.Common     => common,
            Rarity.Uncommon   => uncommon,
            Rarity.Rare       => rare,
            Rarity.Epic       => epic,
            Rarity.Legendary  => legendary,
            _ => common
        };
    }
}
