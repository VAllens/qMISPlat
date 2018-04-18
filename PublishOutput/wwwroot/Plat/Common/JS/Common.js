///获取querystring的值
jQuery.CPGetQuery = function (name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");//不区分大小写 
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return unescape(r[2]);
    return null;
}
//替换模态对话框的方法，即替换showModalDialog，
var CPShowModalDialogInputArgs;//用来存储原showModalDialog通过object参数传入的值
var CPShowModalDialogReturnArgs;//用来存储原showModalDialog通过returnValue返回的值
//在本页面打开模态对话框，由于$.这个关键字符与表达式引擎冲突，所以在配置里可以调用下面这个方法
function CPOpenDialog(url, name, nWidth, nHeight) {
    OpenNewModel(url, name, nWidth, nHeight);

}
//在本页面打开模态对话框
jQuery.CPShowModalDialog = function (url, title, nWidth, nHeight) {
    // OpenNewWindow(url, title, nWidth, nHeight);
    CPOpenDialog(url, title, nWidth, nHeight);

}
//获取页面地址前http://localtion:9000部分
function GetLocatoinProtocolAndHost() {
    return window.location.protocol + "//" + window.location.host;
}
function CPGetQueryString() {
    var result = location.search.match(new RegExp("[\?\&][^\?\&]+=[^\?\&]+", "g"));
    if (result == null)
        return new Array();
    for (var i = 0; i < result.length; i++) {
        result[i] = result[i].substring(1);
    }
    return result;
}
//供使用者调用  
function CPTrim(s) {
    return CPTrimLeft(CPTrimRight(s));
}
//去掉左边的空白  
function CPTrimLeft(s) {
    if (s == null) {
        return "";
    }
    var whitespace = new String(" \t\n\r");
    var str = new String(s);
    if (whitespace.indexOf(str.charAt(0)) != -1) {
        var j = 0, i = str.length;
        while (j < i && whitespace.indexOf(str.charAt(j)) != -1) {
            j++;
        }
        str = str.substring(j, i);
    }
    return str;
}

//去掉右边的空白 www.2cto.com   
function CPTrimRight(s) {
    if (s == null) return "";
    var whitespace = new String(" \t\n\r");
    var str = new String(s);
    if (whitespace.indexOf(str.charAt(str.length - 1)) != -1) {
        var i = str.length - 1;
        while (i >= 0 && whitespace.indexOf(str.charAt(i)) != -1) {
            i--;
        }
        str = str.substring(0, i + 1);
    }
    return str;
}
//判断当前页面地址是否是平台本身内置的页面
function CPUrlAddWebRootPath(url)
{
    var tmpUrl = url;
    tmpUrl = tmpUrl.toLowerCase();
    var tmpWebRootPath = CPWebRootPath;
    tmpWebRootPath = tmpWebRootPath.toLowerCase();
    //自动添加，解决生产环境和开发环境的问题
    if (tmpUrl.indexOf(tmpWebRootPath) == -1) {
        if (tmpUrl.indexOf("/plat/") != -1 || tmpUrl.indexOf("/style/aibabaicon/selicon.html") != -1) {
            url = CPWebRootPath + url;
        }
    }
    return url;
}
//关闭弹出层
function CloseNewModel()
{
    layer.close(layer.index);
    //不知道为什么，调用上述方法时，不会触发事件，所以单独调用下吧
    if (Global_CPCallbackRefreshMethod != null && Global_CPCallbackRefreshMethod != undefined && Global_CPCallbackRefreshMethod != "") {
        eval(Global_CPCallbackRefreshMethod);
        Global_CPCallbackRefreshMethod = "";
    }
}
//打开弹出层，页面上必须引用layer的弹出层组件和jquery
function OpenNewModel(url, title, nWidth, nHeight) {
    var sWidth = $(window).width();
    var sHeight = $(window).height();
    if (Number(nWidth) >= sWidth) {
        nWidth = sWidth - 20;
    }
    if (Number(nHeight) >= sHeight) {
        nHeight = sHeight - 10;
    }
    //自动添加，解决生产环境和开发环境的问题
    url = CPUrlAddWebRootPath(url);
    //弹出一个iframe层

    layer.open({
        type: 2,
        title: title,
        maxmin: false,
        shadeClose: false, //不允许点击外面关闭
        area: [nWidth.toString() + 'px', nHeight.toString() + 'px'],
        content: url,//[url, 'no']
        cancel: function (index, layero) {
            //if (confirm('确定要关闭么')) { //只有当点击confirm框的确定时，该层才会关闭
            //    layer.close(index);
            //    if (Global_CPCallbackRefreshMethod != "") {
            //        eval(Global_CPCallbackRefreshMethod);
            //        Global_CPCallbackRefreshMethod = "";
            //    }
            //}
            layer.close(index);
            //console.log(Global_CPCallbackRefreshMethod);
            if (Global_CPCallbackRefreshMethod != null && Global_CPCallbackRefreshMethod != undefined && Global_CPCallbackRefreshMethod != "") {
                eval(Global_CPCallbackRefreshMethod);
                Global_CPCallbackRefreshMethod = "";
            }
            return true;
        }
    })  ;
}
//全局变量，存储当前要回调刷新的方法
var Global_CPCallbackRefreshMethod = "";
//为解决打开弹出层，要刷新原页面的问题，增加此参数
function FormatUrlForOpenNewModel(url, CPPageIsOpenType, RefreshMethod)
{
    if (RefreshMethod == "")
        return url;
    if (CPPageIsOpenType == "top")
    {
        //TOP
        
        var bHasParent = true;
        if (parent.window.location.href == window.location.href) {
            //没有父页面
            bHasParent = false;
        }
        else
            bHasParent = true;
        if (bHasParent) {
            //第二级
            RefreshMethod = "CPGetContextFrame()." + RefreshMethod;
            bHasParent = false;

            if (parent.window.location.href != parent.parent.window.location.href) {
                bHasParent = true;
            }
            if (bHasParent) {
                //第三级
                RefreshMethod = "CPGetContextFrame()." + RefreshMethod;
                bHasParent = false;
                if (parent.parent.window.location.href != parent.parent.parent.window.location.href) {
                    bHasParent = true;
                }
                if (bHasParent) {
                    //第四级
                    RefreshMethod = "CPGetContextFrame()." + RefreshMethod;
                    bHasParent == false;
                    //暂时支持这么多了。
                }
            }
        }
        top.Global_CPCallbackRefreshMethod = RefreshMethod;
        //TOP
    }
    else if (CPPageIsOpenType == "parent")
    {
        RefreshMethod = "CPGetContextFrame()." + RefreshMethod;
        parent.Global_CPCallbackRefreshMethod = RefreshMethod;
       
    }
    else if (CPPageIsOpenType == "self")
    {
        Global_CPCallbackRefreshMethod = RefreshMethod;

    }
    else {
        Global_CPCallbackRefreshMethod = RefreshMethod;
    }
    return url;
}