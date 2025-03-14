/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.{cshtml,js}',
    './wwwroot/**/*.{html,js}'
  ],
  theme: {
    extend: {
      colors: {
        'ut-orange': {
          DEFAULT: '#ff8811',
          '100': '#361b00',
          '200': '#6c3600',
          '300': '#a25100',
          '400': '#d86c00',
          '500': '#ff8811',
          '600': '#ff9f3f',
          '700': '#ffb76f',
          '800': '#ffcf9f',
          '900': '#ffe7cf'
        },
        'jasmine': {
          DEFAULT: '#f4d06f',
          '100': '#423205',
          '200': '#85640a',
          '300': '#c7960f',
          '400': '#efbc2e',
          '500': '#f4d06f',
          '600': '#f6da8d',
          '700': '#f9e4aa',
          '800': '#fbedc6',
          '900': '#fdf6e3'
        },
        'floral-white': {
          DEFAULT: '#fff8f0',
          '100': '#633500',
          '200': '#c66a00',
          '300': '#ff9c2a',
          '400': '#ffca8d',
          '500': '#fff8f0',
          '600': '#fff9f3',
          '700': '#fffbf6',
          '800': '#fffcf9',
          '900': '#fffefc'
        },
        'tiffany-blue': {
          DEFAULT: '#9dd9d2',
          '100': '#153632',
          '200': '#2a6b64',
          '300': '#3fa195',
          '400': '#66c4b9',
          '500': '#9dd9d2',
          '600': '#b0e0db',
          '700': '#c4e8e4',
          '800': '#d7f0ed',
          '900': '#ebf7f6'
        },
        'space-cadet': {
          DEFAULT: '#392f5a',
          '100': '#0b0a12',
          '200': '#171324',
          '300': '#221d36',
          '400': '#2e2648',
          '500': '#392f5a',
          '600': '#59498b',
          '700': '#7d6db2',
          '800': '#a89dcc',
          '900': '#d4cee5'
        },
        'primary': '#392f5a',
        'primary-dark': '#221d36'
      }
    }
  },
  plugins: []
}