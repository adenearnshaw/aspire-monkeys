const fs = require('node:fs');
const path = require('node:path');

const target = process.env.WEB_BFF_URL || 'https://localhost:7051';

// Angular CLI proxy config
const proxy = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
  },
};

const outPath = path.resolve(__dirname, '..', 'proxy.conf.json');
fs.writeFileSync(outPath, JSON.stringify(proxy, null, 2) + '\n', 'utf8');

console.log(`[proxy] wrote ${outPath} -> ${target}`);
