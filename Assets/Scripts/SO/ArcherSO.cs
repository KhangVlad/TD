using UnityEngine;
[CreateAssetMenu(fileName = "NewArcherData", menuName = "TD/Units/ArcherData")]
public class ArcherSO : UnitSO
{
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;
    public float criticalHitChance = 0.1f;
    public float criticalHitMultiplier = 2f;
}