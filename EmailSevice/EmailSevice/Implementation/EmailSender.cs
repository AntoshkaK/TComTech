using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSevice.Implementation
{   
    public class EmailSender
    {
        //hardcode! this information can be saved in new Entity with crm settings
        private const string crmUrl = "https://kraltd.crm4.dynamics.com";

        private IOrganizationService _service;               

        public EmailSender(IOrganizationService service)
        {
            _service = service;                 
        }      

        public string SendEmail(Guid contactId, Guid userFromId)
        {
            string response = string.Empty;
            try
            {
                var contact = _service.Retrieve("contact", contactId, new ColumnSet(new string[] { "kr_teamid", "fullname" }));
                if (contact != null && contact.Contains("kr_teamid") && contact.Contains("fullname"))
                {
                    var users = GetUsersFromTeam(contact.GetAttributeValue<EntityReference>("kr_teamid").Id);
                    if(users.Count > 0)
                    {
                        response = CreateEmail(users, userFromId, contact);
                    }
                    else response = "Dont found user from current team";
                }
                else response = "Contact not found or Team is empty";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return response;           
        }
        private string CreateEmail(List<Entity> usersTo, Guid userFromId, Entity contact)
        {
            ActivityParty fromParty = new ActivityParty
            {
                PartyId = new EntityReference("systemuser", userFromId)
            };   

            List<ActivityParty> toParty = new List<ActivityParty>();
            foreach(var user in usersTo)
            {                
                toParty.Add( new ActivityParty
                {
                    PartyId = new EntityReference(user.LogicalName, user.Id)
                });
            }
            
            Email email = new Email
            {
                To = toParty,
                From = new ActivityParty[] { fromParty },   
                Description = string.Format("Contact Full Name: {0} \n Contact link: {1}", contact.GetAttributeValue<string>("fullname"),
                    crmUrl + "/main.aspx?etn=contact&pagetype=entityrecord&id=%7B" +
                        contact.Id.ToString().Replace("{",string.Empty).Replace("}", string.Empty) + "%7D"),
                DirectionCode = true,
            };
            var emailId = _service.Create(email);
            CoppyContactAttachmentsToEmail(contact.Id, emailId);

            SendEmailRequest sendEmailreq = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };

            SendEmailResponse sendEmailresp = (SendEmailResponse)_service.Execute(sendEmailreq);
            return sendEmailresp.Subject;
        }
        private void CoppyContactAttachmentsToEmail(Guid contactId, Guid emailId)
        {
            List<Entity> attachments = new List<Entity>();
            var queryByAttribute = new QueryByAttribute(Annotation.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Attributes = { "objectid" },
                Values = { contactId }
            };
            var response = _service.RetrieveMultiple(queryByAttribute);
            if (response != null && response.Entities.Count > 0 )
            {
                attachments = response.Entities.ToList<Entity>();
                foreach(var attachment in attachments)
                {
                    ActivityMimeAttachment attachmentNew = new ActivityMimeAttachment()
                    {
                        Subject = attachment.Contains("subject") ? attachment.GetAttributeValue<string>("subject") : string.Empty,
                        FileName = attachment.Contains("filename") ? attachment.GetAttributeValue<string>("filename") : string.Empty,
                        Body = attachment.Contains("documentbody") ? attachment.GetAttributeValue<string>("documentbody") : string.Empty,
                        MimeType = attachment.GetAttributeValue<string>("mimetype"),
                        AttachmentNumber = 1,
                        ObjectId = new EntityReference(Email.EntityLogicalName, emailId),
                        ObjectTypeCode = Email.EntityLogicalName


                    };
                    _service.Create(attachmentNew);
                }
            }
        }
        private List<Entity> GetUsersFromTeam(Guid teamId)
        {
            List<Entity> users = new List<Entity>();
            QueryExpression query = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("systemuserid", "teammembership", "systemuserid", "systemuserid", JoinOperator.Inner)
                    {
                        LinkEntities =
                        {
                            new LinkEntity("teammembership", "team", "teamid", "teamid", JoinOperator.Inner)
                            {
                                LinkCriteria = new FilterExpression()
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("teamid", ConditionOperator.Equal, teamId)
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var usersResponse  = _service.RetrieveMultiple(query);
            if (usersResponse != null && usersResponse.Entities.Count > 0) users = usersResponse.Entities.ToList<Entity>();
            return users;
        }
        public void UpdatLetterCountForUser(Guid emailId)
        {
            var email = _service.Retrieve("email", emailId, new ColumnSet(new string[] { "from" }));
            if (email != null && email.Contains("from"))
            {                
                foreach(var userFrom in email.GetAttributeValue<EntityCollection>("from").Entities)
                {
                    var user = _service.Retrieve("systemuser", userFrom.GetAttributeValue<EntityReference>("partyid").Id, new ColumnSet("kr_lettercount"));
                    if (user != null)
                    {
                        var letterCountOld = user.Contains("kr_lettercount") ? user.GetAttributeValue<int>("kr_lettercount") : 0;
                        Entity userForUpdate = new Entity("systemuser");
                        userForUpdate.Id = user.Id;
                        userForUpdate["kr_lettercount"] = letterCountOld + 1;

                        _service.Update(userForUpdate);
                    }
                }                
            }
        }
    }
}
