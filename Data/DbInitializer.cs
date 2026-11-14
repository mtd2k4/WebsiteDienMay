using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (context.Categories.Any())
            {
                SeedInitialStockImports(context);
                SeedInitialCoupons(context);
                SeedInitialBanners(context);
                SeedProductVariantsAndVat(context);
                return;
            }

            // 1. Seed Categories (8 danh mục chuẩn Điện Máy Xanh, hình ảnh từ nguồn dienmayxanh.com)
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Tivi",
                    Slug = "tivi",
                    Description = "Smart Tivi, Google Tivi QLED, OLED, Mini LED 4K chính hãng giá tốt",
                    IconUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/Tivi110032.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/24/51/2451461461a5fa993e3b953c12154436.png",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Category
                {
                    Name = "Tủ lạnh",
                    Slug = "tu-lanh",
                    Description = "Tủ lạnh Inverter tiết kiệm điện, Side by Side, Multi Door cao cấp",
                    IconUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/TL105955.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/11/7a/117ae0e6d2499ae572e54335cc2926ea.png",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Category
                {
                    Name = "Máy giặt",
                    Slug = "may-giat",
                    Description = "Máy giặt cửa ngang, cửa trên Inverter giặt sạch sâu, bảo vệ sợi vải",
                    IconUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/MG105936.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/68/45/68457a286f9a7850b25b996b3bfc601c.png",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new Category
                {
                    Name = "Máy lạnh - Điều hòa",
                    Slug = "may-lanh",
                    Description = "Máy lạnh Inverter 1 HP, 1.5 HP, 2 HP làm lạnh nhanh, lọc khí kháng khuẩn",
                    IconUrl = "https://cdn.tgdd.vn/2026/11/timerseo/1751098000005-may-lanh-tron-20261105-600x600.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/f9/d1/f9d1caf83f83d7ed6ed83e452f44fc65.png",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new Category
                {
                    Name = "Điện thoại",
                    Slug = "dien-thoai",
                    Description = "Smartphone chính hãng Apple iPhone, Samsung, OPPO, Xiaomi cấu hình mạnh",
                    IconUrl = "https://cdn.tgdd.vn/Products/Images/42/370982/iphone-18-pro-max-do-thumb.jpg",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/5c/e5/5ce5c8d856c99ef01528033c294d6389.png",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new Category
                {
                    Name = "Laptop",
                    Slug = "laptop",
                    Description = "Laptop học tập, văn phòng, đồ họa kỹ thuật Dell, Asus, HP, Lenovo chính hãng",
                    IconUrl = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003457-laptop-tron-20261104-600x600.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/a6/97/a697dea35d2b2d853ade4d2ccb4d5382.png",
                    DisplayOrder = 6,
                    IsActive = true
                },
                new Category
                {
                    Name = "Gia dụng nhà bếp",
                    Slug = "gia-dung",
                    Description = "Nồi chiên không dầu, nồi cơm điện, máy rửa chén hiện đại tiện nghi",
                    IconUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/GD110019.png",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/d1/ef/d1efb16ffc7b86cc908e6abae633ad88.png",
                    DisplayOrder = 7,
                    IsActive = true
                },
                new Category
                {
                    Name = "Máy lọc nước",
                    Slug = "may-loc-nuoc",
                    Description = "Máy lọc nước RO nóng nguội lạnh, Hydro-ion kiềm bảo vệ sức khỏe gia đình",
                    IconUrl = "https://cdn.tgdd.vn/2026/11/timerseo/325006.jpg",
                    BannerUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/f1/8c/f18c8e87c390c9ee07b16da74bc253fb.png",
                    DisplayOrder = 8,
                    IsActive = true
                }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 2. Seed Brands (Logo chính thức từ CDN dienmayxanh.com)
            var brands = new List<Brand>
            {
                new Brand { Name = "Samsung", Slug = "samsung", Origin = "Hàn Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/89/ba/89baf2121494abea518fca0c710475c6.png" },
                new Brand { Name = "LG", Slug = "lg", Origin = "Hàn Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/b2/fc/b2fc6b8befcdfe03c16d0d4dda903092.png" },
                new Brand { Name = "Sony", Slug = "sony", Origin = "Nhật Bản", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/9c/fa/9cfabab46685f2f48704dc575feb1990.png" },
                new Brand { Name = "TCL", Slug = "tcl", Origin = "Trung Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/33/00/3300cc078c352477f94ca33d8b1149f8.png" },
                new Brand { Name = "Panasonic", Slug = "panasonic", Origin = "Nhật Bản", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/7b/fe/7bfefb3ef979c2a3a4ed10ee8afaec7f.png" },
                new Brand { Name = "Toshiba", Slug = "toshiba", Origin = "Nhật Bản", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/a5/82/a5829434d33371eeab59bfb2aaa47a44.png" },
                new Brand { Name = "Aqua", Slug = "aqua", Origin = "Nhật Bản", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/77/70/7770c6499cb79088e8663b61bda39539.png" },
                new Brand { Name = "Hitachi", Slug = "hitachi", Origin = "Nhật Bản", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/75/15/7515098fb8ade19052fa76e3ed4284ef.png" },
                new Brand { Name = "Casper", Slug = "casper", Origin = "Thái Lan", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/e6/4b/e64bff8ef49f4af70d2981e842933410.png" },
                new Brand { Name = "Apple", Slug = "apple", Origin = "Mỹ", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/57/03/5703d996359650c57421b72f3f7ff5cd.png" },
                new Brand { Name = "Xiaomi", Slug = "xiaomi", Origin = "Trung Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/e2/19/e219542673383aa4475ba130dcaeb2f0.png" },
                new Brand { Name = "OPPO", Slug = "oppo", Origin = "Trung Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/2c/ea/2cea467041fb9effb3a6d3dcc88f38f8.png" },
                new Brand { Name = "Asus", Slug = "asus", Origin = "Đài Loan", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/9f/72/9f72697b78e17090628020cca9cce5e6.png" },
                new Brand { Name = "Dell", Slug = "dell", Origin = "Mỹ", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/c9/ea/c9eac61be9530e9c6c4404ba573086c4.png" },
                new Brand { Name = "HP", Slug = "hp", Origin = "Mỹ", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/b2/11/b2118350c0f60b9ecb1e8ef65cb2de96.png" },
                new Brand { Name = "Lenovo", Slug = "lenovo", Origin = "Trung Quốc", LogoUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Category/0b/90/0b907e4551b7ad8857426905ae627cad.png" }
            };
            context.Brands.AddRange(brands);
            context.SaveChanges();

            int CatId(string slug) => categories.First(c => c.Slug == slug).Id;
            int BrandId(string slug) => brands.First(b => b.Slug == slug).Id;

            // 3. Seed 32 Products across 8 categories (100% real image URLs from dienmayxanh.com)
            var products = new List<Product>
            {
                // ================= DANH MỤC 1: TIVI =================
                new Product
                {
                    Name = "LG Smart TV QNED 4K 65 inch 65QNED70BSA",
                    Code = "DMX-TV01",
                    Slug = "lg-smart-tv-65qned70bsa",
                    CategoryId = CatId("tivi"),
                    BrandId = BrandId("lg"),
                    OriginalPrice = 22900000,
                    Price = 16490000,
                    StockQuantity = 18,
                    SoldCount = 142,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/365275.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/365273.jpg;https://cdn.tgdd.vn/2026/11/timerseo/365298.jpg",
                    ShortDescription = "Màn hình QNED 4K 65 inch sắc nét, Bộ xử lý AI alpha 5 Gen7 4K, Hệ điều hành webOS 24 thông minh",
                    Description = "Smart Tivi QNED LG 4K 65 inch 65QNED70BSA sở hữu công nghệ màu sắc Quantum Dot kết hợp NanoCell mang lại dải màu rực rỡ, chuẩn xác từ mọi góc nhìn. Bộ xử lý AI nâng cấp hình ảnh chuẩn 4K cùng âm thanh vòm AI Sound Pro 9.1.2 sống động cho trải nghiệm giải trí tại gia đẳng cấp.",
                    Specifications = "Kích cỡ màn hình: 65 inch\nĐộ phân giải: 4K Ultra HD (3840 x 2160)\nLoại màn hình: QNED 4K\nHệ điều hành: webOS 24\nCông nghệ hình ảnh: HDR10 Pro, Dynamic Tone Mapping\nCông nghệ âm thanh: AI Sound Pro, Clear Voice Pro\nTần số quét thực: 60 Hz\nKết nối: Wi-Fi, Bluetooth 5.1, 3 cổng HDMI, 2 cổng USB\nNăm ra mắt: 2026\nHãng sản xuất: LG Chính hãng",
                    PromotionText = "Miễn phí công lắp đặt tại nhà; Tặng gói xem truyền hình giải trí 12 tháng; Trả góp 0% lãi suất qua thẻ tín dụng; Bảo hành chính hãng 24 tháng tận nơi",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 86
                },
                new Product
                {
                    Name = "Samsung Smart TV Mini LED 4K 65 inch UA65M8XHA",
                    Code = "DMX-TV02",
                    Slug = "samsung-smart-tv-mini-led-ua65m8xha",
                    CategoryId = CatId("tivi"),
                    BrandId = BrandId("samsung"),
                    OriginalPrice = 26900000,
                    Price = 19990000,
                    StockQuantity = 15,
                    SoldCount = 98,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/364653.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/366151.jpg;https://cdn.tgdd.vn/2026/11/timerseo/337160.jpg",
                    ShortDescription = "Công nghệ đèn nền Quantum Mini LED siêu tương phản, Thiết kế AirSlim tràn viền sang trọng",
                    Description = "Samsung Smart TV Mini LED UA65M8XHA mang đến độ tương phản vượt trội với hệ thống đèn nền Mini LED kiểm soát ánh sáng chính xác. Tích hợp hệ điều hành Tizen OS cùng tính năng điều khiển thông minh SmartThings kết nối hệ sinh thái gia dụng trong nhà.",
                    Specifications = "Kích cỡ màn hình: 65 inch\nĐộ phân giải: 4K Ultra HD\nCông nghệ màn hình: Quantum Mini LED\nHệ điều hành: Tizen OS\nCông nghệ âm thanh: OTS Lite, Q-Symphony\nĐiều khiển: One Remote sạc năng lượng mặt trời\nCổng kết nối: 4 HDMI, 2 USB, LAN, Optical\nBảo hành: 24 tháng chính hãng",
                    PromotionText = "Giảm ngay 500.000đ khi thanh toán chuyển khoản; Miễn phí giao hàng và lắp đặt bán kính 50km; Hỗ trợ trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 74
                },
                new Product
                {
                    Name = "Sony Google TV BRAVIA 2 II 4K 55 inch K-55S25VM2",
                    Code = "DMX-TV03",
                    Slug = "sony-google-tv-bravia-2-k-55s25vm2",
                    CategoryId = CatId("tivi"),
                    BrandId = BrandId("sony"),
                    OriginalPrice = 18500000,
                    Price = 14890000,
                    StockQuantity = 22,
                    SoldCount = 115,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/339081.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/339080.jpg",
                    ShortDescription = "Bộ xử lý X1 4K Processor, Công nghệ Live Color tái tạo màu sắc chân thực, Tìm kiếm giọng nói tiếng Việt",
                    Description = "Google Tivi Sony BRAVIA 2 II 4K 55 inch K-55S25VM2 nổi bật với bộ xử lý 4K Processor X1 mạnh mẽ giảm nhiễu và tăng cường độ chi tiết. Hệ thống loa Dolby Atmos kết hợp X-Protection PRO chống ẩm, chống sét và chống bụi bền bỉ theo tiêu chuẩn Nhật Bản.",
                    Specifications = "Kích cỡ màn hình: 55 inch\nĐộ phân giải: 4K Ultra HD\nBộ xử lý: 4K Processor X1\nHệ điều hành: Google TV\nTìm kiếm giọng nói: Google Assistant tiếng Việt\nCông nghệ âm thanh: Dolby Atmos, S-Master Digital Amplifier\nTổng công suất loa: 20W\nXuất xứ thương hiệu: Nhật Bản",
                    PromotionText = "Tặng giá treo tivi chính hãng; Miễn phí công lắp đặt; Bảo hành điện tử chính hãng Sony Việt Nam 24 tháng",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 63
                },
                new Product
                {
                    Name = "TCL Google TV QD-Mini LED 4K 55 inch 55C6KS",
                    Code = "DMX-TV04",
                    Slug = "tcl-google-tv-qd-mini-led-55c6ks",
                    CategoryId = CatId("tivi"),
                    BrandId = BrandId("tcl"),
                    OriginalPrice = 15900000,
                    Price = 11490000,
                    StockQuantity = 30,
                    SoldCount = 164,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/335746.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/335889.jpg;https://cdn.tgdd.vn/2026/11/timerseo/281936.jpg",
                    ShortDescription = "Màn hình QD-Mini LED độ sáng cao, Âm thanh Onkyo Hi-Fi tích hợp loa trầm, Google TV kho ứng dụng phong phú",
                    Description = "TCL Google TV QD-Mini LED 55C6KS kết hợp ưu điểm của QLED và Mini LED cho độ sáng vượt trội, màu sắc chuẩn điện ảnh cùng hệ thống loa Onkyo danh tiếng.",
                    Specifications = "Kích cỡ màn hình: 55 inch\nĐộ phân giải: 4K Ultra HD\nCông nghệ màn hình: QD-Mini LED\nHệ điều hành: Google TV\nCông nghệ âm thanh: Dolby Atmos, Loa Onkyo\nKết nối: Bluetooth 5.0, Wi-Fi băng tần kép, 3 HDMI\nBảo hành: 24 tháng",
                    PromotionText = "Miễn phí lắp đặt tận nơi; Đổi mới trong 30 ngày đầu nếu có lỗi nhà sản xuất; Trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 52
                },

                // ================= DANH MỤC 2: TỦ LẠNH =================
                new Product
                {
                    Name = "Tủ lạnh Aqua Inverter 646 lít Side by Side AQR-S682XA(WGB)",
                    Code = "DMX-TL01",
                    Slug = "tu-lanh-aqua-inverter-646-lit-aqr-s682xa-wgb",
                    CategoryId = CatId("tu-lanh"),
                    BrandId = BrandId("aqua"),
                    OriginalPrice = 21900000,
                    Price = 15990000,
                    StockQuantity = 12,
                    SoldCount = 87,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/359518110433.jpg",
                    ExtraImages = "https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1943/363377/tu-lanh-funiki-inverter-415-lit-multi-door-hr-m8415ss-thumb-639086520716518196-600x600.jpg",
                    ShortDescription = "Dung tích lớn 646 lít Side by Side, Công nghệ Twin Inverter tiết kiệm điện, Kháng khuẩn khử mùi DEO Fresh",
                    Description = "Tủ lạnh Aqua Inverter 646 lít AQR-S682XA(WGB) có thiết kế mặt gương sang trọng, dung tích cực lớn đáp ứng nhu cầu lưu trữ thực phẩm cho gia đình đông thành viên. Công nghệ Twin Inverter vận hành êm ái và duy trì nhiệt độ ổn định.",
                    Specifications = "Kiểu tủ: Side by Side 2 cánh\nDung tích sử dụng: 646 lít (Trên 5 người)\nCông nghệ tiết kiệm điện: Twin Inverter\nCông nghệ làm lạnh: Làm lạnh đa chiều 360 độ\nCông nghệ kháng khuẩn: Khử mùi DEO Fresh\nChất liệu cửa tủ: Mặt gương cao cấp\nBảo hành máy nén: 10 năm",
                    PromotionText = "Miễn phí vận chuyển và lắp đặt toàn quốc; Bảo hành máy nén Inverter 10 năm; Hỗ trợ trả góp 0%",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 68
                },
                new Product
                {
                    Name = "Tủ lạnh Hitachi Inverter 532 lít Side by Side HRSN9563DDXVN",
                    Code = "DMX-TL02",
                    Slug = "tu-lanh-hitachi-inverter-532-lit-hrsn9563ddxvn",
                    CategoryId = CatId("tu-lanh"),
                    BrandId = BrandId("hitachi"),
                    OriginalPrice = 24500000,
                    Price = 19490000,
                    StockQuantity = 14,
                    SoldCount = 65,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/360346105744.jpg",
                    ShortDescription = "Cảm biến nhiệt Eco thông minh, Làm lạnh vòng cung Dual Surround Cooling, Bảng điều khiển cảm ứng ngoài",
                    Description = "Tủ lạnh Hitachi Inverter 532 lít HRSN9563DDXVN sở hữu hệ thống làm lạnh vòng cung kép giúp bảo quản rau củ và thực phẩm tươi ngon lâu hơn mà không bị lẫn mùi giữa ngăn đông và ngăn mát.",
                    Specifications = "Kiểu tủ: Side by Side\nDung tích sử dụng: 532 lít\nCông nghệ tiết kiệm điện: Inverter kết hợp cảm biến Eco\nCông nghệ kháng khuẩn: Bộ lọc Triple Power 3 lớp\nTiện ích: Bảng điều khiển cảm ứng bên ngoài, Chuông báo khi quên đóng cửa\nThương hiệu: Hitachi Nhật Bản",
                    PromotionText = "Tặng bộ hộp bảo quản thực phẩm cao cấp; Miễn phí giao lắp tận nhà; Trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 49
                },
                new Product
                {
                    Name = "Tủ lạnh Toshiba Inverter 471 lít Multi Door GR-RF606WI-PMV(60)-AG",
                    Code = "DMX-TL03",
                    Slug = "tu-lanh-toshiba-inverter-471-lit-gr-rf606wi-pmv",
                    CategoryId = CatId("tu-lanh"),
                    BrandId = BrandId("toshiba"),
                    OriginalPrice = 19900000,
                    Price = 15490000,
                    StockQuantity = 16,
                    SoldCount = 112,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/342628105514.jpg",
                    ExtraImages = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/370337072738.png",
                    ShortDescription = "Thiết kế Multi Door 4 cánh hiện đại, Công nghệ Origin Inverter, Khử mùi PureBio kết hợp tia Plasma",
                    Description = "Tủ lạnh Toshiba Inverter 471 lít GR-RF606WI-PMV(60)-AG tích hợp kết nối thông minh TSmartLife điều khiển từ xa qua điện thoại và hệ thống khử mùi diệt khuẩn PureBio giữ thực phẩm luôn tươi sạch.",
                    Specifications = "Kiểu tủ: Multi Door 4 cánh\nDung tích sử dụng: 471 lít\nCông nghệ Inverter: Origin Inverter\nCông nghệ bảo quản: Ngăn cấp đông mềm Flexible Zone\nĐiều khiển thông minh: Ứng dụng TSmartLife\nChất liệu khay ngăn: Kính chịu lực cao cấp",
                    PromotionText = "Giảm trực tiếp 4.410.000đ vào giá bán; Miễn phí lắp đặt; Bảo hành chính hãng 24 tháng tận nhà",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 77
                },
                new Product
                {
                    Name = "Tủ lạnh LG Inverter 400 lít Ngăn đá trên F40BG",
                    Code = "DMX-TL04",
                    Slug = "tu-lanh-lg-inverter-400-lit-f40bg",
                    CategoryId = CatId("tu-lanh"),
                    BrandId = BrandId("lg"),
                    OriginalPrice = 14200000,
                    Price = 11290000,
                    StockQuantity = 25,
                    SoldCount = 138,
                    ImageUrl = "https://cdn.tgdd.vn/2026/10/timerseo/359980-600x600-3.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/10/timerseo/327797-600x600-3.jpg",
                    ShortDescription = "Làm lạnh từ cửa tủ DoorCooling+, Ngăn đông mềm Fresh 0 Zone không cần rã đông, Smart Inverter tiết kiệm điện",
                    Description = "Tủ lạnh LG Inverter 400 lít F40BG với công nghệ DoorCooling+ giúp hơi lạnh tỏa đều nhanh chóng ngay từ cửa tủ, kết hợp bộ lọc Hygiene Fresh+ loại bỏ tới 99.99% vi khuẩn.",
                    Specifications = "Kiểu tủ: Ngăn đá trên 2 cánh\nDung tích sử dụng: 400 lít\nCông nghệ tiết kiệm điện: Smart Inverter\nCông nghệ làm lạnh: DoorCooling+, LinearCooling\nNgăn bảo quản: Ngăn thịt cá 0 độ C Fresh 0 Zone\nThương hiệu: LG Hàn Quốc",
                    PromotionText = "Miễn phí giao hàng tận nơi; Bảo hành máy nén 10 năm; Hỗ trợ trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 54
                },

                // ================= DANH MỤC 3: MÁY GIẶT =================
                new Product
                {
                    Name = "Máy giặt Panasonic Inverter 9 Kg Cửa trước NA-FJ90X1DV",
                    Code = "DMX-MG01",
                    Slug = "may-giat-panasonic-inverter-9-kg-na-fj90x1dv",
                    CategoryId = CatId("may-giat"),
                    BrandId = BrandId("panasonic"),
                    OriginalPrice = 12990000,
                    Price = 9890000,
                    StockQuantity = 20,
                    SoldCount = 156,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/367876.jpg",
                    ExtraImages = "https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1944/334977/may-giat-panasonic-inverter-115-kg-na-fd115x3bv-thum-638767059110097582-300x300.jpg",
                    ShortDescription = "Giặt nước nóng StainMaster+ diệt khuẩn 99.99%, Hệ thống bọt siêu mịn ActiveFoam đánh bay vết bẩn cứng đầu",
                    Description = "Máy giặt Panasonic Inverter 9 Kg NA-FJ90X1DV trang bị công nghệ giặt nước nóng StainMaster+ giúp loại bỏ các tác nhân gây dị ứng và vết bẩn cứng đầu trên cổ áo, tay áo hiệu quả đồng thời tiết kiệm điện nước tối ưu.",
                    Specifications = "Loại máy giặt: Cửa trước - Lồng ngang\nKhối lượng giặt: 9 Kg (Phù hợp 3 - 5 người)\nCông nghệ tiết kiệm điện: 3Di Inverter\nTốc độ quay vắt: 1200 vòng/phút\nCông nghệ giặt: StainMaster+ giặt nước nóng, ActiveFoam\nChất liệu lồng giặt: Thép không gỉ\nThương hiệu: Panasonic Nhật Bản",
                    PromotionText = "Miễn phí công lắp đặt; Bảo hành động cơ 12 năm chính hãng; Đổi trả trong 30 ngày nếu lỗi kỹ thuật",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 92
                },
                new Product
                {
                    Name = "Máy giặt Hitachi Inverter 10.5 Kg Cửa ngang BD-1054HVOW",
                    Code = "DMX-MG02",
                    Slug = "may-giat-hitachi-inverter-10-5-kg-bd-1054hvow",
                    CategoryId = CatId("may-giat"),
                    BrandId = BrandId("hitachi"),
                    OriginalPrice = 15490000,
                    Price = 11990000,
                    StockQuantity = 15,
                    SoldCount = 81,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/357869.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/357870.jpg",
                    ShortDescription = "Khối lượng giặt 10.5 Kg, Giặt hơi nước Steam Iron giảm nhăn quần áo, Tự động vệ sinh lồng giặt Auto Self Clean",
                    Description = "Máy giặt Hitachi Inverter 10.5 Kg BD-1054HVOW sử dụng động cơ truyền động Inverter chống rung ồn hiệu quả, tích hợp chương trình giặt hơi nước diệt khuẩn và tự động làm sạch mặt ngoài lồng giặt sau mỗi chu trình.",
                    Specifications = "Loại máy giặt: Cửa ngang\nKhối lượng giặt: 10.5 Kg (Trên 7 người)\nĐộng cơ: Inverter tiết kiệm điện\nChương trình giặt: 16 chương trình chuyên biệt\nTiện ích: Giặt hơi nước, Tự vệ sinh lồng giặt, Khóa trẻ em\nBảo hành: 24 tháng chính hãng",
                    PromotionText = "Tặng chân đế máy giặt chống rung hoặc phiếu mua hàng 300.000đ; Miễn phí lắp đặt tận nhà",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 61
                },
                new Product
                {
                    Name = "Máy giặt Toshiba Inverter 13 Kg Lồng ngang TW-T21BU140UWV(MG)",
                    Code = "DMX-MG03",
                    Slug = "may-giat-toshiba-inverter-13-kg-tw-t21bu140uwv",
                    CategoryId = CatId("may-giat"),
                    BrandId = BrandId("toshiba"),
                    OriginalPrice = 17900000,
                    Price = 13690000,
                    StockQuantity = 10,
                    SoldCount = 74,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/333794.jpg",
                    ExtraImages = "https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1944/364913/may-giat-toshiba-inverter-12-kg-aw-dk1300kv-mk-0-639135021203900212-300x300.jpg",
                    ShortDescription = "Siêu bọt khí nano UFB thấm sâu sợi vải, Công nghệ GreatSteam diệt khuẩn, Điều khiển thông minh qua ứng dụng",
                    Description = "Máy giặt Toshiba Inverter 13 Kg TW-T21BU140UWV(MG) sở hữu khối lượng giặt lớn 13 Kg, thích hợp giặt chăn mền và quần áo cho đại gia đình với công nghệ Ultra Fine Bubble độc quyền từ Toshiba.",
                    Specifications = "Loại máy giặt: Cửa trước\nKhối lượng giặt: 13 Kg\nCông nghệ Inverter: Origin Inverter\nCông nghệ giặt: Ultra Fine Bubble (UFB), Aroma+ lưu hương thơm lâu\nTốc độ vắt tối đa: 1400 vòng/phút\nĐiều khiển: Cảm ứng song ngữ Anh - Việt",
                    PromotionText = "Miễn phí công lắp đặt; Bảo hành động cơ 10 năm; Hỗ trợ trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 58
                },
                new Product
                {
                    Name = "Máy giặt LG AI DD Inverter 9 Kg FB1209S5W",
                    Code = "DMX-MG04",
                    Slug = "may-giat-lg-inverter-9-kg-fb1209s5w",
                    CategoryId = CatId("may-giat"),
                    BrandId = BrandId("lg"),
                    OriginalPrice = 10490000,
                    Price = 7990000,
                    StockQuantity = 28,
                    SoldCount = 215,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/334633.jpg",
                    ShortDescription = "Động cơ truyền động trực tiếp AI DD bảo vệ sợi vải tốt hơn 18%, Giặt hơi nước Steam ngừa dị ứng",
                    Description = "Máy giặt LG Inverter 9 Kg FB1209S5W ứng dụng trí tuệ nhân tạo AI DD tự động cảm biến độ mềm của vải và khối lượng đồ giặt để tối ưu chuyển động lồng giặt, vận hành bền bỉ và cực kỳ êm ái.",
                    Specifications = "Loại máy giặt: Cửa trước\nKhối lượng giặt: 9 Kg\nĐộng cơ: Truyền động trực tiếp Intello DD (AI DD)\nCông nghệ giặt: Giặt hơi nước Steam, 6 chuyển động 6 Motion DD\nTốc độ quay vắt: 1200 vòng/phút\nThương hiệu: LG Hàn Quốc",
                    PromotionText = "Giá sốc trực tuyến giảm 2.500.000đ; Miễn phí giao hàng và lắp đặt; Bảo hành động cơ 10 năm",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 134
                },

                // ================= DANH MỤC 4: MÁY LẠNH - ĐIỀU HÒA =================
                new Product
                {
                    Name = "Máy lạnh Panasonic Inverter 1 HP CU/CS-PU9AKH-8",
                    Code = "DMX-ML01",
                    Slug = "may-lanh-panasonic-inverter-1-hp-cu-cs-pu9akh-8",
                    CategoryId = CatId("may-lanh"),
                    BrandId = BrandId("panasonic"),
                    OriginalPrice = 13290000,
                    Price = 11490000,
                    StockQuantity = 24,
                    SoldCount = 189,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/3051098001627-may-lanh-tron-20261105-600x600.png",
                    ShortDescription = "Công nghệ lọc khí Nanoe-G lọc bụi mịn PM2.5, Chế độ ECO tích hợp AI tiết kiệm điện đến 20%, Làm lạnh nhanh Powerful",
                    Description = "Máy lạnh Panasonic Inverter 1 HP CU/CS-PU9AKH-8 trang bị công nghệ lọc khí Nanoe-G giải phóng hàng nghìn tỷ ion âm bắt giữ hạt bụi mịn PM2.5, mang lại bầu không khí trong lành và mát lạnh tức thì.",
                    Specifications = "Loại máy: Máy lạnh 1 chiều (chỉ làm lạnh)\nCông suất làm lạnh: 1 HP - 9.040 BTU (Phòng dưới 15m2)\nCông nghệ tiết kiệm điện: Inverter kết hợp ECO + AI\nLọc không khí: Công nghệ Nanoe-G lọc bụi mịn PM2.5\nLoại Gas: R-32 thân thiện môi trường\nSản xuất tại: Malaysia",
                    PromotionText = "Miễn phí công lắp đặt và hút chân không; Giảm 50% chi phí ống đồng; Bảo hành máy nén 7 năm",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 148
                },
                new Product
                {
                    Name = "Máy lạnh Samsung WindFree Inverter 1 HP AR40H09D0ATNSV",
                    Code = "DMX-ML02",
                    Slug = "may-lanh-samsung-inverter-1-hp-ar40h09d0atnsv",
                    CategoryId = CatId("may-lanh"),
                    BrandId = BrandId("samsung"),
                    OriginalPrice = 10990000,
                    Price = 8690000,
                    StockQuantity = 32,
                    SoldCount = 143,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/1751098000224-may-lanh-tron-20261105-600x600.png",
                    ShortDescription = "Làm lạnh thông minh không gió buốt, Động cơ Digital Inverter Boost tiết kiệm điện 73%, Lưới lọc kháng khuẩn Easy Filter Plus",
                    Description = "Máy lạnh Samsung Inverter 1 HP AR40H09D0ATNSV giúp làm lạnh nhanh hơn 43% nhờ thiết kế quạt gió lớn hơn và cửa gió rộng hơn, kết hợp công nghệ Digital Inverter Boost duy trì nhiệt độ mong muốn mà không cần bật tắt liên tục.",
                    Specifications = "Loại máy: Máy lạnh 1 chiều\nCông suất: 1 HP - 9.000 BTU\nCông nghệ Inverter: Digital Inverter Boost\nLàm lạnh nhanh: Fast Cooling\nTiện ích: Tự làm sạch Auto Clean, Chế độ ngủ ngon Good Sleep\nBảo hành máy nén: 10 năm",
                    PromotionText = "Miễn phí công lắp đặt; Tặng combo vệ sinh máy lạnh miễn phí năm đầu; Hỗ trợ trả góp 0%",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 87
                },
                new Product
                {
                    Name = "Máy lạnh Daikin Inverter 1 HP ATKB25YVMV",
                    Code = "DMX-ML03",
                    Slug = "may-lanh-daikin-inverter-1-hp-atkb25yvmv",
                    CategoryId = CatId("may-lanh"),
                    BrandId = BrandId("toshiba"),
                    OriginalPrice = 12490000,
                    Price = 10590000,
                    StockQuantity = 19,
                    SoldCount = 176,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/1751098000005-may-lanh-tron-20261105-600x600.png",
                    ShortDescription = "Luồng gió Coanda phân bổ đều khắp phòng, Phin lọc Enzyme Blue khử mùi diệt khuẩn, Bo mạch chống sụt áp bền bỉ",
                    Description = "Máy lạnh Daikin Inverter 1 HP ATKB25YVMV thiết kế mặt nạ Coanda đưa luồng gió đi dọc theo trần nhà, tránh thổi trực tiếp vào cơ thể gây cảm lạnh, cùng dàn tản nhiệt chống ăn mòn cao cấp.",
                    Specifications = "Loại máy: 1 chiều làm lạnh\nCông suất: 1 HP - 9.200 BTU\nCông nghệ tiết kiệm điện: Inverter, Econo\nKháng khuẩn khử mùi: Phin lọc Enzyme Blue\nTiện ích: Chức năng chống ẩm mốc, Luồng gió Coanda\nLoại Gas: R-32",
                    PromotionText = "Miễn phí công lắp đặt tiêu chuẩn; Bảo hành máy nén 5 năm chính hãng; Trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 121
                },
                new Product
                {
                    Name = "Máy lạnh Casper EcoCity Inverter 1 HP NIS-C09R2T62",
                    Code = "DMX-ML04",
                    Slug = "may-lanh-casper-inverter-1-hp-nis-c09r2t62",
                    CategoryId = CatId("may-lanh"),
                    BrandId = BrandId("casper"),
                    OriginalPrice = 7990000,
                    Price = 6290000,
                    StockQuantity = 40,
                    SoldCount = 264,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/1751098000203-may-lanh-tron-20261105-600x600.png",
                    ShortDescription = "Làm lạnh nhanh Turbo trong 30 giây, Công nghệ i-Saving tiết kiệm điện chỉ từ 4.000đ/đêm, Dàn tản nhiệt đồng mạ vàng",
                    Description = "Máy lạnh Inverter 1 HP NIS-C09R2T62 là dòng điều hòa bán chạy hàng đầu nhờ mức giá hợp lý, khả năng làm lạnh siêu tốc Turbo chỉ sau 30 giây khởi động và dàn tản nhiệt đồng mạ vàng bền bỉ.",
                    Specifications = "Loại máy: 1 chiều làm lạnh\nCông suất: 1 HP - 9.000 BTU\nCông nghệ Inverter: i-Saving Inverter\nCảm biến nhiệt độ: iFeel tự điều chỉnh theo thân nhiệt\nTự làm sạch: iClean tự động làm sạch dàn lạnh\nDàn tản nhiệt: 100% ống đồng mạ vàng",
                    PromotionText = "Giá ưu đãi đặc biệt sinh viên và gia đình trẻ; Miễn phí công lắp đặt; Bảo hành máy nén 12 năm",
                    WarrantyMonths = 36,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 195
                },

                // ================= DANH MỤC 5: ĐIỆN THOẠI =================
                new Product
                {
                    Name = "Điện thoại iPhone 18 Pro Max 256GB Chính hãng VN/A",
                    Code = "DMX-DT01",
                    Slug = "iphone-18-pro-max-256gb",
                    CategoryId = CatId("dien-thoai"),
                    BrandId = BrandId("apple"),
                    OriginalPrice = 36990000,
                    Price = 33990000,
                    StockQuantity = 20,
                    SoldCount = 185,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/370982/iphone-18-pro-max-do-thumb.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/Products/Images/42/342679/iphone-17-pro-max-cam-thumb.jpg",
                    ShortDescription = "Chip Apple A20 Pro tiến trình 2nm siêu mạnh, Màn hình Super Retina XDR 6.9 inch ProMotion 120Hz, Camera 48MP Zoom quang 5x",
                    Description = "iPhone 18 Pro Max 256GB chính hãng VN/A mang lại hiệu năng đột phá với vi xử lý Apple A20 Pro kết hợp Apple Intelligence thế hệ mới, khung viền Titan siêu bền cùng hệ thống 3 camera 48MP quay video 4K 120fps Dolby Vision chuyên nghiệp.",
                    Specifications = "Màn hình: OLED 6.9 inch, Super Retina XDR, 120Hz\nChip xử lý: Apple A20 Pro 6 nhân\nRAM: 12 GB\nDung lượng lưu trữ: 256 GB\nCamera sau: Chính 48 MP & Phụ 48 MP, 48 MP (Zoom quang 5x)\nCamera trước: 24 MP TrueDepth\nPin & Sạc: Sạc nhanh 45W, MagSafe 25W\nHệ điều hành: iOS",
                    PromotionText = "Thu cũ đổi mới trợ giá lên đến 3.000.000đ; Giảm 500.000đ khi thanh toán qua QR Ngân hàng; Trả góp 0% lãi suất",
                    WarrantyMonths = 12,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 5.0,
                    RatingCount = 162
                },
                new Product
                {
                    Name = "Điện thoại iPhone 17 Pro Max 256GB Chính hãng VN/A",
                    Code = "DMX-DT02",
                    Slug = "iphone-17-pro-max-256gb",
                    CategoryId = CatId("dien-thoai"),
                    BrandId = BrandId("apple"),
                    OriginalPrice = 32990000,
                    Price = 28490000,
                    StockQuantity = 18,
                    SoldCount = 240,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/342679/iphone-17-pro-max-cam-thumb.jpg",
                    ShortDescription = "Khung Titan sang trọng, Màn hình 6.9 inch 120Hz, Thời lượng pin xem video liên tục 33 giờ",
                    Description = "iPhone 17 Pro Max 256GB sở hữu thiết kế viền màn hình siêu mỏng, hiệu năng ổn định vượt trội cùng hệ thống camera Pro chụp đêm sắc nét và quay phim chống rung Action Mode.",
                    Specifications = "Màn hình: Super Retina XDR OLED 6.9 inch 120Hz\nChip xử lý: Apple A19 Pro\nDung lượng: 256 GB\nCamera sau: 3 camera 48 MP\nKết nối: 5G, Wi-Fi 7, Bluetooth 5.4, USB Type-C\nKháng nước, bụi: Chuẩn IP68",
                    PromotionText = "Tặng phiếu mua phụ kiện Apple trị giá 500.000đ; Bảo hành chính hãng Apple Việt Nam 12 tháng; Trả góp 0%",
                    WarrantyMonths = 12,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 198
                },
                new Product
                {
                    Name = "Điện thoại OPPO Reno16F 5G 8GB/128GB",
                    Code = "DMX-DT03",
                    Slug = "oppo-reno16f-5g-8gb-128gb",
                    CategoryId = CatId("dien-thoai"),
                    BrandId = BrandId("oppo"),
                    OriginalPrice = 9990000,
                    Price = 8490000,
                    StockQuantity = 35,
                    SoldCount = 145,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/Products/Images/42/367596/oppo-a6c-nau-thumb-1.jpg",
                    ShortDescription = "Chuyên gia chân dung AI 50MP, Màn hình AMOLED 120Hz viền mỏng, Sạc siêu nhanh SuperVOOC 45W",
                    Description = "OPPO Reno16F 5G nổi bật với thiết kế mỏng nhẹ thời trang, bộ công cụ chỉnh sửa ảnh AI thông minh (Xóa vật thể AI, Làm nét khuôn mặt AI) cùng pin 5000 mAh sạc nhanh SuperVOOC an toàn.",
                    Specifications = "Màn hình: AMOLED 6.7 inch Full HD+ 120Hz\nRAM: 8 GB (Mở rộng thêm 8 GB RAM ảo)\nBộ nhớ trong: 128 GB\nCamera sau: Chính 50 MP + Góc rộng 8 MP + Macro 2 MP\nCamera trước: 32 MP\nPin & Sạc: 5000 mAh, SuperVOOC 45W\nHỗ trợ mạng: 5G",
                    PromotionText = "Tặng ốp lưng chính hãng trong hộp; Bảo hành 12 tháng tại trung tâm OPPO toàn quốc; Trả góp 0% trả trước 0đ",
                    WarrantyMonths = 12,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 76
                },
                new Product
                {
                    Name = "Điện thoại Xiaomi Redmi Note 17 4G 4GB/128GB",
                    Code = "DMX-DT04",
                    Slug = "xiaomi-redmi-note-17-4g-4gb-128gb",
                    CategoryId = CatId("dien-thoai"),
                    BrandId = BrandId("xiaomi"),
                    OriginalPrice = 5490000,
                    Price = 4590000,
                    StockQuantity = 45,
                    SoldCount = 230,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/369617/xiaomi-redmi-note-17-4g-black-thumb.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/Products/Images/42/369630/xiaomi-redmi-17-4g-black-thumb.jpg",
                    ShortDescription = "Camera chính 108MP siêu sắc nét, Màn hình AMOLED 120Hz 1800 nits, Pin lớn 5500 mAh sạc nhanh 33W",
                    Description = "Xiaomi Redmi Note 17 4GB/128GB là lựa chọn hàng đầu trong phân khúc tầm trung với camera 108MP chi tiết cao, màn hình AMOLED 120Hz mượt mà và kính cường lực Gorilla Glass bền bỉ.",
                    Specifications = "Màn hình: AMOLED 6.67 inch Full HD+ 120Hz\nRAM: 4 GB\nBộ nhớ trong: 128 GB\nCamera sau: 108 MP + 8 MP + 2 MP\nPin: 5500 mAh, Sạc nhanh Turbo 33W\nBảo mật: Vân tay dưới màn hình",
                    PromotionText = "Bảo hành chính hãng 18 tháng đặc quyền; Đổi mới trong 30 ngày nếu lỗi phần cứng; Hỗ trợ trả góp 0%",
                    WarrantyMonths = 18,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 114
                },

                // ================= DANH MỤC 6: LAPTOP =================
                new Product
                {
                    Name = "Laptop Asus Vivobook S14 S3407VA Core 5 210H (LY146W)",
                    Code = "DMX-LT01",
                    Slug = "asus-vivobook-s14-s3407va-core-5-210h",
                    CategoryId = CatId("laptop"),
                    BrandId = BrandId("asus"),
                    OriginalPrice = 20990000,
                    Price = 17990000,
                    StockQuantity = 16,
                    SoldCount = 94,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003498-laptop-tron-20261104-600x600.png",
                    ShortDescription = "Vi xử lý Intel Core 5 210H hiệu năng cao, RAM 16GB DDR5, SSD 512GB NVMe, Vỏ kim loại mỏng nhẹ 1.39 kg",
                    Description = "Laptop Asus Vivobook S14 S3407VA mang đến hiệu năng mạnh mẽ cho sinh viên CNTT và nhân viên văn phòng nhờ chip Intel Core 5 dòng H, RAM 16GB đa nhiệm mượt mà và bản lề mở 180 độ linh hoạt.",
                    Specifications = "CPU: Intel Core 5 210H (8 nhân, 12 luồng, tối đa 4.8 GHz)\nRAM: 16 GB DDR5 5200MHz\nỔ cứng: 512 GB SSD NVMe PCIe Gen 4\nMàn hình: 14 inch WUXGA (1920 x 1200) IPS chống chói\nCard đồ họa: Intel Graphics tích hợp\nTrọng lượng: 1.39 kg\nHệ điều hành: Windows 11 Home SL bản quyền",
                    PromotionText = "Tặng Balo Laptop chống sốc cao cấp; Tặng chuột không dây chính hãng; Giảm thêm 300.000đ cho Học sinh - Sinh viên",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 67
                },
                new Product
                {
                    Name = "Laptop Dell 15 DC15250 i5 1334U 16GB/512GB (DC5I5897W1)",
                    Code = "DMX-LT02",
                    Slug = "dell-15-dc15250-i5-1334u",
                    CategoryId = CatId("laptop"),
                    BrandId = BrandId("dell"),
                    OriginalPrice = 18490000,
                    Price = 15890000,
                    StockQuantity = 22,
                    SoldCount = 128,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003649-laptop-tron-20261104-600x600.png",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003992-laptop-tron-20261104-600x600.png",
                    ShortDescription = "Intel Core i5 1334U, RAM 16GB, SSD 512GB, Màn hình 15.6 inch Full HD 120Hz mượt mà, Bền bỉ chuẩn Dell",
                    Description = "Laptop Dell 15 DC15250 i5 1334U sở hữu màn hình lớn 15.6 inch tần số quét 120Hz giảm mỏi mắt, bàn phím Fullsize có cụm phím số tiện lợi cho kế toán, lập trình và học tập.",
                    Specifications = "CPU: Intel Core i5 1334U (10 nhân, 12 luồng, tới 4.6 GHz)\nRAM: 16 GB DDR4\nỔ cứng: 512 GB SSD NVMe PCIe\nMàn hình: 15.6 inch Full HD (1920 x 1080) 120Hz WVA\nCổng kết nối: USB Type-C, 2x USB 3.2, HDMI, Khe thẻ SD\nHệ điều hành: Windows 11 Home SL + Office Home",
                    PromotionText = "Tặng Balo Laptop + Chuột không dây; Bảo hành tận nơi ProSupport Dell Việt Nam; Hỗ trợ trả góp 0%",
                    WarrantyMonths = 12,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 89
                },
                new Product
                {
                    Name = "Laptop HP 240R G10 Core 5 120U 16GB/512GB (C3RU7AT)",
                    Code = "DMX-LT03",
                    Slug = "hp-240r-g10-core-5-120u",
                    CategoryId = CatId("laptop"),
                    BrandId = BrandId("hp"),
                    OriginalPrice = 17990000,
                    Price = 15290000,
                    StockQuantity = 18,
                    SoldCount = 79,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003457-laptop-tron-20261104-600x600.png",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003571-laptop-tron-20261104-600x600.png",
                    ShortDescription = "Chip Intel Core 5 120U thế hệ mới, Thiết kế gọn nhẹ 1.36 kg màu bạc thanh lịch, Webcam FHD khử ồn AI",
                    Description = "Laptop HP 240R G10 Core 5 120U là dòng laptop doanh nghiệp bền bỉ của HP với thiết kế thanh lịch, bàn phím gõ êm và thời lượng pin dài cho cả ngày làm việc di động.",
                    Specifications = "CPU: Intel Core 5 120U (Tối đa 5.0 GHz)\nRAM: 16 GB DDR4 3200MHz\nỔ cứng: 512 GB SSD NVMe\nMàn hình: 14 inch Full HD IPS chống chói\nTrọng lượng: 1.36 kg\nHệ điều hành: Windows 11 bản quyền",
                    PromotionText = "Tặng Balo Laptop HP chính hãng; Cài đặt phần mềm văn phòng miễn phí trọn đời; Trả góp 0% lãi suất",
                    WarrantyMonths = 12,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 53
                },
                new Product
                {
                    Name = "Laptop Lenovo IdeaPad Slim 3 15IPH11 Ultra 5 322 (83UR00A4VN)",
                    Code = "DMX-LT04",
                    Slug = "lenovo-ideapad-slim-3-15iph11-ultra-5",
                    CategoryId = CatId("laptop"),
                    BrandId = BrandId("lenovo"),
                    OriginalPrice = 19490000,
                    Price = 16490000,
                    StockQuantity = 21,
                    SoldCount = 103,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/0220042003922-laptop-tron-20261104-600x600.png",
                    ShortDescription = "Vi xử lý Intel Core Ultra 5 tích hợp NPU AI, Độ bền chuẩn quân đội MIL-STD-810H, Sạc nhanh Rapid Charge",
                    Description = "Lenovo IdeaPad Slim 3 15IPH11 trang bị vi xử lý Intel Core Ultra 5 tích hợp nhân xử lý AI chuyên dụng, đạt chứng nhận độ bền quân đội Mỹ MIL-STD-810H chống va đập và rung lắc tốt.",
                    Specifications = "CPU: Intel Core Ultra 5 322 tích hợp NPU AI\nRAM: 16 GB LPDDR5x\nỔ cứng: 512 GB SSD PCIe 4.0\nMàn hình: 15.3 inch WUXGA IPS 300 nits\nTính năng đặc biệt: Công tắc khóa camera vật lý, Sạc nhanh Rapid Charge\nBảo hành: 24 tháng Lenovo Premium Care",
                    PromotionText = "Bảo hành VIP 24 tháng tận nơi; Tặng Balo Laptop + Chuột không dây; Giảm thêm cho sinh viên",
                    WarrantyMonths = 24,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 71
                },

                // ================= DANH MỤC 7: GIA DỤNG NHÀ BẾP =================
                new Product
                {
                    Name = "Nồi chiên không dầu Sunhouse 6.5 lít SHD4030",
                    Code = "DMX-GD01",
                    Slug = "noi-chien-khong-dau-sunhouse-6-5-lit-shd4030",
                    CategoryId = CatId("gia-dung"),
                    BrandId = BrandId("aqua"),
                    OriginalPrice = 2490000,
                    Price = 1490000,
                    StockQuantity = 50,
                    SoldCount = 312,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/4844099000276-328000-clean-20261102-600x600.png",
                    ShortDescription = "Dung tích lớn 6.5 lít chiên gà nguyên con, Công nghệ Rapid Air giảm 80% dầu mỡ, Cửa sổ kính quan sát thực phẩm",
                    Description = "Nồi chiên không dầu Sunhouse 6.5 lít SHD4030 có công suất 1750W làm chín thực phẩm nhanh chóng, lòng nồi phủ chống dính Whitford (Mỹ) an toàn tuyệt đối cho sức khỏe.",
                    Specifications = "Dung tích sử dụng: 6.5 lít\nCông suất: 1750W\nCông nghệ làm nóng: Rapid Air luân chuyển khí nóng 360 độ\nNhiệt độ điều chỉnh: 80 - 200 độ C\nHẹn giờ: 0 - 60 phút\nChất liệu lòng nồi: Hợp kim nhôm phủ chống dính Whitford",
                    PromotionText = "Giảm trực tiếp 1.000.000đ; Đổi mới trong 30 ngày đầu nếu có lỗi kỹ thuật; Giao hàng nhanh trong 2 giờ",
                    WarrantyMonths = 12,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = false,
                    RatingAverage = 4.9,
                    RatingCount = 184
                },
                new Product
                {
                    Name = "Nồi chiên không dầu AVA 9 lít Cửa kính cảm ứng ZG93MW",
                    Code = "DMX-GD02",
                    Slug = "noi-chien-khong-dau-ava-9-lit-zg93mw",
                    CategoryId = CatId("gia-dung"),
                    BrandId = BrandId("toshiba"),
                    OriginalPrice = 2890000,
                    Price = 1790000,
                    StockQuantity = 38,
                    SoldCount = 195,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/4844142000250-338763-clean-20261102-600x600.png",
                    ShortDescription = "Dung tích đại 9 lít, Bảng điều khiển cảm ứng 8 menu cài đặt sẵn, Lòng nồi chống dính dễ vệ sinh",
                    Description = "Nồi chiên không dầu AVA 9 lít ZG93MW thiết kế hiện đại với màn hình cảm ứng nhạy bén cùng cửa kính trong suốt và đèn chiếu sáng bên trong giúp theo dõi thức ăn chín mà không cần kéo giỏ chiên.",
                    Specifications = "Dung tích: 9 lít\nCông suất: 1800W\nBảng điều khiển: Cảm ứng có màn hình hiển thị LED\nChương trình nấu: 8 chế độ cài đặt sẵn\nTiện ích: Có cửa sổ kính và đèn trong khoang nồi, Tự ngắt khi quá nhiệt",
                    PromotionText = "Giao hàng tận nơi toàn quốc; Bảo hành chính hãng 12 tháng tại tất cả siêu thị",
                    WarrantyMonths = 12,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = false,
                    RatingAverage = 4.8,
                    RatingCount = 96
                },
                new Product
                {
                    Name = "Nồi chiên không dầu BlueStone 7 lít AFB-5856",
                    Code = "DMX-GD03",
                    Slug = "noi-chien-khong-dau-bluestone-7-lit-afb-5856",
                    CategoryId = CatId("gia-dung"),
                    BrandId = BrandId("panasonic"),
                    OriginalPrice = 3290000,
                    Price = 2190000,
                    StockQuantity = 28,
                    SoldCount = 142,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/4844099000284-333957-clean-20261102-600x600.png",
                    ShortDescription = "Dung tích 7 lít, Lòng nồi phủ chống dính Greblon cao cấp của Đức, Bảo hành chính hãng 24 tháng",
                    Description = "Nồi chiên không dầu BlueStone 7 lít AFB-5856 nổi bật với lớp phủ chống dính Greblon chuẩn Đức siêu bền, chịu nhiệt cao và hoàn toàn không giải phóng chất độc hại khi nướng ở nhiệt độ 200 độ C.",
                    Specifications = "Dung tích: 7 lít\nCông suất: 1800W\nChất liệu lòng nồi: Hợp kim nhôm phủ chống dính Greblon (Đức)\nĐiều khiển: Núm xoay cơ bền bỉ hoặc Cảm ứng\nBảo hành: 24 tháng chính hãng BlueStone",
                    PromotionText = "Bảo hành chính hãng lên tới 24 tháng; Đổi trả 30 ngày nếu lỗi nhà sản xuất",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = false,
                    RatingAverage = 4.9,
                    RatingCount = 82
                },
                new Product
                {
                    Name = "Máy rửa chén độc lập Bosch Wifi SMS2HTI79E (12 bộ Châu Âu)",
                    Code = "DMX-GD04",
                    Slug = "may-rua-chen-doc-lap-bosch-sms2hti79e",
                    CategoryId = CatId("gia-dung"),
                    BrandId = BrandId("hitachi"),
                    OriginalPrice = 19900000,
                    Price = 15490000,
                    StockQuantity = 10,
                    SoldCount = 54,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/5475/318118/may-rua-chen-doc-lap-bosch-sms2hti79e-310324-104044-600x600.jpg",
                    ShortDescription = "Sức chứa 12 bộ chén đĩa Châu Âu (3-4 bữa ăn Việt), Rửa nước nóng diệt khuẩn, Điều khiển từ xa Home Connect",
                    Description = "Máy rửa chén độc lập Bosch SMS2HTI79E nhập khẩu châu Âu trang bị động cơ không chổi than EcoSilence Drive vận hành siêu êm 46 dB, sấy tăng cường Extra Dry giúp bát đĩa khô ráo và tiệt trùng.",
                    Specifications = "Loại máy: Máy rửa chén độc lập\nSức chứa: 12 bộ đồ ăn Châu Âu\nLượng nước tiêu thụ: Khoảng 9.5 lít/lần rửa\nĐộ ồn: 46 dB siêu êm\nCông nghệ sấy: Sấy tăng cường Extra Dry\nKết nối thông minh: Ứng dụng Home Connect qua Wi-Fi",
                    PromotionText = "Miễn phí công lắp đặt; Tặng bộ viên rửa chén Finish dùng thử; Hỗ trợ trả góp 0% lãi suất",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 45
                },

                // ================= DANH MỤC 8: MÁY LỌC NƯỚC =================
                new Product
                {
                    Name = "Máy lọc nước RO Karofi PLATINUM U12X 12 lõi",
                    Code = "DMX-MLN01",
                    Slug = "may-loc-nuoc-ro-karofi-platinum-u12x-12-loi",
                    CategoryId = CatId("may-loc-nuoc"),
                    BrandId = BrandId("aqua"),
                    OriginalPrice = 8990000,
                    Price = 6490000,
                    StockQuantity = 24,
                    SoldCount = 167,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/may-loc-nuoc-ro-karofi-platinum-u12x-12-loi-thumb000751.jpg",
                    ShortDescription = "Hệ thống 12 lõi lọc Smax thế hệ mới, Màng lọc RO 100 GPD sản xuất tại Mỹ, Bổ sung Hydrogen chống lão hóa",
                    Description = "Máy lọc nước RO Karofi PLATINUM U12X 12 lõi đạt chuẩn nước uống trực tiếp QCVN 6-1:2010/BYT của Bộ Y Tế, gấp đôi lượng Hydrogen hoạt hóa và khoáng chất tự nhiên tốt cho sức khỏe.",
                    Specifications = "Kiểu lắp đặt: Tủ đứng hoặc lắp âm\nSố lõi lọc: 12 lõi lọc Smax hiệu suất cao\nMàng lọc: RO 100 GPD Purifim Mỹ\nCông suất lọc: 20 lít/giờ\nDung tích bình chứa: 8.5 lít\nBảo hành: 36 tháng chính hãng Karofi",
                    PromotionText = "Miễn phí giao hàng và công lắp đặt; Tặng bộ lõi lọc thô 1-2-3 dự phòng; Bảo hành chính hãng 36 tháng",
                    WarrantyMonths = 36,
                    IsFlashSale = true,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 112
                },
                new Product
                {
                    Name = "Máy lọc nước RO nóng nguội lạnh Hydro-ion kiềm Karofi PLATINUM SA5",
                    Code = "DMX-MLN02",
                    Slug = "may-loc-nuoc-ro-nong-nguoi-lanh-karofi-platinum-sa5",
                    CategoryId = CatId("may-loc-nuoc"),
                    BrandId = BrandId("aqua"),
                    OriginalPrice = 18900000,
                    Price = 13990000,
                    StockQuantity = 12,
                    SoldCount = 78,
                    ImageUrl = "https://cdnv2.tgdd.vn/pim/cdn/images/202610/may-loc-nuoc-dien-giai-ion-kiem-nong-nguoi-lanh-karofi-platinum-sa5-5-tam-dien-cuc-3090855.webp",
                    ShortDescription = "Tích hợp 7 tấm điện cực Titan phủ Bạch kim tạo nước Hydro-ion kiềm, 3 chế độ nước Nóng - Nguội - Lạnh tiện lợi",
                    Description = "Máy lọc nước RO nóng nguội lạnh Hydro-ion kiềm Karofi PLATINUM SA5 cung cấp nguồn nước kiềm giàu Hydrogen giúp trung hòa axit dư thừa, đồng thời tích hợp làm nóng nhanh pha trà, pha sữa và làm lạnh sâu sảng khoái.",
                    Specifications = "Loại máy: Máy lọc nước RO kết hợp Điện phân Hydro-ion kiềm\nChế độ nước: Nóng (85-95 độ C) - Nguội - Lạnh (8-10 độ C) - Ion kiềm\nĐiện cực: 7 tấm điện cực Titan phủ Platinum\nCông nghệ làm lạnh: Block làm lạnh sâu\nBảo hành: 36 tháng chính hãng",
                    PromotionText = "Giảm ngay 4.910.000đ; Miễn phí lắp đặt tận nhà; Hỗ trợ trả góp 0% lãi suất",
                    WarrantyMonths = 36,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.9,
                    RatingCount = 64
                },
                new Product
                {
                    Name = "Máy lọc nước RO Sunhouse UltraPURE SHA76623KL 11 lõi",
                    Code = "DMX-MLN03",
                    Slug = "may-loc-nuoc-ro-sunhouse-ultrapure-sha76623kl",
                    CategoryId = CatId("may-loc-nuoc"),
                    BrandId = BrandId("toshiba"),
                    OriginalPrice = 7990000,
                    Price = 5490000,
                    StockQuantity = 30,
                    SoldCount = 139,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/326047.jpg",
                    ShortDescription = "Công nghệ lọc UltraPURE chống tái nhiễm khuẩn, 11 lõi lọc tạo nước kiềm Hydrogen, Mặt kính tràn viền sang trọng",
                    Description = "Máy lọc nước RO Sunhouse UltraPURE SHA76623KL sở hữu thiết kế tủ kính cường lực tràn viền hiện đại, hệ thống 11 cấp lọc loại bỏ 99.99% vi khuẩn và kim loại nặng.",
                    Specifications = "Kiểu tủ: Tủ đứng mặt kính tràn viền\nSố lõi lọc: 11 lõi lọc UltraPURE\nCông suất lọc: 15 - 20 lít/giờ\nTiện ích: Tự động sục rửa màng RO, Ngừng hoạt động khi áp lực nước thấp\nBảo hành: 24 tháng",
                    PromotionText = "Miễn phí vận chuyển và lắp đặt; Tặng phiếu giảm giá 20% khi thay lõi lọc định kỳ",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 73
                },
                new Product
                {
                    Name = "Máy lọc nước RO nóng nguội lạnh Hydrogen Sanaky SNK-S3HK85 10 lõi",
                    Code = "DMX-MLN04",
                    Slug = "may-loc-nuoc-ro-nong-nguoi-lanh-sanaky-snk-s3hk85",
                    CategoryId = CatId("may-loc-nuoc"),
                    BrandId = BrandId("panasonic"),
                    OriginalPrice = 8490000,
                    Price = 5990000,
                    StockQuantity = 22,
                    SoldCount = 118,
                    ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/362948.jpg",
                    ExtraImages = "https://cdn.tgdd.vn/2026/11/timerseo/325006.jpg",
                    ShortDescription = "2 vòi 3 chế độ nước Nóng - Nguội - Lạnh, 10 lõi lọc bổ sung khoáng và Hydrogen, Làm lạnh sâu tiết kiệm điện",
                    Description = "Máy lọc nước RO nóng nguội lạnh Hydrogen Sanaky SNK-S3HK85 đáp ứng trọn vẹn nhu cầu nước nóng pha cà phê, nước lạnh giải nhiệt mùa hè và nước nguội giàu khoáng chất cho cả gia đình.",
                    Specifications = "Số vòi: 2 vòi - 3 chế độ (Nóng, Nguội, Lạnh)\nSố lõi lọc: 10 lõi lọc RO Hydrogen\nCông suất lọc: 15 lít/giờ\nNhiệt độ nóng: 85 - 95 độ C\nNhiệt độ lạnh: 10 - 15 độ C\nBảo hành: 24 tháng chính hãng",
                    PromotionText = "Miễn phí công lắp đặt; Đổi trả trong 30 ngày nếu lỗi kỹ thuật; Hỗ trợ trả góp 0%",
                    WarrantyMonths = 24,
                    IsFlashSale = false,
                    IsFeatured = true,
                    IsInstallment0Percent = true,
                    RatingAverage = 4.8,
                    RatingCount = 59
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();

            // 4. Seed Users (Tài khoản Quản trị viên & Sinh viên thực hiện đề tài)
            var users = new List<User>
            {
                new User
                {
                    FullName = "Nguyễn Trường Giang",
                    Username = "admin",
                    Email = "truonggiang.cntt@vui.edu.vn",
                    Phone = "0988123456",
                    PasswordHash = "123456",
                    Address = "Khoa CNTT - Trường Đại học Công nghiệp Việt Trì, Phú Thọ",
                    Role = "Admin",
                    CreatedAt = DateTime.Now.AddDays(-60),
                    IsActive = true
                },
                new User
                {
                    FullName = "Nguyễn Trường Giang",
                    Username = "giangnt",
                    Email = "giang22a4802010012@vui.edu.vn",
                    Phone = "0977654321",
                    PasswordHash = "123456",
                    Address = "TP. Việt Trì, Tỉnh Phú Thọ",
                    Role = "Customer",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    IsActive = true
                },
                new User
                {
                    FullName = "Trần Minh Tuấn",
                    Username = "minhtuan",
                    Email = "minhtuan.pt@gmail.com",
                    Phone = "0912345678",
                    PasswordHash = "123456",
                    Address = "Phường Tiên Cát, TP. Việt Trì, Phú Thọ",
                    Role = "Customer",
                    CreatedAt = DateTime.Now.AddDays(-15),
                    IsActive = true
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // 5. Seed Sample Orders
            var p1 = products[0];
            var p5 = products[4];
            var p9 = products[8];
            var p17 = products[16];

            var sampleOrders = new List<Order>
            {
                new Order
                {
                    OrderCode = "DMX2026110101",
                    UserId = users[1].Id,
                    CustomerName = "Nguyễn Trường Giang",
                    CustomerPhone = "0977654321",
                    CustomerEmail = "giang22a4802010012@vui.edu.vn",
                    ShippingAddress = "Số 9 Đường Tiên Sơn, Phường Tiên Cát, TP. Việt Trì, Phú Thọ",
                    Note = "Giao giờ hành chính, hỗ trợ lắp đặt treo tường",
                    SubTotal = p1.Price + p9.Price,
                    DiscountAmount = 500000,
                    CouponCode = "DMX500K",
                    VatAmount = Math.Round((p1.Price + p9.Price - 500000) * 0.08m, 0),
                    TotalAmount = (p1.Price + p9.Price - 500000) + Math.Round((p1.Price + p9.Price - 500000) * 0.08m, 0),
                    PaymentMethod = "COD",
                    PaymentStatus = "Đã thanh toán",
                    OrderStatus = "Hoàn thành",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail { ProductId = p1.Id, ProductName = p1.Name, ProductImage = p1.ImageUrl, UnitPrice = p1.Price, Quantity = 1 },
                        new OrderDetail { ProductId = p9.Id, ProductName = p9.Name, ProductImage = p9.ImageUrl, UnitPrice = p9.Price, Quantity = 1 }
                    }
                },
                new Order
                {
                    OrderCode = "DMX2026110402",
                    UserId = users[2].Id,
                    CustomerName = "Trần Minh Tuấn",
                    CustomerPhone = "0912345678",
                    CustomerEmail = "minhtuan.pt@gmail.com",
                    ShippingAddress = "Đại lộ Hùng Vương, Phường Gia Cẩm, TP. Việt Trì, Phú Thọ",
                    Note = "Gọi điện trước khi giao 30 phút",
                    SubTotal = p5.Price,
                    DiscountAmount = 0,
                    VatAmount = Math.Round(p5.Price * 0.08m, 0),
                    TotalAmount = p5.Price + Math.Round(p5.Price * 0.08m, 0),
                    PaymentMethod = "Banking",
                    PaymentStatus = "Đã thanh toán",
                    OrderStatus = "Đang giao hàng",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail { ProductId = p5.Id, ProductName = p5.Name, ProductImage = p5.ImageUrl, UnitPrice = p5.Price, Quantity = 1 }
                    }
                },
                new Order
                {
                    OrderCode = "DMX2026110603",
                    UserId = users[1].Id,
                    CustomerName = "Lê Hoàng Nam",
                    CustomerPhone = "0965888999",
                    CustomerEmail = "hoangnam@gmail.com",
                    ShippingAddress = "Phường Thanh Miếu, TP. Việt Trì, Phú Thọ",
                    Note = "Đăng ký trả góp 0% qua thẻ tín dụng",
                    SubTotal = p17.Price,
                    DiscountAmount = 200000,
                    CouponCode = "DIENMAYXANH",
                    VatAmount = Math.Round((p17.Price - 200000) * 0.08m, 0),
                    TotalAmount = (p17.Price - 200000) + Math.Round((p17.Price - 200000) * 0.08m, 0),
                    PaymentMethod = "Installment",
                    PaymentStatus = "Chờ duyệt hồ sơ trả góp",
                    OrderStatus = "Chờ xác nhận",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail { ProductId = p17.Id, ProductName = p17.Name, ProductImage = p17.ImageUrl, UnitPrice = p17.Price, Quantity = 1 }
                    }
                }
            };
            context.Orders.AddRange(sampleOrders);
            context.SaveChanges();

            // 6. Seed Sample Product Reviews
            var reviews = new List<ProductReview>
            {
                new ProductReview
                {
                    ProductId = p1.Id,
                    CustomerName = "Nguyễn Trường Giang",
                    CustomerPhone = "0977***321",
                    Rating = 5,
                    Comment = "Tivi LG 65QNED70BSA màu sắc rất đẹp, âm thanh to rõ, nhân viên Điện Máy Xanh giao hàng và lắp đặt tại Việt Trì rất nhiệt tình.",
                    CreatedAt = DateTime.Now.AddDays(-4),
                    IsApproved = true
                },
                new ProductReview
                {
                    ProductId = p1.Id,
                    CustomerName = "Phạm Văn Hùng",
                    CustomerPhone = "0982***115",
                    Rating = 5,
                    Comment = "Hình ảnh 4K sắc nét, điều khiển giọng nói tiếng Việt nhạy, mức giá khuyến mãi rất hợp lý.",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    IsApproved = true
                },
                new ProductReview
                {
                    ProductId = p5.Id,
                    CustomerName = "Trần Minh Tuấn",
                    CustomerPhone = "0912***678",
                    Rating = 5,
                    Comment = "Tủ lạnh Aqua 646 lít cực kỳ rộng rãi, mặt gương sang trọng, chạy êm và làm đá nhanh.",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsApproved = true
                }
            };
            context.ProductReviews.AddRange(reviews);
            context.SaveChanges();

            SeedInitialStockImports(context);
            SeedInitialCoupons(context);
            SeedInitialBanners(context);
            SeedProductVariantsAndVat(context);
        }

        private static void SeedInitialCoupons(ApplicationDbContext context)
        {
            if (context.Coupons.Any())
            {
                return;
            }

            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    Code = "DMX100K",
                    Description = "Voucher ưu đãi giảm trực tiếp 100.000đ cho đơn hàng từ 1.000.000đ",
                    DiscountType = "FixedAmount",
                    DiscountValue = 100000,
                    MinOrderAmount = 1000000,
                    UsageLimit = 1000,
                    UsedCount = 42,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddMonths(6),
                    IsActive = true
                },
                new Coupon
                {
                    Code = "DMX500K",
                    Description = "Voucher siêu hội tri ân giảm 500.000đ cho đơn hàng từ 5.000.000đ",
                    DiscountType = "FixedAmount",
                    DiscountValue = 500000,
                    MinOrderAmount = 5000000,
                    UsageLimit = 500,
                    UsedCount = 89,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddMonths(6),
                    IsActive = true
                },
                new Coupon
                {
                    Code = "GIAM1TRIEU",
                    Description = "Voucher VIP giảm ngay 1.000.000đ khi mua Tivi, Tủ lạnh từ 15.000.000đ",
                    DiscountType = "FixedAmount",
                    DiscountValue = 1000000,
                    MinOrderAmount = 15000000,
                    UsageLimit = 200,
                    UsedCount = 28,
                    StartDate = DateTime.Now.AddDays(-15),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true
                },
                new Coupon
                {
                    Code = "TRIAN5PT",
                    Description = "Ưu đãi tri ân thành viên giảm 5% giá trị đơn hàng (tối đa 1.500.000đ)",
                    DiscountType = "Percentage",
                    DiscountValue = 5,
                    MinOrderAmount = 3000000,
                    MaxDiscountAmount = 1500000,
                    UsageLimit = 500,
                    UsedCount = 65,
                    StartDate = DateTime.Now.AddDays(-20),
                    EndDate = DateTime.Now.AddMonths(4),
                    IsActive = true
                },
                new Coupon
                {
                    Code = "DIENMAYXANH",
                    Description = "Mã chào mừng khách hàng thân thiết giảm 200.000đ cho đơn từ 2.000.000đ",
                    DiscountType = "FixedAmount",
                    DiscountValue = 200000,
                    MinOrderAmount = 2000000,
                    UsageLimit = 2000,
                    UsedCount = 134,
                    StartDate = DateTime.Now.AddDays(-40),
                    EndDate = DateTime.Now.AddMonths(12),
                    IsActive = true
                }
            };

            context.Coupons.AddRange(coupons);
            context.SaveChanges();
        }

        private static void SeedInitialStockImports(ApplicationDbContext context)
        {
            if (context.StockImports.Any() || !context.Products.Any())
            {
                return;
            }

            var initialProducts = context.Products
                .Include(p => p.Brand)
                .OrderBy(p => p.Id)
                .Take(8)
                .ToList();

            var imports = new List<StockImport>();
            int idx = 1;
            foreach (var prod in initialProducts)
            {
                var totalImported = prod.StockQuantity + prod.SoldCount;
                imports.Add(new StockImport
                {
                    ReceiptCode = $"PNK20261101{idx:D2}",
                    ProductId = prod.Id,
                    Quantity = totalImported,
                    ImportPrice = Math.Round(prod.Price * 0.82m, 0),
                    StockBefore = 0,
                    StockAfter = totalImported,
                    SupplierName = $"Nhà phân phối chính hãng {prod.Brand?.Name ?? "Điện Máy"}",
                    Note = "Nhập kho lô hàng đầu kỳ theo hợp đồng phân phối chính hãng",
                    CreatedBy = "Quản trị viên hệ thống",
                    CreatedAt = DateTime.Now.AddDays(-10 + idx)
                });
                idx++;
            }

            context.StockImports.AddRange(imports);
            context.SaveChanges();
        }

        private static void SeedInitialBanners(ApplicationDbContext context)
        {
            if (context.Banners.Any())
            {
                return;
            }

            var banners = new List<Banner>
            {
                new Banner
                {
                    Title = "Đại Tiệc Siêu Sale Điện Máy Xanh - Giảm Đến 50%",
                    Subtitle = "Trả góp 0% lãi suất - Miễn phí công lắp đặt toàn quốc",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/75/19/7519c4a5b9aa9c47d3f5d17225b17314.jpg",
                    LinkUrl = "/Product",
                    Position = "Hero",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Lễ Hội Tivi - Tủ Lạnh - Máy Giặt Chính Hãng",
                    Subtitle = "Tặng thêm Voucher 1.000.000đ - Bảo hành tận nhà 24 tháng",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/e3/96/e3965d650c8be5f0ac30293be625496c.jpg",
                    LinkUrl = "/Product",
                    Position = "Hero",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Khuyến mãi Smart Tivi OLED/QLED 4K",
                    Subtitle = "Giảm sâu đến 35% kèm quà tặng loa thanh",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/5c/e5/5ce5c8d856c99ef01528033c294d6389.png",
                    LinkUrl = "/Product?categoryId=1",
                    Position = "SubBanner",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Tủ lạnh Side by Side - Tiết kiệm điện Inverter",
                    Subtitle = "Bảo hành máy nén 10 năm chính hãng",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/a6/97/a697dea35d2b2d853ade4d2ccb4d5382.png",
                    LinkUrl = "/Product?categoryId=2",
                    Position = "SubBanner",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Máy giặt lồng ngang AI Inverter",
                    Subtitle = "Giặt hơi nước diệt khuẩn 99.9%",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/d1/ef/d1efb16ffc7b86cc908e6abae633ad88.png",
                    LinkUrl = "/Product?categoryId=3",
                    Position = "SubBanner",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new Banner
                {
                    Title = "Máy lạnh Inverter - Bao trọn gói lắp đặt",
                    Subtitle = "Làm lạnh nhanh Turbo - Lọc bụi mịn PM2.5",
                    ImageUrl = "https://cdnv2.tgdd.vn/mwg-static/common/Banner/f1/8c/f18c8e87c390c9ee07b16da74bc253fb.png",
                    LinkUrl = "/Product?categoryId=4",
                    Position = "SubBanner",
                    DisplayOrder = 4,
                    IsActive = true
                }
            };

            context.Banners.AddRange(banners);
            context.SaveChanges();
        }

        private static void SeedProductVariantsAndVat(ApplicationDbContext context)
        {
            var productsToUpdateVariants = context.Products
                .Include(p => p.Category)
                .Where(p => string.IsNullOrEmpty(p.ProductVariants) || !p.ProductVariants.Contains("http"))
                .ToList();

            bool changed = false;
            foreach (var p in productsToUpdateVariants)
            {
                var catSlug = p.Category?.Slug ?? "";
                if (p.Code == "DMX-DT01")
                {
                    p.ProductVariants =
                        "256GB - Đỏ Ruby | 33990000 | 36990000 | https://cdn.tgdd.vn/Products/Images/42/370982/iphone-18-pro-max-do-thumb.jpg\n" +
                        "256GB - Titan Sa Mạc | 33990000 | 36990000 | https://cdn.tgdd.vn/Products/Images/42/342679/iphone-17-pro-max-cam-thumb.jpg\n" +
                        "512GB - Titan Đen | 39990000 | 42990000 | https://cdn.tgdd.vn/Products/Images/42/369617/xiaomi-redmi-note-17-4g-black-thumb.jpg\n" +
                        "1TB - Trắng Titan | 45990000 | 48990000 | https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg";
                    changed = true;
                }
                else if (p.Code == "DMX-DT02")
                {
                    p.ProductVariants =
                        "256GB - Titan Cam | 28490000 | 32990000 | https://cdn.tgdd.vn/Products/Images/42/342679/iphone-17-pro-max-cam-thumb.jpg\n" +
                        "256GB - Titan Đỏ | 28490000 | 32990000 | https://cdn.tgdd.vn/Products/Images/42/370982/iphone-18-pro-max-do-thumb.jpg\n" +
                        "512GB - Titan Trắng | 33990000 | 37990000 | https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg\n" +
                        "1TB - Titan Đen | 38990000 | 42990000 | https://cdn.tgdd.vn/Products/Images/42/369617/xiaomi-redmi-note-17-4g-black-thumb.jpg";
                    changed = true;
                }
                else if (p.Code == "DMX-DT03")
                {
                    p.ProductVariants =
                        "8GB/128GB - Trắng Ánh Sao | 8490000 | 9990000 | https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg\n" +
                        "8GB/256GB - Nâu Cà Phê | 9490000 | 10990000 | https://cdn.tgdd.vn/Products/Images/42/367596/oppo-a6c-nau-thumb-1.jpg\n" +
                        "12GB/256GB - Đen Thạch Anh | 10490000 | 11990000 | https://cdn.tgdd.vn/Products/Images/42/369617/xiaomi-redmi-note-17-4g-black-thumb.jpg";
                    changed = true;
                }
                else if (p.Code == "DMX-DT04")
                {
                    p.ProductVariants =
                        "4GB/128GB - Đen Bán Dạ | 4590000 | 5490000 | https://cdn.tgdd.vn/Products/Images/42/369617/xiaomi-redmi-note-17-4g-black-thumb.jpg\n" +
                        "6GB/128GB - Đen Nhám | 4990000 | 5990000 | https://cdn.tgdd.vn/Products/Images/42/369630/xiaomi-redmi-17-4g-black-thumb.jpg\n" +
                        "8GB/256GB - Trắng Băng Tuyết | 5690000 | 6690000 | https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg";
                    changed = true;
                }
                else if (catSlug == "dien-thoai")
                {
                    p.ProductVariants =
                        $"128GB - Đen Tiêu Chuẩn | {p.Price:0} | {p.OriginalPrice:0} | {p.ImageUrl}\n" +
                        $"256GB - Trắng Bạc Titan | {p.Price + 2500000:0} | {p.OriginalPrice + 2500000:0} | https://cdnv2.tgdd.vn/pim/cdn/images/202610/oppo-reno16f-trang-thumb170202.jpg\n" +
                        $"512GB - Vàng Sa Mạc | {p.Price + 5500000:0} | {p.OriginalPrice + 5500000:0} | https://cdn.tgdd.vn/Products/Images/42/342679/iphone-17-pro-max-cam-thumb.jpg";
                    changed = true;
                }
                else if (catSlug == "tu-lanh")
                {
                    var blackImg = p.ImageUrl;
                    var silverImg = "https://cdnv2.tgdd.vn/mwg-static/dmx/Products/Images/1943/363377/tu-lanh-funiki-inverter-415-lit-multi-door-hr-m8415ss-thumb-639086520716518196-600x600.jpg";
                    var grayImg = "https://cdn.tgdd.vn/2026/10/timerseo/327797-600x600-3.jpg";

                    if (p.Code == "DMX-TL03")
                    {
                        blackImg = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/342628105514.jpg";
                        silverImg = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/370337072738.png";
                        grayImg = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/360346105744.jpg";
                    }
                    else if (p.Code == "DMX-TL04")
                    {
                        blackImg = "https://cdn.tgdd.vn/2026/10/timerseo/359980-600x600-3.jpg";
                        silverImg = "https://cdn.tgdd.vn/2026/10/timerseo/327797-600x600-3.jpg";
                        grayImg = "https://cdnv2.tgdd.vn/pim/cdn/images/202611/359518110433.jpg";
                    }

                    p.ProductVariants =
                        $"Màu Đen Ánh Kim (Bản Tiêu Chuẩn) | {p.Price:0} | {p.OriginalPrice:0} | {blackImg}\n" +
                        $"Màu Bạc Inox (Thép Không Gỉ) | {p.Price + 900000:0} | {p.OriginalPrice + 1000000:0} | {silverImg}\n" +
                        $"Màu Xám Khói (Lấy Nước Ngoài) | {p.Price + 1800000:0} | {p.OriginalPrice + 2000000:0} | {grayImg}";
                    changed = true;
                }
                else if (catSlug == "laptop")
                {
                    p.ProductVariants =
                        $"RAM 8GB / SSD 512GB - Màu Bạc | {p.Price:0} | {p.OriginalPrice:0} | {p.ImageUrl}\n" +
                        $"RAM 16GB / SSD 512GB - Màu Xám Titan | {p.Price + 2000000:0} | {p.OriginalPrice + 2000000:0} | https://cdn.tgdd.vn/2026/11/timerseo/0220042003992-laptop-tron-20261104-600x600.png\n" +
                        $"RAM 16GB / SSD 1TB - Màu Đen Nhám | {p.Price + 4200000:0} | {p.OriginalPrice + 4200000:0} | https://cdn.tgdd.vn/2026/11/timerseo/0220042003571-laptop-tron-20261104-600x600.png";
                    changed = true;
                }
                else if (catSlug == "tivi")
                {
                    p.ProductVariants =
                        $"55 inch - 4K Tiêu Chuẩn | {p.Price:0} | {p.OriginalPrice:0} | {p.ImageUrl}\n" +
                        $"65 inch - 4K Màn Hình Lớn | {p.Price + 4000000:0} | {p.OriginalPrice + 4500000:0} | https://cdn.tgdd.vn/2026/11/timerseo/365298.jpg\n" +
                        $"75 inch - 4K Rạp Hát Tại Gia | {p.Price + 9500000:0} | {p.OriginalPrice + 10500000:0} | https://cdn.tgdd.vn/2026/11/timerseo/366151.jpg";
                    changed = true;
                }
                else if (catSlug == "may-lanh")
                {
                    p.ProductVariants =
                        $"1 HP (9.000 BTU) - Trắng Tinh Khôi | {p.Price:0} | {p.OriginalPrice:0} | {p.ImageUrl}\n" +
                        $"1.5 HP (12.000 BTU) - Trắng Ngà | {p.Price + 2200000:0} | {p.OriginalPrice + 2500000:0} | https://cdn.tgdd.vn/2026/11/timerseo/1751098000224-may-lanh-tron-20261105-600x600.png\n" +
                        $"2 HP (18.000 BTU) - Trắng Viền Bạc | {p.Price + 5800000:0} | {p.OriginalPrice + 6500000:0} | https://cdn.tgdd.vn/2026/11/timerseo/1751098000005-may-lanh-tron-20261105-600x600.png";
                    changed = true;
                }
                else if (catSlug == "may-giat")
                {
                    p.ProductVariants =
                        $"9.5 Kg - Màu Xám Đen | {p.Price:0} | {p.OriginalPrice:0} | {p.ImageUrl}\n" +
                        $"10.5 Kg - Màu Bạc Inox | {p.Price + 1500000:0} | {p.OriginalPrice + 1800000:0} | https://cdn.tgdd.vn/2026/11/timerseo/357870.jpg\n" +
                        $"12 Kg - Màu Trắng Tinh Khôi | {p.Price + 2800000:0} | {p.OriginalPrice + 3200000:0} | https://cdn.tgdd.vn/2026/11/timerseo/334633.jpg";
                    changed = true;
                }
            }

            var ordersWithoutVat = context.Orders.Where(o => o.VatAmount == 0 && o.SubTotal > 0).ToList();
            foreach (var ord in ordersWithoutVat)
            {
                var taxable = Math.Max(0, ord.SubTotal - ord.DiscountAmount);
                ord.VatAmount = Math.Round(taxable * 0.08m, 0);
                ord.TotalAmount = taxable + ord.VatAmount;
                if (ord.PaymentMethod == "Installment" && string.IsNullOrEmpty(ord.InstallmentInfo))
                {
                    ord.InstallmentInfo = "Trả góp 0% qua Thẻ tín dụng (Vietcombank) | Kỳ hạn: 6 tháng | Trả trước: 0%";
                }
                changed = true;
            }

            if (changed)
            {
                context.SaveChanges();
            }
        }
    }
}

