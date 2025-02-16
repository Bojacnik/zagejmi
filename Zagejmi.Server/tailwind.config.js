/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: 'jit',
    content: [
        "./**/*.{razor,html,cshtml}",
        "../BojaWeb.Client/**/*.{razor,html,cshtml}",
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}

