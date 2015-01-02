/*
 dhtmlxScheduler.Net v.3.2.0 Professional Evaluation

This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Scheduler.plugin(function(e){e.form_blocks.multiselect={render:function(e){for(var t="<div class='dhx_multi_select_"+e.name+"' style='overflow: auto; height: "+e.height+"px; position: relative;' >",a=0;a<e.options.length;a++)t+="<label><input type='checkbox' value='"+e.options[a].key+"'/>"+e.options[a].label+"</label>",convertStringToBoolean(e.vertical)&&(t+="<br/>");return t+="</div>"},set_value:function(t,a,i,n){function s(e){for(var a=t.getElementsByTagName("input"),i=0;i<a.length;i++)a[i].checked=!!e[a[i].value]
}for(var r=t.getElementsByTagName("input"),d=0;d<r.length;d++)r[d].checked=!1;var o={};if(i[n.map_to]){for(var _=(i[n.map_to]+"").split(","),d=0;d<_.length;d++)o[_[d]]=!0;s(o)}else{if(e._new_event||!n.script_url)return;var l=document.createElement("div");l.className="dhx_loading",l.style.cssText="position: absolute; top: 40%; left: 40%;",t.appendChild(l),dhtmlxAjax.get(n.script_url+"?dhx_crosslink_"+n.map_to+"="+i.id+"&uid="+e.uid(),function(e){for(var a=e.doXPath("//data/item"),i={},r=0;r<a.length;r++)i[a[r].getAttribute(n.map_to)]=!0;
s(i),t.removeChild(l)})}},get_value:function(e){for(var t=[],a=e.getElementsByTagName("input"),i=0;i<a.length;i++)a[i].checked&&t.push(a[i].value);return t.join(",")},focus:function(){}}});