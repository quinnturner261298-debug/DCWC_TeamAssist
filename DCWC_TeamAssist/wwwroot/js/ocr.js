// OCR functionality using Tesseract.js
window.ocrHelper = {
    processImage: async function (imageDataUrl, dotNetHelper) {
        try {
            // Show progress updates
            const worker = await Tesseract.createWorker('eng', 1, {
                logger: m => {
                    if (m.status === 'recognizing text') {
                        dotNetHelper.invokeMethodAsync('UpdateProgress', Math.round(m.progress * 100));
                    }
                }
            });

            const { data: { text } } = await worker.recognize(imageDataUrl);
            await worker.terminate();

            return text;
        } catch (error) {
            console.error('OCR Error:', error);
            return null;
        }
    },

    // Pre-process image for better OCR results
    preprocessImage: function (imageDataUrl) {
        return new Promise((resolve) => {
            const img = new Image();
            img.onload = function () {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                canvas.width = img.width;
                canvas.height = img.height;
                
                // Draw original image
                ctx.drawImage(img, 0, 0);
                
                // Get image data
                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;
                
                // Convert to grayscale and increase contrast
                for (let i = 0; i < data.length; i += 4) {
                    const avg = (data[i] + data[i + 1] + data[i + 2]) / 3;
                    const contrast = 1.5; // Increase contrast
                    const adjusted = ((avg - 128) * contrast) + 128;
                    
                    data[i] = adjusted;     // Red
                    data[i + 1] = adjusted; // Green
                    data[i + 2] = adjusted; // Blue
                }
                
                ctx.putImageData(imageData, 0, 0);
                resolve(canvas.toDataURL('image/png'));
            };
            img.src = imageDataUrl;
        });
    }
};
