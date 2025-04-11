using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using CustomizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(HoaBang.Main), "PvzRhTomiSakaeMods v1.0 - HoaBang", "1.0.0", "TomiSakae")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace HoaBang
{
    public class Main : MelonMod
    {
        // Thay vì OnInitializeMelon, thử OnApplicationStart để đảm bảo chạy sớm hơn
        // Hoặc bạn có thể giữ OnInitializeMelon nếu cách này không hiệu quả
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("[PvzRhTomiSakaeMods] PvzRhTomiSakaeMods v1.0 - HoaBang đã khởi động!");
            LoadAssetsSync(); // Gọi hàm tải đồng bộ
        }

        // Hàm tải đồng bộ
        private void LoadAssetsSync()
        {
            string bundlePath = "Mods/TomiSakae_CayTuyChinh/hoabang";
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
                PlantType iceSunflowerType = (PlantType)2032; // Đổi ID thành 2032 cho Hoa Băng

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
                                    newProducer.thePlantType = iceSunflowerType;
                                }
                                else
                                {
                                    existingProducer.thePlantType = iceSunflowerType;
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
                    CustomCore.RegisterCustomPlant<Producer, LopHoaBang>(
                       (int)iceSunflowerType, prefabObj, previewObj,
                       new List<ValueTuple<int, int>> {
                            new ValueTuple<int, int>(1, 10),
                            new ValueTuple<int, int>(10, 1)
                       },
                       0f, 15f, 0, 300, 15f, 200
                   );

                    // --- THÊM THÔNG TIN ALMANAC NGAY SAU KHI ĐĂNG KÝ ---
                    string plantName = "Hướng Dương Băng"; // Tên hiển thị
                    string plantDescription =
                        "Khi tạo ánh nắng sẽ đồng thời đóng băng tất cả zombie trên cùng 1 hàng.\n" + // Dòng tagline
                        "Sản lượng nắng: <color=blue>25 ánh nắng/15 giây</color>\n" + // Dòng sản lượng (dùng màu xanh)
                        "Công thức: <color=blue>Hoa Hướng Dương + Nấm Băng</color>\n\n" + // Dòng công thức (dùng màu xanh) - Thêm \n\n để có dòng trống
                        "Hướng Dương Băng tỏa ra hơi lạnh đóng băng zombie, giúp bạn có thêm thời gian phòng thủ."; // Phần mô tả lore
                    CustomCore.AddPlantAlmanacStrings((int)iceSunflowerType, plantName, plantDescription);
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

        // --- PATCH MỚI: ĐÓNG BĂNG ZOMBIE KHI ICESUNFLOWER TẠO SUN ---
        [HarmonyPatch(typeof(Producer), "ProduceSun")] // Patch vào cùng hàm
        public static class IceSunflower_ProduceSun_FreezeZombies_Patch
        {
            // Biến tĩnh để lưu trữ hàng của cây Hoa Băng đã kích hoạt gần đây nhất
            public static int lastActivatedRow = -1;
            public static float lastActivatedTime = 0f;

            // Postfix chạy SAU khi ProduceSun gốc hoàn thành
            public static void Postfix(Producer __instance)
            {
                // Bước 1: Kiểm tra xem có phải là IceSunflower không
                if (__instance != null && __instance.thePlantType == (PlantType)2032)
                {
                    // Bước 2: Đảm bảo Board tồn tại
                    if (Board.Instance == null)
                    {
                        MelonLogger.Warning("[PvzRhTomiSakaeMods] IceSunflower Patch: Board.Instance là null, không thể đóng băng zombie.");
                        return;
                    }

                    // Bước 3: Lấy dòng của cây
                    int plantRow = __instance.thePlantRow;
                    
                    // Lưu lại hàng đã kích hoạt và thời gian
                    lastActivatedRow = plantRow;
                    lastActivatedTime = Time.time;

                    // Bước 4: Đóng băng tất cả zombie trên cùng hàng
                    try
                    {
                        int zombiesCount = 0;

                        // Lặp qua tất cả zombie trong zombieArray
                        foreach (Zombie zombie in Board.Instance.zombieArray)
                        {
                            if (zombie == null) continue;

                            // Xử lý zombie dựa trên hàng
                            if (zombie.theZombieRow == plantRow)
                            {
                                // Đóng băng zombie trên cùng hàng với cây
                                zombiesCount++;
                                
                                // Đóng băng zombie trong 5 giây
                                zombie.SetFreeze(5f);
                                
                                // Tạo hiệu ứng băng tại vị trí zombie
                                try
                                {
                                    Vector3 zombiePos = zombie.transform.position;
                                    Board.Instance.CreateFreeze(zombiePos);
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Warning("[PvzRhTomiSakaeMods] IceSunflower: Không thể tạo hiệu ứng băng: {0}", ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[PvzRhTomiSakaeMods] IceSunflower Patch: Lỗi khi đóng băng zombie: {0}\n{1}", ex.Message, ex.StackTrace);
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
                // Nếu không phải do Hoa Băng gọi (thời gian > 0.1s), cho phép chạy bình thường
                if (Time.time - IceSunflower_ProduceSun_FreezeZombies_Patch.lastActivatedTime > 0.1f)
                {
                    return true; // Cho phép phương thức gốc chạy
                }

                // Nếu là do Hoa Băng gọi, chỉ cho phép đóng băng zombie trên cùng hàng
                int zombieRow = __instance.theZombieRow;
                int plantRow = IceSunflower_ProduceSun_FreezeZombies_Patch.lastActivatedRow;

                // Nếu zombie không nằm trên cùng hàng với cây Hoa Băng, chặn hiệu ứng đóng băng
                if (zombieRow != plantRow)
                {
                    return false; // Không cho phép phương thức gốc chạy
                }

                // Cho phép đóng băng zombie trên cùng hàng
                return true;
            }
        }
    }
}
