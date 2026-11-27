--!strict
local Config = {}
Config.Version = "0.4.0-dev.4"
Config.Player = {MaxHealth=100, Attack=12, Defense=5, MaxStamina=100, MaxEssence=100, WalkSpeed=16, RunSpeed=23}
Config.Combat = {BasicAttackMultiplier=1, CriticalMultiplier=1.5, DefenseFactor=0.35, AttackRange=9, AttackCooldown=0.65, EnemyAlertDuration=12, EnemyReturnDistance=4}
Config.Movement = {StaminaDrainPerSecond=24, StaminaRegenPerSecond=16, SprintRequestInterval=0.15}
Config.Abilities = {
 SealStrike={Multiplier=1.8, EssenceCost=15, Cooldown=6, Range=11},
 ShadowDash={Multiplier=1, EssenceCost=20, Cooldown=8, Range=13, DashImpulse=58},
 EarthRend={Multiplier=2.5, EssenceCost=30, Cooldown=14, Range=12, Radius=10},
 Barrier={EssenceCost=25, Cooldown=18, Duration=4, ShieldHealthPercent=0.30},
 AbyssRelease={Multiplier=4, EssenceCost=60, Cooldown=45, Range=14, Vulnerability=0.10, VulnerabilityDuration=5},
}
Config.Enemies = {
 GoblinScavenger={DisplayName="Гоблин-падальщик",MaxHealth=55,Attack=9,Defense=2,WalkSpeed=14,DetectionRange=18,AttackRange=5,AttackCooldown=1.8,XP=12,Color="Moss"},
 BoneArcher={DisplayName="Костяной стрелок",MaxHealth=45,Attack=12,Defense=1,WalkSpeed=11,DetectionRange=26,AttackRange=24,AttackCooldown=2.5,XP=15,Color="Bone"},
 WanderingShade={DisplayName="Блуждающая тень",MaxHealth=65,Attack=13,Defense=3,WalkSpeed=18,DetectionRange=22,AttackRange=6,AttackCooldown=4,XP=18,Color="Shade"},
 AshenWolf={DisplayName="Пепельный волк",MaxHealth=80,Attack=15,Defense=3,WalkSpeed=20,DetectionRange=25,AttackRange=5,AttackCooldown=2.2,XP=20,Color="Ash"},
}
Config.World = {HubSize=420, EnemyPopulation=6, RubbleCount=90, SpawnPosition=Vector3.new(0,5,165)}
return Config
