using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using CustomizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(HoaHaiDauLua.Main), "PvzRhTomiSakaeMods v1.0 - HoaHaiDauLua", "1.0.0", "TomiSakae")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace HoaHaiDauLua
{
    public class Main : MelonMod
    {
        // Thay vì OnInitializeMelon, thử OnApplicationStart để đảm bảo chạy sớm hơn
        // Hoặc bạn có thể giữ OnInitializeMelon nếu cách này không hiệu quả
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("[PvzRhTomiSakaeMods] PvzRhTomiSakaeMods v1.0 - HoaHaiDauLua đã khởi động!");
            LoadAssetsSync(); // Gọi hàm tải đồng bộ
        }

        // Hàm tải đồng bộ
        private void LoadAssetsSync()
        {
            string bundlePath = "Mods/TomiSakae_CayTuyChinh/hoahaidaulua";
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
                PlantType dualFireSunflowerType = (PlantType)2033; // Đổi ID thành 2033 cho Hoa Hai Đầu Lửa

                foreach (UnityEngine.Object obj in bundle.LoadAllAssets())
                {
                    GameObject testObj = obj.TryCast<GameObject>();
                    if (testObj != null)
                    {
                        if (testObj.name == "TwinFlowerPrefab")
                        {
                            prefabObj = testObj.Cast<GameObject>();
                            if (prefabObj != null)
                            {
                                Producer existingProducer = prefabObj.GetComponent<Producer>();
                                if (existingProducer == null)
                                {
                                    Producer newProducer = prefabObj.AddComponent<Producer>();
                                    newProducer.thePlantType = dualFireSunflowerType;
                                }
                                else
                                {
                                    existingProducer.thePlantType = dualFireSunflowerType;
                                }
                            }
                        }
                        else if (testObj.name == "TwinFlowerPreview")
                        {
                            previewObj = testObj.Cast<GameObject>();
                        }
                    }
                }

                if (prefabObj == null) MelonLogger.Warning("[PvzRhTomiSakaeMods] Không tìm thấy GameObject TwinFlowerPrefab.");
                if (previewObj == null) MelonLogger.Warning("[PvzRhTomiSakaeMods] Không tìm thấy GameObject TwinFlowerPreview.");

                // --- Bước 3: Đăng ký (NGAY LẬP TỨC sau khi load) ---
                if (prefabObj != null && previewObj != null)
                {
                    // Việc gọi RegisterCustomPlant ở đây sẽ gọi AddFusion ngay lập tức
                    // Các tham số: plantType, prefab, preview, recipeList, 
                    // cooldownTime, produceInterval, produceCount, sunCost, health, cost
                    CustomCore.RegisterCustomPlant<Producer, LopHoaHaiDauLua>(
                        (int)dualFireSunflowerType, prefabObj, previewObj,
                        new List<ValueTuple<int, int>> {
                            new ValueTuple<int, int>(2031, 2031),
                            new ValueTuple<int, int>(2031, 2031)
                        },
                        0f, 15f, 0, 300, 15f, 350
                    );

                    // --- THÊM THÔNG TIN ALMANAC NGAY SAU KHI ĐĂNG KÝ ---
                    string plantName = "Hướng Dương Hai Đầu Lửa"; // Tên hiển thị mới
                    string plantDescription =
                        "Khi tạo ánh nắng sẽ tạo ra 2 mặt trời cùng lúc và tạo 2 đường lửa của ớt trên hai hàng liền kề.\n" + // Dòng tagline mới
                        "Sản lượng nắng: <color=red>50 ánh nắng/15 giây</color>\n" + // Tăng sản lượng nắng
                        "Công thức: <color=red>Hướng Dương Lửa + Hướng Dương Lửa</color>\n\n" + // Công thức mới
                        "Hướng Dương Hai Đầu Lửa với hai đầu hoa riêng biệt, có khả năng tạo ra hai luồng lửa và hai mặt trời cùng lúc, giúp bạn thu hoạch nhiều nắng hơn và đối phó với zombie trên nhiều hàng hiệu quả hơn."; // Mô tả lore mới
                    CustomCore.AddPlantAlmanacStrings((int)dualFireSunflowerType, plantName, plantDescription);
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

        // --- PATCH MỚI: TẠO LỬA VÀ THÊM MẶT TRỜI KHI DUAL FIRESUNFLOWER TẠO SUN ---
        [HarmonyPatch(typeof(Producer), "ProduceSun")] // Patch vào cùng hàm
        public static class DualFireSunflower_ProduceSun_CreateFireLine_Patch
        {
            private static bool isRecursiveCall = false; // Biến tĩnh để theo dõi việc gọi lại

            // Postfix chạy SAU khi ProduceSun gốc hoàn thành
            public static void Postfix(Producer __instance)
            {
                // Bước 1: Kiểm tra xem có phải là DualFireSunflower không
                if (__instance != null && __instance.thePlantType == (PlantType)2033)
                {
                    // Bước 2: Đảm bảo Board tồn tại
                    if (Board.Instance == null)
                    {
                        MelonLogger.Warning("[PvzRhTomiSakaeMods] DualFireSunflower Patch: Board.Instance là null, không thể tạo dòng lửa.");
                        return;
                    }

                    // Bước 3: Lấy dòng của cây
                    int plantRow = __instance.thePlantRow;
                    int totalRows = Board.Instance.rowNum;

                    // Bước 4: Tạo thêm một mặt trời
                    try
                    {
                        // Gọi lại phương thức ProduceSun của Producer để tạo mặt trời thứ hai
                        // Lưu ý: Điều này có thể gây ra đệ quy vô hạn nếu không xử lý cẩn thận
                        // Chúng ta cần đảm bảo rằng chỉ gọi lại ProduceSun một lần
                        if (!isRecursiveCall)
                        {
                            isRecursiveCall = true;
                            __instance.ProduceSun(); // Gọi lại phương thức ProduceSun
                            isRecursiveCall = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualFireSunflower Patch: Lỗi khi tạo thêm mặt trời: {0}\n{1}", ex.Message, ex.StackTrace);
                    }

                    // Bước 5: Gọi hàm tạo lửa của Board cho hàng hiện tại
                    try
                    {
                        // Tạo lửa ở hàng hiện tại
                        Board.Instance.CreateFireLine(plantRow, 1800, false, false, true);
                        
                        // Tạo lửa ở hàng kế tiếp (nếu không phải hàng cuối)
                        if (plantRow < totalRows - 1) {
                            Board.Instance.CreateFireLine(plantRow + 1, 1800, false, false, true);
                        }
                        // Nếu ở hàng cuối, tạo lửa ở hàng trên
                        else if (plantRow > 0) {
                            Board.Instance.CreateFireLine(plantRow - 1, 1800, false, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualFireSunflower Patch: Lỗi khi gọi Board.CreateFireLine: {0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
        }
    }
}
