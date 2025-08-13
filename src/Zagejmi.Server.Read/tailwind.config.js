// tailwind.config.js
module.exports = {
    mode: 'jit',
    content: [
        "./**/*.{razor,html,cshtml}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    950: 'var(--color-primary-950)',
                    900: 'var(--color-primary-900)',
                },
                accent: {
                    50:  'var(--color-accent-50)',
                    100: 'var(--color-accent-100)',
                    200: 'var(--color-accent-200)',
                    300: 'var(--color-accent-300)',
                    400: 'var(--color-accent-400)',
                    500: 'var(--color-accent-500)',
                    600: 'var(--color-accent-600)',
                    700: 'var(--color-accent-700)',
                    800: 'var(--color-accent-800)',
                    900: 'var(--color-accent-900)',
                    950: 'var(--color-accent-950)',
                },
                // Custom text color names
                'text-default': 'var(--color-text-default)',
                'text-muted':   'var(--color-text-muted)',
                'text-accent':  'var(--color-text-accent)',
                'text-dark':    'var(--color-text-dark)',
            },
        },
    },
    plugins: [],
}