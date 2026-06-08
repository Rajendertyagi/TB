// 1. The Master Dictionary (Add all your XAML paths here)
const IconPaths = {
    'back': 'M 15 18 l -6 -6 l 6 -6',
    'forward': 'M 9 18 l 6 -6 l -6 -6',
    'refresh': 'M 3 12 a 9 9 0 1 0 2.6 -6.4 L 2 9 M 9 9 H 2 V 2',
    'stop': 'M 18 6 L 6 18 M 6 6 l 12 12',
    'home': 'M 4 10 l 8 -7 l 8 7 M 6 10 v 9 a 2 2 0 0 0 2 2 h 8 a 2 2 0 0 0 2 -2 v -9',
    'search': 'M 21 21 l -4.35 -4.35 M 10.5 17 a 6.5 6.5 0 1 0 0 -13 a 6.5 6.5 0 0 0 0 13 z',
    'settings': 'M 12 15 a 3 3 0 1 0 0 -6 a 3 3 0 0 0 0 6 z M 12 2 v 2 M 12 20 v 2 M 4.93 4.93 l 1.41 1.41 M 17.66 17.66 l 1.41 1.41 M 2 12 h 2 M 20 12 h 2 M 4.93 19.07 l 1.41 -1.41 M 17.66 6.34 l 1.41 -1.41',
    'add-tab': 'M 12 5 v 14 M 5 12 h 14',
    'tab-close': 'M 16 8 L 8 16 M 8 8 l 8 8',
    'window-min': 'M 4 12 h 16',
    'window-max': 'M 4 4 h 16 v 16 h -16 z',
    'window-close': 'M 18 6 L 6 18 M 6 6 l 12 12',
    'menu': 'M 3 12 h 18 M 3 6 h 18 M 3 18 h 18',
    'lock': 'M 16 11 V 7 a 4 4 0 0 0 -8 0 v 4 M 7 11 h 10 a 2 2 0 0 1 2 2 v 7 a 2 2 0 0 1 -2 2 H 7 a 2 2 0 0 1 -2 -2 v -7 a 2 2 0 0 1 2 -2 z',
    'info': 'M 12 2 a 10 10 0 1 0 0 20 a 10 10 0 1 0 0 -20 z M 12 16 v -4 M 12 8 h .01'
};

// 2. Helper function to generate the SVG string
function getIcon(name) {
    const path = IconPaths[name] || '';
    return `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="${path}"/></svg>`;
}

// 3. Auto-inject icons into static HTML elements on load
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-icon]').forEach(el => {
        el.innerHTML = getIcon(el.getAttribute('data-icon'));
    });
});