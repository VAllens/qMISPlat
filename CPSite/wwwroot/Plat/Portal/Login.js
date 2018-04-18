//登录方法
function LoginMethod()
{
    var url = CPWebRootPath + "/api/COOrganEngine/Login?LoginName=" + CPTrim($("#username").val()) + "&UserPwd=" + CPTrim($("#password").val()) + "&DeviceType=1";
    $.getJSON(url, function (data) {
        if (data.Result == false)
        {
            alert("登录失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            //alert("登录成功！");;
            url = CPWebRootPath + DefaultUrl;
            window.location.href = url;
        }
    });
}
document.onkeydown = function (e) {
    if (!e) e = window.event; //火狐中是 window.event
    if ((e.keyCode || e.which) == 13) {
        LoginMethod();
    }
}