﻿using Microsoft.AspNetCore.Mvc;
using HRMManagement.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRMManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Resources;
using System.Reflection.Metadata.Ecma335;

namespace HRMManagement.Controllers
{
    public class InfoController : Controller
    {
        private readonly INhanVien _nhanvien;
        private readonly IChucVu _chucvu;
        private readonly IViTricv _vitricv;
        private readonly IPhongBan _phongban;
        private readonly HrmContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public InfoController(HrmContext context, INhanVien nhanvien, IPhongBan phongban, IViTricv vitricv, IChucVu chucvu, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _nhanvien = nhanvien;
            _vitricv = vitricv;
            _phongban = phongban;
            _chucvu = chucvu;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, string positionFilter, string titleFilter, string departmentFilter, int? pageNumber)
        {
            List<Display> query = (from nv in _context.Nhanviens
                                   join cv in _context.Chucvus
                                   on nv.IdchucVu equals cv.Id
                                   join pb in _context.Phongbans
                                   on nv.IdphongBan equals pb.Id
                                   join vt in _context.Vitricvs
                                   on nv.IdviTri equals vt.Id
                                   select new Display
                                   {
                                       Id = nv.Id,
                                       HoDem = nv.HoDem,
                                       Ten = nv.Ten,
                                       TenChucVu = cv.TenChucVu,
                                       TenPhongBan = pb.TenPhongBan,
                                       TenVitri = vt.TenVitri
                                   }).ToList();

            // Search
            if (!string.IsNullOrEmpty(searchString) && query != null)
            {
                if (searchString == "all")
                {
                    return View(query);
                }

                searchString = searchString.ToLowerInvariant();

                query = query.Where(n =>
                    (n.TenPhongBan != null && n.TenPhongBan.ToLowerInvariant().Contains(searchString))
                    || (n.TenVitri != null && n.TenVitri.ToLowerInvariant().Contains(searchString))
                    || (n.TenChucVu != null && n.TenChucVu.ToLowerInvariant().Contains(searchString))
                    || (n.Ten != null && n.Ten.ToLowerInvariant().Contains(searchString))
                    || (n.HoDem != null && n.HoDem.ToLowerInvariant().Contains(searchString))
                    || (n.Ten != null && n.HoDem != null && (n.HoDem + " " + n.Ten).ToLowerInvariant().Contains(searchString))
                    || (n.Id != null && n.Id.ToString().ToLowerInvariant().Contains(searchString))
                    || (n.Sdt != null && n.Sdt.ToLowerInvariant().Contains(searchString))
                    || (n.Cccd != null && n.Cccd.ToLowerInvariant().Contains(searchString))
                // More ...
                ).OrderBy(n => n.Ten).ToList();
            }

            // Filter

            if (!string.IsNullOrEmpty(departmentFilter) && query != null)
            {
                query = query.Where(n => n.TenPhongBan != null && n.TenPhongBan != null && n.TenPhongBan.Contains(departmentFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(titleFilter) && query != null)
            {
                query = query.Where(n => n.TenChucVu != null && n.TenChucVu != null && n.TenChucVu.Contains(titleFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(positionFilter) && query != null)
            {
                query = query.Where(n => n.TenVitri != null && n.TenVitri != null && n.TenVitri.Contains(positionFilter)).ToList();
            }


            // Filters take advantage of the search feature
            // Get the unique presence of columns  
            var uniquePhongBan = query.Select(n => n.TenPhongBan).Distinct().OrderBy(x => x).ToList();
            var uniqueChucVu = query.Select(n => n.TenChucVu).Distinct().OrderBy(x => x).ToList();
            var uniqueViTri = query.Select(n => n.TenVitri).Distinct().OrderBy(x => x).ToList();


            ViewBag.uniquePhongBan = uniquePhongBan;
            ViewBag.uniqueChucVu = uniqueChucVu;
            ViewBag.uniqueViTri = uniqueViTri;

            ////

            if (pageNumber == null || pageNumber < 1)
            {
                pageNumber = 1;
            }

            int pageSize = 10;

            return View(PaginatedList<Display>.Create(query, pageNumber ?? 1, pageSize));
        
    }

            public async Task<IActionResult> Display(string id)
        {
            var query = (from nv in _context.Nhanviens
                         join cv in _context.Chucvus
                         on nv.IdchucVu equals cv.Id
                         join pb in _context.Phongbans
                         on nv.IdphongBan equals pb.Id
                         join vt in _context.Vitricvs
                         on nv.IdviTri equals vt.Id
                         select new Display
                         {
                             Id = nv.Id,
                             HoDem = nv.HoDem,
                             Ten = nv.Ten,
                             NgaySinh = nv.NgaySinh,
                             GioiTinh = nv.GioiTinh,
                             Cccd = nv.Cccd,
                             DiaChi = nv.DiaChi,
                             Sdt = nv.Sdt,
                             Email = nv.Email,
                             TenChucVu = cv.TenChucVu,
                             TenPhongBan = pb.TenPhongBan,
                             TenVitri = vt.TenVitri
                         }).ToList();
            var ma = query.FirstOrDefault(p => p.Id == id);

            return View(ma);
        }

        public async Task<IActionResult> Add()
        {
            var chucvus = await _chucvu.GetAllAsync();
            ViewBag.chucvus = new SelectList(chucvus, "Id", "TenChucVu");
            var vitricvs = await _vitricv.GetAllAsync();
            ViewBag.vitricvs = new SelectList(vitricvs, "Id", "TenVitri");
            var phongbans = await _phongban.GetAllAsync();
            ViewBag.phongbans = new SelectList(phongbans, "Id", "TenPhongBan");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Nhanvien nv, int ChucVu, int PhongBan, int ViTricv)
        {

            if (ModelState.IsValid)
            {
                nv.IdchucVu = ChucVu;
                nv.IdphongBan = PhongBan;
                nv.IdviTri = ViTricv;
                await _nhanvien.AddAsync(nv);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return BadRequest(ModelState);
            }

        }


        public async Task<IActionResult> Delete(string id)
        {
            var query = (from nv in _context.Nhanviens
                         join cv in _context.Chucvus
                         on nv.IdchucVu equals cv.Id
                         join pb in _context.Phongbans
                         on nv.IdphongBan equals pb.Id
                         join vt in _context.Vitricvs
                         on nv.IdviTri equals vt.Id
                         select new Display
                         {
                             Id = nv.Id,
                             HoDem = nv.HoDem,
                             Ten = nv.Ten,
                             NgaySinh = nv.NgaySinh,
                             GioiTinh = nv.GioiTinh,
                             Cccd = nv.Cccd,
                             DiaChi = nv.DiaChi,
                             Sdt = nv.Sdt,
                             Email = nv.Email,
                             TenChucVu = cv.TenChucVu,
                             TenPhongBan = pb.TenPhongBan,
                             TenVitri = vt.TenVitri
                         }).ToList();
            var ma = query.FirstOrDefault(p => p.Id == id);
            return View(ma);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _nhanvien.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public async Task<IActionResult> Update(string id)
        {
            var nv = await _nhanvien.GetByIdAsync(id);
            if (nv == null)
            {
                return NotFound();
            }
            return View(nv);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, Nhanvien nv)
        {
            if (id != nv.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingNhanvien = await _nhanvien.GetByIdAsync(id);
                if (existingNhanvien == null)
                {
                    return NotFound("Employee not found");
                }
                existingNhanvien.HoDem = nv.HoDem;
                existingNhanvien.Ten = nv.Ten;
                existingNhanvien.NgaySinh = nv.NgaySinh;
                bool gioitinh;
                if (nv.GioiTinh == true)
                {
                    gioitinh = true;
                }
                else gioitinh = false;
                existingNhanvien.GioiTinh = nv.GioiTinh;
                existingNhanvien.Cccd = nv.Cccd;
                existingNhanvien.DiaChi = nv.DiaChi;
                existingNhanvien.Email = nv.Email;
                existingNhanvien.Sdt = nv.Sdt;
                await _nhanvien.UpdateAsync(existingNhanvien);
                return RedirectToAction(nameof(Display), new { id = nv.Id });
            }
            else
            {
                return BadRequest(ModelState);
            }
            var ma = _context.Nhanviens.FirstOrDefault(p => p.Id == id);
            return View(ma);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/image", image.FileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/image/" + image.FileName;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages( string id, [FromForm] IFormFile imageFile)
        {
                if (imageFile != null && imageFile.Length > 0)
                {
                try
                {
                    string fileName = $"{id}.jpg";
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "profileimages");
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return Content($"Error: {ex.Message}");
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
    
}
