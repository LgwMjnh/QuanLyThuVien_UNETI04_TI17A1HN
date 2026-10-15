using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class PhieuMuonController : Controller
    {
        // GET: /PhieuMuon
        public IActionResult Index()
        {
            return View();
        }

        // GET: /PhieuMuon/DangKy
        public IActionResult DangKy()
        {
            return View();
        }

        // GET: /PhieuMuon/Details
        public IActionResult Details()
        {
            return View();
        }
    }
}
