﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FindeyVouchers.Domain;
using FindeyVouchers.Domain.EfModels;
using SendGrid;

namespace FindeyVouchers.Interfaces
{
    public interface IMailService
    {
        Task<Response> SendMail(string receiverAddress, string subject, string body,
            List<SendgridMailAttachment> files);

        Task<Response> SendMail(string receiverAddress, string subject, string body);
        public string GetPasswordForgetEmail(string username, string url);
        string GetVerificationEmail(string username, string url);
        string GetVoucherSoldHtml(CustomerVoucher voucher, string fileName);
        string GetVoucherSoldHtmlBody(string companyName, string htmlVouchers);
        string GetVoucherNoticiationHtml(MerchantVoucher voucher, int count);

        string GetVoucherNotificationHtmlBody(string companyName, string htmlVouchers, decimal totalAmount,
            int totalCount);
    }
}