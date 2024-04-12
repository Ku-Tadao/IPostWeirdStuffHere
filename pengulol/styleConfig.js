export const elementsToStyle = [
    {
        selector: '.group-header',
        style: {
            'background-color': '#13151b',
            'margin-bottom': '5px',
            'margin-left': '5px',
            'margin-right': '5px',
            'border-radius': '8px'
        }
    },
    {
        selector: '.friend-requests',
        style: {
            'background-color': '#312d28',
            'color': '#ba9b55'
        }
    },
    {
        selector: '.social-count-badge',
        style: {
            'background-color': '#2d171b',
            'color': '#df3a3a',
            'align-items': 'center',
            'justify-content': 'center',
            'padding': '1px 8px'
        }
    },
    {
        selector: '.lol-social-sidebar',
        style: {
            'background-color': '#0e1015'
        }
    },
    {
        selector: 'lol-social-chat-window #chat-window-wrapper',
        style: {
            'background-color': 'rgba(14, 16, 21, 0.88)',
            'border': 'thin solid #13151b'
        }
    },
    {
        selector: '.lol-social-actions-bar .friend-finder-button, .lol-social-actions-bar .folder-button, .lol-social-actions-bar .options-button, .lol-social-actions-bar .filter-button',
        style: {
            'background-color': '#7f8085'
        }
    },
    {
        selector: '.navbar_backdrop',
        style: {
            'background-color': 'rgba(14, 16, 21, 0.85)'
        }
    },
    {
        selector: '.their-bubble',
        style: {
            'background-color': '#132e31'
        },
        pseudo: {
            '::after': {
                'left': '-5px',
                'border-top-color': '#132e31'
            }
        }
    },
    {
        selector: '.my-bubble',
        style: {
            'background-color': '#2d171b'
        },
        pseudo: {
            '::after': {
                'right': '-5px',
                'border-top-color': '#2d171b'
            }
        }
    },
    {
        selector: ':root',
        style: {
            '--font-body': '"Inter", Arial, "Helvetica Neue", Helvetica, sans-serif',
            '--font-display': '"Inter", Arial, "Helvetica Neue", Helvetica, sans-serif'
        }
    }
];
