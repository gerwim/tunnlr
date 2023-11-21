import { chromium, type FullConfig } from '@playwright/test';
import { exec } from 'child_process';

async function globalTeardown(config: FullConfig) {
    await dockerComposeDown();
}

async function dockerComposeDown() {
    return new Promise((resolve, reject) => {
        exec('docker compose down', (err, stdout, stderr) => {
            if (err) {
                reject(err);
            } else {
                resolve(stdout);
            }
        });
    });
}

export default globalTeardown;