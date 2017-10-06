var KR = KR || {};
KR.Contact = KR.Contract || {};

KR.Contact.Ribbon = {
	
	SendEmail: function(){
	
		var team = Xrm.Page.getAttribute("kr_teamid");
		if(team != null && team.getValue() != null){				
			var sendEmailResponse = KR.Contact.Ribbon.SendEmailAction();
			var message = "";
			var level = "";		
		
			if(sendEmailResponse.search("CRM") == 1){
				message = "Email" + sendEmailResponse + " sent.";
				level = "INFO";
			}
			else{
				message = "Error: " + sendEmailResponse;
				level = "ERROR";
			}
			
			Xrm.Page.ui.setFormNotification(message, level, "0B9EB194-C144-4A8D-B8A2-183DA6D7F816");
		}
		else alert("Please fill Responsible work group");
	},


	SendEmailEnableRule: function (){		
		var formLabel = Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
		var formType = Xrm.Page.ui.getFormType();
		if (formType != 1 && KR.Contact.Ribbon.CheckUserRoles() && formLabel == "Contact") {
			return false;
		}
		return true;
	},
	
	CheckUserRoles: function(){		
		var countOfRoles = 0;
		var roles = Xrm.Page.context.getUserRoles();
		if(roles.length >= 2){
			roles.forEach(function(roleId){			
				var roleName = KR.Contact.Ribbon.GetRoleNameById(roleId);			
				if(roleName == "System Administrator" || roleName == "System Customizer") countOfRoles++;
			});	
			if(countOfRoles == 2) return true;	
		}
		return false;
	},
	GetRoleNameById: function(roleId){
		var roleName = "";
		var requestUrl = KR.Contact.Ribbon.GetClientUrl(roleId);  

		var retrieveReq = new XMLHttpRequest();
		retrieveReq.open("GET", requestUrl, false);
		retrieveReq.setRequestHeader("Accept", "application/json");
		retrieveReq.setRequestHeader("Content-Type", "application/json;charset=utf-8");
		retrieveReq.onreadystatechange = function () {		
			if (retrieveReq.readyState == 4) {
				if (retrieveReq.status == 200) {
					var retrieved = JSON.parse(retrieveReq.responseText).d;			
					roleName = retrieved.results[0].Name;
				}
			}
		};
		retrieveReq.send();
		return roleName;   
	},
	
	GetClientUrl: function(roleId) {
		if (typeof Xrm.Page.context == "object") {
			clientUrl = Xrm.Page.context.getClientUrl();
		}
		var ServicePath = "";
		if(roleId == "")  ServicePath = "/XRMServices/2011/Organization.svc/web";
		else ServicePath = "/XRMServices/2011/OrganizationData.svc/RoleSet?$select=Name&$filter=RoleId eq guid'" + roleId + "'";
		
		return clientUrl + ServicePath;
	},
	
	SendEmailAction: function() {    
		var ContactId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");;	
		var response = "Email Send Action don't give success results";
		var requestXML =
			"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">\
				<s:Body>\
				   <Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">\
					 <request xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\">\
					   <a:Parameters xmlns:b=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">\
						 <a:KeyValuePairOfstringanyType>\
						   <b:key>ContactId</b:key>\
						   <b:value i:type=\"c:string\" xmlns:c=\"http://www.w3.org/2001/XMLSchema\">" + ContactId + "</b:value>\
						  </a:KeyValuePairOfstringanyType>\
					   </a:Parameters>\
					   <a:RequestId i:nil=\"true\" />\
					   <a:RequestName>kr_SendEmailAction</a:RequestName>\
					 </request>\
				   </Execute>\
				 </s:Body>\
			   </s:Envelope>";

		var req = new XMLHttpRequest();
		req.open("POST", KR.Contact.Ribbon.GetClientUrl(""), false)
		req.setRequestHeader("Accept", "application/xml, text/xml, */*");
		req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
		req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");		
		req.send(requestXML);		

		if (req.status == 200 && req.responseXML != null) {
			if (req.responseXML.getElementsByTagName('b:value') != null) {
				var responseValue = req.responseXML.getElementsByTagName('b:value');
				if (responseValue.length > 0 && responseValue.item(0) != null) {
					response = responseValue.item(0).textContent;                
				}
			}
		}
		return response;
	}
}





