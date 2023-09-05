# shellcheck disable=SC2034

# Deployment environment for resource identifiers
ENV=$(basename "${BASH_SOURCE%.*}")

# Prefix for resource identifiers
PREFIX="tts"

# Either AzureCloud or AzureUSGovernment
CLOUD_NAME="AzureCloud"

# Default location
LOCATION="westus2"

# Default resource group
RESOURCE_GROUP="rg-core-${ENV}"

# Resource group for matching API
MATCH_RESOURCE_GROUP="rg-match-${ENV}"

# Security resource group
SECURITY_RESOURCE_GROUP="rg-security-${ENV}"

# IaC Feature Flags (WIP)
PROVISION_APIM_RESOURCES="true"
PROVISION_CORE_DATABASE_RESOURCES="true"
PROVISION_DASHBOARD_APP_SERVICE="true"
PROVISION_DASHBOARD_FD="true"
PROVISION_METRICS_RESOURCES="true"
PROVISION_QUERY_TOOL_APP_SERVICE="true"
PROVISION_QUERY_TOOL_FD="true"

# OIDC configuration - all apps
IDP_OIDC_IP_RANGES=""

# OIDC configuration - Dashboard app
DASHBOARD_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
DASHBOARD_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
DASHBOARD_APP_IDP_CLIENT_ID="d7281a84-817d-4a76-8005-b29f67594340"

# OIDC configuration - Query Tool app
QUERY_TOOL_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
QUERY_TOOL_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
QUERY_TOOL_APP_IDP_CLIENT_ID="71286b1e-5f5a-4757-ab5f-714802f33277"

# SIEM tool app registration name
SIEM_RECEIVER="${PREFIX}-siem-tool-${ENV}"

# Azure Storage SKU for per-state storage accounts and storage accounts backing function apps
STORAGE_SKU="Standard_ZRS" # Standard Zone Redundant Storage
# STORAGE_SKU="Standard_LRS" # Standard Locally Redundant Storage (Use this when ZRS is not available in the region)

# Event Hub
# EH_ZONE_REDUNDANCY parameter must be set to false for regions that do not
# support zone redundancy, like USGovArizona in a DR scenario
EH_ZONE_REDUNDANCY="true"

# Log Analytics
LOG_ANALYTICS_RETENTION_DAYS="60"

# Azure API Management (APIM)
# Email
APIM_EMAIL="noreply@tts.test"
# SKU
APIM_SKU="Developer"

# Security Contacts / Notifications
# Additional email addresses (separated by semicolons)
SECURITY_ADDITIONAL_EMAILS="security@tts.test;noreply@tts.test"
# String array containing any of: 'AccountAdmin','Contributor','Owner','ServiceAdmin'
SECURITY_NOTIFICATION_ROLES='("Contributor","Owner")'
# String containing one of: 'Low', 'Medium', 'High'
SECURITY_MINIMAL_SEVERITY="Medium"

# Activity Log
# Default email address for Activity Log Alerts
ACTIVITY_LOG_ALERT_EMAIL="security@tts.test"
# Activity Log is subscription-wide, so it's only needed for one environment,
# not every environment that is nested in the subscription.
CONFIGURE_ACTIVITY_LOG="true"


# Email Configuration
ENABLE_EMAILS="false"
EMAIL_ZONE_REDUNDANCY="1 2 3"
# EMAIL_ZONE_REDUNDANCY defines the zones for the Azure Public IP.
# USGovVirgina supports three zones, but USGovArizona only supportes one.
# EMAIL_ZONE_REDUNDANCY="1"

# Email Relay
SMTP_SERVER="smtp.gmail.com"
SMTP_FROM_EMAIL="noreply@tts.test"
SMTP_CC_EMAIL="noreply@tts.test"
SMTP_REPLYTO_EMAIL="noreply@tts.test"
SMTP_BCC_EMAIL="noreply@tts.test"
