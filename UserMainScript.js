var KR = KR || {};
KR.SystemUser = KR.SystemUser || {};

KR.SystemUser.Form = {
	EmailFilterSubGrid: function () {
		debugger;
		var formLabel = Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
		var formType = Xrm.Page.ui.getFormType();
		if (formType != 1 && formLabel == "User") {
			var emailFilterSubGrid = function () {
				debugger;
				var objSubGrid = window.parent.document.getElementById("EmailSubGrid");

				if (objSubGrid == null) {
					setTimeout(emailFilterSubGrid, 2000);
					return;
				}
				debugger;
				var userId = Xrm.Page.data.entity.getId();

				var fetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">\
								  <entity name="email">\
									<attribute name="subject" />\
									<attribute name="regardingobjectid" />\
									<attribute name="from" />\
									<attribute name="to" />\
									<attribute name="prioritycode" />\
									<attribute name="statuscode" />\
									<attribute name="modifiedon" />\
									<attribute name="activityid" />\
									<order attribute="subject" descending="false" />\
									<filter type="and">\
									  <condition attribute="ownerid" operator="eq" uitype="systemuser" value="'+ userId +'" />\
									  <condition attribute="statecode" operator="eq" value="1" />\
									</filter>\
									<link-entity name="activitymimeattachment" from="objectid" to="activityid" link-type="inner" alias="ap" />\
								  </entity>\
								</fetch>';
				if (objSubGrid.control != null){
					
					objSubGrid.control.SetParameter("fetchXml", fetchXml);
					objSubGrid.control.refresh();
				}
				else setTimeout(emailFilterSubGrid, 500);
			}
			emailFilterSubGrid();
		}        
    },
}
