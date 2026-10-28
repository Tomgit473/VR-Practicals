import QRCode from 'qrcode';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Parse CLI arguments (--url <URL> or --output <dir>)
const args = process.argv.slice(2);
let targetUrl = process.env.DEPLOYED_URL || 'https://your-username.github.io/web-ar/';

for (let i = 0; i < args.length; i++) {
  if ((args[i] === '--url' || args[i] === '-u') && args[i + 1]) {
    targetUrl = args[i + 1];
    i++;
  }
}

const assetsDir = path.resolve(__dirname, '../assets');
if (!fs.existsSync(assetsDir)) {
  fs.mkdirSync(assetsDir, { recursive: true });
}

const pngPath = path.join(assetsDir, 'qr-code.png');
const svgPath = path.join(assetsDir, 'qr-code.svg');

// High error correction ('H') allows up to ~30% damage / occlusions,
// providing optimal scanning on mobile screens, printed posters, and in dim light.
const qrOptions = {
  errorCorrectionLevel: 'H',
  type: 'image/png',
  quality: 0.95,
  margin: 4, // 4-module quiet zone per ISO/IEC 18004
  scale: 12,
  color: {
    dark: '#0a0a0c', // High contrast deep charcoal
    light: '#ffffff' // Pure crisp white
  }
};

async function generate() {
  console.log('----------------------------------------------------');
  console.log('WebAR QR Code Generator');
  console.log(`Target URL: ${targetUrl}`);
  console.log('Error Correction: Level H (High ~30% redundancy)');
  console.log('Quiet Zone: 4 modules');
  console.log('----------------------------------------------------');

  try {
    // 1. Generate PNG (high resolution for print)
    await QRCode.toFile(pngPath, targetUrl, {
      ...qrOptions,
      width: 1024
    });
    console.log(`[SUCCESS] Generated PNG QR code -> ${pngPath}`);

    // 2. Generate SVG (infinite vector scalability for large posters/signage)
    const svgString = await QRCode.toString(targetUrl, {
      ...qrOptions,
      type: 'svg'
    });
    fs.writeFileSync(svgPath, svgString, 'utf8');
    console.log(`[SUCCESS] Generated SVG QR code -> ${svgPath}`);

    console.log('\nUse either qr-code.png or qr-code.svg on your printed posters.');
    console.log('To specify a custom URL, run:');
    console.log('  node generate-qr.js --url https://your-domain.com');
  } catch (err) {
    console.error('[ERROR] Failed to generate QR codes:', err);
    process.exit(1);
  }
}

generate();
