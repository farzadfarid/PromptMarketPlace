function toggleShowcaseInput(type) {
    var isImage = type === 'Image';
    var isMedia = type === 'Video' || type === 'Audio';
    document.getElementById('image-input').classList.toggle('d-none', !isImage);
    document.getElementById('media-url-input').classList.toggle('d-none', !isMedia);
    document.getElementById('text-input').classList.toggle('d-none', isImage || isMedia);

    if (isMedia) {
        var label = document.getElementById('media-url-label');
        var hint = document.getElementById('media-url-hint');
        var field = document.getElementById('media-url-field');
        if (type === 'Video') {
            if (label) label.textContent = 'لینک ویدیو';
            if (hint) hint.textContent = 'لینک مستقیم فایل mp4 یا آدرس CDN';
            if (field) field.placeholder = 'https://example.com/video.mp4';
        } else {
            if (label) label.textContent = 'لینک فایل صوتی';
            if (hint) hint.textContent = 'لینک مستقیم فایل mp3 یا ogg';
            if (field) field.placeholder = 'https://example.com/audio.mp3';
        }
    }
}
