using EmailApp.Context;
using EmailApp.Entities;
using EmailApp.Models;
using EmailApp.Context;
using IdentityEmailApp.Entities;
using EmailApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityEmailApp.Controllers
{
    public class MessageController(AppDbContext _context, UserManager<AppUser> _userManager) : Controller
    {
        private async Task SetMessageCounts()
        {
            var userName = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    ViewBag.unreadCount = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsRead == false && x.IsDeleted == false);
                    ViewBag.trashCount = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsDeleted == true);
                    ViewBag.importantCount = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false);

                    ViewBag.UserFirstName = user.FirstName;
                    ViewBag.UserLastName = user.LastName;

                    ViewBag.RecentMessages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .Take(3)
                .ToListAsync();
                }
            }
        }
        [Authorize]
        public async Task<IActionResult> Index(int page = 1)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return View(new List<Message>());
            }

            int pageSize = 5; // Her sayfada 5 mesaj
            int skip = (page - 1) * pageSize;

            var totalMessages = await _context.Messages
                .Where(x => x.ReceiverId == user.Id && x.IsDeleted == false)
                .CountAsync();

            var messages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize);
            ViewBag.TotalMessages = totalMessages;

            return View(messages);
        }

        public async Task<IActionResult> MessageDetail(int id)
        {
            await SetMessageCounts();

            var message = _context.Messages.Include(x => x.Sender).FirstOrDefault(x => x.MessageId == id);
            return View(message);
        }

        public async Task<IActionResult> SendMessage()
        {
            await SetMessageCounts();

            var model = new SendMessageViewModel();

            // Eğer draft düzenleniyorsa, bilgileri doldur
            if (TempData["DraftId"] != null)
            {
                model.ReceiverEmail = TempData["ReceiverEmail"]?.ToString() ?? "";
                model.Subject = TempData["Subject"]?.ToString() ?? "";
                model.Body = TempData["Body"]?.ToString() ?? "";
                ViewBag.DraftId = TempData["DraftId"];
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity?.Name);
            var receiver = await _userManager.FindByEmailAsync(model.ReceiverEmail);

            var message = new Message
            {
                Body = model.Body,
                Subject = model.Subject,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                SendDate = DateTime.Now,
            };
            _context.Messages.Add(message);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }



        public IActionResult MarkAsRead(int id)
        {
            var message = _context.Messages.Find(id);
            if (message != null)
            {
                message.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult MarkAsUnread(int id)
        {
            var message = _context.Messages.Find(id);
            if (message != null)
            {
                message.IsRead = false;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteMessage(int id)
        {
            var message = _context.Messages.Find(id);
            if (message != null)
            {
                message.IsDeleted = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Trash()
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return View(new List<Message>());
            }

            var deletedMessages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsDeleted == true)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(deletedMessages);
        }

        public IActionResult RestoreMessage(int id)
        {
            var message = _context.Messages.Find(id);
            if (message != null)
            {
                message.IsDeleted = false;
                _context.SaveChanges();
            }
            return RedirectToAction("Trash");
        }

        public IActionResult PermanentlyDeleteMessage(int id)
        {
            var message = _context.Messages.Find(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
            }
            return RedirectToAction("Trash");
        }
        public async Task<IActionResult> SentMessages(int page = 1)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return View(new List<Message>());
            }

            int pageSize = 5; // Her sayfada 5 mesaj
            int skip = (page - 1) * pageSize;

            var totalMessages = await _context.Messages
                .Where(x => x.SenderId == user.Id && x.IsDeleted == false && x.IsDraft == false)
                .CountAsync();

            var messages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Receiver)
                .Where(x => x.SenderId == user.Id && x.IsDeleted == false && x.IsDraft == false)
                .OrderByDescending(x => x.SendDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize);
            ViewBag.TotalMessages = totalMessages;

            return View(messages);
        }

        public IActionResult MarkAsImportant(int id)
        {
            var message = _context.Messages.Find(id);
            if (message is not null)
            {
                message.IsImportant = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult MarkAsNotImportant(int id)
        {
            var message = _context.Messages.Find(id);
            if (message is not null)
            {
                message.IsImportant = false;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ImportantMessages(int page = 1)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return View(new List<Message>());
            }

            // Important count'u ekle
            ViewBag.importantCount = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var totalMessages = await _context.Messages
                .Where(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false)
                .CountAsync();

            var messages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize);
            ViewBag.TotalMessages = totalMessages;

            return View(messages);
        }


        //Draft section

        [HttpPost]
        public async Task<IActionResult> SaveDraft(SendMessageViewModel model)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity?.Name);

            var message = new Message
            {
                Body = model.Body,
                Subject = model.Subject,
                ReceiverId = null, //Draft için bir receiver yok
                SenderId = sender.Id,
                SendDate = DateTime.UtcNow,
                IsDraft = true,
                DraftDate = DateTime.UtcNow,

            };

            _context.Messages.Add(message);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Drafts(int page = 1)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return View(new List<Message>());
            }

            int pageSize = 5;
            int skip = (page - 1) * pageSize;
            var totalDrafts = await _context.Messages
            .Where(x => x.SenderId == user.Id && x.IsDraft == true)
            .CountAsync();

            var drafts = await _context.Messages.AsNoTracking().Where(x => x.SenderId == user.Id && x.IsDraft == true)
                .OrderByDescending(x => x.DraftDate).Skip(skip).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalDrafts / pageSize);
            ViewBag.TotalMessages = totalDrafts;

            return View(drafts);
        }

        public async Task<IActionResult> EditDraft(int id)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return RedirectToAction("Drafts");
            }

            var draft = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == id && x.SenderId == user.Id && x.IsDraft == true);

            if (draft == null)
            {
                return RedirectToAction("Drafts");
            }

            // Draft bilgilerini TempData'ya koy
            TempData["DraftId"] = id;
            TempData["ReceiverEmail"] = draft.Receiver?.Email ?? "";
            TempData["Subject"] = draft.Subject;
            TempData["Body"] = draft.Body;

            // Compose sayfasına yönlendir
            return RedirectToAction("SendMessage");
        }


        public async Task<IActionResult> DeleteDraft(int id)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return RedirectToAction("Drafts");
            }

            var draft = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == id && x.SenderId == user.Id && x.IsDraft == true);

            if (draft != null)
            {
                _context.Messages.Remove(draft); // Fiziksel silme (draft olduğu için)
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Drafts");
        }

    }
}
