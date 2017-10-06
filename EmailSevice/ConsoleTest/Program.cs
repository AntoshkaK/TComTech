using EmailSevice.Implementation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new CrmConnection("Crm");
            var _service = new OrganizationService(connection);


            EmailSender emailSender = new EmailSender(_service);            
            //emailSender.SendEmail(new Guid("7AEEC295-A6A9-E711-A826-000D3A2A3E08"), new Guid("E34F105F-C4A9-E711-A82D-000D3A2654F3"));

            var email = _service.Retrieve("email", new Guid("E9B7BE10-61AA-E711-A826-000D3A2A3E08"), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));


        }
    }
}
