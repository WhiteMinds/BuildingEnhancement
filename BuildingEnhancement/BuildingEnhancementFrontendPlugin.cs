using System;
using System.Collections.Generic;
using TaiwuModdingLib.Core.Plugin;
using HarmonyLib;
using Config;
using UnityEngine;
using System.Reflection;
using System.Linq;
using TaiwuModdingLib.Core.Utils;

namespace BuildingEnhancement
{
    [PluginConfig("BuildingEnhancement", "WhiteMind", "1.0.0")]
    public class BuildingEnhancementFrontendPlugin : TaiwuRemakePlugin
    {
        // 等级超过 2 位数会导致相关的 UI 错乱
        public static int taiwuVillageMaxLevel = 99;
        public static int warehouseMaxLevel = 99;
        public static int residenceMaxLevel = 99;
        public static int smithyMaxLevel = 40;
        public static int goldsmithMaxLevel = 40;

        public override void OnModSettingUpdate()
        {
            ModManager.GetSetting(ModIdStr, "Slider_TaiwuVillageMaxLevel", ref taiwuVillageMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_WarehouseMaxLevel", ref warehouseMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_ResidenceMaxLevel", ref residenceMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_SmithyMaxLevel", ref smithyMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_GoldsmithMaxLevel", ref goldsmithMaxLevel);
            setBuildingsMaxLv();
        }

        Harmony harmony;
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(BuildingEnhancementFrontendPlugin));
        }

        public void setBuildingsMaxLv()
        {
            /**
             * EffectId 来自 Assembly-CSharp\Config\BuildingBlock.cs 中 new BuildingBlockItem 时提供的 ExpandInfos。
             * 根据 resources.assets\ExportedProject\Assets\TextAsset\BuildingScale.ref.txt 得到 EffectId 对应的中文名。
             * 每等级的数据来自 Assembly-CSharp\Config\BuildingScale.cs。
             */

            var jiaoPoolLvVals = new int[taiwuVillageMaxLevel];
            for (int i = 0; i < jiaoPoolLvVals.Length; i++) jiaoPoolLvVals[i] = 9;
            setBuildingMaxLv("太吾村", taiwuVillageMaxLevel, new[] {
                // 太吾村建设空间
                new LevelEffectData(144, 2),
                // 太吾村石屋容量
                new LevelEffectData(145, 5),
                // 太吾村蛟池数量
                new LevelEffectData(146, jiaoPoolLvVals),
            });

            setBuildingMaxLv("仓库", warehouseMaxLevel, new[] {
                // 仓库重量上限
                new LevelEffectData(150, 20),
                // 仓库资源储量
                new LevelEffectData(151, 1500)
            });

            setBuildingMaxLv("居所", residenceMaxLevel, new[] {
                // 居所居住空间
                new LevelEffectData(152, 2),
                // 居所维护费用
                new LevelEffectData(153, 1)
            });

            setBuildingMaxLv("铁匠铺", smithyMaxLevel, new[] {
                // 铁匠铺建筑产能，不宜过高所以从每级 +4 削减到 +1
                new LevelEffectData(197, 1),
                // 铁匠铺出售栏位
                new LevelEffectData(198, 1)
            });

            setBuildingMaxLv("金铺", goldsmithMaxLevel, new[] {
                // 金铺建筑产能，不宜过高所以从每级 +4 削减到 +1
                new LevelEffectData(251, 1),
                // 金铺出售栏位
                new LevelEffectData(252, 1)
            });
        }

        public void setBuildingMaxLv(string buildingRefName, int newMaxLv, LevelEffectData[] levelEffectDataArr)
        {
            BuildingBlockItem buildingBlockItem = BuildingBlock.Instance.GetItem((short)BuildingBlock.Instance.GetItemId(buildingRefName));
            int needAddLv = newMaxLv - buildingBlockItem.MaxLevel;

            // type readonly sbyte
            var maxLevelField = typeof(BuildingBlockItem).GetField("MaxLevel", BindingFlags.Public | BindingFlags.Instance);
            maxLevelField.SetValue(buildingBlockItem, (sbyte)newMaxLv);

            if (needAddLv <= 0) return;

            // LevelEffect 应该只影响建筑扩建面板的显示，不影响实际数值，所以要按照实际的公式来设置。
            foreach (var data in levelEffectDataArr)
            {
                var scaleItem = BuildingScale.Instance.GetItem(data.effectId);
                // Assembly-CSharp\Config\BuildingScale.cs 中的代码配置 BuildingScaleItem 时，有一些建筑上限 10 级的会给后面 10 级填充 -1 的数值，这里修正。
                for (int i = 0; i < scaleItem.LevelEffect.Count(); i++)
                {
                    if (scaleItem.LevelEffect[i] != -1) continue;
                    if (data.perLvIncrementVal == null)
                    {
                        scaleItem.LevelEffect[i] = data.manuaLvVals[scaleItem.LevelEffect.Count];
                    }
                    else
                    {
                        var valInLv1 = scaleItem.LevelEffect[0];
                        // Example: lv11 = valInLv1 + 10 * perLvIncrementVal
                        scaleItem.LevelEffect[i] = ((int)(valInLv1 + i * data.perLvIncrementVal));
                    }
                }

                for (int i = 0; i < needAddLv; i++)
                {
                    if (data.perLvIncrementVal == null)
                    {
                        scaleItem.LevelEffect.Add(data.manuaLvVals[scaleItem.LevelEffect.Count]);
                    }
                    else
                    {
                        var valInLv1 = scaleItem.LevelEffect[0];
                        // Example: lv21 = valInLv1 + 20 * perLvIncrementVal
                        scaleItem.LevelEffect.Add((int)(valInLv1 + scaleItem.LevelEffect.Count * data.perLvIncrementVal));
                    }
                }
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(UI_BuildingManage), "UpdateExpandInfoItem")]
        //public static void Patch3()
        //{
        //    Debug.Log("called UI_BuildingManage UpdateExpandInfoItem");
        //}
    }

    public class LevelEffectData
    {
        public LevelEffectData(
            short effectId,
            int perLvIncrementVal
        )
        {
            this.effectId = effectId;
            this.perLvIncrementVal = perLvIncrementVal;
        }

        public LevelEffectData(short effectId, int[] manuaLvVals)
        {
            this.effectId = effectId;
            this.manuaLvVals = manuaLvVals;
        }

        public short effectId;
        public int? perLvIncrementVal;
        // 手动提供所有等级的数值
        public int[] manuaLvVals = new int[] { };
    }
}
