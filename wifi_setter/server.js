// server.js
const express = require('express');
const fs = require('fs');
const path = require('path');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const port = 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(express.static('public'));

// Serve the HTML page
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Handle form submission
app.post('/save-config', (req, res) => {
    const { ssid, password } = req.body;

    // Log the body for debugging
    console.log(`Received body: ${JSON.stringify(req.body, null, 2)}`);

    if (!ssid || !password) {
        return res.status(400).send('Invalid input');
    }

    const newNetwork = `
network={
    ssid="${ssid}"
    psk="${password}"
}`;

    // /etc/wpa_supplicant/wpa_supplicant.conf
    const customPath = 'D:/iot-portal/wifi_setter/wpa_supplicant.conf';

    // Backup the original file
    const backupPath = 'D:/iot-portal/wifi_setter/wpa_supplicant.conf.bak';
    fs.copyFile(customPath, backupPath, (err) => {
        if (err) {
            console.error('Error creating backup:', err);
        } else {
            console.log('Backup created at', backupPath);
        }
    });

    // Read the existing content of the file
    fs.readFile(customPath, 'utf8', (readErr, data) => {
        if (readErr) {
            console.error('Error reading file:', readErr);
            return res.status(500).send('Error reading file');
        }

        // Append the new network configuration to the existing content
        const updatedConfig = data.trim() + '\n' + newNetwork;

        // Write the updated content back to the original file
        fs.writeFile(customPath, updatedConfig, (writeErr) => {
            if (writeErr) {
                console.error('Error writing file:', writeErr);
                return res.status(500).send('Error writing file');
            }

            console.log('Configuration saved successfully');
            res.send('Configuration saved successfully.');
        });
    });
});

// Start the server
app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});
