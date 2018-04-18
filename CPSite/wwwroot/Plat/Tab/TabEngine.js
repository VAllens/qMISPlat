//全局传入参数start
//表单唯一标识符
var CPTabGlobal_TabCode = $.CPGetQuery("TabCode"); if (CPTabGlobal_TabCode == null || CPTabGlobal_TabCode == undefined) CPTabGlobal_TabCode = "";
 
//全局传入参数end

//全局变量start 
//全局对象
var CPTabGlobal_TabObj = null;
/**
 *公用函数区start
 */
function TabSetContext(url)
{
    //f增加这个，用来告诉表单，保存后如何刷新原页面
    var tmpUrl = url;
    $("#CPTabFrame").attr("src", CPWebRootPath + tmpUrl);
}
//获取本页面里通过layer产生弹出层的iframe
function CPGetContextFrame() {
    return document.getElementById("CPTabFrame").contentWindow;
}
$(function () {
    var getDataUrl = CPWebRootPath + "/api/TabEngine/GetTabInfo?TabCode=" + CPTabGlobal_TabCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    //为了表达式，需要把URL里其它字段再加上
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "tabcode"
            || tempSarray[0].toLowerCase() == "curuserid"
            || tempSarray[0].toLowerCase() == "curuseriden"
        )
            continue;
        getDataUrl += "&" + sResultArray[mm];
    }
    $.getJSON(getDataUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        var sHtml = "";
        CPTabGlobal_TabObj = data.Tab;
        var defaultUrl = "";
        $.each(CPTabGlobal_TabObj.ItemCol, function (nIndex, nObj) {
            if (nIndex == 0)
            {
                sHtml = "<li class=\"layui-this\">" + nObj.EleTitle + "</li>";
                defaultUrl = nObj.TargetUrl;
            }
            else 
                sHtml += "<li>" + nObj.EleTitle + "</li>";
        });
        TabSetContext(defaultUrl);
        $(".layui-tab-title").html(sHtml);
        layui.use('element', function () {
            var element = layui.element;
            //一些事件监听
            element.on('tab(CPTabBrief)', function (data) {
                var url = "";
                $.each(CPTabGlobal_TabObj.ItemCol, function (nIndex, nObj) {
                    if (nIndex == data.index) {
                        url = nObj.TargetUrl;
                        return;
                    }                   
                });
                TabSetContext(url);
            });
        });
        //设置高度
        var nHeight = $(window).height();
        nHeight = nHeight - $(".layui-tab-title").height()-25;
        $("#CPTabFrame").height(nHeight);
    });
});


