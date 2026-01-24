namespace SellerInventory.Client.BlazorWeb.Services;

public interface ILocalizationService
{
    string this[string key] { get; }
    Language CurrentLanguage { get; }
}

public class LocalizationService : ILocalizationService
{
    private readonly ILanguageService _languageService;

    private readonly Dictionary<Language, Dictionary<string, string>> _translations = new()
    {
        {
            Language.ENG, new Dictionary<string, string>
            {
                // Navigation & Pages
                { "Products", "Products" },
                { "Categories", "Categories" },
                { "Orders", "Orders" },
                { "Reports", "Reports" },
                { "Users", "Users" },
                { "POS", "Point of Sale" },
                { "Logout", "Logout" },

                // Products Page
                { "ProductList", "Product List" },
                { "AddProduct", "Add Product" },
                { "ImportExcel", "Import Excel" },
                { "SearchProducts", "Search products..." },
                { "Image", "Image" },
                { "Product", "Product" },
                { "SKU", "SKU" },
                { "Category", "Category" },
                { "Cost", "Cost" },
                { "Price", "Price" },
                { "Stock", "Stock" },
                { "Status", "Status" },
                { "Actions", "Actions" },
                { "Edit", "Edit" },
                { "Delete", "Delete" },
                { "Save", "Save" },
                { "Cancel", "Cancel" },
                { "ProductName", "Product Name" },
                { "Description", "Description" },
                { "CostPrice", "Cost Price" },
                { "SellPrice", "Sell Price" },
                { "StockQuantity", "Stock Quantity" },
                { "IsActive", "Active" },
                { "Inactive", "Inactive" },
                { "UploadImage", "Upload Image" },
                { "ClickToUpload", "Click to upload" },
                { "Uploading", "Uploading..." },
                { "SelectCategory", "Select Category" },
                { "AllCategories", "All Categories" },

                // Categories Page
                { "CategoryList", "Category List" },
                { "AddCategory", "Add Category" },
                { "SearchCategories", "Search categories..." },
                { "CategoryName", "Category Name" },
                { "NoCategories", "No categories available" },

                // Orders Page
                { "OrderList", "Order List" },
                { "Date", "Date" },
                { "Items", "Items" },
                { "Total", "Total" },
                { "Completed", "Completed" },
                { "ViewDetails", "View Details" },
                { "NoOrders", "No orders found" },
                { "OrderNumber", "Order Number" },
                { "SearchOrders", "Search orders..." },
                { "Pending", "Pending" },
                { "Confirmed", "Confirmed" },
                { "Cancelled", "Cancelled" },

                // POS Page
                { "PointOfSale", "Point of Sale" },
                { "ShoppingCart", "Shopping Cart" },
                { "CartEmpty", "Cart is empty" },
                { "ClickProducts", "Click on products to add them" },
                { "Subtotal", "Subtotal" },
                { "Tax", "Tax (10%)" },
                { "CompleteOrder", "Complete Order" },
                { "Processing", "Processing..." },
                { "InStock", "in stock" },
                { "LowStock", "Low stock" },
                { "NoProducts", "No products available" },
                { "OrderCompleted", "Order completed!" },
                { "OrderFailed", "Failed to create order" },

                // Reports Page
                { "ReportList", "Report List" },
                { "SalesReport", "Sales Report" },
                { "TotalRevenue", "Total Revenue" },
                { "AverageOrderValue", "Average Order Value" },
                { "OrderCount", "Order Count" },
                { "StartDate", "Start Date" },
                { "EndDate", "End Date" },
                { "Generate", "Generate Report" },
                { "SalesSummary", "Sales Summary" },
                { "TotalOrders", "Total Orders" },
                { "ProductsSold", "Products Sold" },
                { "DailySales", "Daily Sales" },
                { "SelectDate", "Select Date" },
                { "AverageOrder", "Avg Order" },
                { "TopSellingProducts", "Top Selling Products" },
                { "NoSalesForDate", "No sales recorded for this date" },

                // Users Page
                { "UserList", "User List" },
                { "AddUser", "Add User" },
                { "Username", "Username" },
                { "Email", "Email" },
                { "Role", "Role" },
                { "Manager", "Manager" },
                { "Admin", "Admin" },
                { "Staff", "Staff" },
                { "FullName", "Full Name" },
                { "Password", "Password" },
                { "ResetPassword", "Reset Password" },
                { "NewPassword", "New Password" },
                { "ConfirmPassword", "Confirm Password" },
                { "PasswordMismatch", "Passwords do not match" },
                { "Activate", "Activate" },
                { "Deactivate", "Deactivate" },
                { "UserManagement", "User Management" },
                { "Created", "Created" },
                { "UserIsActive", "Active" },
                { "UserInactive", "Inactive" },

                // Import
                { "ImportProducts", "Import Products" },
                { "SelectFile", "Select Excel File" },
                { "Preview", "Preview" },
                { "StartImport", "Start Import" },
                { "ImportSuccess", "Import completed successfully!" },
                { "ImportFailed", "Import failed" },
                { "NewCategories", "New Categories" },
                { "Progress", "Progress" },

                // Validation & Messages
                { "RequiredField", "This field is required" },
                { "InvalidEmail", "Invalid email address" },
                { "SuccessfullySaved", "Successfully saved" },
                { "SuccessfullyDeleted", "Successfully deleted" },
                { "DeleteConfirm", "Are you sure you want to delete?" },
                { "Error", "Error" },
                { "Success", "Success" },
                { "Warning", "Warning" },
                { "Info", "Info" },

                // Common
                { "Loading", "Loading..." },
                { "NoData", "No data available" },
                { "Export", "Export" },
                { "Import", "Import" },
                { "Filter", "Filter" },

                // Dashboard
                { "Dashboard", "Dashboard" },
                { "TodaysOrders", "Today's Orders" },
                { "TodaysRevenue", "Today's Revenue" },
                { "TotalProducts", "Total Products" },
                { "TopSellingProductsToday", "Top Selling Products Today" },
                { "QuantitySold", "Quantity Sold" },
                { "Revenue", "Revenue" },
                { "NoSalesToday", "No sales recorded today" },

                // Store Registration
                { "RegisterStore", "Register Store" },
                { "CreateYourStoreDescription", "Create your store and start managing your inventory" },
                { "StoreInformation", "Store Information" },
                { "StoreName", "Store Name" },
                { "StoreSlug", "Store URL Slug" },
                { "StoreSlugHelper", "This will be your store's unique URL identifier" },
                { "Location", "Location" },
                { "Address", "Address" },
                { "Industry", "Industry" },
                { "OwnerInformation", "Owner Information" },
                { "StoreRegisteredSuccessfully", "Store registered successfully!" },
                { "RegistrationFailed", "Registration failed. Please try again." },
                { "AlreadyHaveAccount", "Already have an account?" },
                { "Login", "Login" },

                // Invitations
                { "InviteUser", "Invite User" },
                { "InviteUserDescription", "Send an invitation email to add a new user to your store" },
                { "SendInvitation", "Send Invitation" },
                { "InvitationSentSuccessfully", "Invitation sent successfully!" },
                { "FailedToSendInvitation", "Failed to send invitation" },
                { "PleaseInputEmail", "Please enter an email address" },
                { "Close", "Close" },
                { "ExpiresAt", "Expires at" },

                // Accept Invitation
                { "AcceptInvitation", "Accept Invitation" },
                { "ValidatingInvitation", "Validating invitation" },
                { "InvalidInvitation", "Invalid Invitation" },
                { "NoInvitationToken", "No invitation token provided" },
                { "InvitationNotFound", "This invitation could not be found or has been revoked" },
                { "ErrorValidatingInvitation", "An error occurred while validating the invitation" },
                { "InvitationAlreadyUsed", "Invitation Already Used" },
                { "InvitationAlreadyUsedMessage", "This invitation has already been used. Please log in with your account." },
                { "InvitationExpired", "Invitation Expired" },
                { "InvitationExpiredMessage", "This invitation has expired. Please ask the store manager to send a new invitation." },
                { "JoinStore", "Join Store" },
                { "YouHaveBeenInvitedTo", "You have been invited to join" },
                { "CreateYourAccount", "Create Your Account" },
                { "PasswordsDoNotMatch", "Passwords do not match" },
                { "PasswordTooShort", "Password must be at least 6 characters" },
                { "WelcomeToStore", "Welcome to" },
                { "FailedToAcceptInvitation", "Failed to accept invitation. Please try again." },
                { "BackToLogin", "Back to Login" },
                { "PleaseInputAllFields", "Please fill in all required fields" },

                // Roles
                { "SystemAdmin", "System Admin" },
            }
        },
        {
            Language.VIE, new Dictionary<string, string>
            {
                // Navigation & Pages
                { "Products", "Sản phẩm" },
                { "Categories", "Danh mục" },
                { "Orders", "Đơn hàng" },
                { "Reports", "Báo cáo" },
                { "Users", "Người dùng" },
                { "POS", "Bán hàng" },
                { "Logout", "Đăng xuất" },

                // Products Page
                { "ProductList", "Danh sách sản phẩm" },
                { "AddProduct", "Thêm sản phẩm" },
                { "ImportExcel", "Nhập từ Excel" },
                { "SearchProducts", "Tìm kiếm sản phẩm..." },
                { "Image", "Hình ảnh" },
                { "Product", "Sản phẩm" },
                { "SKU", "SKU" },
                { "Category", "Danh mục" },
                { "Cost", "Giá vốn" },
                { "Price", "Giá bán" },
                { "Stock", "Kho" },
                { "Status", "Trạng thái" },
                { "Actions", "Hành động" },
                { "Edit", "Sửa" },
                { "Delete", "Xóa" },
                { "Save", "Lưu" },
                { "Cancel", "Hủy" },
                { "ProductName", "Tên sản phẩm" },
                { "Description", "Mô tả" },
                { "CostPrice", "Giá vốn" },
                { "SellPrice", "Giá bán" },
                { "StockQuantity", "Số lượng kho" },
                { "IsActive", "Đang Kinh doanh" },
                { "Inactive", "Ngừng Kinh doanh" },
                { "UploadImage", "Tải lên hình ảnh" },
                { "ClickToUpload", "Nhấp để tải lên" },
                { "Uploading", "Đang tải lên..." },
                { "SelectCategory", "Chọn danh mục" },
                { "AllCategories", "Tất cả danh mục" },

                // Categories Page
                { "CategoryList", "Danh sách danh mục" },
                { "AddCategory", "Thêm danh mục" },
                { "SearchCategories", "Tìm kiếm danh mục..." },
                { "CategoryName", "Tên danh mục" },
                { "NoCategories", "Không có danh mục nào" },

                // Orders Page
                { "OrderList", "Danh sách đơn hàng" },
                { "Date", "Ngày tháng" },
                { "Items", "Mặt hàng" },
                { "Total", "Tổng cộng" },
                { "Completed", "Hoàn thành" },
                { "ViewDetails", "Xem chi tiết" },
                { "NoOrders", "Không tìm thấy đơn hàng" },
                { "OrderNumber", "Mã đơn hàng" },
                { "SearchOrders", "Tìm kiếm đơn hàng..." },
                { "Pending", "Chờ xử lý" },
                { "Confirmed", "Đã xác nhận" },
                { "Cancelled", "Đã hủy" },

                // POS Page
                { "PointOfSale", "Bán hàng" },
                { "ShoppingCart", "Giỏ hàng" },
                { "CartEmpty", "Giỏ hàng trống" },
                { "ClickProducts", "Nhấp vào sản phẩm để thêm vào giỏ" },
                { "Subtotal", "Tạm tính" },
                { "Tax", "Thuế (10%)" },
                { "CompleteOrder", "Hoàn thành đơn hàng" },
                { "Processing", "Đang xử lý..." },
                { "InStock", "trong kho" },
                { "LowStock", "Kho thấp" },
                { "NoProducts", "Không có sản phẩm" },
                { "OrderCompleted", "Đơn hàng hoàn thành!" },
                { "OrderFailed", "Không thể tạo đơn hàng" },

                // Reports Page
                { "ReportList", "Danh sách báo cáo" },
                { "SalesReport", "Báo cáo bán hàng" },
                { "TotalRevenue", "Tổng doanh thu" },
                { "AverageOrderValue", "Giá trị đơn hàng trung bình" },
                { "OrderCount", "Số đơn hàng" },
                { "StartDate", "Ngày bắt đầu" },
                { "EndDate", "Ngày kết thúc" },
                { "Generate", "Tạo báo cáo" },
                { "SalesSummary", "Tổng hợp doanh số" },
                { "TotalOrders", "Tổng đơn hàng" },
                { "ProductsSold", "Sản phẩm đã bán" },
                { "DailySales", "Doanh số hàng ngày" },
                { "SelectDate", "Chọn ngày" },
                { "AverageOrder", "Đơn hàng TB" },
                { "TopSellingProducts", "Sản phẩm bán chạy" },
                { "NoSalesForDate", "Không có doanh số cho ngày này" },

                // Users Page
                { "UserList", "Danh sách người dùng" },
                { "AddUser", "Thêm người dùng" },
                { "Username", "Tên đăng nhập" },
                { "Email", "Email" },
                { "Role", "Vai trò" },
                { "Manager", "Quản lý" },
                { "Admin", "Quản trị viên" },
                { "Staff", "Nhân viên" },
                { "FullName", "Họ và tên" },
                { "Password", "Mật khẩu" },
                { "ResetPassword", "Đặt lại mật khẩu" },
                { "NewPassword", "Mật khẩu mới" },
                { "ConfirmPassword", "Xác nhận mật khẩu" },
                { "PasswordMismatch", "Mật khẩu không khớp" },
                { "Activate", "Kích hoạt" },
                { "Deactivate", "Vô hiệu hóa" },
                { "UserManagement", "Quản lý người dùng" },
                { "Created", "Ngày tạo" },
                { "UserIsActive", "Đang hoạt động" },
                { "UserInactive", "Đã vô hiệu hóa" },

                // Import
                { "ImportProducts", "Nhập sản phẩm" },
                { "SelectFile", "Chọn tệp Excel" },
                { "Preview", "Xem trước" },
                { "StartImport", "Bắt đầu nhập" },
                { "ImportSuccess", "Nhập thành công!" },
                { "ImportFailed", "Nhập thất bại" },
                { "NewCategories", "Danh mục mới" },
                { "Progress", "Tiến trình" },

                // Validation & Messages
                { "RequiredField", "Trường này là bắt buộc" },
                { "InvalidEmail", "Địa chỉ email không hợp lệ" },
                { "SuccessfullySaved", "Lưu thành công" },
                { "SuccessfullyDeleted", "Xóa thành công" },
                { "DeleteConfirm", "Bạn có chắc chắn muốn xóa?" },
                { "Error", "Lỗi" },
                { "Success", "Thành công" },
                { "Warning", "Cảnh báo" },
                { "Info", "Thông tin" },

                // Common
                { "Loading", "Đang tải..." },
                { "NoData", "Không có dữ liệu" },
                { "Export", "Xuất" },
                { "Import", "Nhập" },
                { "Filter", "Lọc" },

                // Dashboard
                { "Dashboard", "Bảng điều khiển" },
                { "TodaysOrders", "Đơn hàng hôm nay" },
                { "TodaysRevenue", "Doanh thu hôm nay" },
                { "TotalProducts", "Tổng sản phẩm" },
                { "TopSellingProductsToday", "Sản phẩm bán chạy hôm nay" },
                { "QuantitySold", "Số lượng bán" },
                { "Revenue", "Doanh thu" },
                { "NoSalesToday", "Không có bán hàng hôm nay" },

                // Store Registration
                { "RegisterStore", "Đăng ký cửa hàng" },
                { "CreateYourStoreDescription", "Tạo cửa hàng và bắt đầu quản lý kho hàng của bạn" },
                { "StoreInformation", "Thông tin cửa hàng" },
                { "StoreName", "Tên cửa hàng" },
                { "StoreSlug", "Mã URL cửa hàng" },
                { "StoreSlugHelper", "Đây sẽ là mã định danh URL duy nhất của cửa hàng" },
                { "Location", "Địa điểm" },
                { "Address", "Địa chỉ" },
                { "Industry", "Ngành nghề" },
                { "OwnerInformation", "Thông tin chủ sở hữu" },
                { "StoreRegisteredSuccessfully", "Đăng ký cửa hàng thành công!" },
                { "RegistrationFailed", "Đăng ký thất bại. Vui lòng thử lại." },
                { "AlreadyHaveAccount", "Đã có tài khoản?" },
                { "Login", "Đăng nhập" },

                // Invitations
                { "InviteUser", "Mời người dùng" },
                { "InviteUserDescription", "Gửi email mời để thêm người dùng mới vào cửa hàng của bạn" },
                { "SendInvitation", "Gửi lời mời" },
                { "InvitationSentSuccessfully", "Gửi lời mời thành công!" },
                { "FailedToSendInvitation", "Gửi lời mời thất bại" },
                { "PleaseInputEmail", "Vui lòng nhập địa chỉ email" },
                { "Close", "Đóng" },
                { "ExpiresAt", "Hết hạn lúc" },

                // Accept Invitation
                { "AcceptInvitation", "Chấp nhận lời mời" },
                { "ValidatingInvitation", "Đang xác thực lời mời" },
                { "InvalidInvitation", "Lời mời không hợp lệ" },
                { "NoInvitationToken", "Không có mã lời mời" },
                { "InvitationNotFound", "Không tìm thấy lời mời này hoặc đã bị thu hồi" },
                { "ErrorValidatingInvitation", "Đã xảy ra lỗi khi xác thực lời mời" },
                { "InvitationAlreadyUsed", "Lời mời đã được sử dụng" },
                { "InvitationAlreadyUsedMessage", "Lời mời này đã được sử dụng. Vui lòng đăng nhập bằng tài khoản của bạn." },
                { "InvitationExpired", "Lời mời đã hết hạn" },
                { "InvitationExpiredMessage", "Lời mời này đã hết hạn. Vui lòng yêu cầu quản lý cửa hàng gửi lời mời mới." },
                { "JoinStore", "Tham gia cửa hàng" },
                { "YouHaveBeenInvitedTo", "Bạn đã được mời tham gia" },
                { "CreateYourAccount", "Tạo tài khoản của bạn" },
                { "PasswordsDoNotMatch", "Mật khẩu không khớp" },
                { "PasswordTooShort", "Mật khẩu phải có ít nhất 6 ký tự" },
                { "WelcomeToStore", "Chào mừng đến với" },
                { "FailedToAcceptInvitation", "Chấp nhận lời mời thất bại. Vui lòng thử lại." },
                { "BackToLogin", "Quay lại Đăng nhập" },
                { "PleaseInputAllFields", "Vui lòng điền đầy đủ các trường bắt buộc" },

                // Roles
                { "SystemAdmin", "Quản trị viên hệ thống" },
            }
        }
    };

    public LocalizationService(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    public string this[string key]
    {
        get
        {
            var language = _languageService.CurrentLanguage;
            if (_translations.TryGetValue(language, out var dict))
            {
                return dict.TryGetValue(key, out var value) ? value : key;
            }
            return key;
        }
    }

    public Language CurrentLanguage => _languageService.CurrentLanguage;
}
