var WebGLUtils = {
  IsMobile: function() {
    var ua = navigator.userAgent || navigator.vendor || "";
    var platform = navigator.platform || "";
    var maxTP = navigator.maxTouchPoints || 0;
    var uaLower = ua.toLowerCase();

    // 一般的なモバイル
    if (/android|iphone|ipod|ipad|webos|blackberry|iemobile|opera mini/i.test(uaLower)) {
      return true;
    }

    // iPadOS 13以降
    if (platform === "MacIntel" && maxTP > 1) {
      return true;
    }

    return false;
  }
};

mergeInto(LibraryManager.library, WebGLUtils);
