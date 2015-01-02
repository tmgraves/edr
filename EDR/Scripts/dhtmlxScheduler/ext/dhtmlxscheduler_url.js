/*
 dhtmlxScheduler.Net v.3.2.0 Professional Evaluation

This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Scheduler.plugin(function(e){e.attachEvent("onTemplatesReady",function(){var t=!0,a=e.date.str_to_date("%Y-%m-%d"),n=e.date.date_to_str("%Y-%m-%d");e.attachEvent("onBeforeViewChange",function(e,i,r,s){if(t){t=!1;for(var d={},o=(document.location.hash||"").replace("#","").split(","),_=0;_<o.length;_++){var l=o[_].split("=");2==l.length&&(d[l[0]]=l[1])}if(d.date||d.mode){try{this.setCurrentView(d.date?a(d.date):null,d.mode||null)}catch(c){this.setCurrentView(d.date?a(d.date):null,r)}return!1}}var h="#date="+n(s||i)+",mode="+(r||e);
return document.location.hash=h,!0})})});