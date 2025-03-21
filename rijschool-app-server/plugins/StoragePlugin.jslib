mergeInto(LibraryManager.library, {
    GetStoredCode: function() {
        var code = localStorage.getItem('rijschoolAppCode');
        var bufferSize = lengthBytesUTF8(code) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(code, buffer, bufferSize);
        return buffer;
    }
}); 