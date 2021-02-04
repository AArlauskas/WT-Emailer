using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using emailer_backend.Models;

namespace emailer_backend.Controllers
{
    public class CodesController : ApiController
    {
        private EmailerEntities db = new EmailerEntities();

        // GET: api/Codes
        public IQueryable<Code> GetCodes()
        {
            return db.Codes;
        }

        [HttpGet]
        [Route("api/codes/overview")]
        [Authorize(Roles = "admin")]
        public IHttpActionResult GetCodesOVerview()
        {
            var codes = db.Codes.Select(code => new
            {
                code.Status,
                Code = code.Personal_Code,
                Email = code.User.Email
            }).ToList();

            return Ok(codes);
        }

        [HttpGet]
        [Route("api/codes/generate")]
        [Authorize(Roles = "admin")]
        public IHttpActionResult GenerateCode()
        {
            string dictionaryString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder resultStringBuilder = new StringBuilder();
            var random = new Random();
            for(int i = 0; i < 16; i++)
            {
                resultStringBuilder.Append(dictionaryString[random.Next(dictionaryString.Length)]);
                if(i == 3 || i == 7 || i == 11)
                {
                    resultStringBuilder.Append("-");
                }
            }

            var result = resultStringBuilder.ToString();

            var code = new Code()
            {
                Personal_Code = result,
                Status = "Created",
            };
            db.Codes.Add(code);
            db.SaveChanges();

            return Ok(code.Personal_Code);
        }
        

        // GET: api/Codes/5
        [ResponseType(typeof(Code))]
        public IHttpActionResult GetCode(int id)
        {
            Code code = db.Codes.Find(id);
            if (code == null)
            {
                return NotFound();
            }

            return Ok(code);
        }

        // PUT: api/Codes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCode(int id, Code code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != code.Id)
            {
                return BadRequest();
            }

            db.Entry(code).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CodeExists(id))
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

        // POST: api/Codes
        [ResponseType(typeof(Code))]
        public IHttpActionResult PostCode(Code code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Codes.Add(code);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = code.Id }, code);
        }

        // DELETE: api/Codes/5
        [ResponseType(typeof(Code))]
        public IHttpActionResult DeleteCode(int id)
        {
            Code code = db.Codes.Find(id);
            if (code == null)
            {
                return NotFound();
            }

            db.Codes.Remove(code);
            db.SaveChanges();

            return Ok(code);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CodeExists(int id)
        {
            return db.Codes.Count(e => e.Id == id) > 0;
        }
    }
}