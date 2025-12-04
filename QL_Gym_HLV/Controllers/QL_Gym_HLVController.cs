using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using QL_Gym_HLV.Models;

namespace QL_Gym_HLV.Controllers
{
    public class QL_Gym_HLVController : Controller
    {
        // GET: QL_Gym_HLV
        QL_GymEntities db = new QL_GymEntities();
        private void loadSchedule(DateTime selectDate)
        {
            //Lấy ngày đầu tuần với cuối tuần
            DateTime today = selectDate;
            int diff = (int)today.DayOfWeek;
            if (diff == 0)
            {
                diff += 7;
            }
            DateTime monday = today.AddDays(-diff);
            DateTime sunday = monday.AddDays(7);

            NhanVien s = (NhanVien)Session["hlv"];

            //Lấy danh sách lịch lớp của nhân viên đã đăng nhập
            var sa = db.LichLop.Where(n =>
            n.LopHoc.MaNV == s.MaNV &&
            n.NgayHoc <= sunday &&
            n.NgayHoc >= monday
            ).ToList();

            //Lấy danh sách lịch pt của nhân viên đăng nhập
            var pt = db.LichTapPT.Where(n => n.DangKyPT.MaNV == s.MaNV).ToList();

            //Cho vào viewbag để hiện trên view và dùng làm điều kiện
            ViewBag.Monday = monday.AddDays(1);
            ViewBag.Sunday = sunday;
            ViewBag.SelectedDate = selectDate;

            //ViewBag lịch pt
            ViewBag.SangPT = pt.Where(n => n.GioBatDau < new TimeSpan(12, 0, 0)).OrderBy(n => n.GioBatDau).ToList();
            ViewBag.ChieuPT = pt.Where(n => n.GioBatDau < new TimeSpan(17, 0, 0) && n.GioBatDau >= new TimeSpan(13, 0, 0)).OrderBy(n => n.GioBatDau).ToList();
            ViewBag.ToiPT = pt.Where(n => n.GioBatDau < new TimeSpan(21, 0, 0) && n.GioBatDau >= new TimeSpan(18, 0, 0)).OrderBy(n => n.GioBatDau).ToList();

            //ViewBag lịch lớp
            ViewBag.Sang = sa.Where(n => n.GioBatDau < new TimeSpan(12, 0, 0)).OrderBy(n => n.GioBatDau).ToList();
            ViewBag.Chieu = sa.Where(n => n.GioBatDau < new TimeSpan(17, 0, 0) && n.GioBatDau >= new TimeSpan(13, 0, 0)).OrderBy(n => n.GioBatDau).ToList();
            ViewBag.Toi = sa.Where(n => n.GioBatDau < new TimeSpan(21, 0, 0) && n.GioBatDau >= new TimeSpan(18, 0, 0)).OrderBy(n => n.GioBatDau).ToList();
        }
        public ActionResult LichLamViec()
        {
            if (Session["hlv"] == null)
            {
                return View("Error");
            }
            loadSchedule(DateTime.Now);
            return View();
        }
        public ActionResult DanhSachLop()
        {
            if (Session["hlv"] == null)
            {
                return View("Error");
            }
            NhanVien nv = (NhanVien)Session["hlv"];
            return View(db.LopHoc.Where(n => n.MaNV == nv.MaNV).ToList());
        }
        public ActionResult ChinhSuaLop(int id)
        {
            var lop = db.LopHoc.Find(id);
            return PartialView(lop);
        }
        [HttpPost]
        public ActionResult ChinhSuaLop(int id, string TenLop, decimal HocPhi, DateTime NgayBatDau, DateTime NgayKetThuc, int SiSoToiDa)
        {
            var lop = db.LopHoc.Find(id);

            lop.TenLop = TenLop;
            lop.HocPhi = HocPhi;
            lop.NgayBatDau = NgayBatDau;
            lop.NgayKetThuc = NgayKetThuc;
            lop.SiSoToiDa = SiSoToiDa;
            
            UpdateModel(lop);
            db.SaveChanges();
            return RedirectToAction("DanhSachLop");
        }
        public ActionResult DanhSachKhachPT()
        {
            if (Session["hlv"] == null)
            {
                return View("Error");
            }
            var order = new List<string> { "Chờ duyệt", "Còn hiệu lực", "Kết thúc" };
            NhanVien nv = (NhanVien)Session["hlv"];
            return View(db.DangKyPT.Where(n => n.MaNV == nv.MaNV || n.TrangThai == "Chờ duyệt").
                OrderBy(kh => kh.TrangThai == "Chờ duyệt" ? 0 :
                              kh.TrangThai == "Còn hiệu lực" ? 1 : 2).ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NhanPT(int id, int kh, decimal hocPhi)
        {
            NhanVien nv = (NhanVien)Session["hlv"];
            KhachHang k = db.KhachHang.Find(kh);
            var dk = db.DangKyPT.Find(id);

            var mucgiam = k.LoaiKhachHang.MucGiam / 100;
            HoaDon hd = new HoaDon()
            {
                MaKH = k.MaKH,
                NgayLap = DateTime.Now,
                TongTien = hocPhi * dk.SoBuoi,
                GiamGia = (hocPhi * dk.SoBuoi) * mucgiam,
                ThanhTien = (hocPhi * dk.SoBuoi) - (hocPhi * dk.SoBuoi * mucgiam),
                TrangThai = "Chưa thanh toán"
            };
            db.HoaDon.Add(hd);
            db.SaveChanges();

            ChiTietHoaDon cthd = new ChiTietHoaDon()
            {
                MaHD = hd.MaHD,
                MaSP = null,
                MaDKGT = null,
                MaDKLop = null,
                MaDKPT = id,
                SoLuong = 1,
                DonGia = hocPhi
            };
            db.ChiTietHoaDon.Add(cthd);
            db.SaveChanges();
            
            dk.GiaMoiBuoi = hocPhi;
            dk.MaNV = nv.MaNV;
            dk.TrangThai = "Còn hiệu lực";
            UpdateModel(dk);
            db.SaveChanges();
            return RedirectToAction("DanhSachKhachPT");
        }
        [HttpGet]
        public ActionResult TimKiem(string keyword, int loai)
        {
            NhanVien nv = (NhanVien)Session["hlv"];
            if (loai == 2)
            {
                var kh_nv = db.DangKyPT.Where(n => n.MaNV == nv.MaNV).ToList();
                var ds = kh_nv.Where(n => n.MaNV == nv.MaNV && n.KhachHang.SDT.ToLower().Contains(keyword.ToLower().Trim())).ToList();
                return View("DanhSachKhachPT", ds);
            }
            if (loai == 1)
            {
                var ds = db.LopHoc.Where(n =>n.MaNV == nv.MaNV && n.TenLop.ToLower().Contains(keyword.ToLower().Trim())).ToList();
                return View("DanhSachLop", ds);
            }
            return View();
        }
        public ActionResult ThemLop()
        {
            ViewBag.MaCM = new SelectList(db.ChuyenMon.ToList(), "MaCM", "TenChuyenMon");
            return PartialView();
        }
        [HttpPost]
        public ActionResult ThemLop(string tenLop, int siSo, DateTime ngayBatDau, DateTime ngayKetThuc,
            TimeSpan? gioBatDau, TimeSpan? gioKetThuc, decimal hocPhi, int maCM)
        {
            NhanVien nv = (NhanVien)Session["hlv"];
            try
            {
                if (gioBatDau != null && gioKetThuc != null)
                {
                    db.sp_ThemLopHocVaLich(tenLop, maCM, hocPhi, ngayBatDau, ngayKetThuc, siSo, nv.MaNV, gioBatDau, gioKetThuc);
                    TempData["Message"] = "Thêm lớp và lịch thành công!";
                }
                else
                {
                    LopHoc lh = new LopHoc()
                    {
                        TenLop = tenLop,
                        MaCM = maCM,
                        HocPhi = hocPhi,
                        NgayBatDau = ngayBatDau,
                        NgayKetThuc = ngayKetThuc,
                        SiSoToiDa = siSo,
                        MaNV = nv.MaNV
                    };
                    db.LopHoc.Add(lh);
                    db.SaveChanges();
                    TempData["Message"] = "Thêm lớp thành công!";
                }
            }
            catch (Exception ex)
            {
                var sqlEx = ex.GetBaseException();
                if (sqlEx != null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["Message"] = sqlEx.Message;
                }
                else
                {
                    TempData["AlertType"] = "danger";
                    TempData["Message"] = "Có lỗi xảy ra!";
                }
            }
            return RedirectToAction("DanhSachLop");
        }
        public ActionResult XoaLichVaLop(int id)
        {
            db.XoaLichVaLop(id);
            return RedirectToAction("DanhSachLop");
        }
        public ActionResult ChinhSuaLich(DateTime? selectedDate)
        {
            if (Session["hlv"] == null)
            {
                return View("Error");
            }
            DateTime dateDisplay = DateTime.Now;
            if (selectedDate.HasValue)
            {
                dateDisplay = selectedDate.Value;
            }
            loadSchedule(dateDisplay);
            return View();
        }
        public ActionResult ThemLich(DateTime date)
        {
            NhanVien nv = (NhanVien)Session["hlv"];

            ViewBag.date = date;
            var lophoc = db.LopHoc.Where(n => n.NgayKetThuc >= date && n.MaNV == nv.MaNV && n.NgayBatDau <= date);
            ViewBag.Lop = new SelectList(lophoc, "MaLop", "TenLop");

            var pt = (from dk in db.DangKyPT
                      join kh in db.KhachHang on dk.MaKH equals kh.MaKH
                      select new
                      {                 
                          MANV = dk.MaNV,
                          Ma = dk.MaDKPT,
                          Ten = kh.TenKH,
                          TT = dk.TrangThai
                      }).Where(n => n.TT == "Còn hiệu lực" && n.MANV == nv.MaNV).ToList();
            ViewBag.PT = new SelectList(pt, "Ma", "Ten");
            return PartialView();
        }
        public ActionResult XoaLich(int id, int loai, DateTime date)
        {
            if (loai == 1)
            {
                var lich = db.LichLop.Find(id);
                db.LichLop.Remove(lich);
            }
            else
            {
                var lich = db.LichTapPT.Find(id);
                db.LichTapPT.Remove(lich);
            }
            db.SaveChangesAsync();
            return RedirectToAction("ChinhSuaLich", new { selectedDate = date });
        }
        [HttpPost]
        public ActionResult ThemLich(int? MaLop, int? MaDKPT, TimeSpan gioBatDau, TimeSpan gioKetThuc, DateTime ngayHoc)
        {
            NhanVien nv = (NhanVien)Session["hlv"];
            try
            {
                if (MaLop != null)
                {
                    db.sp_ThemLichLop(MaLop, nv.MaNV, ngayHoc, gioBatDau, gioKetThuc);
                    TempData["Message"] = "Thêm lịch thành công!";
                }
                if (MaDKPT != null)
                {
                    db.sp_ThemLichTapPT(MaDKPT, ngayHoc, gioBatDau, gioKetThuc, "Chưa tập");
                    TempData["Message"] = "Thêm lịch thành công!";
                }
            }
            catch (Exception ex)
            {
                var sqlEx = ex.GetBaseException();
                if (sqlEx != null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["Message"] = sqlEx.Message;
                }
                else
                {
                    TempData["AlertType"] = "danger";
                    TempData["Message"] = "Có lỗi xảy ra!";
                }
            }
            return RedirectToAction("ChinhSuaLich", new { selectedDate = ngayHoc });
        }
        [HttpGet]
        public ActionResult ChonNgay(DateTime selectedDate)
        {
            loadSchedule(selectedDate);
            return View("LichLamViec");
        }
        public ActionResult TuanTruocHoacSau(DateTime selectedDate, int i, int id)
        {
            selectedDate = selectedDate.AddDays(i);
            loadSchedule(selectedDate);
            if (id == 1)
                return View("LichLamViec");
            else
                return View("ChinhSuaLich");
        }
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult HLV_DangNhap()
        {
            if (Session["hlv"] != null)
                return RedirectToAction("LichLamViec");
            return View();
        }
        [HttpPost]
        public ActionResult HLV_DangNhap(FormCollection fm)
        {
            string user = fm["user"];
            string pass = fm["password"];
            NhanVien hlv = db.NhanVien.FirstOrDefault(m => m.TenDangNhap == user && m.MatKhau == pass);
            if (hlv == null)
            {
                return View("HLV_DangNhap");
            }
            Session["hlv"] = hlv;
            return RedirectToAction("LichLamViec");
        }
        public ActionResult HLV_DangXuat()
        {
            Session["hlv"] = null;
            return View("HLV_DangNhap");
        }
        public ActionResult _MenuChucNang()
        {
            return PartialView();
        }
    }
}