function downloadcsv(filename, content) {
    const element = document.createElement('a');
    const file = new Blob([content], { type: 'text/csv' });
    element.href = URL.createObjectURL(file);
    element.download = filename;
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}

function printCoursePlanPdf(htmlContent) {
    // Create a new window for printing
    const printWindow = window.open('', '_blank', 'width=800,height=600');

    if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();

        // Wait for images to load before printing
        printWindow.onload = function() {
            setTimeout(function() {
                printWindow.print();
            }, 500);
        };
    } else {
        alert('Please allow pop-ups to download the PDF');
    }
}

function downloadFile(url, filename) {
    // Create a temporary anchor element
    const element = document.createElement('a');
    element.href = url;
    element.download = filename || 'download';
    element.target = '_blank';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}

function downloadCertificateAsPdf(htmlContent, filename) {
    // Open a new window with the certificate HTML
    const printWindow = window.open('', '_blank', 'width=800,height=1000');

    if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();

        // Wait for content to load, then trigger print/save as PDF
        printWindow.onload = function() {
            setTimeout(function() {
                printWindow.print();
            }, 500);
        };
    } else {
        alert('Please allow pop-ups to download the certificate');
    }
}

function downloadFileFromBase64(base64, contentType, fileName) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

function openPrintWindow(htmlContent) {
    const printWindow = window.open('', '_blank', 'width=1000,height=700');
    if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();
        printWindow.onload = function() {
            setTimeout(function() {
                printWindow.print();
            }, 500);
        };
    } else {
        alert('Please allow pop-ups to export PDF');
    }
}