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
    var iTop = (window.screen.availHeight - 30 - nHeight) / 2;       //获得窗口的垂直位置;
    var iLeft = (window.screen.availWidth - 10 - nWidth) / 2;           //获得窗口的水平位置;
    var name = "新窗口_" + Math.random();

    //IE9下面不能加name
    window.open(url, '', "height=" + nHeight + ", width=" + nWidth + ",top=" + iTop + ",left=" + iLeft + ", location=yes,menubar=no,resizable=yes,scrollbars=yes,status=no,toolbar=no"); //写成一行

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
    for (var i = 0; i < result.length; i++) {
        result[i] = result[i].substring(1);
    }
    return result;
}