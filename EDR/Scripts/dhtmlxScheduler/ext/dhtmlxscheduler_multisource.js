/*
 dhtmlxScheduler.Net v.3.2.0 Professional Evaluation

This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Scheduler.plugin(function(e){!function(){function t(e){var t=function(){};return t.prototype=e,t}var a=e._load;e._load=function(e,i){if(e=e||this._load_url,"object"==typeof e)for(var n=t(this._loaded),s=0;s<e.length;s++)this._loaded=new n,a.call(this,e[s],i);else a.apply(this,arguments)}}()});