// Write your Javascript code.
"use strict";

function ShowTip_Error(message) {
    ShowTips(message, "error");
}
function ShowTip_Success(message) {
    ShowTips(message, "success");
}
//tipType:info,success,warning,error
function ShowTips(message, tipType) {
    alert(message);
}

function PostApi(btnDocElement, url, data, successFun) {
    var btn = $(btnDocElement);

    btn.attr("disabled", "disabled");

    $.post(url, data)
        .success(successFun)
        .error(function() {
            ShowTip_Error("��������ǰ�����Ƿ�������");
        })
        .complete(function() {
            btn.removeAttr("disabled");
        });
}

/**
 *  ȫ�ֽӿڽ���Ƿ�ɹ���֤
 * @returns  false ʧ�ܣ�true �ɹ�
 */
object.prototype.isRetOk= function() {
    var self = this;
    if (!self.ret && self.ret != 0) {
        return false;
    }
    return true;
}