using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using CustomizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(HoaLua.Main), "PvzRhTomiSakaeMods v1.0 - HoaLua", "1.0.0", "TomiSakae")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace HoaLua
{
    public class Main : MelonMod
    {
        // Thay vì OnInitializeMelon, thử OnApplicationStart để đảm bảo chạy sớm hơn
        // Hoặc bạn có thể giữ OnInitializeMelon nếu cách này không hiệu quả
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("[PvzRhTomiSakaeMods] PvzRhTomiSakaeMods v1.0 - HoaLua đã khởi động!");
            LoadAssetsSync(); // Gọi hàm tải đồng bộ
        }

        // Hàm tải đồng bộ
        private void LoadAssetsSync()
        {
            string bundlePath = "Mods/TomiSakae_CayTuyChinh/hoalua";
            string fullPath = Path.Combine(MelonEnvironment.GameRootDirectory, bundlePath);

            // --- Bước 1: Kiểm tra sự tồn tại của file ---
            if (!File.Exists(fullPath))
            {
                MelonLogger.Error("[PvzRhTomiSakaeMods] [Kiểm tra File] KHÔNG TÌM THẤY file AssetBundle tại: {0}", fullPath);
                return; // Dừng lại nếu file không tồn tại
            }

            AssetBundle bundle = null;
            try
            {
                // --- Bước 2: Tải đồng bộ ---
                bundle = AssetBundle.LoadFromFile(fullPath);

                if (bundle == null)
                {
                    MelonLogger.Error("[PvzRhTomiSakaeMods] [Kiểm tra Tải] THẤT BẠI khi tải AssetBundle từ: {0}. File có thể bị hỏng hoặc không tương thích.", fullPath);
                    return; // Dừng lại nếu tải thất bại
                }

                GameObject prefabObj = null;
                GameObject previewObj = null;
                PlantType fireSunflowerType = (PlantType)961;

                foreach (UnityEngine.Object obj in bundle.LoadAllAssets())
                {
                    GameObject testObj = obj.TryCast<GameObject>();
                    if (testObj != null)
                    {
                        if (testObj.name == "SunflowerPrefab")
                        {
                            prefabObj = testObj.Cast<GameObject>();
                            if (prefabObj != null)
                            {
                                Producer existingProducer = prefabObj.GetComponent<Producer>();
                                if (existingProducer == null)
                                {
                                    Producer newProducer = prefabObj.AddComponent<Producer>();
                                    newProducer.thePlantType = fireSunflowerType;
                                }
                                else
                                {
                                    existingProducer.thePlantType = fireSunflowerType;
                                }
                            }
                        }
                        else if (testObj.name == "SunflowerPreview")
                        {
                            previewObj = testObj.Cast<GameObject>();
                        }
                    }
                }

                if (prefabObj == null) MelonLogger.Warning("[PvzRhTomiSakaeMods] Không tìm thấy GameObject SunflowerPrefab.");
                if (previewObj == null) MelonLogger.Warning("[PvzRhTomiSakaeMods] Không tìm thấy GameObject SunflowerPreview.");

                // --- Bước 3: Đăng ký (NGAY LẬP TỨC sau khi load) ---
                if (prefabObj != null && previewObj != null)
                {
                    // Việc gọi RegisterCustomPlant ở đây sẽ gọi AddFusion ngay lập tức
                    // Các tham số: plantType, prefab, preview, recipeList, 
                    // attackInterval, produceInterval, attackDamage, maxHealth, cd, sun
                    CustomCore.RegisterCustomPlant<Producer, LopHoaLua>(
                        (int)fireSunflowerType, prefabObj, previewObj,
                        new List<ValueTuple<int, int>> {
                            new ValueTuple<int, int>(1, 16),
                            new ValueTuple<int, int>(16, 1)
                        },
                        0f, 7.5f, 0, 300, 7.5f, 25
                    );
                    // Đã điều chỉnh produceInterval về mức tiêu chuẩn
                    // Đã giảm số lượng mặt trời từ 300 xuống 25 (tiêu chuẩn của hướng dương)
                }
                else
                {
                    MelonLogger.Error("[PvzRhTomiSakaeMods] Không thể đăng ký cây tùy chỉnh vì không tìm thấy prefab hoặc preview.");
                }

            }
            catch (Exception ex)
            {
                MelonLogger.Error("[PvzRhTomiSakaeMods] Đã xảy ra lỗi trong quá trình tải hoặc xử lý asset: {0}", ex);
            }
            finally
            {
                // Giải phóng bộ nhớ bundle dù thành công hay thất bại (nếu bundle đã được load)
                if (bundle != null)
                {
                    bundle.Unload(false); // false để giữ lại các assets đã load
                }
            }
        }

        // --- PATCH MỚI: TẠO LỬA KHI FIRESUNFLOWER TẠO SUN ---
        [HarmonyPatch(typeof(Producer), "ProduceSun")] // Patch vào cùng hàm
        public static class FireSunflower_ProduceSun_CreateFireLine_Patch
        {
            // Postfix chạy SAU khi ProduceSun gốc hoàn thành
            public static void Postfix(Producer __instance)
            {
                // Bước 1: Kiểm tra xem có phải là FireSunflower không
                if (__instance != null && __instance.thePlantType == (PlantType)961)
                {
                    // Bước 2: Đảm bảo Board tồn tại
                    if (Board.Instance == null)
                    {
                        MelonLogger.Warning("[PvzRhTomiSakaeMods] FireSunflower Patch: Board.Instance là null, không thể tạo dòng lửa.");
                        return;
                    }

                    // Bước 3: Lấy dòng của cây
                    int plantRow = __instance.thePlantRow;

                    // Bước 4: Gọi hàm tạo lửa của Board
                    try
                    {
                        // Sử dụng các tham số mặc định của CreateFireLine hoặc tùy chỉnh nếu muốn
                        // CreateFireLine(int theFireRow, int damage = 1800, bool fromZombie = false, bool fix = false, bool shake = true)
                        Board.Instance.CreateFireLine(plantRow, 1800, false, false, true); // Giữ damage mặc định, không phải từ zombie, không fix, có rung lắc
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] FireSunflower Patch: Lỗi khi gọi Board.CreateFireLine: {0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
        }
    }
}
