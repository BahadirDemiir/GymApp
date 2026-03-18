using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using Microsoft.EntityFrameworkCore;
using GymAppFresh.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymAppFresh.Areas.Admin.Controllers {
    [Area("Admin")]
    public class GymController : Controller {         
        private readonly DataContext context;

        public GymController(DataContext context) {
            this.context = context;
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        public IActionResult addGym() {
            ViewBag.Cities=context.Cities.Select(c=>new SelectListItem{
            Value=c.CityId.ToString(),
            Text=c.Name
        }).ToList();

        ViewBag.Roles = context.Roles.Select(r => new SelectListItem {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();
        return View();
        }
        

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult addGym(GymCreateVm vm, List<IFormFile> photos) {
            var imagePath = new List<string>();

            if (photos != null && photos.Count > 0) {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "gyms");
                if (!Directory.Exists(uploadPath)) {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var photo in photos) {
                    if (photo.Length > 0) {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        var path = Path.Combine(uploadPath, fileName);
                        
                        try {
                            using (var stream = new FileStream(path, FileMode.Create)) {
                                photo.CopyTo(stream);
                            }
                            imagePath.Add("/images/gyms/" + fileName);
                        } catch (Exception ex) {
                            Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                        }
                    }
                }
            }

            var gymLocation = new GymLocation
            {
                Name = vm.Name,
                currentMember = vm.CurrentMember,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                OpenTime = vm.OpenTime,
                CloseTime = vm.CloseTime,
                IsActive = vm.IsActive,
                WOpenTime=vm.WOpenTime,
                WCloseTime=vm.WCloseTime,
                CityId=vm.CityId,

                Images = string.Join(",", imagePath)
            };

            context.GymLocations.Add(gymLocation);
            context.SaveChanges(); 

            if (photos != null) {
                foreach (var imagePathItem in imagePath) {
                    var gymImage = new GymImage
                    {
                        GymLocationId = gymLocation.Id,
                        ImagePath = imagePathItem
                    };
                    context.GymImages.Add(gymImage);
                }
                context.SaveChanges(); 
            }

            return RedirectToAction("gymList", "Gym", new { area = "" });
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        public IActionResult EditGym() {
            var gyms=context.GymLocations.OrderBy(g=>g.Name).ToList();
            return View(gyms);
        }

        [Authorize(Roles="Admin")]
        [HttpPost]
        public IActionResult EditGym(List<GymLocation> gyms) {
            foreach (var gym in gyms) {
                var dbGym=context.GymLocations.Find(gym.Id);
                if (dbGym!=null) {
                    dbGym.IsActive=gym.IsActive;
                }
            }
            context.SaveChanges();
            return RedirectToAction("gymList", "Gym", new { area = "" });
        }
        
        [Authorize(Roles="Admin")]
        [HttpGet]
        public IActionResult AddGymStaff() {
            ViewBag.Roles = context.Roles.Select(r => new SelectListItem {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();
            
            ViewBag.GymLocations = context.GymLocations
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList();
            
            return View();
        }
        [Authorize(Roles="Admin")]
        [HttpPost]
        public IActionResult AddGymStaff(GymStaff vm) {
            context.GymStaffs.Add(vm);
            context.SaveChanges();
            return RedirectToAction("gymList", "Gym", new { area = "" });
        }

    }
}