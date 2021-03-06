﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FindeyVouchers.Domain;
using FindeyVouchers.Domain.EfModels;
using FindeyVouchers.Interfaces;
using FindeyVouchers.Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using Stripe;

namespace FindeyVouchers.Website.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly ICustomerService _customerService;
        private readonly string _endpointSecret;
        private readonly IMerchantService _merchantService;
        private readonly IPaymentService _paymentService;
        private readonly IVoucherService _voucherService;

        public OrderController(IMerchantService merchantService, ICustomerService customerService,
            IPaymentService paymentService, IVoucherService voucherService, IConfiguration configuration)
        {
            _merchantService = merchantService;
            _customerService = customerService;
            _paymentService = paymentService;
            _voucherService = voucherService;
            _apiKey = configuration.GetValue<string>("StripeApiKey");
            _endpointSecret = configuration.GetValue<string>("StripeHookSecret");
        }

        [HttpPost]
        [Route("payment/intent")]
        public IActionResult InitiatePaymentIntent([FromBody] JsonElement body)
        {
            // Set your secret key. Remember to switch to your live secret key in production!
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            try
            {
                StripeConfiguration.ApiKey = _apiKey;
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var response = JsonSerializer.Deserialize<PaymentIntentRequest>(body.ToString(), options);
                var user = _merchantService.GetMerchantInfo(response.CompanyName);
                if (user == null) return NotFound("Merchant not found");

                var createOptions = new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "ideal",
                        "card"
                    },
                    Amount = response.Amount,
                    Currency = "eur",
                    ApplicationFeeAmount = 1,
                    TransferData = new PaymentIntentTransferDataOptions
                    {
                        Destination = user.StripeAccountId
                    }
                };
                var service = new PaymentIntentService();
                var intent = service.Create(createOptions);
                return Ok(new {client_secret = intent.ClientSecret});
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("payment/ideal")]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                // var stripeEvent = EventUtility.ConstructEvent(json,
                //     Request.Headers["Stripe-Signature"], _endpointSecret);
                var stripeEvent = EventUtility.ParseEvent(json, false);

                // Handle the event

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                // Always log all!
                _paymentService.UpdatePayment(new PaymentStatusResponse
                {
                    PaymentId = paymentIntent.Id,
                    Amount = paymentIntent.Amount.Value,
                    Created = paymentIntent.Created,
                    PaymentStatus = paymentIntent.Status,
                    ErrorMessage = paymentIntent.LastPaymentError?.ToString()
                });
                // Only fullfilment when succes
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    await _voucherService.HandleFulfillment(paymentIntent.Id);
                }

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("payment/response")]
        public async Task<IActionResult> FinishOrder([FromBody] JsonElement body)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var response = JsonSerializer.Deserialize<PaymentStatusResponse>(body.ToString(), options);
            _paymentService.UpdatePayment(response);
            if (response.PaymentStatus.Equals("succeeded"))
                await _voucherService.HandleFulfillment(response.PaymentId);

            return Ok();
        }


        [HttpPost]
        [Route("create")]
        public IActionResult CreateOrder([FromBody] JsonElement body)
        {
            // create user
            // create customer voucher for user

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var response = JsonSerializer.Deserialize<CreateOrderRequest>(body.ToString(), options);
            var customer = _customerService.CreateCustomer(response.Customer);
            _paymentService.CreatePayment(new Payment
            {
                Id = response.PaymentId,
                Amount = 0,
                Status = null,
                Created = DateTime.Now
            });

            foreach (var merchantVoucher in response.Vouchers)
                for (var i = 0; i < merchantVoucher.Amount; i++)
                    _voucherService.CreateCustomerVoucher(customer, merchantVoucher, response.PaymentId);


            return Ok();
        }
    }
}