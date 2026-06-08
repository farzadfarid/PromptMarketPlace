function toggleShowcaseInput(type) {
    document.getElementById('image-input').classList.toggle('d-none', type !== 'Image');
    document.getElementById('text-input').classList.toggle('d-none', type === 'Image');
}
