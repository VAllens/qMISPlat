//定义是框架菜单
var Globa_IsCPMenuFrame = "true";
//JavaScript代码区域
layui.use('element', function () {
    var element = layui.element;

});
//获取本页面里通过layer产生弹出层的iframe
function CPGetContextFrame() {
    return document.getElementById("CPFrameMenu").contentWindow;
}
//设定左侧菜单滚动条
function CPFrameSetModuleScroll()
{
    $(".gdt").mCustomScrollbar({
        theme: "minimal-dark"
    })
    $('.all-nav .item').hover(function () {
        var childIsLoad = $(this).attr("data-childIsLoad");
        if (childIsLoad == "false") {
            //未加载
            var thisObjTmp = this;
            var moduleId = $(this).attr("data-moduleid");
            var modulename = $(this).attr("data-modulename");
            var iconParent = $(this).attr("data-icon");
            var url = CPWebRootPath + "/api/CPModuleEngine/GetChildModule?SysId=1&ParentModuleId=" + moduleId + "&IsLoadChildModule=true&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
            //再加其它参数
            var sResultArray = CPGetQueryString();
            for (var mm = 0; mm < sResultArray.length; mm++) {
                var tempSarray = sResultArray[mm].split('=');
                if (tempSarray[0].toLowerCase() == "sysid"
                    || tempSarray[0].toLowerCase() == "parentmoduleid"
                    || tempSarray[0].toLowerCase() == "isloadchildmodule"
                    || tempSarray[0].toLowerCase() == "curuserid"
                    || tempSarray[0].toLowerCase() == "curuseriden"
                )
                    continue;
                url += "&" + sResultArray[mm];
            }
            $.getJSON(url, function (data) {
                if (data.Result == false) {
                    alert(data.ErrorMsg);
                    return false;
                }
                $(thisObjTmp).attr("data-childIsLoad", "true");
                var sHTML = "";
                if (data.ModuleCol != null && data.ModuleCol.length > 0) {
                    sHTML += "<div class=\"product-wrap pos03\">";
                    sHTML += " <h2>";
                    sHTML += "    <i class=\"icon iconfont " + iconParent + "\"></i> " + modulename;
                    sHTML += " </h2>";
                    sHTML += " <div class=\"gdt\" style=\"height:90%;\">";
                    sHTML += "    <ul class=\"list-wrap\">";
                }
                $.each(data.ModuleCol, function (nIndex, nObj) {
                    var icon = "icon-businesscard_fill";
                    if (nObj.Icon != null && nObj.Icon != "")
                        icon = nObj.Icon; 
                    if (nObj.ChildModule != null && nObj.ChildModule.length > 0) {
                        //有子菜单
                        sHTML += "<li  class=\"ng-scope\">";
                        sHTML += "        <p style=\"background-color:#e7e7e7\">";
                        sHTML += "            <span class=\"icon iconfont " + icon + "\"  style=\"font-size:20px;vertical-align: middle;\"></span>";
                        sHTML += "           <span class=\"level-title\">" + nObj.ModuleName + "</span>";
                        sHTML += "          <span class=\"icon iconfont icon-unfold right\"></span>";
                        sHTML += "        </p>";
                        //子菜单start
                        sHTML += "<ul>";
                        $.each(nObj.ChildModule, function (nChildIndex, nChildObj) {
                            var iconChild = "icon-businesscard_fill";
                            if (nChildObj.Icon != null && nChildObj.Icon != "")
                                iconChild = nChildObj.Icon;
                            sHTML += "  <li id=\"CPModule_" + nObj.Id + "\"  onclick=\"CPFrameAddTab('" + nChildObj.ModuleName + "','" + nChildObj.ModuleUrl + "','" + nChildObj.Id + "'," + nChildObj.OpenType + "); \" >";
                            sHTML += "             <p >";
                            sHTML += "            <span class=\"icon iconfont " + iconChild + "\" style=\"font-size:20px;vertical-align: middle;\"></span>";
                            sHTML += "                 <span class=\"level-title ng-binding\">" + nChildObj.ModuleName + "</span>";
                            sHTML += "             </p>";
                            sHTML += "         </li>";
                        });
                        sHTML += "</ul>";
                        //子菜单end
                        sHTML += "   </li>";
                    }
                    else {
                        //没有子菜单
                        sHTML += "<li   id=\"CPModule_" + nObj.Id + "\"  onclick=\"CPFrameAddTab('" + nObj.ModuleName + "','" + nObj.ModuleUrl + "','" + nObj.Id + "'," + nObj.OpenType + "); \" >";
                        sHTML += "        <p >";
                        sHTML += "            <span class=\"icon iconfont " + icon + "\" style=\"font-size:20px;vertical-align: middle;\"></span>";
                        sHTML += "           <span class=\"level-title ng-binding\">" + nObj.ModuleName + "</span>";
                        sHTML += "        </p>";
                        sHTML += "   </li>";
                    }
                });
                if (data.ModuleCol != null && data.ModuleCol.length > 0) {
                    sHTML += "</ul></div></div>";
                } 
                $("#CPModule_" + moduleId).append(sHTML);
                $(".gdt").mCustomScrollbar({
                    theme: "minimal-dark"
                });
                $(".product-wrap").css("left", "95px");
                $(thisObjTmp).addClass('active').find('s').hide();
                $(thisObjTmp).find('.product-wrap').show();
            });
        }
        else {
            $(this).addClass('active').find('s').hide();
            $(this).find('.product-wrap').show();
        }
        //$(window).load(function () {
        //    $("#ccc").mCustomScrollbar();
        //});
    }, function () {
        $(this).removeClass('active').find('s').show();
        $(this).find('.product-wrap').hide();
    })
    $(".product-wrap").hide();
    $(".product-wrap").css("left", "95px");

    // var menuSlide= true;
    $(".product-wrap li p").unbind('click').click(function () {
        event.stopPropagation();
        if ($(this).siblings().length == 0)
            return 
        var menuSlide = $(this).siblings("ul").css("display") == "none" ? false : true;

        if (menuSlide) {
            $(this).siblings("ul").slideUp(200);
            $(this).find(".fa-angle-down").removeClass('fa-angle-down').addClass("fa-angle-left")
        } else {
            $(this).siblings("ul").slideDown(200);
            $(this).find(".fa-angle-left").removeClass("fa-angle-left").addClass('fa-angle-down')
        }
    })
    // $(".dropdown-menu").hover
    $(".navbar-li").hover(function () {
        $(this).find('.dropdown-menu').slideDown(100);
    }, function () {
        $(this).find('.dropdown-menu').slideUp(100);
    })
}
$(function () {
    //CPFrameSetModuleScroll();
    var url = CPWebRootPath + "/api/CPModuleEngine/GetChildModule?SysId=1&ParentModuleId=-1&IsLoadChildModule=true&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "sysid"
            || tempSarray[0].toLowerCase() == "parentmoduleid"
            || tempSarray[0].toLowerCase() == "isloadchildmodule"
            || tempSarray[0].toLowerCase() == "curuserid"
            || tempSarray[0].toLowerCase() == "curuseriden"
        )
            continue;
        url += "&" + sResultArray[mm];
    }
    $.getJSON(url, function (data) {
        if (data.Result == false)
        {
            alert(data.ErrorMsg);
            return false;
        }
        var sHTML = "";
        $.each(data.ModuleCol, function (nIndex, nObj) {
            var icon = "icon-businesscard_fill";
            if (nObj.Icon != null && nObj.Icon != "")
                icon = nObj.Icon;
            var onclickS = ""; 
            if (nObj.ChildModule == null || nObj.ChildModule.length <= 0)
            {
                onclickS = " onclick=\"CPFrameAddTab('" + nObj.ModuleName + "','" + nObj.ModuleUrl + "','" + nObj.Id + "'," + nObj.OpenType + "); \" ";
            }
            sHTML += " <div class=\"item\" style=\"cursor:pointer;\" " + onclickS + "  id=\"CPModule_" + nObj.Id + "\" data-childIsLoad=\"false\" data-moduleid=\"" + nObj.Id + "\" data-modulename=\"" + nObj.ModuleName + "\" data-icon=\"" + icon + "\">";
            sHTML += "     <div class=\"product\">";
            sHTML += "        <a target=\"_blank\">";
            sHTML += "            <h1>";
            sHTML += "                <i class=\"icon iconfont " + icon + "\" style=\"font-size:40px;\"></i>";
            sHTML += "            </h1>";
            sHTML += "            " + nObj.ModuleName;
            sHTML += "        </a>";
            sHTML += "    </div>";
            sHTML += "</div>";
            if (nIndex == 0)
            {
                Global_CPFrameCurModuleId = nObj.Id;
                CPFrameAddTab(nObj.ModuleName, nObj.ModuleUrl, nObj.Id, nObj.OpenType);
            }
        });
        $("#LeftMenuContainer").append(sHTML);
        CPFrameSetModuleScroll();
        layui.use('element', function () {
            var element = layui.element;
            //一些事件监听
            element.on('tab(CPFrameModuleTab)', function (dataTab) {
               
            });
        });
    });
    //设置右侧高度

    $(".layui-tab-content").height($(window).height() - $("#CPFrameTopNav").height() - $(".leaf-main-content-content").height()-8);
})
//记录当前点击的模块ID
var Global_CPFrameCurModuleId;
function CPFrameInnerTabClick(moduleId)
{
    Global_CPFrameCurModuleId = moduleId;
}
//添加内置框架页签
function CPFrameAddTab(tabTitle, tabUrl, tabModuleId,openType)
{
    if (openType == 1)
    {
        if ($("li[lay-id='" + tabModuleId + "']").length <= 0) {
            if (tabUrl == null || tabUrl == "") {
                tabUrl = "about:_blank;";
            }
            else {
                tabUrl = CPUrlAddWebRootPath(tabUrl);
            }
            $(".layui-tab-title").append("<li lay-id=\"" + tabModuleId + "\" onclick=\"CPFrameInnerTabClick(" + tabModuleId+ ")\">" + tabTitle + "</li>");
            var sContext = "<div class=\"layui-tab-item\">";
            sContext += "<iframe scrolling=\"auto\" id=\"CPModuleIFrame_" + tabModuleId + "\" class=\"\" frameborder=\"0\" style=\"width:100%;height: " + $(".layui-tab-content").height() + "px; z - index:999999\"";
            sContext += " src=\"" + tabUrl + "\" ></iframe></div>";
            //console.log(sContext);
            $(".layui-tab-content").append(sContext);
            layui.element.init();
        }
        layui.element.tabChange("CPFrameModuleTab", tabModuleId);
        Global_CPFrameCurModuleId = tabModuleId;
    }
    else if (openType == 2)
    {
        if (tabUrl == null || tabUrl == "") {
            tabUrl = "about:_blank;";
        }
        else {
            tabUrl = CPUrlAddWebRootPath(tabUrl);
            
        }
        window.open(tabUrl);
    }
   
}
//注销
function LoginOut()
{

    window.location.href = CPWebRootPath +  "/Plat/Portal/LoginOut";
}
 //定义是框架菜单
var Globa_IsCPMenuFrame = "true";
//获取本页面里通过layer产生弹出层的iframe
function CPGetContextFrame() {
    return document.getElementById("CPModuleIFrame_" + Global_CPFrameCurModuleId).contentWindow;
}
//点击头像
function UserPhotoClick(userId)
{
    var url = "/Plat/Tab/TabView?TabCode=Tab201711051943150006&UserId=" + userId;
    var moduleId = -1;
    CPFrameAddTab("个人信息维护", url, moduleId, 1);
}