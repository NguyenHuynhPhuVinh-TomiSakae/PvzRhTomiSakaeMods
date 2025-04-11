using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using CustomizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(HoaHaiDauLuaBang.Main), "PvzRhTomiSakaeMods v1.0 - HoaHaiDauLuaBang", "1.0.0", "TomiSakae")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace HoaHaiDauLuaBang
{
    public class Main : MelonMod
    {
        // Thay vì OnInitializeMelon, thử OnApplicationStart để đảm bảo chạy sớm hơn
        // Hoặc bạn có thể giữ OnInitializeMelon nếu cách này không hiệu quả
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("[PvzRhTomiSakaeMods] PvzRhTomiSakaeMods v1.0 - HoaHaiDauLuaBang đã khởi động!");
            LoadAssetsSync(); // Gọi hàm tải đồng bộ
        }

        // Hàm tải đồng bộ
        private void LoadAssetsSync()
        {
            string bundlePath = "Mods/TomiSakae_CayTuyChinh/hoahaidauluabang";
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
                PlantType dualFireIceSunflowerType = (PlantType)2035; // ID cho Hoa Hai Đầu Lửa Băng

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
                                    newProducer.thePlantType = dualFireIceSunflowerType;
                                }
                                else
                                {
                                    existingProducer.thePlantType = dualFireIceSunflowerType;
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
                    CustomCore.RegisterCustomPlant<Producer, LopHoaHaiDauLuaBang>(
                        (int)dualFireIceSunflowerType, prefabObj, previewObj,
                        new List<ValueTuple<int, int>> {
                            new ValueTuple<int, int>(2031, 2032),
                            new ValueTuple<int, int>(2032, 2031)
                        },
                        0f, 15f, 0, 300, 15f, 500
                    );

                    // --- THÊM THÔNG TIN ALMANAC NGAY SAU KHI ĐĂNG KÝ ---
                    string plantName = "Hướng Dương Hai Đầu Lửa Băng"; // Tên hiển thị mới
                    string plantDescription =
                        "Khi tạo ánh nắng sẽ tạo ra 2 mặt trời cùng lúc, tạo 2 đường lửa và đóng băng zombie trên hai hàng ngẫu nhiên.\n" + // Dòng tagline mới
                        "Sản lượng nắng: <color=red>50 ánh nắng/15 giây</color>\n" + // Sản lượng nắng
                        "Công thức: <color=red>Hướng Dương Lửa + Hướng Dương Băng</color>\n\n" + // Công thức mới
                        "Hướng Dương Hai Đầu Lửa Băng là sự kết hợp hoàn hảo giữa Hướng Dương Lửa và Hướng Dương Băng, có khả năng tạo ra hai mặt trời, đồng thầm vừa tạo lửa vừa đóng băng zombie trên nhiều hàng, giúp bạn vừa thu hoạch nhiều nắng vừa kiểm soát đám đông zombie hiệu quả."; // Mô tả lore mới
                    CustomCore.AddPlantAlmanacStrings((int)dualFireIceSunflowerType, plantName, plantDescription);
                }
                else
                {
                    MelonLogger.Error("[PvzRhTomiSakaeMods] Không thể đăng ký Hoa Hai Đầu Lửa Băng do thiếu prefab hoặc preview.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[PvzRhTomiSakaeMods] Lỗi khi tải và đăng ký Hoa Hai Đầu Lửa Băng: {0}\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                // --- Bước 4: Giải phóng bundle ---
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
            }
        }

        // --- PATCH MỚI: TẠO LỬA, BĂNG VÀ THÊM MẶT TRỜI KHI DUAL FIREICESUNFLOWER TẠO SUN ---
        [HarmonyPatch(typeof(Producer), "ProduceSun")]
        public static class DualFireIceSunflower_ProduceSun_CreateEffects_Patch
        {
            private static bool isRecursiveCall = false; // Biến tĩnh để theo dõi việc gọi lại
            public static float lastActivatedTime = 0f; // Thời điểm kích hoạt gần nhất
            public static int lastActivatedRow = -1; // Hàng được kích hoạt gần nhất
            public static int secondFreezeRow = -1; // Hàng thứ hai để đóng băng

            // Postfix chạy SAU khi ProduceSun gốc hoàn thành
            public static void Postfix(Producer __instance)
            {
                // Bước 1: Kiểm tra xem có phải là DualFireIceSunflower không
                if (__instance != null && __instance.thePlantType == (PlantType)2035)
                {
                    // Bước 2: Đảm bảo Board tồn tại
                    if (Board.Instance == null)
                    {
                        MelonLogger.Warning("[PvzRhTomiSakaeMods] DualFireIceSunflower Patch: Board.Instance là null, không thể tạo hiệu ứng.");
                        return;
                    }

                    // Bước 3: Lấy dòng của cây
                    int plantRow = __instance.thePlantRow;
                    int totalRows = Board.Instance.rowNum;
                    lastActivatedRow = plantRow; // Lưu lại hàng hiện tại
                    lastActivatedTime = Time.time; // Lưu lại thầm điểm kích hoạt

                    // Bước 4: Tạo thêm một mặt trời
                    try
                    {
                        // Gọi lại phương thức ProduceSun của Producer để tạo mặt trời thứ hai
                        if (!isRecursiveCall)
                        {
                            isRecursiveCall = true;
                            __instance.ProduceSun(); // Gọi lại phương thức ProduceSun
                            isRecursiveCall = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualFireIceSunflower Patch: Lỗi khi tạo thêm mặt trời: {0}", ex.Message);
                    }

                    // Bước 5: Tạo hiệu ứng lửa
                    try
                    {
                        // Tạo lửa ở hàng hiện tại
                        Board.Instance.CreateFireLine(plantRow, 1800, false, false, true);

                        // Ngẫu nhiên chọn hàng trên hoặc hàng dưới cho hiệu ứng lửa
                        bool useUpperRowForFire = UnityEngine.Random.value > 0.5f;
                        int fireRow = -1;

                        if (useUpperRowForFire && plantRow > 0)
                        {
                            // Tạo lửa ở hàng trên
                            fireRow = plantRow - 1;
                        }
                        else if (!useUpperRowForFire && plantRow < totalRows - 1)
                        {
                            // Tạo lửa ở hàng dưới
                            fireRow = plantRow + 1;
                        }
                        else
                        {
                            // Nếu không thể tạo ở hàng đã chọn, tạo ở hàng còn lại
                            if (plantRow > 0)
                            {
                                fireRow = plantRow - 1;
                            }
                            else if (plantRow < totalRows - 1)
                            {
                                fireRow = plantRow + 1;
                            }
                        }

                        // Tạo lửa ở hàng đã chọn
                        if (fireRow != -1)
                        {
                            Board.Instance.CreateFireLine(fireRow, 1800, false, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualFireIceSunflower Patch: Lỗi khi tạo lửa: {0}", ex.Message);
                    }

                    // Bước 6: Đóng băng zombie
                    try
                    {
                        // Ngẫu nhiên chọn hàng trên hoặc hàng dưới cho hiệu ứng băng
                        bool useUpperRowForIce = UnityEngine.Random.value > 0.5f;
                        secondFreezeRow = -1;

                        if (useUpperRowForIce && plantRow > 0)
                        {
                            // Chọn hàng trên
                            secondFreezeRow = plantRow - 1;
                        }
                        else if (!useUpperRowForIce && plantRow < totalRows - 1)
                        {
                            // Chọn hàng dưới
                            secondFreezeRow = plantRow + 1;
                        }
                        else
                        {
                            // Nếu không thể chọn hàng đã định, chọn hàng còn lại
                            if (plantRow > 0)
                            {
                                secondFreezeRow = plantRow - 1;
                            }
                            else if (plantRow < totalRows - 1)
                            {
                                secondFreezeRow = plantRow + 1;
                            }
                        }

                        int zombiesCount = 0;

                        // Lặp qua tất cả zombie trong zombieArray
                        foreach (Zombie zombie in Board.Instance.zombieArray)
                        {
                            if (zombie == null) continue;

                            // Xử lý zombie trên hàng hiện tại hoặc hàng thứ hai
                            if (zombie.theZombieRow == plantRow || zombie.theZombieRow == secondFreezeRow)
                            {
                                zombiesCount++;

                                // Đóng băng zombie trong 10 giây
                                zombie.SetFreeze(10f);

                                // Tạo hiệu ứng băng tại vị trí zombie
                                try
                                {
                                    UnityEngine.Vector3 zombiePos = zombie.transform.position;
                                    Board.Instance.CreateFreeze(zombiePos);
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Warning("[PvzRhTomiSakaeMods] DualFireIceSunflower: Không thể tạo hiệu ứng băng: {0}", ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualFireIceSunflower Patch: Lỗi khi đóng băng zombie: {0}", ex.Message);
                    }
                }
            }
        }

        // --- PATCH MỚI: CHẶN HIỆU ỨNG ĐÓNG BĂNG CHO ZOMBIE Ở HÀNG KHÁC ---
        [HarmonyPatch(typeof(Zombie), "SetFreeze")]
        public static class Zombie_SetFreeze_Patch
        {
            // Prefix chạy TRƯỚC khi SetFreeze gốc chạy
            // Trả về false để ngăn phương thức gốc chạy
            public static bool Prefix(Zombie __instance, float time)
            {
                // Nếu không phải do Hoa Hai Đầu Lửa Băng gọi (thời gian > 0.1s), cho phép chạy bình thường
                if (Time.time - DualFireIceSunflower_ProduceSun_CreateEffects_Patch.lastActivatedTime > 0.1f)
                {
                    return true; // Cho phép phương thức gốc chạy
                }

                // Nếu là do Hoa Hai Đầu Lửa Băng gọi, chỉ cho phép đóng băng zombie trên các hàng đã chọn
                int zombieRow = __instance.theZombieRow;
                int plantRow = DualFireIceSunflower_ProduceSun_CreateEffects_Patch.lastActivatedRow;
                int secondRow = DualFireIceSunflower_ProduceSun_CreateEffects_Patch.secondFreezeRow;
                
                // Cho phép đóng băng zombie trên cùng hàng với cây
                if (zombieRow == plantRow)
                {
                    return true;
                }
                
                // Cho phép đóng băng zombie trên hàng thứ hai đã chọn
                if (zombieRow == secondRow)
                {
                    return true;
                }

                // Không cho phép đóng băng zombie ở các hàng khác
                return false;
            }
        }
    }
}
