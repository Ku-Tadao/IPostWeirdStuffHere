// TODO: 
// Add error handling and logging for all functions and methods to make the extension more robust and user-friendly
// Add comments to explain the code and the logic behind it for beginners

console.log('Script started');

// Path: styleConfig.js
import { elementsToStyle } from './styleConfig.js';

// Load Inter font
function loadInterFont() {
    if (!document.querySelector('link[href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap"]')) {
        const link = document.createElement('link');
        link.href = 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap';
        link.rel = 'stylesheet';
        document.head.appendChild(link);
    }
}

// Apply CSS rules to the document
function applyCSSRules(doc) {
    const styleSheet = doc.head.appendChild(doc.createElement("style"));
    elementsToStyle.forEach(item => {
        const cssString = `${item.selector} { ${Object.entries(item.style).map(([key, value]) => `${key}: ${value} !important`).join("; ")}; }`;
        styleSheet.sheet.insertRule(cssString, styleSheet.sheet.cssRules.length);

        if (item.pseudo) {
            Object.entries(item.pseudo).forEach(([pseudoElement, styles]) => {
                const pseudoCssString = `${item.selector}${pseudoElement} { ${Object.entries(styles).map(([key, value]) => `${key}: ${value} !important`).join("; ")}; }`;
                styleSheet.sheet.insertRule(pseudoCssString, styleSheet.sheet.cssRules.length);
            });
        }
    });
}

// Setup document styles and observer
function setupDocumentStylesAndObserver(doc) {
    applyCSSRules(doc);

    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1) { 
                    if (node.tagName === 'IFRAME' && node.contentWindow) {
                        if (node.contentWindow.document.readyState === 'complete') {
                            setupDocumentStylesAndObserver(node.contentDocument);
                        } else {
                            node.addEventListener('load', () => setupDocumentStylesAndObserver(node.contentDocument));
                        }
                    }
                }
            });
        });
    });

    observer.observe(doc.body, {
        childList: true,
        subtree: true
    });
}

// Load Inter font and setup document styles and observer
if (document.readyState === 'complete') {
    loadInterFont();
    setupDocumentStylesAndObserver(document);
} else {
    window.addEventListener('load', () => {
        loadInterFont();
        setupDocumentStylesAndObserver(document);
    });
}
