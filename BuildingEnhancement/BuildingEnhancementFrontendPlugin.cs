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
        public static int taiwuVillageMaxLevel = 20;
        public static int residenceMaxLevel = 20;
        public static int comfortableHouseMaxLevel = 20;
        public static int warehouseMaxLevel = 20;
        public static int resourceMaxLevel = 20;
        public static int readingMaxLevel = 20;
        public static int flashMaxLevel = 20;
        public static int kungfuMaxLevel = 20;
        public static int attainmentMaxLevel = 20;
        public static int moneyMaxLevel = 20;
        public static int authorityMaxLevel = 20;
        public static int getVillagerMaxLevel = 20;

        public override void OnModSettingUpdate()
        {
            ModManager.GetSetting(ModIdStr, "Slider_TaiwuVillageMaxLevel", ref taiwuVillageMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_ResidenceMaxLevel", ref residenceMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_ComfortableHouseMaxLevel", ref comfortableHouseMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_WarehouseMaxLevel", ref warehouseMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_ResourceMaxLevel", ref resourceMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_ReadingMaxLevel", ref readingMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_FlashMaxLevel", ref flashMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_KungfuMaxLevel", ref kungfuMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_AttainmentMaxLevel", ref attainmentMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_MoneyMaxLevel", ref moneyMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_AuthorityMaxLevel", ref authorityMaxLevel);
            ModManager.GetSetting(ModIdStr, "Slider_GetVillagerMaxLevel", ref getVillagerMaxLevel);
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
                // 太吾村蛟池数量
                new LevelEffectData(146, jiaoPoolLvVals),
            });

            setBuildingMaxLv("太吾氏祠堂", residenceMaxLevel);
            setBuildingMaxLv("居所", residenceMaxLevel);
            setBuildingMaxLv("厢房", comfortableHouseMaxLevel);
            setBuildingMaxLv("仓库", warehouseMaxLevel);

            // 天然资源类
            var resourceBuildingBlockItems = BuildingBlock.Instance.Where((item) => item.Class == EBuildingBlockClass.BornResource).ToList();
            resourceBuildingBlockItems.ForEach((item) => setBuildingMaxLv(item, resourceMaxLevel, new LevelEffectData[] { }));

            // 收获资源、物品类
            var resourceMiningBuildingBlockItems = BuildingBlock.Instance.Where((item) => item.Class == EBuildingBlockClass.Resource).ToList();
            resourceMiningBuildingBlockItems.ForEach((item) => setBuildingMaxLv(item, resourceMaxLevel, new LevelEffectData[] { }));
            var lifeSkillMaterialFarmingBuildingNames = new List<string> {
                "茶园", "蒸酒坊", "淘洗池", "精炼室", "伐木场", "林场", "药圃", "养药室", "炼瘴池", "废人窟",
                "百花瀑", "奇珍园", "浣宝池", "金刚解玉台", "四季园", "天成乡"
            };
            lifeSkillMaterialFarmingBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, resourceMaxLevel));

            // 研读效率建筑（功法、技艺的基础建筑）
            List<string> readingBuildingNames = new List<string> {
                "练功房", "琴舍", "弈轩", "书房", "画阁", "观星台", "甘泉厅", "火炼室", "木工房", "药房",
                "幽室", "绣楼", "巧匠屋", "云房", "禅房", "食窖", "长街",
            };
            readingBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, readingMaxLevel));

            // 灵光乍现
            List<string> flashBuildingNames = new List<string> {
                "移情居", "琉璃馆", "龙龟山", "五色窟", "摘星楼", "胡舫", "水排", "黑脂池", "兽医馆", "豢养室",
                "缀星阁", "攻玉楼", "辟谷崖", "见性洞", "百兽园", "大游园"
            };
            flashBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, flashMaxLevel));

            // 功法修炼突破
            List<string> kungfuBuildingNames = new List<string> {
                "静室", "桩林", "绝诣堂", "木人阵", "铜人阵", "黑泥潭", "靶场", "试剑台", "神刀堂",
                "演武场", "异人馆", "风室", "天机阁", "空谷", "隔世塔", "凌云阶", "密室", "搏虎牢", "绵铁二壁",
                "倒穹斗", "暗室", "剑冢", "修罗场", "铁驹阵", "八阵图", "千丝巷", "追影洞", "七弦楼"
            };
            kungfuBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, kungfuMaxLevel));

            // 造诣类建筑（增加造诣、减少造诣要求）
            List<string> attainmentBuildingNames = new List<string>
            {
                "仙境", "镜中台", "照夜楼", "天香彩阁", "昆仑塔", "神仙苑", "陨铁矿场", "神火铸", "木料标本", "神木林",
                "银盖冰棺", "神农涧", "无人居", "神龙柱", "蛇骨绫机", "神彩梭", "赤血镜", "神光璧", "朝元洞", "枯荣台",
                "灶王玄鼎", "龙宫", "玄圃"
            };
            attainmentBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, attainmentMaxLevel));

            // TODO: 除了减少造诣之外的其他技艺类特殊建筑，如凤凰台

            // 银钱建筑
            List<string> moneyBuildingNames = new List<string> {
                "镖局", "乐坊", "棋馆", "书铺", "画铺", "占卜馆", "茶馆", "酒肆", "铁匠铺", "木工铺",
                "熟药铺", "毒市", "布庄", "珠宝铺", "法事道场", "寺院", "酒楼", "市集", "赌坊", "青楼",
            };
            moneyBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, moneyMaxLevel));

            // 威望建筑
            List<string> authorityBuildingNames = new List<string> {
                "知客亭", "百戏园", "石谱园", "翰苑", "流光园", "祭天高台", "四海府", "金铺", "营造坊", "病坊",
                "密医", "锦绣阁", "琳琅阁", "三清殿", "法堂", "争妍阁", "游园",
            };
            authorityBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, authorityMaxLevel));

            // 人才招揽建筑
            List<string> getVillagerBuildingNames = new List<string>
            {
                "炼神峰", "知音阁", "斗弈台", "书院", "丹青馆", "方士馆", "闻香苑", "锻冶坊", "制木坊", "药师馆",
                "暗牢", "织造坊", "毛石坊", "道观", "佛塔", "百家宴", "花舫", "勾栏瓦舍", "贤士馆",
            };
            getVillagerBuildingNames.ForEach((buildingName) => setBuildingMaxLv(buildingName, getVillagerMaxLevel));
        }

        public void setBuildingMaxLv(string buildingRefName, int newMaxLv)
        {
            setBuildingMaxLv(buildingRefName, newMaxLv, new LevelEffectData[] { });
        }

        public void setBuildingMaxLv(string buildingRefName, int newMaxLv, LevelEffectData[] customLevelEffectDataArr)
        {
            BuildingBlockItem buildingBlockItem = BuildingBlock.Instance.GetItem((short)BuildingBlock.Instance.GetItemId(buildingRefName));
            setBuildingMaxLv(buildingBlockItem, newMaxLv, customLevelEffectDataArr);
        }

        public void setBuildingMaxLv(BuildingBlockItem buildingBlockItem, int newMaxLv, LevelEffectData[] customLevelEffectDataArr)
        {
            int needAddLv = newMaxLv - buildingBlockItem.MaxLevel;

            // type readonly sbyte
            var maxLevelField = typeof(BuildingBlockItem).GetField("MaxLevel", BindingFlags.Public | BindingFlags.Instance);
            maxLevelField.SetValue(buildingBlockItem, (sbyte)newMaxLv);

            if (needAddLv <= 0 || buildingBlockItem.ExpandInfos == null) return;

            // 自动配置缺失的 LevelEffectData
            List<LevelEffectData> levelEffectDataArr = customLevelEffectDataArr.ToList();
            buildingBlockItem.ExpandInfos.ForEach((scaleItemId) =>
            {
                if (levelEffectDataArr.Any(data => data.effectId == scaleItemId)) return;
                var scaleItem = BuildingScale.Instance.GetItem(scaleItemId);
                var valInLv1 = scaleItem.LevelEffect[0];
                var valInLv2 = scaleItem.LevelEffect[1];
                levelEffectDataArr.Add(new LevelEffectData(scaleItemId, valInLv2 - valInLv1));
            });

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
