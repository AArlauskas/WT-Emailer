using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using emailer_backend.Models;

namespace emailer_backend.Controllers
{
    public class UsersController : ApiController
    {
        private EmailerEntities db = new EmailerEntities();

        // GET: api/Users
        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        [HttpGet]
        [Route("api/users/personal")]
        [Authorize(Roles = "admin, user")]
        public IHttpActionResult GetPersonalInformation()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var user = db.Users.Find(id);

            var response = new
            {
                user.Email,
                Role = user.IsAdmin ? "admin" : "user"
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("api/users/overview")]
        [Authorize(Roles = "admin")]
        public IHttpActionResult GetUsersOverview()
        {
            var users = db.Users.Select(user => new
            {
                Code = user.Code.Personal_Code,
                Role = user.IsAdmin,
                user.Status,
                user.Email,
                user.SentCount
            }).ToList();

            return Ok(users);
        }

        [HttpGet]
        [Route("api/users/emailer")]
        [Authorize(Roles = "user")]
        public IHttpActionResult GetEmailerInformation()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var user = db.Users.Find(id);

            var result = new
            {
                user.Email,
                user.SentCount,
                user.Topic,
                user.Message,
                user.Status
            };

            return Ok(result);
        }

        [HttpPost]
        [Route("api/users/register")]
        [AllowAnonymous]
        public IHttpActionResult Register(RegisterUserRequest request)
        {
            var code = db.Codes.Where(tempCode => tempCode.Status == "Created").FirstOrDefault(tempCode => tempCode.Personal_Code == request.code);
            if(code == null)
            {
                return BadRequest();
            }

            bool isEmailValid;

            try
            {
                var addr = new System.Net.Mail.MailAddress(request.email);
                isEmailValid = addr.Address == request.email;
            }
            catch
            {
                isEmailValid = false;
            }

            if(!isEmailValid || request.password.Length < 5)
            {
                return NotFound();
            }

            var existingUser = db.Users.FirstOrDefault(temp => temp.Email == request.email && temp.Password == request.password);

            if(existingUser != null)
            {
                db.Users.Find(existingUser.Id).Code = code;
                db.Users.Find(existingUser.Id).SentCount = 0;
                db.Users.Find(existingUser.Id).Status = "Registered";
                db.Codes.Find(code.Id).Status = "Used";
                db.Codes.Find(code.Id).User = existingUser;
                db.SaveChanges();
                return Ok();
            }

            var user = new User()
            {
                Email = request.email,
                Password = request.password,
                IsAdmin = false,
                Topic = "This is where your topic goes",
                Message = "<h1>This is the email's body.</h1><p><br></p><p>You can customize it to your liking.</p>",
                Status = "Registered",
                Code = code,
                SentCount = 0
            };

            db.Users.Add(user);
            db.Codes.Find(code.Id).Status = "Used";
            db.Codes.Find(code.Id).User = user;
            db.SaveChanges();

            var startupPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
            Directory.CreateDirectory(startupPath + "/Uploads/" + user.Id);
            Directory.CreateDirectory(startupPath + "/Logs/" + user.Id);

            return Ok();
            
        }

        [HttpPost]
        [Route("api/users/emailer")]
        [Authorize(Roles = "user")]
        public IHttpActionResult PostEmailerInformation(SaveChangesRequest request)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);

            db.Users.Find(id).Message = request.Message;
            db.Users.Find(id).Topic = request.Topic;
            db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("api/users/test")]
        [Authorize(Roles = "user")]
        public IHttpActionResult SendTestEmail()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var user = db.Users.Find(id);

            try
            {
                var mail = new MailMessage();
                SmtpClient config = new SmtpClient("smtp.gmail.com");
                config.Port = 587;
                config.EnableSsl = true;
                config.Credentials = new NetworkCredential(user.Email, user.Password);
                mail.IsBodyHtml = true;
                mail.From = new MailAddress(user.Email);
                mail.To.Add(new MailAddress(user.Email));
                mail.Subject = user.Topic;
                mail.Body = user.Message;

                var root = HttpContext.Current.Server.MapPath("~/Uploads/" + user.Id + "/");
                var fileRoutes = Directory.GetFiles(root).ToList();

                foreach(var route in fileRoutes)
                {
                    var attachment = new Attachment(route);
                    mail.Attachments.Add(attachment);
                }

                config.Send(mail);
            }
            catch
            {
                return BadRequest();
            }
            finally
            {
                db.Users.Find(id).SentCount++;
                db.SaveChanges();
            }

            return Ok();
        }

        [HttpPost]
        [Route("api/users/register/test")]
        [AllowAnonymous]
        public IHttpActionResult SendRegisterTestEmail(RegisterUserRequest request)
        {

            var temp = db.Codes.Where(code => code.Status == "Created").FirstOrDefault(code => request.code == code.Personal_Code);
            if(temp == null)
            {
                return BadRequest();
            }

            try
            {
                var mail = new MailMessage();
                SmtpClient config = new SmtpClient("smtp.gmail.com");
                config.Port = 587;
                config.EnableSsl = true;
                config.Credentials = new NetworkCredential(request.email, request.password);
                mail.From = new MailAddress(request.email);
                mail.To.Add(new MailAddress(request.email));
                mail.Subject = "Test email from WT-Emailer";
                mail.Body = "This is a test email. If you got this message, it means that the system can send emails correctly. Please return back to the registration screen";
                config.Send(mail);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/Users/file-list")]
        [Authorize(Roles = "user")]
        public IHttpActionResult getFileList()
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var foundUser = db.Users.FirstOrDefault(user => user.Id == id);
            var startupPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
            var fileRoutes = Directory.GetFiles(startupPath + "/Uploads/" + foundUser.Id);
            List<string> files = new List<string>();
            foreach (string route in fileRoutes)
            {
                files.Add(Path.GetFileName(route));
            }
            if (files.Count() == 0)
            {
                return Ok("");
            }
            return Ok(files);
        }

        [HttpPost]
        [Route("api/Users/upload-files")]
        [Authorize(Roles = "user")]
        public HttpResponseMessage UploadFiles()
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 2)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            var identity = (ClaimsIdentity)User.Identity;
            int id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var foundUser = db.Users.FirstOrDefault(user => user.Id == id);
            var root = HttpContext.Current.Server.MapPath("~/Uploads/" + foundUser.Id + "/");
            var fileRoutes = Directory.GetFiles(root).ToList();
            List<string> currentFiles = new List<string>();
            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                var filePath = root + postedFile.FileName;
                currentFiles.Add(filePath);
                if (fileRoutes.Contains(filePath))
                {
                    continue;
                }
                postedFile.SaveAs(filePath);
            }
            fileRoutes.RemoveAll(temp => currentFiles.Contains(temp));
            foreach (var temp in fileRoutes)
            {
                File.Delete(temp);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        [Route("api/users/count")]
        [Authorize(Roles = "user")]
        public IHttpActionResult GetCount()
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            int count = db.Users.Find(id).SentCount;
            return Ok(count);
        }

        [HttpGet]
        [Route("api/users/usa")]
        [Authorize(Roles = "user")]
        public IHttpActionResult SendEmailsUSA()
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var user = db.Users.Find(id);

            HostingEnvironment.QueueBackgroundWorkItem(async token => {
                await Emailer.SendEmailsToUSA(user.Id,user.Email, user.Password, user.Message, user.Topic);
            });

            db.Users.Find(id).Status = "Progress";
            db.SaveChanges();
            return Ok();
        }

        // GET: api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult GetUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Users
        [ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
    }
}