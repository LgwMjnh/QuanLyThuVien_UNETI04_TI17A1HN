using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Helpers
{
    public static class QuyenTruyCap
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";

        public const string VaiTroAdmin = "Admin";
        public const string VaiTroDocGia = "DocGia";

        public static int? LayMaTaiKhoan(HttpContext httpContext)
            => httpContext.Session.GetInt32(MaTaiKhoan);

        public static string? LayVaiTro(HttpContext httpContext)
            => httpContext.Session.GetString(VaiTro);

        public static bool DaDangNhap(HttpContext httpContext)
            => LayMaTaiKhoan(httpContext) != null;

        public static bool LaAdmin(HttpContext httpContext)
            => string.Equals(LayVaiTro(httpContext), VaiTroAdmin, StringComparison.OrdinalIgnoreCase);

        public static bool LaDocGia(HttpContext httpContext)
        {
            var vaiTro = LayVaiTro(httpContext);
            return string.Equals(vaiTro, VaiTroDocGia, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(vaiTro, "Độc giả", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class YeuCauDangNhapAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!QuyenTruyCap.DaDangNhap(context.HttpContext))
            {
                GanThongBao(context, "Vui lòng đăng nhập để sử dụng chức năng này.");
                context.Result = new RedirectToActionResult("Index", "DangNhap", null);
            }
        }

        private static void GanThongBao(ActionExecutingContext context, string noiDung)
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["Error"] = noiDung;
            }
        }
    }

    public class YeuCauAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!QuyenTruyCap.DaDangNhap(context.HttpContext) || !QuyenTruyCap.LaAdmin(context.HttpContext))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] = "Chức năng này chỉ dành cho quản trị viên.";
                }
                context.Result = new RedirectToActionResult("Index", "DangNhap", null);
            }
        }
    }

    public class YeuCauDocGiaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!QuyenTruyCap.DaDangNhap(context.HttpContext) || !QuyenTruyCap.LaDocGia(context.HttpContext))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] = "Chức năng này chỉ dành cho độc giả đã đăng nhập.";
                }
                context.Result = new RedirectToActionResult("Index", "DangNhap", null);
            }
        }
    }
}
