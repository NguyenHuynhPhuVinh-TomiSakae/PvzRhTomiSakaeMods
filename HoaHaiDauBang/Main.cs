using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using CustomizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Utils;
using System.Numerics;

[assembly: MelonInfo(typeof(HoaHaiDauBang.Main), "PvzRhTomiSakaeMods v1.0 - HoaHaiDauBang", "1.0.0", "TomiSakae")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace HoaHaiDauBang
{
    public class Main : MelonMod
    {
        // Thay vì OnInitializeMelon, thử OnApplicationStart để đảm bảo chạy sớm hơn
        // Hoặc bạn có thể giữ OnInitializeMelon nếu cách này không hiệu quả
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("[PvzRhTomiSakaeMods] PvzRhTomiSakaeMods v1.0 - HoaHaiDauBang đã khởi động!");
            LoadAssetsSync(); // Gọi hàm tải đồng bộ
        }

        // Hàm tải đồng bộ
        private void LoadAssetsSync()
        {
            string bundlePath = "Mods/TomiSakae_CayTuyChinh/hoahaidaubang";
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
                PlantType dualIceSunflowerType = (PlantType)2034; // Đổi ID thành 2034 cho Hoa Hai Đầu Băng

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
                                    newProducer.thePlantType = dualIceSunflowerType;
                                }
                                else
                                {
                                    existingProducer.thePlantType = dualIceSunflowerType;
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
                    CustomCore.RegisterCustomPlant<Producer, LopHoaHaiDauBang>(
                        (int)dualIceSunflowerType, prefabObj, previewObj,
                        new List<ValueTuple<int, int>> {
                            new ValueTuple<int, int>(2032, 2032),
                            new ValueTuple<int, int>(2032, 2032)
                        },
                       0f, 15f, 0, 300, 15f, 400
                    );

                    // --- THÊM THÔNG TIN ALMANAC NGAY SAU KHI ĐĂNG KÝ ---
                    string plantName = "Hướng Dương Hai Đầu Băng"; // Tên hiển thị mới
                    string plantDescription =
                        "Khi tạo ánh nắng sẽ tạo ra 2 mặt trời cùng lúc và đóng băng zombie trên hai hàng liền kề.\n" + // Dòng tagline mới
                        "Sản lượng nắng: <color=blue>50 ánh nắng/15 giây</color>\n" + // Tăng sản lượng nắng
                        "Công thức: <color=blue>Hướng Dương Băng + Hướng Dương Băng</color>\n\n" + // Công thức mới
                        "Hướng Dương Hai Đầu Băng với hai đầu hoa riêng biệt, có khả năng tạo ra hai mặt trời cùng lúc và đóng băng zombie trên hai hàng, giúp bạn thu hoạch nhiều nắng hơn và làm chậm nhiều zombie hơn."; // Mô tả lore mới
                    CustomCore.AddPlantAlmanacStrings((int)dualIceSunflowerType, plantName, plantDescription);
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

        // --- PATCH MỚI: ĐÓNG BĂNG ZOMBIE KHI ICESUNFLOWER TẠO SUN ---
        [HarmonyPatch(typeof(Producer), "ProduceSun")] // Patch vào cùng hàm
        public static class IceSunflower_ProduceSun_FreezeZombies_Patch
        {
            // Biến tĩnh để lưu trữ thông tin về hàng đã kích hoạt và thời gian
            public static int lastActivatedRow = -1;
            public static float lastActivatedTime = 0f;
            private static bool isRecursiveCall = false; // Biến tĩnh để theo dõi việc gọi lại

            // Postfix chạy SAU khi ProduceSun gốc hoàn thành
            public static void Postfix(Producer __instance)
            {
                // Bước 1: Kiểm tra xem có phải là DualIceSunflower không
                if (__instance != null && __instance.thePlantType == (PlantType)2034)
                {
                    // Bước 2: Đảm bảo Board tồn tại
                    if (Board.Instance == null)
                    {
                        MelonLogger.Warning("[PvzRhTomiSakaeMods] DualIceSunflower Patch: Board.Instance là null, không thể đóng băng zombie.");
                        return;
                    }

                    // Bước 3: Tạo thêm một mặt trời
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
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualIceSunflower Patch: Lỗi khi tạo thêm mặt trời: {0}\n{1}", ex.Message, ex.StackTrace);
                    }

                    // Bước 4: Lấy dòng của cây
                    int plantRow = __instance.thePlantRow;
                    int totalRows = Board.Instance.rowNum;

                    // Lưu lại hàng đã kích hoạt và thời gian
                    lastActivatedRow = plantRow;
                    lastActivatedTime = Time.time;

                    // Bước 5: Đóng băng tất cả zombie trên cùng hàng và hàng kế tiếp
                    try
                    {
                        int zombiesCount = 0;
                        
                        // Ngẫu nhiên chọn hàng trên hoặc hàng dưới
                        bool useUpperRow = UnityEngine.Random.value > 0.5f;
                        int secondRow = -1; // Hàng thứ hai để đóng băng
                        
                        // Xác định hàng thứ hai dựa trên lựa chọn ngẫu nhiên
                        if (useUpperRow && plantRow > 0) {
                            // Chọn hàng trên
                            secondRow = plantRow - 1;
                        }
                        else if (!useUpperRow && plantRow < totalRows - 1) {
                            // Chọn hàng dưới
                            secondRow = plantRow + 1;
                        }
                        else {
                            // Nếu không thể chọn hàng đã định, chọn hàng còn lại
                            if (plantRow > 0) {
                                secondRow = plantRow - 1;
                            }
                            else if (plantRow < totalRows - 1) {
                                secondRow = plantRow + 1;
                            }
                        }

                        Zombie_SetFreeze_Patch.secondActivatedRow = secondRow; // Lưu lại hàng thứ hai đã chọn

                        // Lặp qua tất cả zombie trong zombieArray
                        foreach (Zombie zombie in Board.Instance.zombieArray)
                        {
                            if (zombie == null) continue;

                            // Xử lý zombie trên hàng hiện tại
                            if (zombie.theZombieRow == plantRow || zombie.theZombieRow == secondRow)
                            {
                                // Đóng băng zombie trên cùng hàng với cây hoặc hàng thứ hai
                                zombiesCount++;

                                // Đóng băng zombie trong 10 giây (tăng thời gian đóng băng)
                                zombie.SetFreeze(10f);

                                // Tạo hiệu ứng băng tại vị trí zombie
                                try
                                {
                                    UnityEngine.Vector3 zombiePos = zombie.transform.position;
                                    Board.Instance.CreateFreeze(zombiePos);
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Warning("[PvzRhTomiSakaeMods] DualIceSunflower: Không thể tạo hiệu ứng băng: {0}", ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] DualIceSunflower Patch: Lỗi khi đóng băng zombie: {0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        // --- PATCH MỚI: CHẶN HIỆU ỨNG ĐÓNG BĂNG CHO ZOMBIE Ở HÀNG KHÁC ---
        [HarmonyPatch(typeof(Zombie), "SetFreeze")]
        public static class Zombie_SetFreeze_Patch
        {
            // Biến tĩnh để lưu trữ hàng thứ hai đã chọn
            public static int secondActivatedRow = -1;
            
            // Prefix chạy TRƯỚC khi SetFreeze gốc chạy
            // Trả về false để ngăn phương thức gốc chạy
            public static bool Prefix(Zombie __instance, float time)
            {
                // Nếu không phải do Hoa Băng gọi (thời gian > 0.1s), cho phép chạy bình thường
                if (Time.time - IceSunflower_ProduceSun_FreezeZombies_Patch.lastActivatedTime > 0.1f)
                {
                    return true; // Cho phép phương thức gốc chạy
                }

                // Nếu là do Hoa Băng gọi, chỉ cho phép đóng băng zombie trên các hàng đã chọn
                int zombieRow = __instance.theZombieRow;
                int plantRow = IceSunflower_ProduceSun_FreezeZombies_Patch.lastActivatedRow;
                
                // Cho phép đóng băng zombie trên cùng hàng với cây
                if (zombieRow == plantRow)
                {
                    return true;
                }
                
                // Cho phép đóng băng zombie trên hàng thứ hai đã chọn
                if (zombieRow == secondActivatedRow)
                {
                    return true;
                }

                // Không cho phép đóng băng zombie ở các hàng khác
                return false;
            }
        }
    }
}
