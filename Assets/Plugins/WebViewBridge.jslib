// JavaScript bridge for WebView communication
// This file should be placed in Assets/Plugins/

mergeInto(LibraryManager.library, {
    SendMessageToJS: function (eventType, data) {
        var eventTypeStr = UTF8ToString(eventType);
        var dataStr = UTF8ToString(data);
        
        // Send message to host mobile app via window.postMessage
        if (window.unityWebViewBridge && typeof window.unityWebViewBridge.sendMessage === 'function') {
            window.unityWebViewBridge.sendMessage(eventTypeStr, dataStr);
        } else {
            console.log('[Unity WebView Bridge] Message:', eventTypeStr, dataStr);
        }
    }
});
