/*
 dhtmlxScheduler.Net v.3.2.0 Professional Evaluation

This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Scheduler.plugin(function(t){t.config.active_link_view="day",t._active_link_click=function(e){var n=e.target||event.srcElement,i=n.getAttribute("jump_to"),o=t.date.str_to_date(t.config.api_date);return i?(t.setCurrentView(o(i),t.config.active_link_view),e&&e.preventDefault&&e.preventDefault(),!1):void 0},t.attachEvent("onTemplatesReady",function(){var e=function(e,n){n=n||e+"_scale_date",t.templates["_active_links_old_"+n]||(t.templates["_active_links_old_"+n]=t.templates[n]);var i=t.templates["_active_links_old_"+n],o=t.date.date_to_str(t.config.api_date);
t.templates[n]=function(t){return"<a jump_to='"+o(t)+"' href='#'>"+i(t)+"</a>"}};if(e("week"),e("","month_day"),this.matrix)for(var n in this.matrix)e(n);this._detachDomEvent(this._obj,"click",t._active_link_click),dhtmlxEvent(this._obj,"click",t._active_link_click)})});