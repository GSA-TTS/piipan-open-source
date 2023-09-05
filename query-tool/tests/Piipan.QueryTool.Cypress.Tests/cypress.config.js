const { defineConfig } = require('cypress')
const { prepareAudit } = require("@cypress-audit/lighthouse");
const { pa11y } = require("@cypress-audit/pa11y");

module.exports = defineConfig({
    viewportHeight: 1000,
    viewportWidth: 1280,
    screenshotOnRunFailure: true,
    video: true,
    e2e: {
        // We've imported your old cypress plugins here.
        // You may want to clean this up later by importing these.
        setupNodeEvents(on, config) {
            on("before:browser:launch", (launchOptions, browser = {}) => {
                prepareAudit(launchOptions);
            });

            on("task", {
                pa11y: pa11y(console.log.bind(console)),
            });
            return require('./cypress/plugins/index.js')(on, config)
        },
        baseUrl: 'https://localhost:5001',
    },
})
