using System.Text.Json;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Data
{
    public static class BankConfigStore
    {
        private static readonly object _lock = new();

        private static string GetFilePath()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "bank_configs.json");
        }

        public static List<BankAccountConfig> GetAll()
        {
            lock (_lock)
            {
                var path = GetFilePath();
                if (!File.Exists(path))
                {
                    var defaults = GetDefaultSeeds();
                    SaveAllInternal(defaults);
                    return defaults;
                }

                try
                {
                    var json = File.ReadAllText(path);
                    var items = JsonSerializer.Deserialize<List<BankAccountConfig>>(json);
                    if (items == null || items.Count == 0)
                    {
                        var defaults = GetDefaultSeeds();
                        SaveAllInternal(defaults);
                        return defaults;
                    }
                    return items.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id).ToList();
                }
                catch
                {
                    return GetDefaultSeeds();
                }
            }
        }

        public static BankAccountConfig GetDefault()
        {
            var all = GetAll();
            return all.FirstOrDefault(x => x.IsDefault && x.IsActive)
                ?? all.FirstOrDefault(x => x.IsActive)
                ?? all.FirstOrDefault()
                ?? GetDefaultSeeds().First();
        }

        public static List<BankAccountConfig> GetActiveAccounts()
        {
            return GetAll().Where(x => x.IsActive).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id).ToList();
        }

        public static string BuildVietQrUrl(BankAccountConfig bank, decimal amount, string addInfo)
        {
            var code = string.IsNullOrWhiteSpace(bank.BankCode) ? "MB" : bank.BankCode.Trim();
            var accNo = string.IsNullOrWhiteSpace(bank.AccountNumber) ? "0988123456" : bank.AccountNumber.Trim();
            var tpl = string.IsNullOrWhiteSpace(bank.QrTemplate) ? "compact2" : bank.QrTemplate.Trim();
            var holder = Uri.EscapeDataString(string.IsNullOrWhiteSpace(bank.AccountHolder) ? "SIEU THI DIEN MAY XANH" : bank.AccountHolder.Trim().ToUpperInvariant());
            var info = Uri.EscapeDataString(string.IsNullOrWhiteSpace(addInfo) ? "THANH TOAN" : addInfo.Trim());
            return $"https://img.vietqr.io/image/{code}-{accNo}-{tpl}.png?amount={(long)Math.Max(0, amount)}&addInfo={info}&accountName={holder}";
        }

        public static void Save(BankAccountConfig model)
        {
            lock (_lock)
            {
                var list = GetAll();
                model.BankCode = (model.BankCode ?? "MB").Trim().ToUpperInvariant();
                model.BankName = (model.BankName ?? "MBBank - Ngân hàng TMCP Quân Đội").Trim();
                model.AccountNumber = (model.AccountNumber ?? "").Trim();
                model.AccountHolder = (model.AccountHolder ?? "").Trim().ToUpperInvariant();
                model.Branch = string.IsNullOrWhiteSpace(model.Branch) ? "Chi nhánh Trung tâm TP.HCM" : model.Branch.Trim();
                model.QrTemplate = string.IsNullOrWhiteSpace(model.QrTemplate) ? "compact2" : model.QrTemplate.Trim();
                model.UpdatedAt = DateTime.Now;

                if (model.Id > 0)
                {
                    var existing = list.FirstOrDefault(x => x.Id == model.Id);
                    if (existing != null)
                    {
                        if (model.IsDefault)
                        {
                            foreach (var b in list) b.IsDefault = false;
                            model.IsActive = true;
                        }
                        existing.BankCode = model.BankCode;
                        existing.BankName = model.BankName;
                        existing.AccountNumber = model.AccountNumber;
                        existing.AccountHolder = model.AccountHolder;
                        existing.Branch = model.Branch;
                        existing.QrTemplate = model.QrTemplate;
                        existing.IsDefault = model.IsDefault;
                        existing.IsActive = model.IsActive;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }
                else
                {
                    model.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1;
                    if (model.IsDefault || !list.Any(x => x.IsDefault))
                    {
                        foreach (var b in list) b.IsDefault = false;
                        model.IsDefault = true;
                        model.IsActive = true;
                    }
                    list.Add(model);
                }

                EnsureOneDefault(list);
                SaveAllInternal(list);
            }
        }

        public static void SetDefault(int id)
        {
            lock (_lock)
            {
                var list = GetAll();
                var target = list.FirstOrDefault(x => x.Id == id);
                if (target != null)
                {
                    foreach (var b in list) b.IsDefault = false;
                    target.IsDefault = true;
                    target.IsActive = true;
                    target.UpdatedAt = DateTime.Now;
                    SaveAllInternal(list);
                }
            }
        }

        public static void ToggleActive(int id)
        {
            lock (_lock)
            {
                var list = GetAll();
                var target = list.FirstOrDefault(x => x.Id == id);
                if (target != null)
                {
                    target.IsActive = !target.IsActive;
                    if (!target.IsActive && target.IsDefault)
                    {
                        target.IsDefault = false;
                    }
                    EnsureOneDefault(list);
                    SaveAllInternal(list);
                }
            }
        }

        public static bool Delete(int id, out string error)
        {
            error = string.Empty;
            lock (_lock)
            {
                var list = GetAll();
                if (list.Count <= 1)
                {
                    error = "Hệ thống phải giữ lại tối thiểu 1 tài khoản ngân hàng để tạo mã VietQR.";
                    return false;
                }

                var target = list.FirstOrDefault(x => x.Id == id);
                if (target == null)
                {
                    error = "Không tìm thấy tài khoản ngân hàng cần xóa.";
                    return false;
                }

                list.Remove(target);
                EnsureOneDefault(list);
                SaveAllInternal(list);
                return true;
            }
        }

        private static void EnsureOneDefault(List<BankAccountConfig> list)
        {
            if (!list.Any()) return;
            if (!list.Any(x => x.IsDefault && x.IsActive))
            {
                var firstActive = list.FirstOrDefault(x => x.IsActive) ?? list.First();
                firstActive.IsActive = true;
                firstActive.IsDefault = true;
            }
        }

        private static void SaveAllInternal(List<BankAccountConfig> list)
        {
            var path = GetFilePath();
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static List<BankAccountConfig> GetDefaultSeeds()
        {
            return new List<BankAccountConfig>
            {
                new BankAccountConfig
                {
                    Id = 1,
                    BankCode = "MB",
                    BankName = "MBBank - Ngân hàng TMCP Quân Đội",
                    AccountNumber = "0988123456",
                    AccountHolder = "SIEU THI DIEN MAY XANH",
                    Branch = "Chi nhánh Sở Giao Dịch TP. Hồ Chí Minh",
                    QrTemplate = "compact2",
                    IsDefault = true,
                    IsActive = true,
                    UpdatedAt = DateTime.Now
                },
                new BankAccountConfig
                {
                    Id = 2,
                    BankCode = "VCB",
                    BankName = "Vietcombank - Ngân hàng TMCP Ngoại thương Việt Nam",
                    AccountNumber = "1029384756",
                    AccountHolder = "CONG TY CP THE GIOI DI DONG DMX",
                    Branch = "Chi nhánh Tân Bình - TP. Hồ Chí Minh",
                    QrTemplate = "compact2",
                    IsDefault = false,
                    IsActive = true,
                    UpdatedAt = DateTime.Now
                },
                new BankAccountConfig
                {
                    Id = 3,
                    BankCode = "TCB",
                    BankName = "Techcombank - Ngân hàng TMCP Kỹ thương Việt Nam",
                    AccountNumber = "19038889996868",
                    AccountHolder = "SIEU THI DIEN MAY XANH",
                    Branch = "Chi nhánh Sài Gòn",
                    QrTemplate = "compact2",
                    IsDefault = false,
                    IsActive = true,
                    UpdatedAt = DateTime.Now
                }
            };
        }
    }
}

