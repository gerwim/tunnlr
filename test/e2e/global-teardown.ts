import { chromium, type FullConfig } from '@playwright/test';

async function globalTeardown(config: FullConfig) {
    const { exec } = require('child_process');
    exec('docker compose down', (err, stdout, stderr) => {
        if (err) {
            // some err occurred
            console.error(err);
        } else {
            // the *entire* stdout and stderr (buffered)
            console.log(`stdout: ${stdout}`);
            console.log(`stderr: ${stderr}`);
        }
    });
}

export default globalTeardown;