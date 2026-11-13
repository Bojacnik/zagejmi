// tailwind.config.js
// Theme colors live in wwwroot/app.css as RGB channels; `<alpha-value>` lets Tailwind apply opacity
// modifiers such as bg-primary-950/80. A plain var() (hex) silently drops every `/xx` class.
const themeColor = (name) => `rgb(var(--color-${name}) / <alpha-value>)`;

module.exports = {
    mode: 'jit',
    content: [
        "./**/*.{razor,html,cshtml}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    950: themeColor('primary-950'),
                    900: themeColor('primary-900'),
                    800: themeColor('primary-800'),
                    700: themeColor('primary-700'),
                    600: themeColor('primary-600'),
                },
                accent: {
                    50:  themeColor('accent-50'),
                    100: themeColor('accent-100'),
                    200: themeColor('accent-200'),
                    300: themeColor('accent-300'),
                    400: themeColor('accent-400'),
                    500: themeColor('accent-500'),
                    600: themeColor('accent-600'),
                    700: themeColor('accent-700'),
                    800: themeColor('accent-800'),
                    900: themeColor('accent-900'),
                    950: themeColor('accent-950'),
                },
                // Custom text color names
                'text-default': themeColor('text-default'),
                'text-muted':   themeColor('text-muted'),
                'text-accent':  themeColor('text-accent'),
                'text-dark':    themeColor('text-dark'),
            },
        },
    },
    plugins: [],
}
