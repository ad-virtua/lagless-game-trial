var WebGLUtils = {
  // IsMobileという名前で関数を定義
  IsMobile: function() {
    var ua = navigator.userAgent || navigator.vendor || window.opera || "";
    var platform = navigator.platform || "";
    var maxTP = navigator.maxTouchPoints || 0;

    // UAを小文字に変換しておく
    var uaLower = ua.toLowerCase();

    // 一般的なモバイルデバイス（Android / iPhone / iPod / 旧iPadなど）
    if (/android|iphone|ipod|ipad|webos|blackberry|iemobile|opera mini/i.test(uaLower)) {
      return true;
    }

    // iPadOS 13以降ではUAが "Macintosh" になるケースを補正
    // Macっぽいがタッチ可能な場合はiPadとみなす
    if (platform === "MacIntel" && maxTP > 1) {
      return true;
    }

    // その他はPC扱い
    return false;
  }
};

// mergeIntoを使ってUnity側から呼べるように登録
mergeInto(LibraryManager.library, WebGLUtils);
