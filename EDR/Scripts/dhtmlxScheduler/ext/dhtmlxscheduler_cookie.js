/*
 dhtmlxScheduler.Net v.3.2.0 Professional Evaluation

This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Scheduler.plugin(function(e){!function(){function t(e,t,i){var n=e+"="+i+(t?"; "+t:"");document.cookie=n}function i(e){var t=e+"=";if(document.cookie.length>0){var i=document.cookie.indexOf(t);if(-1!=i){i+=t.length;var n=document.cookie.indexOf(";",i);return-1==n&&(n=document.cookie.length),document.cookie.substring(i,n)}}return""}var n=!0;e.attachEvent("onBeforeViewChange",function(s,a,r,d){var o=(e._obj.id||"scheduler")+"_settings";if(n){n=!1;var l=i(o);if(l){e._min_date||(e._min_date=d),l=unescape(l).split("@"),l[0]=this.templates.xml_date(l[0]);
var h=this.isViewExists(l[1])?l[1]:r,_=isNaN(+l[0])?d:l[0];return window.setTimeout(function(){e.setCurrentView(_,h)},1),!1}}var c=escape(this.templates.xml_format(d||a)+"@"+(r||s));return t(o,"expires=Sun, 31 Jan 9999 22:00:00 GMT",c),!0});var s=e._load;e._load=function(){var t=arguments;if(!e._date&&e._load_mode){var i=this;window.setTimeout(function(){s.apply(i,t)},1)}else s.apply(this,t)}}()});