using emailer_backend.Models;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace emailer_backend.Controllers
{
    public class CheckoutController : ApiController
    {
        private EmailerEntities db = new EmailerEntities();

        [HttpPost]
        [AllowAnonymous]
        [Route("api/checkout/create")]
        public IHttpActionResult Create()
        {
            string dictionaryString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder resultStringBuilder = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < 16; i++)
            {
                resultStringBuilder.Append(dictionaryString[random.Next(dictionaryString.Length)]);
                if (i == 3 || i == 7 || i == 11)
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

            var domain = "http://localhost:3000/checkout";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 1600,
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Code to access emailing system"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = domain + "?success=true&code=" + result,
                CancelUrl = domain + "?success=false"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(session.Id);

        }

    }
}
