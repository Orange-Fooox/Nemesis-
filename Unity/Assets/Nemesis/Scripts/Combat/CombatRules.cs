using UnityEngine;
namespace Nemesis.Combat {
 public static class CombatRules {
  public const float DefenseFactor=0.35f;
  public static float CalculateDamage(float attack,float multiplier,float defense,float vulnerability=1f)
   => Mathf.Max(1f,attack*multiplier-defense*DefenseFactor)*Mathf.Max(0f,vulnerability);
 }
}