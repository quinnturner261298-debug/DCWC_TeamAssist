// OCR functionality using Tesseract.js
window.ocrHelper = {
    processImage: async function (imageDataUrl, dotNetHelper) {
        console.log('?? JS OCR: Starting Tesseract.js processing...');
        try {
            console.log('   ?? Creating Tesseract worker...');
            // Show progress updates
            const worker = await Tesseract.createWorker('eng', 1, {
                logger: m => {
                    if (m.status === 'recognizing text') {
                        const progress = Math.round(m.progress * 100);
                        console.log(`   ? OCR Progress: ${progress}%`);
                        dotNetHelper.invokeMethodAsync('UpdateProgress', progress);
                    } else {
                        console.log(`   ?? OCR: ${m.status} - ${m.progress ? Math.round(m.progress * 100) + '%' : ''}`);
                    }
                }
            });

            console.log('   ? Worker created, starting text recognition...');
            const { data: { text } } = await worker.recognize(imageDataUrl);
            console.log(`   ? Text recognition complete! Extracted ${text.length} characters`);
            console.log(`   ?? First 100 chars: "${text.substring(0, 100)}${text.length > 100 ? '...' : ''}"`);
            
            await worker.terminate();
            console.log('   ?? Worker terminated');

            return text;
        } catch (error) {
            console.error('? JS OCR Error:', error);
            console.error('   Stack:', error.stack);
            return null;
        }
    },

    // Pre-process image for better OCR results
    preprocessImage: function (imageDataUrl) {
        console.log('?? JS OCR: Preprocessing image...');
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = function () {
                console.log(`   ?? Image loaded: ${img.width}x${img.height}`);
                
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                canvas.width = img.width;
                canvas.height = img.height;
                
                // Draw original image
                ctx.drawImage(img, 0, 0);
                console.log('   ? Image drawn to canvas');
                
                // Get image data
                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;
                console.log(`   ?? Processing ${data.length / 4} pixels...`);
                
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
                const processedDataUrl = canvas.toDataURL('image/png');
                console.log(`   ? Preprocessing complete (output: ${processedDataUrl.length} chars)`);
                resolve(processedDataUrl);
            };
            img.onerror = function(error) {
                console.error('? Image load error:', error);
                reject(error);
            };
            img.src = imageDataUrl;
        });
    }
};
